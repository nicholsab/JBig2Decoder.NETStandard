using System;
using System.Collections.Generic;
using System.Text;

namespace JBig2Decoder.NETCore
{
	public class RefinementRegionSegment : RegionSegment
	{
		private RefinementRegionFlags refinementRegionFlags = new RefinementRegionFlags();

		private bool inlineImage;

		private int noOfReferedToSegments;

		int[] referedToSegments;

		public RefinementRegionSegment(JBIG2StreamDecoder streamDecoder, bool inlineImage, int[] referedToSegments, int noOfReferedToSegments) : base(streamDecoder)
		{
			this.inlineImage = inlineImage;
			this.referedToSegments = referedToSegments;
			this.noOfReferedToSegments = noOfReferedToSegments;
		}

		public void ReadSegment()
		{
			if (JBIG2StreamDecoder.debug)
				Console.WriteLine("==== Reading Generic Refinement Region ====");

			base.ReadSegment();

			/** read text region segment flags */
			ReadGenericRegionFlags();

			short[] genericRegionAdaptiveTemplateX = new short[2];
			short[] genericRegionAdaptiveTemplateY = new short[2];

			int template = refinementRegionFlags.GetFlagValue(RefinementRegionFlags.GR_TEMPLATE);
			if (template == 0)
			{
				genericRegionAdaptiveTemplateX[0] = ReadATValue();
				genericRegionAdaptiveTemplateY[0] = ReadATValue();
				genericRegionAdaptiveTemplateX[1] = ReadATValue();
				genericRegionAdaptiveTemplateY[1] = ReadATValue();
			}

			if (noOfReferedToSegments == 0 || inlineImage)
			{
				PageInformationSegment pageSegment = decoder.FindPageSegement(segmentHeader.GetPageAssociation());
				JBIG2Bitmap pageBitmap = pageSegment.GetPageBitmap();

				if (pageSegment.GetPageBitmapHeight() == -1 && regionBitmapYLocation + regionBitmapHeight > pageBitmap.GetHeight())
				{
					pageBitmap.Expand(regionBitmapYLocation + regionBitmapHeight, pageSegment.GetPageInformationFlags().GetFlagValue(PageInformationFlags.DEFAULT_PIXEL_VALUE));
				}
			}

			if (noOfReferedToSegments > 1)
			{
				if (JBIG2StreamDecoder.debug)
					Console.WriteLine("Bad reference in JBIG2 generic refinement Segment");

				return;
			}

			JBIG2Bitmap referedToBitmap;
			if (noOfReferedToSegments == 1)
			{
				referedToBitmap = decoder.FindBitmap(referedToSegments[0]);
			}
			else
			{
				PageInformationSegment pageSegment = decoder.FindPageSegement(segmentHeader.GetPageAssociation());
				JBIG2Bitmap pageBitmap = pageSegment.GetPageBitmap();

				referedToBitmap = pageBitmap.GetSlice(regionBitmapXLocation, regionBitmapYLocation, regionBitmapWidth, regionBitmapHeight);
			}

			arithmeticDecoder.ResetRefinementStats(template, null);
			arithmeticDecoder.Start();

			bool typicalPredictionGenericRefinementOn = refinementRegionFlags.GetFlagValue(RefinementRegionFlags.TPGDON) != 0;

			JBIG2Bitmap bitmap = new JBIG2Bitmap(regionBitmapWidth, regionBitmapHeight, arithmeticDecoder, huffmanDecoder, mmrDecoder);

			bitmap.ReadGenericRefinementRegion(template, typicalPredictionGenericRefinementOn, referedToBitmap, 0, 0, genericRegionAdaptiveTemplateX, genericRegionAdaptiveTemplateY);

			if (inlineImage)
			{
				PageInformationSegment pageSegment = decoder.FindPageSegement(segmentHeader.GetPageAssociation());
				JBIG2Bitmap pageBitmap = pageSegment.GetPageBitmap();

				int extCombOp = regionFlags.GetFlagValue(RegionFlags.EXTERNAL_COMBINATION_OPERATOR);

				pageBitmap.Combine(bitmap, regionBitmapXLocation, regionBitmapYLocation, extCombOp);
			}
			else
			{
				bitmap.SetBitmapNumber(GetSegmentHeader().GetSegmentNumber());
				decoder.AppendBitmap(bitmap);
			}
		}

		private void ReadGenericRegionFlags()
		{
			/** extract text region Segment flags */
			short refinementRegionFlagsField = decoder.Readbyte();

			refinementRegionFlags.SetFlags(refinementRegionFlagsField);

			if (JBIG2StreamDecoder.debug)
				Console.WriteLine("generic region Segment flags = " + refinementRegionFlagsField);
		}

		public RefinementRegionFlags GetGenericRegionFlags()
		{
			return refinementRegionFlags;
		}
	}
}
