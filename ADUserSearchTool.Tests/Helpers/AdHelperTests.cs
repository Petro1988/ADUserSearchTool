using ADUserSearchTool.Helpers;
using Xunit;

namespace ADUserSearchTool.Tests.Helpers
{
    public class AdHelperTests
    {
        [Fact]
        public void NormalizeNumber_RemovesNonDigits()
        {
            string result = AdHelper.NormalizeNumber("+49 (40) 123-456");

            Assert.Equal("4940123456", result);
        }

        [Fact]
        public void ExtractCn_ReturnsCnValue()
        {
            string dn = "CN=GG_IT,OU=Groups,DC=firma,DC=local";

            string result = AdHelper.ExtractCn(dn);

            Assert.Equal("GG_IT", result);
        }

        [Fact]
        public void ExtractOu_ReturnsOuPath()
        {
            string dn = "CN=Petru Gavriliuc,OU=IT,OU=Hamburg,DC=firma,DC=local";

            string result = AdHelper.ExtractOu(dn);

            Assert.Equal("IT / Hamburg", result);
        }

        [Fact]
        public void IsAccountDisabled_ReturnsTrue_WhenDisabledFlagIsSet()
        {
            bool result = AdHelper.IsAccountDisabled(514);

            Assert.True(result);
        }

        [Fact]
        public void IsAccountDisabled_ReturnsFalse_WhenDisabledFlagIsNotSet()
        {
            bool result = AdHelper.IsAccountDisabled(512);

            Assert.False(result);
        }
    }
}