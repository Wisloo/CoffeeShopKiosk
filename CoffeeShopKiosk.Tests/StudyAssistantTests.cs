using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CoffeeShopKiosk.ViewModels;
using CoffeeShopKiosk.Services;
using CoffeeShopKiosk.Models;

namespace CoffeeShopKiosk.Tests
{
    [TestClass]
    public class StudyAssistantTests
    {
        private class MalformedThenGoodAI : IAIService
        {
            private int _calls = 0;
            public Task<string> ChatAsync(string userMessage) {
                _calls++;
                if (_calls == 1) return Task.FromResult("{");
                var json = "{ \"summary\": \"Corrected summary\", \"tips\": [\"Tip one\", \"Tip two\"], \"action\": \"Do X\", \"confidence\": 0.85 }";
                return Task.FromResult(json);
            }

            public Task<string> GetStudyTipAsync(string topic) => ChatAsync(topic);
            public Task<string> ChatAsync(string userMessage, object? context = null) => ChatAsync(userMessage);
            public Task<string> ChatStreamAsync(string userMessage, Action<string> onPartial, object? context = null) => Task.FromResult("{");
            public Task<bool> TestApiKeyAsync() => Task.FromResult(true);
        }

        [TestMethod]
        public async Task StudyChat_RetriesAndParses()
        {
            var ai = new MalformedThenGoodAI();
            var vm = new StudyChatViewModel(ai);
            vm.InputText = "How do I study?";

            await vm.Send();

            Assert.IsTrue(vm.Messages.Count >= 2);
            var assistant = vm.Messages[1];
            Assert.IsTrue(assistant.Text.Contains("Corrected summary"));
            Assert.AreEqual(0.85, assistant.Confidence, 0.01);
        }
    }
}