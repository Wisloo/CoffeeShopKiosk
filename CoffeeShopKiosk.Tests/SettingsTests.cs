using Microsoft.VisualStudio.TestTools.UnitTesting;
using CoffeeShopKiosk.Models;

namespace CoffeeShopKiosk.Tests
{
    [TestClass]
    public class SettingsTests
    {
        [TestMethod]
        public void AppSettings_Defaults_ShouldContainVisualThemeDefaults()
        {
            var s = new AppSettings();
            Assert.AreEqual("Cafe", s.VisualTheme);
        }
    }
}