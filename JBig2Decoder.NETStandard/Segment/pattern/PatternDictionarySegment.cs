using System;
using System.Collections.Generic;
using System.Text;

namespace JBig2Decoder.NETCore
{
	public class PatternDictionarySegment : Segment
	{

		PatternDictionaryFlags patternDictionaryFlags = new PatternDictionaryFlags();
		private int width;
		private int height;
		private int grayMax;
		private JBIG2Bitmap[] bitmaps;
		private int size;

		public PatternDictionarySegment(JBIG2StreamDecoder streamDecoder) : base(streamDecoder) { }

		public override void ReadSegment()
		{
			/** read text region Segment flags */
			ReadPatternDictionaryFlags();

			width = decoder.Readbyte();
			height = decoder.Readbyte();

			if (JBIG2StreamDecoder.debug)
				Console.WriteLine("pattern dictionary size = " + width + " , " + height);

			short[] buf = new short[4];
			decoder.Readbyte(buf);
			grayMax = BinaryOperation.GetInt32(buf);

			if (JBIG2StreamDecoder.debug)
				Console.WriteLine("grey max = " + grayMax);

			bool useMMR = patternDictionaryFlags.GetFlagValue(PatternDictionaryFlags.HD_MMR) == 1;
			int template = patternDictionaryFlags.GetFlagValue(PatternDictionaryFlags.HD_TEMPLATE);

			if (!useMMR)
			{
				arithmeticDecoder.ResetGenericStats(template, null);
				arithmeticDecoder.Start();
			}

			short[] genericBAdaptiveTemplateX = new short[4], genericBAdaptiveTemplateY = new short[4];

			genericBAdaptiveTemplateX[0] = (short)-width;
			genericBAdaptiveTemplateY[0] = 0;
			genericBAdaptiveTemplateX[1] = -3;
			genericBAdaptiveTemplateY[1] = -1;
			genericBAdaptiveTemplateX[2] = 2;
			genericBAdaptiveTemplateY[2] = -2;
			genericBAdaptiveTemplateX[3] = -2;
			genericBAdaptiveTemplateY[3] = -2;

			size = grayMax + 1;

			JBIG2Bitmap bitmap = new JBIG2Bitmap(size * width, height, arithmeticDecoder, huffmanDecoder, mmrDecoder);
			bitmap.Clear(0);
			bitmap.ReadBitmap(useMMR, template, false, false, null, genericBAdaptiveTemplateX, genericBAdaptiveTemplateY, segmentHeader.GetSegmentDataLength() - 7);

			JBIG2Bitmap[] bitmaps = new JBIG2Bitmap[size];

			int x = 0;
			for (int i = 0; i < size; i++)
			{
				bitmaps[i] = bitmap.GetSlice(x, 0, width, height);
				x += width;
			}

			this.bitmaps = bitmaps;
		}


		public JBIG2Bitmap[] GetBitmaps()
		{
			return bitmaps;
		}

		private void ReadPatternDictionaryFlags()
		{
			short patternDictionaryFlagsField = decoder.Readbyte();

			patternDictionaryFlags.SetFlags(patternDictionaryFlagsField);

			if (JBIG2StreamDecoder.debug)
				Console.WriteLine("pattern Dictionary flags = " + patternDictionaryFlagsField);
		}

		public PatternDictionaryFlags GetPatternDictionaryFlags()
		{
			return patternDictionaryFlags;
		}

		public int GetSize()
		{
			return size;
		}
	}
}
