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
            var sc=tagHelper.Tag("\"����ʡ������߮����\"");

        }
        [Fact]
        public void StringContents()
        {
            Init();
            var tagHelper = new Zoonic.StringContent.BosonNLPStringTag();
            var sc = tagHelper.Tag(new string[] { "����ʡ������߮����", "����ʡ������߮����" });

        }
    }
}
