using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShopKiosk.Services
{
    public interface IAIService
    {
        Task<string> GetStudyTipAsync(string topic);
        Task<string> ChatAsync(string userMessage);
    }

    public class AIService : IAIService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public AIService(HttpClient httpClient = null)
        {
            _http = httpClient ?? new HttpClient();
            _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty;
        }

        public async Task<string> GetStudyTipAsync(string topic)
        {
            // Keep existing behavior — delegate to the chat interface with a constrained prompt
            var prompt = BuildPrompt(topic);
            return await ChatAsync(prompt);
        }

        public async Task<string> ChatAsync(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                // local fallback: echo helpful guidance when API not configured
                return GetLocalTip(userMessage);
            }

            try
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var messages = new[]
                {
                    new { role = "system", content = "You are a helpful, concise study assistant for college students. Keep answers short (3 brief points when possible), friendly and action-oriented." },
                    new { role = "user", content = userMessage }
                };

                var payload = new
                {
                    model = "gpt-4o-mini",
                    messages = messages,
                    max_tokens = 350,
                    temperature = 0.7
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var resp = await _http.PostAsync("https://api.openai.com/v1/chat/completions", content);
                resp.EnsureSuccessStatusCode();

                var respJson = await resp.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(respJson);
                var msg = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
                return msg ?? "";
            }
            catch (Exception ex)
            {
                return "Sorry, I couldn't reach the study assistant right now. Try again later. (" + ex.Message + ")";
            }
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