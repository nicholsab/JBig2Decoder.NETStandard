//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.Formats;
//using SixLabors.ImageSharp.PixelFormats;

namespace JBig2Decoder.NETStandard.Test
{
    [TestClass]
    public class JBig2Tests
    {
        [TestMethod()]
        [DeploymentItem("TestData\\")]
        [DeploymentItem("a8.jb2")]
        public void VerifySizeTest()
        {
            string file = @"a8.jb2";
            var input = File.ReadAllBytes(file);
            var jbig = new JBIG2StreamDecoder();
            int width = 0;
            int height = 0;
            jbig.DecodeJBIG2(input, out width, out height);

            Assert.AreEqual(1132, width);
            Assert.AreEqual(1484, height);
        }

        [TestMethod()]
        [DeploymentItem("TestData\\")]
        [DeploymentItem(@"c0001.jb2")]
        public void VerifyBigSizeTest()
        {
            string file = @"c0001.jb2";
            var input = File.ReadAllBytes(file);
            var jbig = new JBIG2StreamDecoder();
            int width = 0;
            int height = 0;
            jbig.DecodeJBIG2(input, out width, out height);

            Assert.AreEqual(6532, width);
            Assert.AreEqual(4672, height);
        }

        [TestMethod()]
        [DeploymentItem("TestData\\")]
        [DeploymentItem(@"c0001.jb2")]
        public void VerifyImageTest()
        {
            string file = @"c0001.jb2";
            var input = File.ReadAllBytes(file);
            var jbig = new JBIG2StreamDecoder();
            int width = 0;
            int height = 0;
            var output = jbig.DecodeJBIG2(input, out width, out height);

            Assert.AreEqual(91552512, output.Length);
        }

        [TestMethod()]
        [DeploymentItem("TestData\\")]
        [DeploymentItem(@"crows1.jb2")]
        public void VerifyImage2Test()
        {
            string file = @"crows1.jb2";
            var input = File.ReadAllBytes(file);
            var jbig = new JBIG2StreamDecoder();
            int width = 0;
            int height = 0;
            var output = jbig.DecodeJBIG2(input, out width, out height);

            //var image = Image.LoadPixelData<Rgb24>(output, width, height);
            //image.SaveAsBmp("c:\\temp\\x1.bmp");

            Assert.AreEqual(1588125, output.Length);
        }
    }
}