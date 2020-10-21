using System;
using System.Collections.Generic;
using System.Text;

namespace JBig2Decoder.NETCore
{
  public class GenericRegionSegment : RegionSegment
  {
    private GenericRegionFlags genericRegionFlags = new GenericRegionFlags();

    private bool inlineImage;
    private bool unknownLength = false;

    public GenericRegionSegment(JBIG2StreamDecoder streamDecoder, bool inlineImage) : base(streamDecoder)
    {
      this.inlineImage = inlineImage;
    }

    public override void ReadSegment()
    {

      if (JBIG2StreamDecoder.debug)
        Console.WriteLine("==== Reading Immediate Generic Region ====");

      base.ReadSegment();

      /** read text region Segment flags */
      ReadGenericRegionFlags();

      bool useMMR = genericRegionFlags.GetFlagValue(GenericRegionFlags.MMR) != 0;
      int template = genericRegionFlags.GetFlagValue(GenericRegionFlags.GB_TEMPLATE);

      short[] genericBAdaptiveTemplateX = new short[4];
      short[] genericBAdaptiveTemplateY = new short[4];

      if (!useMMR)
      {
        if (template == 0)
        {
          genericBAdaptiveTemplateX[0] = ReadATValue();
          genericBAdaptiveTemplateY[0] = ReadATValue();
          genericBAdaptiveTemplateX[1] = ReadATValue();
          genericBAdaptiveTemplateY[1] = ReadATValue();
          genericBAdaptiveTemplateX[2] = ReadATValue();
          genericBAdaptiveTemplateY[2] = ReadATValue();
          genericBAdaptiveTemplateX[3] = ReadATValue();
          genericBAdaptiveTemplateY[3] = ReadATValue();
        }
        else
        {
          genericBAdaptiveTemplateX[0] = ReadATValue();
          genericBAdaptiveTemplateY[0] = ReadATValue();
        }

        arithmeticDecoder.ResetGenericStats(template, null);
        arithmeticDecoder.Start();
      }

      bool typicalPredictionGenericDecodingOn = genericRegionFlags.GetFlagValue(GenericRegionFlags.TPGDON) != 0;
      int length = segmentHeader.GetSegmentDataLength();

      if (length == -1)
      {
        /** 
         * length of data is unknown, so it needs to be determined through examination of the data.
         * See 7.2.7 - Segment data length of the JBIG2 specification.
         */

        unknownLength = true;

        short match1;
        short match2;

        if (useMMR)
        {
          // look for 0x00 0x00 (0, 0)

          match1 = 0;
          match2 = 0;
        }
        else
        {
          // look for 0xFF 0xAC (255, 172)

          match1 = 255;
          match2 = 172;
        }

        int bytesRead = 0;
        while (true)
        {
          short bite1 = decoder.Readbyte();
          bytesRead++;

          if (bite1 == match1)
          {
            short bite2 = decoder.Readbyte();
            bytesRead++;

            if (bite2 == match2)
            {
              length = bytesRead - 2;
              break;
            }
          }
        }

        decoder.MovePointer(-bytesRead);
      }

      JBIG2Bitmap bitmap = new JBIG2Bitmap(regionBitmapWidth, regionBitmapHeight, arithmeticDecoder, huffmanDecoder, mmrDecoder);
      bitmap.Clear(0);
      bitmap.ReadBitmap(useMMR, template, typicalPredictionGenericDecodingOn, false, null, genericBAdaptiveTemplateX, genericBAdaptiveTemplateY, useMMR ? 0 : length - 18);



      if (inlineImage)
      {
        PageInformationSegment pageSegment = decoder.FindPageSegement(segmentHeader.GetPageAssociation());
        JBIG2Bitmap pageBitmap = pageSegment.GetPageBitmap();

        int extCombOp = regionFlags.GetFlagValue(RegionFlags.EXTERNAL_COMBINATION_OPERATOR);

        if (pageSegment.GetPageBitmapHeight() == -1 && regionBitmapYLocation + regionBitmapHeight > pageBitmap.GetHeight())
        {
          pageBitmap.Expand(regionBitmapYLocation + regionBitmapHeight,
              pageSegment.GetPageInformationFlags().GetFlagValue(PageInformationFlags.DEFAULT_PIXEL_VALUE));
        }

        pageBitmap.Combine(bitmap, regionBitmapXLocation, regionBitmapYLocation, extCombOp);
      }
      else
      {
        bitmap.SetBitmapNumber(GetSegmentHeader().GetSegmentNumber());
        decoder.AppendBitmap(bitmap);
      }


      if (unknownLength)
      {
        decoder.MovePointer(4);
      }

    }

    private void ReadGenericRegionFlags()
    {
      /** extract text region Segment flags */
      short genericRegionFlagsField = decoder.Readbyte();

      genericRegionFlags.SetFlags(genericRegionFlagsField);

      if (JBIG2StreamDecoder.debug)
        Console.WriteLine("generic region Segment flags = " + genericRegionFlagsField);
    }

    public GenericRegionFlags GetGenericRegionFlags()
    {
      return genericRegionFlags;
    }
  }
}
