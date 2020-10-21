using System;
using System.Collections.Generic;
using System.Text;

namespace JBig2Decoder.NETCore
{
  public class HalftoneRegionSegment : RegionSegment
  {
    private HalftoneRegionFlags halftoneRegionFlags = new HalftoneRegionFlags();
    private bool inlineImage;

    public HalftoneRegionSegment(JBIG2StreamDecoder streamDecoder, bool inlineImage)
        : base(streamDecoder)
    {
      this.inlineImage = inlineImage;
    }

    public override void ReadSegment()
    {
      base.ReadSegment();

      /** read text region Segment flags */
      ReadHalftoneRegionFlags();

      short[] buf = new short[4];
      decoder.Readbyte(buf);
      int gridWidth = BinaryOperation.GetInt32(buf);

      buf = new short[4];
      decoder.Readbyte(buf);
      int gridHeight = BinaryOperation.GetInt32(buf);

      buf = new short[4];
      decoder.Readbyte(buf);
      int gridX = BinaryOperation.GetInt32(buf);

      buf = new short[4];
      decoder.Readbyte(buf);
      int gridY = BinaryOperation.GetInt32(buf);

      if (JBIG2StreamDecoder.debug)
        Console.WriteLine("grid pos and size = " + gridX + ',' + gridY + ' ' + gridWidth + ',' + gridHeight);

      buf = new short[2];
      decoder.Readbyte(buf);
      int stepX = BinaryOperation.GetInt16(buf);

      buf = new short[2];
      decoder.Readbyte(buf);
      int stepY = BinaryOperation.GetInt16(buf);

      if (JBIG2StreamDecoder.debug)
        Console.WriteLine("step size = " + stepX + ',' + stepY);

      int[] referedToSegments = segmentHeader.GetReferredToSegments();
      if (referedToSegments.Length != 1)
      {
        Console.WriteLine("Error in halftone Segment. refSegs should == 1");
      }

      Segment segment = decoder.FindSegment(referedToSegments[0]);
      if (segment.GetSegmentHeader().GetSegmentType() != Segment.PATTERN_DICTIONARY)
      {
        if (JBIG2StreamDecoder.debug)
          Console.WriteLine("Error in halftone Segment. bad symbol dictionary reference");
      }

      PatternDictionarySegment patternDictionarySegment = (PatternDictionarySegment)segment;

      int bitsPerValue = 0, i = 1;
      while (i < patternDictionarySegment.GetSize())
      {
        bitsPerValue++;
        i <<= 1;
      }

      JBIG2Bitmap bitmap = patternDictionarySegment.GetBitmaps()[0];
      long patternWidth = bitmap.GetWidth();
      long patternHeight = bitmap.GetHeight();

      if (JBIG2StreamDecoder.debug)
        Console.WriteLine("pattern size = " + patternWidth + ',' + patternHeight);

      bool useMMR = halftoneRegionFlags.GetFlagValue(HalftoneRegionFlags.H_MMR) != 0;
      int template = halftoneRegionFlags.GetFlagValue(HalftoneRegionFlags.H_TEMPLATE);

      if (!useMMR)
      {
        arithmeticDecoder.ResetGenericStats(template, null);
        arithmeticDecoder.Start();
      }

      int halftoneDefaultPixel = halftoneRegionFlags.GetFlagValue(HalftoneRegionFlags.H_DEF_PIXEL);
      bitmap = new JBIG2Bitmap(regionBitmapWidth, regionBitmapHeight, arithmeticDecoder, huffmanDecoder, mmrDecoder);
      bitmap.Clear(halftoneDefaultPixel);

      bool enableSkip = halftoneRegionFlags.GetFlagValue(HalftoneRegionFlags.H_ENABLE_SKIP) != 0;

      JBIG2Bitmap skipBitmap = null;
      if (enableSkip)
      {
        skipBitmap = new JBIG2Bitmap(gridWidth, gridHeight, arithmeticDecoder, huffmanDecoder, mmrDecoder);
        skipBitmap.Clear(0);
        for (int y = 0; y < gridHeight; y++)
        {
          for (int x = 0; x < gridWidth; x++)
          {
            int xx = gridX + y * stepY + x * stepX;
            int yy = gridY + y * stepX - x * stepY;

            if (((xx + patternWidth) >> 8) <= 0 || (xx >> 8) >= regionBitmapWidth || ((yy + patternHeight) >> 8) <= 0 || (yy >> 8) >= regionBitmapHeight)
            {
              skipBitmap.SetPixel(y, x, 1);
            }
          }
        }
      }

      int[] grayScaleImage = new int[gridWidth * gridHeight];

      short[] genericBAdaptiveTemplateX = new short[4], genericBAdaptiveTemplateY = new short[4];

      genericBAdaptiveTemplateX[0] = (short)(template <= 1 ? 3 : 2);
      genericBAdaptiveTemplateY[0] = -1;
      genericBAdaptiveTemplateX[1] = -3;
      genericBAdaptiveTemplateY[1] = -1;
      genericBAdaptiveTemplateX[2] = 2;
      genericBAdaptiveTemplateY[2] = -2;
      genericBAdaptiveTemplateX[3] = -2;
      genericBAdaptiveTemplateY[3] = -2;

      JBIG2Bitmap grayBitmap;

      for (int j = bitsPerValue - 1; j >= 0; --j)
      {
        grayBitmap = new JBIG2Bitmap(gridWidth, gridHeight, arithmeticDecoder, huffmanDecoder, mmrDecoder);

        grayBitmap.ReadBitmap(useMMR, template, false, enableSkip, skipBitmap, genericBAdaptiveTemplateX, genericBAdaptiveTemplateY, -1);

        i = 0;
        for (int row = 0; row < gridHeight; row++)
        {
          for (int col = 0; col < gridWidth; col++)
          {
            int bit = grayBitmap.GetPixel(col, row) ^ grayScaleImage[i] & 1;
            grayScaleImage[i] = (grayScaleImage[i] << 1) | bit;
            i++;
          }
        }
      }

      int combinationOperator = halftoneRegionFlags.GetFlagValue(HalftoneRegionFlags.H_COMB_OP);

      i = 0;
      for (int col = 0; col < gridHeight; col++)
      {
        int xx = gridX + col * stepY;
        int yy = gridY + col * stepX;
        for (int row = 0; row < gridWidth; row++)
        {
          if (!(enableSkip && skipBitmap.GetPixel(col, row) == 1))
          {
            JBIG2Bitmap patternBitmap = patternDictionarySegment.GetBitmaps()[grayScaleImage[i]];
            bitmap.Combine(patternBitmap, xx >> 8, yy >> 8, combinationOperator);
          }

          xx += stepX;
          yy -= stepY;

          i++;
        }
      }

      if (inlineImage)
      {
        PageInformationSegment pageSegment = decoder.FindPageSegement(segmentHeader.GetPageAssociation());
        JBIG2Bitmap pageBitmap = pageSegment.GetPageBitmap();

        int externalCombinationOperator = regionFlags.GetFlagValue(RegionFlags.EXTERNAL_COMBINATION_OPERATOR);
        pageBitmap.Combine(bitmap, regionBitmapXLocation, regionBitmapYLocation, externalCombinationOperator);
      }
      else
      {
        bitmap.SetBitmapNumber(GetSegmentHeader().GetSegmentNumber());
        decoder.AppendBitmap(bitmap);
      }

    }

    private void ReadHalftoneRegionFlags()
    {
      /** extract text region Segment flags */
      short halftoneRegionFlagsField = decoder.Readbyte();

      halftoneRegionFlags.SetFlags(halftoneRegionFlagsField);

      if (JBIG2StreamDecoder.debug)
        Console.WriteLine("generic region Segment flags = " + halftoneRegionFlagsField);
    }

    public HalftoneRegionFlags GetHalftoneRegionFlags()
    {
      return halftoneRegionFlags;
    }
  }
}
