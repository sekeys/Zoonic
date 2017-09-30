using System;
using Xunit;

namespace Zoonic.Test
{
    public class UnitTest1
    {
        public void Init()
        {

            Zoonic.Configuration.ConfigurationManager.Manager
                .AddFile(System.IO.Path.Combine(@"E:\DevCodes\Zoonic\Zhimi", "setting.json"))
                .Configure();
        }
        [Fact]
        public void StringContent()
        {
           Init();
            var tagHelper =new Zoonic.StringContent.BosonNLPStringTag();
            var sc=tagHelper.Tag("\"江西省上饶市弋阳县\"");

        }
        [Fact]
        public void StringContents()
        {
            Init();
            var tagHelper = new Zoonic.StringContent.BosonNLPStringTag();
            var sc = tagHelper.Tag(new string[] { "江西省上饶市弋阳县", "江西省上饶市弋阳县" });

        }
    }
}
