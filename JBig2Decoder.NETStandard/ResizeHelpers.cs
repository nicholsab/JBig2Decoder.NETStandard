using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;

namespace JBig2Decoder.NETCore
{
  public static class ResizeHelpers
  {
    public static byte[] ScaleImage(byte[] file, int maxWidth, int maxHeight)
    {
      System.Drawing.Image image = System.Drawing.Image.FromStream(new MemoryStream(file));
      var ratioX = (double)maxWidth / image.Width;
      var ratioY = (double)maxHeight / image.Height;
      var ratio = Math.Min(ratioX, ratioY);

      var newWidth = (int)(image.Width * ratio);
      var newHeight = (int)(image.Height * ratio);

      var newImage = new Bitmap(newWidth, newHeight);
      Graphics.FromImage(newImage).DrawImage(image, 0, 0, newWidth, newHeight);
      return newImage.ToByteArray();
    }

  }
  public static class ImageExtensions
  {
    public static Bitmap ToBitmap(this byte[] byteArrayIn)
    {
      MemoryStream ms = new MemoryStream(byteArrayIn);
      Bitmap returnImage = new Bitmap(ms);
      return returnImage;
    }
    public static byte[] ToByteArray(this Bitmap img)
    {
      byte[] byteArray = Array.Empty<byte>();
      using (MemoryStream stream = new MemoryStream())
      {
        img.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
        stream.Close();
        byteArray = stream.ToArray();
      }
      return byteArray;
    }

  }
}
