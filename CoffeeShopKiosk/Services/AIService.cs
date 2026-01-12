using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CoffeeShopKiosk.Services
{
    public interface IAIService
    {
        Task<string> GetStudyTipAsync(string topic);
        Task<string> ChatAsync(string userMessage, object? context = null);
        Task<string> ChatStreamAsync(string userMessage, Action<string> onPartial, object? context = null);
        Task<bool> TestApiKeyAsync();
    }

    public class AIService : IAIService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public AIService(HttpClient httpClient = null)
        {
            _http = httpClient ?? new HttpClient();
            _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty;
            // If no environment key is configured, fall back to stored local settings (if present)
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                try
                {
                    var s = new SettingsService();
                    _apiKey = s.Settings.OpenAIKey ?? string.Empty;
                }
                catch
                {
                    // ignore and continue with empty key (local fallback)
                }
            }
        }

        // Fetch top Wikipedia search snippets for a query (title, snippet, url)
        public async Task<List<(string title, string snippet, string url)>> GetWikiSnippetsAsync(string query, int max = 3)
        {
            var results = new List<(string, string, string)>();
            try
            {
                var url = "https://en.wikipedia.org/w/api.php?action=query&list=search&srsearch=" + Uri.EscapeDataString(query) + "&utf8=&format=json";
                using var resp = await _http.GetAsync(url);
                resp.EnsureSuccessStatusCode();
                var json = await resp.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var search = doc.RootElement.GetProperty("query").GetProperty("search");
                int count = 0;
                foreach (var item in search.EnumerateArray())
                {
                    if (count++ >= max) break;
                    var title = item.GetProperty("title").GetString() ?? string.Empty;
                    var snippet = item.GetProperty("snippet").GetString() ?? string.Empty;
                    // snippet contains HTML-like <span class="searchmatch"> tags — strip simple tags
                    snippet = System.Text.RegularExpressions.Regex.Replace(snippet, "<.*?>", string.Empty);
                    var pageUrl = "https://en.wikipedia.org/wiki/" + Uri.EscapeDataString(title.Replace(' ', '_'));
                    results.Add((title, snippet, pageUrl));
                }
            }
            catch { }

            return results;
        }

        public async Task<string> GetStudyTipAsync(string topic)
        {
            // Keep existing behavior — delegate to the chat interface with a constrained prompt
            var prompt = BuildPrompt(topic);
            return await ChatAsync(prompt);
        }

        public async Task<string> ChatAsync(string userMessage, object? context = null)
        {
            // Build strong system prompt with few-shot examples requesting JSON output and including wiki snippets when available
            var system = "You are a concise, friendly study assistant for college students. Keep answers short and action-oriented. Reply with valid JSON and ONLY valid JSON using fields: summary (1 sentence), tips (array of 1-2 sentence tips), action (one short actionable item), confidence (0-1), sources (array of source URLs). Do NOT include any explanatory text, markdown, or additional commentary — only emit the JSON object. Use the provided 'wiki snippets' to ground facts when present and include any URLs you used in the sources array. If you cannot answer, set summary to 'I don't know' and offer a strategy to find out.";

            // few-shot examples (include sources)
            var example1User = "How can I study for an upcoming calculus exam?";
            var example1Assistant = "{ \"summary\": \"Focus on problem practice and concept mapping.\", \"tips\": [\"Do 3-5 practice problems for each topic.\", \"Create a one-page formula cheat-sheet.\"], \"action\": \"Start a 25-min Pomodoro and solve 3 problems\", \"confidence\": 0.9, \"sources\": [] }";

            var example2User = "I don't know what the answer is to a question about niche genetics details.";
            var example2Assistant = "{ \"summary\": \"I don't know\", \"tips\": [\"Check primary literature or consult your instructor for niche genetics topics.\"], \"action\": \"Search PubMed or ask your instructor\", \"confidence\": 0.0, \"sources\": [] }";

            var messages = new List<object>
            {
                new { role = "system", content = system },
                new { role = "user", content = example1User },
                new { role = "assistant", content = example1Assistant },
                new { role = "user", content = example2User },
                new { role = "assistant", content = example2Assistant },
                new { role = "user", content = userMessage }
            };

            // Try to include wiki snippets to ground the answer
            try
            {
                var wiki = await GetWikiSnippetsAsync(userMessage, 3);
                if (wiki.Count > 0)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Wiki snippets (relevant):");
                    foreach (var w in wiki)
                    {
                        sb.AppendLine($"- {w.title}: {w.snippet} ({w.url})");
                    }

                    messages.Add(new { role = "system", content = sb.ToString() });
                }
            }
            catch { }

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                // If no API key is configured, try to answer with Wikipedia snippets when possible (sourced, deterministic)
                try
                {
                    var wiki = await GetWikiSnippetsAsync(userMessage, 3);
                    if (wiki.Count > 0)
                    {
                        // Build a structured response from the top snippet(s)
                        var first = wiki[0];
                        // Use first sentence of snippet as concise summary when possible
                        var summary = first.snippet;
                        var dot = summary.IndexOf('.');
                        if (dot > 0) summary = summary.Substring(0, dot + 1);

                        var studyResp = new {
                            summary = summary,
                            tips = new[] { "Read the source for details.", "Compare multiple sources for accuracy.", "Take notes and summarize key facts." },
                            action = "Open the first source",
                            confidence = 0.7,
                            sources = wiki.ConvertAll(w => w.url)
                        };

                        var json = JsonSerializer.Serialize(studyResp);
                        // record raw response when debug logging enabled
                        try {
                            var settings = new SettingsService();
                            if (settings.Settings.EnableAIDebugLogging)
                            {
                                var telemetry = new TelemetryService();
                                telemetry.RecordRawResponse(json);
                            }
                        } catch { }

                        return json;
                    }
                }
                catch { }

                // fallback: return structured JSON so UI parsing works consistently
                var local = GetLocalStructuredTip(userMessage);
                try
                {
                    var settings = new SettingsService();
                    if (settings.Settings.EnableAIDebugLogging)
                    {
                        var telemetry = new TelemetryService();
                        telemetry.RecordRawResponse(local);
                    }
                }
                catch { }
                return local;
            }

            try
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var payload = new
                {
                    model = "gpt-4o-mini",
                    messages = messages,
                    max_tokens = 400,
                    temperature = 0.25
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var resp = await _http.PostAsync("https://api.openai.com/v1/chat/completions", content);
                resp.EnsureSuccessStatusCode();

                var respJson = await resp.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(respJson);
                var msg = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;

                // record raw response when debug logging enabled
                try {
                    var settings = new SettingsService();
                    if (settings.Settings.EnableAIDebugLogging)
                    {
                        var telemetry = new TelemetryService();
                        telemetry.RecordRawResponse(msg);
                    }
                } catch { }

                return msg;
            }
            catch (Exception ex)
            {
                return "{ \"summary\": \"I couldn't reach the study assistant right now.\", \"tips\": [\"Try again later.\"], \"action\": \"Check your network or API key\", \"confidence\": 0.0 }";
            }
        }

        public async Task<bool> TestApiKeyAsync()
        {
            // Quick check against models endpoint
            if (string.IsNullOrWhiteSpace(_apiKey)) return false;
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, "https://api.openai.com/v1/models");
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                using var resp = await _http.SendAsync(req);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // New: streaming variant that calls the API with stream=true and reports partial text via callback
        public async Task<string> ChatStreamAsync(string userMessage, Action<string> onPartial, object? context = null)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                // If no API key, try to stream a WIkipedia-backed structured response where possible
                var wiki = await GetWikiSnippetsAsync(userMessage, 3);
                if (wiki.Count > 0)
                {
                    var first = wiki[0];
                    var summary = first.snippet;
                    var dot = summary.IndexOf('.');
                    if (dot > 0) summary = summary.Substring(0, dot + 1);

                    var studyRespObj = new {
                        summary = summary,
                        tips = new[] { "Read the source for details.", "Compare multiple sources for accuracy.", "Take notes and summarize key facts." },
                        action = "Open the first source",
                        confidence = 0.7,
                        sources = wiki.ConvertAll(w => w.url)
                    };

                    var studyJson = JsonSerializer.Serialize(studyRespObj);

                    // Stream it char by char
                    foreach (var ch in studyJson)
                    {
                        onPartial(ch.ToString());
                        await Task.Delay(4);
                    }

                    // record for debug
                    try
                    {
                        var settings = new SettingsService();
                        if (settings.Settings.EnableAIDebugLogging)
                        {
                            var telemetry = new TelemetryService();
                            telemetry.RecordRawResponse(studyJson);
                        }
                    }
                    catch { }

                    return studyJson;
                }

                // fallback to local structured tip
                var local = GetLocalStructuredTip(userMessage);
                foreach (var ch in local)
                {
                    onPartial(ch.ToString());
                    await Task.Delay(4); // slight delay to mimic streaming
                }

                // record the raw local fallback for debugging if enabled
                try
                {
                    var settings = new SettingsService();
                    if (settings.Settings.EnableAIDebugLogging)
                    {
                        var telemetry = new TelemetryService();
                        telemetry.RecordRawResponse(local);
                    }
                }
                catch { }

                return local;
            }

            // Build same messages as ChatAsync
            var system = "You are a concise, friendly study assistant for college students. Keep answers short and action-oriented. Reply with valid JSON and ONLY valid JSON using fields: summary (1 sentence), tips (array of 1-2 sentence tips), action (one short actionable item), confidence (0-1). Do NOT include any explanatory text, markdown, or additional commentary — only emit the JSON object. If you cannot answer, set summary to 'I don't know' and offer a strategy to find out.";
            var example1User = "How can I study for an upcoming calculus exam?";
            var example1Assistant = "{ \"summary\": \"Focus on problem practice and concept mapping.\", \"tips\": [\"Do 3-5 practice problems for each topic.\", \"Create a one-page formula cheat-sheet.\"], \"action\": \"Start a 25-min Pomodoro and solve 3 problems\", \"confidence\": 0.9 }";
            var example2User = "I don't know what the answer is to a question about niche genetics details.";
            var example2Assistant = "{ \"summary\": \"I don't know\", \"tips\": [\"Check primary literature or consult your instructor for niche genetics topics.\"], \"action\": \"Search PubMed or ask your instructor\", \"confidence\": 0.0 }";

            var messages = new List<object>
            {
                new { role = "system", content = system },
                new { role = "user", content = example1User },
                new { role = "assistant", content = example1Assistant },
                new { role = "user", content = example2User },
                new { role = "assistant", content = example2Assistant },
                new { role = "user", content = userMessage }
            };

            try
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

                var payload = new
                {
                    model = "gpt-4o-mini",
                    messages = messages,
                    max_tokens = 400,
                    temperature = 0.6,
                    stream = true
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions") { Content = content };
                using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
                resp.EnsureSuccessStatusCode();

                using var stream = await resp.Content.ReadAsStreamAsync();
                using var reader = new System.IO.StreamReader(stream);
                string? line;
                var sb = new System.Text.StringBuilder();
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // SSE lines often begin with "data: " prefix
                    var data = line.StartsWith("data: ") ? line.Substring(6) : line;
                    if (data == "[DONE]") break;

                    try
                    {
                        using var doc = JsonDocument.Parse(data);
                        var root = doc.RootElement;
                        if (root.TryGetProperty("choices", out var choices))
                        {
                            var delta = choices[0].GetProperty("delta");
                            if (delta.TryGetProperty("content", out var contentDelta))
                            {
                                var partial = contentDelta.GetString() ?? string.Empty;
                                // report partial to callback and accumulate
                                onPartial(partial);
                                sb.Append(partial);
                            }
                        }
                    }
                    catch
                    {
                        // ignore parse errors for intermediate lines
                    }
                }

                var final = sb.ToString();

                // record raw response for debugging if enabled
                try
                {
                    var settings = new SettingsService();
                    if (settings.Settings.EnableAIDebugLogging)
                    {
                        var telemetry = new TelemetryService();
                        telemetry.RecordRawResponse(final);
                    }
                }
                catch { }

                return final;
            }
            catch
            {
                var fallback = "{ \"summary\": \"I couldn't reach the study assistant right now.\", \"tips\": [\"Try again later.\"], \"action\": \"Check your network or API key\", \"confidence\": 0.0 }";
                return fallback;
            }
        }

        private string GetLocalStructuredTip(string topic)
        {
            var tips = new[]
            {
                "Break the material into 25-minute focused chunks.",
                "Summarize what you learned out loud.",
                "Teach it to someone or write a short summary."
            };

            var obj = new { summary = $"Quick tips for '{topic}'", tips = tips, action = "Start a 25-min Pomodoro", confidence = 0.6 };
            return JsonSerializer.Serialize(obj);
        }

        private string BuildPrompt(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic)) topic = "general study tips for productivity";
            return $"You are a helpful study assistant. Provide 3 concise, practical study tips (2-3 sentences each) for the following topic: {topic}. Use an encouraging tone and include a short action item at the end.";
        }

        private string GetLocalTip(string topic)
        {
            // Simple local fallback tips
            if (string.IsNullOrWhiteSpace(topic))
            {
                return "Try the Pomodoro technique: 25 minutes focused work followed by a 5-minute break. Remove distractions, set a single goal for the session, and review what you accomplished after each cycle.";
            }

            return $"Quick tips for '{topic}': 1) Break the material into 25-minute focused chunks. 2) Summarize what you learned out loud. 3) Teach it to someone or write a short summary. Use a timer to stay disciplined.";
        }
    }
}