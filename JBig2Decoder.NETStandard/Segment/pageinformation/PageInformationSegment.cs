using System;
using System.Collections.Generic;
using System.Text;

namespace JBig2Decoder.NETCore
{
	public class PageInformationSegment : Segment
	{

		private int pageBitmapHeight, pageBitmapWidth;
		private int yResolution, xResolution;

		PageInformationFlags pageInformationFlags = new PageInformationFlags();
		private int pageStriping;

		private JBIG2Bitmap pageBitmap;

		public PageInformationSegment(JBIG2StreamDecoder streamDecoder) : base(streamDecoder) { }

		public PageInformationFlags GetPageInformationFlags()
		{
			return pageInformationFlags;
		}

		public JBIG2Bitmap GetPageBitmap()
		{
			return pageBitmap;
		}

		public override void ReadSegment()
		{

			if (JBIG2StreamDecoder.debug)
				Console.WriteLine("==== Reading Page Information Dictionary ====");

			short[] buff = new short[4];
			decoder.Readbyte(buff);
			pageBitmapWidth = BinaryOperation.GetInt32(buff);

			buff = new short[4];
			decoder.Readbyte(buff);
			pageBitmapHeight = BinaryOperation.GetInt32(buff);

			if (JBIG2StreamDecoder.debug)
				Console.WriteLine("Bitmap size = " + pageBitmapWidth + 'x' + pageBitmapHeight);

			buff = new short[4];
			decoder.Readbyte(buff);
			xResolution = BinaryOperation.GetInt32(buff);

			buff = new short[4];
			decoder.Readbyte(buff);
			yResolution = BinaryOperation.GetInt32(buff);

			if (JBIG2StreamDecoder.debug)
				Console.WriteLine("Resolution = " + xResolution + 'x' + yResolution);

			/** extract page information flags */
			short pageInformationFlagsField = decoder.Readbyte();

			pageInformationFlags.SetFlags(pageInformationFlagsField);

			if (JBIG2StreamDecoder.debug)
				Console.WriteLine("symbolDictionaryFlags = " + pageInformationFlagsField);

			buff = new short[2];
			decoder.Readbyte(buff);
			pageStriping = BinaryOperation.GetInt16(buff);

			if (JBIG2StreamDecoder.debug)
				Console.WriteLine("Page Striping = " + pageStriping);

			int defPix = pageInformationFlags.GetFlagValue(PageInformationFlags.DEFAULT_PIXEL_VALUE);

			int height;

			if (pageBitmapHeight == -1)
			{
				height = pageStriping & 0x7fff;
			}
			else
			{
				height = pageBitmapHeight;
			}

			pageBitmap = new JBIG2Bitmap(pageBitmapWidth, height, arithmeticDecoder, huffmanDecoder, mmrDecoder);
			pageBitmap.Clear(defPix);
		}

		public int GetPageBitmapHeight()
		{
			return pageBitmapHeight;
		}
	}
}
