# JBig2Decoder.NETStandard

Converts a JBIG2 (*.jb2) image to a RGB byte array. JBIG2 images offer lossless compression for bi-level (black and white) images.  The spec was released in 2000, at the peak of fax machine technology.

There are no package dependencies and it is compiled for:
    
1) .NET 8.0
2) .NET 6.0
3) .NET Standard 2.0
4) .NET Framework 4.8


# Usage

```
string file = "pic.jb2";
var input = File.ReadAllBytes(file);
var jbig = new JBIG2StreamDecoder();
int width = 0;
int height = 0;
byte[] output = jbig.DecodeJBIG2(input, out width, out height);
// the resulting 'byte[] output' is a RGB array

// Then the bytes can be converted to an image using your favorite image processing library.
// To convert to a png using ImageSharp:
var image = Image.LoadPixelData<Rgb24>(output, width, height);
image.SaveAsPng("pic.png");
```

# Versions 

* 1.5.1 - Negative bug fix

* 1.5.0 - .NET 8

* 1.4.0 - All package dependencies removed and only a byte array is created

* 1.3.0 - All libraries converted to .NET Standard 2.0


# Original History

* JBig2Decoder.NETCore by Winking
https://github.com/winkingsoftware/JBig2Decoder.NETCore

* Which is from JBig2Decoder.NET by devteamexpress 
https://github.com/devteamexpress/JBig2Decoder.NET/tree/master/JBig2Decoder

* Which is a .NET Port of the java JPedal JBig2Decoder Implementation.

# NuGet

https://www.nuget.org/packages/JBig2Decoder.NETStandard/ 

# github

@nicholsab
  
