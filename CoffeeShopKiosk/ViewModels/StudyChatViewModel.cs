using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CoffeeShopKiosk.Models;
using CoffeeShopKiosk.Services;

namespace CoffeeShopKiosk.ViewModels
{
    public class StudyChatViewModel : INotifyPropertyChanged
    {
        private readonly IAIService _ai;
        private string _inputText = string.Empty;
        private bool _isBusy;
        private bool _warnedNoApiKey = false;
        private bool _useChatMode = false; // free-text chat mode
        private double _messageFontSize = 13.0;

        public StudyChatViewModel(IAIService ai)
        {
            _ai = ai;
            Messages = new ObservableCollection<ChatMessage>();
            SendCommand = new RelayCommand<object>(async _ => await Send(), _ => !IsBusy);
            ClearCommand = new RelayCommand<object>(_ => Messages.Clear(), _ => !IsBusy);
            HelpFeedbackCommand = new RelayCommand<object>(p => SubmitFeedback(p));
        }

        public bool UseChatMode
        {
            get => _useChatMode;
            set { _useChatMode = value; OnPropertyChanged(); }
        }

        public double MessageFontSize
        {
            get => _messageFontSize;
            set { _messageFontSize = value; OnPropertyChanged(); }
        }

        public RelayCommand<object> HelpFeedbackCommand { get; }

        private void SubmitFeedback(object? param)
        {
            // param is a tuple (ChatMessage message, bool helpful)
            if (param is System.Tuple<ChatMessage, bool> t)
            {
                var msg = t.Item1;
                var helpful = t.Item2;
                // Simple telemetry: append to local telemetry log
                try
                {
                    var telemetry = new Services.TelemetryService();
                    telemetry.RecordFeedback(msg.Text, msg.Source ?? "unknown", helpful);
                }
                catch { }
            }
        }
        public ObservableCollection<ChatMessage> Messages { get; }

        public string InputText
        {
            get => _inputText;
            set { _inputText = value; OnPropertyChanged(); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand SendCommand { get; }
        public ICommand ClearCommand { get; }

        public async Task Send()
        {
            var text = InputText?.Trim();
            if (string.IsNullOrEmpty(text)) return;

            try
            {
                IsBusy = true;
                // append user message
                Messages.Add(new ChatMessage { Role = "user", Text = text, Timestamp = DateTimeOffset.Now, Source = "user" });
                InputText = string.Empty;

                // If no API key is configured, show a one-time system notice so the user knows this is a Wikipedia/local fallback
                try
                {
                    var settings = new SettingsService();
                    if (!_warnedNoApiKey && string.IsNullOrWhiteSpace(settings.Settings.OpenAIKey))
                    {
                        Messages.Add(new ChatMessage { Role = "system", Text = "Note: No OpenAI API key configured — the assistant will use Wikipedia/local sources. Configure an API key in Study Settings for remote LLM responses.", Timestamp = DateTimeOffset.Now, Source = "system" });
                        _warnedNoApiKey = true;
                    }
                }
                catch { }

                // Add placeholder assistant message which we'll update as stream/response arrives
                var assistantMsg = new ChatMessage { Role = "assistant", Text = "Thinking...", Timestamp = DateTimeOffset.Now, Source = "remote" };
                Messages.Add(assistantMsg);

                // Streaming: update assistant text as partials arrive. Pass ChatMode context so the service can produce free-text if requested
                bool partialStarted = false;
                var final = await (_ai as Services.AIService)?.ChatStreamAsync(text, partial =>
                {
                    // This callback may be on a non-UI thread; dispatch asynchronously to avoid blocking
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                        if (!partialStarted && assistantMsg.Text == "Thinking...")
                        {
                            assistantMsg.Text = partial;
                            partialStarted = true;
                        }
                        else
                        {
                            assistantMsg.Text += partial;
                        }
                    }));
                }, new { ChatMode = UseChatMode }) ?? await _ai.ChatAsync(text, new { ChatMode = UseChatMode });

                if (UseChatMode)
                {
                    // Free-text chat mode: prefer plain text responses but accept JSON when provided
                    if (string.IsNullOrWhiteSpace(final))
                    {
                        assistantMsg.Text = "Sorry, I couldn't generate an answer right now. Try again.";
                        assistantMsg.Source = "fallback";
                        assistantMsg.Confidence = 0.0;
                    }
                    else
                    {
                        var parsedChat = ParseStructuredResponse(final);
                        if (parsedChat != null)
                        {
                            var sb = new System.Text.StringBuilder();
                            if (!string.IsNullOrWhiteSpace(parsedChat.Summary)) sb.AppendLine(parsedChat.Summary);
                            if (parsedChat.Tips != null && parsedChat.Tips.Count > 0)
                            {
                                foreach (var t in parsedChat.Tips) sb.AppendLine("• " + t);
                            }
                            if (!string.IsNullOrWhiteSpace(parsedChat.Action)) sb.AppendLine("Action: " + parsedChat.Action);

                            assistantMsg.Text = sb.ToString().Trim();
                            assistantMsg.Confidence = parsedChat.Confidence;
                            assistantMsg.Source = parsedChat.Source ?? "remote";
                            assistantMsg.Sources.Clear();
                            if (parsedChat.Sources != null)
                            {
                                foreach (var s in parsedChat.Sources) assistantMsg.Sources.Add(s);
                            }
                        }
                        else
                        {
                            assistantMsg.Text = final.Trim();
                            assistantMsg.Source = "remote";
                            assistantMsg.Confidence = 0.0;
                        }
                    }
                }
                else
                {
                    // Structured mode: existing behavior — attempt to parse and retry corrections if needed
                    var parsed = ParseStructuredResponse(final);

                    if (parsed == null)
                    {
                        for (int attempt = 0; attempt < 2 && parsed == null; attempt++)
                        {
                            var correction = "Please reply with ONLY valid JSON in this format exactly as shown (no surrounding commentary): {\"summary\": \"one-sentence summary\", \"tips\": [\"tip 1\", \"tip 2\"], \"action\": \"one short action\", \"confidence\": 0.0, \"sources\": [\"url1\"] }.\nExample: {\"summary\":\"Focus on problem practice.\",\"tips\":[\"Do 3 problems per topic.\",\"Make a one-page formula sheet.\"],\"action\":\"Start a 25-min Pomodoro\",\"confidence\":0.9,\"sources\":[\"https://en.wikipedia.org/wiki/Studying\"]}.\nPrevious response was: " + final;
                            var retryResponse = await _ai.ChatAsync(correction);
                            if (!string.IsNullOrWhiteSpace(retryResponse))
                            {
                                parsed = ParseStructuredResponse(retryResponse);
                                if (parsed != null)
                                {
                                    // Use the parsed correction
                                    final = retryResponse;
                                    break;
                                }
                            }
                        }

                        if (parsed == null)
                        {
                            if (string.IsNullOrWhiteSpace(final) || final.Trim() == "{" || (final.Trim().StartsWith("{") && !final.Trim().EndsWith("}")))
                            {
                                final = "{ \"summary\": \"I couldn't get a full response right now.\", \"tips\": [\"Try again later or check your API key in Visual Settings.\"], \"action\": \"Try again\", \"confidence\": 0.0 }";
                                parsed = ParseStructuredResponse(final);
                            }
                        }

                    }

                    if (parsed != null)
                    {
                        // Format for display
                        var sb = new System.Text.StringBuilder();
                        if (!string.IsNullOrWhiteSpace(parsed.Summary)) sb.AppendLine(parsed.Summary);
                        if (parsed.Tips != null && parsed.Tips.Count > 0)
                        {
                            foreach (var t in parsed.Tips) sb.AppendLine("• " + t);
                        }
                        if (!string.IsNullOrWhiteSpace(parsed.Action)) sb.AppendLine("Action: " + parsed.Action);

                        assistantMsg.Text = sb.ToString().Trim();
                        assistantMsg.Confidence = parsed.Confidence;
                        assistantMsg.Source = parsed.Source ?? "remote";
                        if (parsed.Sources != null && parsed.Sources.Count > 0)
                        {
                            assistantMsg.Sources.Clear();
                            foreach (var s in parsed.Sources) assistantMsg.Sources.Add(s);
                        }
                    }
                    else
                    {
                        assistantMsg.Text = final ?? assistantMsg.Text;
                        assistantMsg.Source = string.IsNullOrWhiteSpace(final) ? "local" : "remote";
                    }

                    // If the assistant text looks like a truncated token (e.g., just "{" or very short), replace with a clear friendly fallback so the user isn't shown raw JSON fragments
                    var shortText = (assistantMsg.Text ?? string.Empty).Trim();
                    if (shortText == "{" || shortText == "}" || shortText.Length <= 3)
                    {
                        assistantMsg.Text = "Sorry, I couldn't generate a full answer right now. Try again or enable AI debug logging and check the raw response.";
                        assistantMsg.Source = "fallback";
                        assistantMsg.Confidence = 0.0;
                    }
                }

            }
            finally
            {
                IsBusy = false;
            }
        }

        private StudyResponse? ParseStructuredResponse(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            var opts = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var s = text.Trim();
            try
            {
                // First try to parse the whole response directly (best case)
                var direct = System.Text.Json.JsonSerializer.Deserialize<StudyResponse>(s, opts);
                if (direct != null) return direct;
            }
            catch { }

            try
            {
                // If the model wrapped the JSON in quotes ("{...}"), strip and unescape then try again
                if (s.Length > 2 && s.StartsWith("\"") && s.EndsWith("\""))
                {
                    var inner = s.Substring(1, s.Length - 2);
                    try
                    {
                        inner = System.Text.RegularExpressions.Regex.Unescape(inner);
                        var parsedInner = System.Text.Json.JsonSerializer.Deserialize<StudyResponse>(inner, opts);
                        if (parsedInner != null) return parsedInner;
                    }
                    catch { }
                }
            }
            catch { }

            try
            {
                // Try to extract first {...} block (handles surrounding text)
                var start = s.IndexOf('{');
                var end = s.LastIndexOf('}');
                if (start >= 0 && end > start)
                {
                    var json = s.Substring(start, end - start + 1);
                    try
                    {
                        var parsed = System.Text.Json.JsonSerializer.Deserialize<StudyResponse>(json, opts);
                        if (parsed != null) return parsed;
                    }
                    catch { }
                }
            }
            catch { }

            // If we reach here, parsing failed. Log for debugging if enabled.
            try
            {
                var settings = new Services.SettingsService();
                if (settings.Settings.EnableAIDebugLogging)
                {
                    var telemetry = new Services.TelemetryService();
                    telemetry.RecordRawResponse(text ?? string.Empty);
                }
            }
            catch { }

            return null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}