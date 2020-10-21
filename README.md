# JBig2Decoder.NETStandard
A .NET Standard version by nicholsab of JBig2Decoder.  from:
    
* JBig2Decoder.NETCore by Winking
https://github.com/winkingsoftware/JBig2Decoder.NETCore

* Which is from JBig2Decoder.NET by devteamexpress 
https://github.com/devteamexpress/JBig2Decoder.NET/tree/master/JBig2Decoder

* Which is a .NET Port of the JPedal JBig2Decoder Implementation.

# Usage

```
var file = new FileInfo(@"picture.jb2");
var ms = new MemoryStream();
file.OpenRead().CopyTo(ms);

var jbig2 = new JBIG2StreamDecoder();
var rawBytes = jbig2.DecodeJBIG2(ms.ToArray());

var image = Image.FromStream(new MemoryStream(rawBytes));
Console.WriteLine(image.Width + " " + image.Height);
```

# NuGet
https://www.nuget.org/packages/JBig2Decoder.NETStandard/ 
