using System;
using System.Collections.Generic;
using System.Text;

namespace JBig2Decoder.NETCore
{
  public abstract class RegionSegment : Segment
  {
    protected int regionBitmapWidth, regionBitmapHeight;
    protected int regionBitmapXLocation, regionBitmapYLocation;

    protected RegionFlags regionFlags = new RegionFlags();

    public RegionSegment(JBIG2StreamDecoder streamDecoder) : base(streamDecoder) { }

    public override void ReadSegment()
    {
      short[] buff = new short[4];
      decoder.Readbyte(buff);
      regionBitmapWidth = BinaryOperation.GetInt32(buff);

      buff = new short[4];
      decoder.Readbyte(buff);
      regionBitmapHeight = BinaryOperation.GetInt32(buff);

      if (JBIG2StreamDecoder.debug)
        Console.WriteLine("Bitmap size = " + regionBitmapWidth + 'x' + regionBitmapHeight);

      buff = new short[4];
      decoder.Readbyte(buff);
      regionBitmapXLocation = BinaryOperation.GetInt32(buff);

      buff = new short[4];
      decoder.Readbyte(buff);
      regionBitmapYLocation = BinaryOperation.GetInt32(buff);

      if (JBIG2StreamDecoder.debug)
        Console.WriteLine("Bitmap location = " + regionBitmapXLocation + ',' + regionBitmapYLocation);

      /** extract region Segment flags */
      short regionFlagsField = decoder.Readbyte();

      regionFlags.SetFlags(regionFlagsField);

      if (JBIG2StreamDecoder.debug)
        Console.WriteLine("region Segment flags = " + regionFlagsField);
    }
  }
}
