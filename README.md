# JBig2Decoder.NETStandard

Converts a JBig2 (*.jb2) image to a RGB byte array. There are no package dependencies and it is compiled for:
    
1) .NET 6.0
2) .NET Standard 2.1
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

// Then the bytes can be converted to an image using another library if needed.
// To convert to a png using ImageSharp:
var image = Image.LoadPixelData<Rgb24>(output, width, height);
image.SaveAsPng("pic.png");
```

# Original History

* JBig2Decoder.NETCore by Winking
https://github.com/winkingsoftware/JBig2Decoder.NETCore

* Which is from JBig2Decoder.NET by devteamexpress 
https://github.com/devteamexpress/JBig2Decoder.NET/tree/master/JBig2Decoder

* Which is a .NET Port of the java JPedal JBig2Decoder Implementation.

# NuGet
https://www.nuget.org/packages/JBig2Decoder.NETStandard/ 

@nicholsab
  