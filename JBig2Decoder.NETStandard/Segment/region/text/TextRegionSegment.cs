using System;
using System.Collections.Generic;
using System.Text;

namespace JBig2Decoder.NETCore
{
	public class TextRegionSegment : RegionSegment
	{
		private TextRegionFlags textRegionFlags = new TextRegionFlags();

		private TextRegionHuffmanFlags textRegionHuffmanFlags = new TextRegionHuffmanFlags();

		private bool inlineImage;

		private short[] symbolRegionAdaptiveTemplateX = new short[2], symbolRegionAdaptiveTemplateY = new short[2];

		public TextRegionSegment(JBIG2StreamDecoder streamDecoder, bool inlineImage) : base(streamDecoder)
		{
			this.inlineImage = inlineImage;
		}

		public override void ReadSegment()
		{
			if (JBIG2StreamDecoder.debug)
				Console.WriteLine("==== Reading Text Region ====");

			base.ReadSegment();

			/** read text region Segment flags */
			ReadTextRegionFlags();

			short[] buff = new short[4];
			decoder.Readbyte(buff);
			long noOfSymbolInstances = BinaryOperation.GetInt32(buff);

			if (JBIG2StreamDecoder.debug)
				Console.WriteLine("noOfSymbolInstances = " + noOfSymbolInstances);

			int noOfReferredToSegments = segmentHeader.GetReferredToSegmentCount();
			int[] referredToSegments = segmentHeader.GetReferredToSegments();

			//List codeTables = new ArrayList();
			List<Segment> segmentsReferenced = new List<Segment>();
			long noOfSymbols = 0;

			if (JBIG2StreamDecoder.debug)
				Console.WriteLine("noOfReferredToSegments = " + noOfReferredToSegments);
			int i; // i = 0;
			for (i = 0; i < noOfReferredToSegments; i++)
			{
				Segment seg = decoder.FindSegment(referredToSegments[i]);
				int type = seg.GetSegmentHeader().GetSegmentType();

				if (type == Segment.SYMBOL_DICTIONARY)
				{
					segmentsReferenced.Add(seg);
					noOfSymbols += ((SymbolDictionarySegment)seg).GetNoOfExportedSymbols();
				}
				else if (type == Segment.TABLES)
				{
					//codeTables.add(seg);
				}
			}

			long symbolCodeLength = 0;
			int count = 1;

			while (count < noOfSymbols)
			{
				symbolCodeLength++;
				count <<= 1;
			}

			int currentSymbol = 0;
			JBIG2Bitmap[] symbols = new JBIG2Bitmap[noOfSymbols];
			foreach (Segment it in segmentsReferenced)
			{
				if (it.GetSegmentHeader().GetSegmentType() == Segment.SYMBOL_DICTIONARY)
				{
					JBIG2Bitmap[] bitmaps = ((SymbolDictionarySegment)it).GetBitmaps();
					for (int j = 0; j < bitmaps.Length; j++)
					{
						symbols[currentSymbol] = bitmaps[j];
						currentSymbol++;
					}
				}
			}

			long[,] huffmanFSTable = null;
			long[,] huffmanDSTable = null;
			long[,] huffmanDTTable = null;

			long[,] huffmanRDWTable = null;
			long[,] huffmanRDHTable = null;

			long[,] huffmanRDXTable = null;
			long[,] huffmanRDYTable = null;
			long[,] huffmanRSizeTable = null;

			bool sbHuffman = textRegionFlags.GetFlagValue(TextRegionFlags.SB_HUFF) != 0;

			i = 0;
			if (sbHuffman)
			{
				int sbHuffFS = textRegionHuffmanFlags.GetFlagValue(TextRegionHuffmanFlags.SB_HUFF_FS);
				if (sbHuffFS == 0)
				{
					huffmanFSTable = HuffmanDecoder.huffmanTableF;
				}
				else if (sbHuffFS == 1)
				{
					huffmanFSTable = HuffmanDecoder.huffmanTableG;
				}
				else
				{

				}

				int sbHuffDS = textRegionHuffmanFlags.GetFlagValue(TextRegionHuffmanFlags.SB_HUFF_DS);
				if (sbHuffDS == 0)
				{
					huffmanDSTable = HuffmanDecoder.huffmanTableH;
				}
				else if (sbHuffDS == 1)
				{
					huffmanDSTable = HuffmanDecoder.huffmanTableI;
				}
				else if (sbHuffDS == 2)
				{
					huffmanDSTable = HuffmanDecoder.huffmanTableJ;
				}
				else
				{

				}

				int sbHuffDT = textRegionHuffmanFlags.GetFlagValue(TextRegionHuffmanFlags.SB_HUFF_DT);
				if (sbHuffDT == 0)
				{
					huffmanDTTable = HuffmanDecoder.huffmanTableK;
				}
				else if (sbHuffDT == 1)
				{
					huffmanDTTable = HuffmanDecoder.huffmanTableL;
				}
				else if (sbHuffDT == 2)
				{
					huffmanDTTable = HuffmanDecoder.huffmanTableM;
				}
				else
				{

				}

				int sbHuffRDW = textRegionHuffmanFlags.GetFlagValue(TextRegionHuffmanFlags.SB_HUFF_RDW);
				if (sbHuffRDW == 0)
				{
					huffmanRDWTable = HuffmanDecoder.huffmanTableN;
				}
				else if (sbHuffRDW == 1)
				{
					huffmanRDWTable = HuffmanDecoder.huffmanTableO;
				}
				else
				{

				}

				int sbHuffRDH = textRegionHuffmanFlags.GetFlagValue(TextRegionHuffmanFlags.SB_HUFF_RDH);
				if (sbHuffRDH == 0)
				{
					huffmanRDHTable = HuffmanDecoder.huffmanTableN;
				}
				else if (sbHuffRDH == 1)
				{
					huffmanRDHTable = HuffmanDecoder.huffmanTableO;
				}
				else
				{

				}

				int sbHuffRDX = textRegionHuffmanFlags.GetFlagValue(TextRegionHuffmanFlags.SB_HUFF_RDX);
				if (sbHuffRDX == 0)
				{
					huffmanRDXTable = HuffmanDecoder.huffmanTableN;
				}
				else if (sbHuffRDX == 1)
				{
					huffmanRDXTable = HuffmanDecoder.huffmanTableO;
				}
				else
				{

				}

				int sbHuffRDY = textRegionHuffmanFlags.GetFlagValue(TextRegionHuffmanFlags.SB_HUFF_RDY);
				if (sbHuffRDY == 0)
				{
					huffmanRDYTable = HuffmanDecoder.huffmanTableN;
				}
				else if (sbHuffRDY == 1)
				{
					huffmanRDYTable = HuffmanDecoder.huffmanTableO;
				}
				else
				{

				}

				int sbHuffRSize = textRegionHuffmanFlags.GetFlagValue(TextRegionHuffmanFlags.SB_HUFF_RSIZE);
				if (sbHuffRSize == 0)
				{
					huffmanRSizeTable = HuffmanDecoder.huffmanTableA;
				}
				else
				{

				}
			}

			long[][] runLengthTable = new long[36][];
			long[][] symbolCodeTable = new long[noOfSymbols + 1][];
			if (sbHuffman)
			{

				decoder.ConsumeRemainingBits();

				for (i = 0; i < 32; i++)
				{
					runLengthTable[i] = new long[] { i, decoder.ReadBits(4), 0, 0 };
				}

				runLengthTable[32] = new long[] { 0x103, decoder.ReadBits(4), 2, 0 };

				runLengthTable[33] = new long[] { 0x203, decoder.ReadBits(4), 3, 0 };

				runLengthTable[34] = new long[] { 0x20b, decoder.ReadBits(4), 7, 0 };

				runLengthTable[35] = new long[] { 0, 0, HuffmanDecoder.jbig2HuffmanEOT };

				runLengthTable = HuffmanDecoder.BuildTable(runLengthTable, 35);

				for (i = 0; i < noOfSymbols; i++)
				{
					symbolCodeTable[i] = new long[] { i, 0, 0, 0 };
				}

				i = 0;
				while (i < noOfSymbols)
				{
					long j = huffmanDecoder.DecodeInt(runLengthTable).IntResult();
					if (j > 0x200)
					{
						for (j -= 0x200; j != 0 && i < noOfSymbols; j--)
						{
							symbolCodeTable[i++][1] = 0;
						}
					}
					else if (j > 0x100)
					{
						for (j -= 0x100; j != 0 && i < noOfSymbols; j--)
						{
							symbolCodeTable[i][1] = symbolCodeTable[i - 1][1];
							i++;
						}
					}
					else
					{
						symbolCodeTable[i++][1] = j;
					}
				}

				symbolCodeTable[noOfSymbols][1] = 0;
				symbolCodeTable[noOfSymbols][2] = HuffmanDecoder.jbig2HuffmanEOT;
				symbolCodeTable = HuffmanDecoder.BuildTable(symbolCodeTable, (int)noOfSymbols);

				decoder.ConsumeRemainingBits();
			}
			else
			{
				symbolCodeTable = null;
				arithmeticDecoder.ResetIntStats((int)symbolCodeLength);
				arithmeticDecoder.Start();
			}

			bool symbolRefine = textRegionFlags.GetFlagValue(TextRegionFlags.SB_REFINE) != 0;
			long logStrips = textRegionFlags.GetFlagValue(TextRegionFlags.LOG_SB_STRIPES);
			int defaultPixel = textRegionFlags.GetFlagValue(TextRegionFlags.SB_DEF_PIXEL);
			int combinationOperator = textRegionFlags.GetFlagValue(TextRegionFlags.SB_COMB_OP);
			bool transposed = textRegionFlags.GetFlagValue(TextRegionFlags.TRANSPOSED) != 0;
			int referenceCorner = textRegionFlags.GetFlagValue(TextRegionFlags.REF_CORNER);
			int sOffset = textRegionFlags.GetFlagValue(TextRegionFlags.SB_DS_OFFSET);
			int template = textRegionFlags.GetFlagValue(TextRegionFlags.SB_R_TEMPLATE);

			if (symbolRefine)
			{
				arithmeticDecoder.ResetRefinementStats(template, null);
			}

			JBIG2Bitmap bitmap = new JBIG2Bitmap(regionBitmapWidth, regionBitmapHeight, arithmeticDecoder, huffmanDecoder, mmrDecoder);

			bitmap.ReadTextRegion2(sbHuffman, symbolRefine, noOfSymbolInstances, logStrips, noOfSymbols, symbolCodeTable, symbolCodeLength, symbols, defaultPixel, combinationOperator, transposed, referenceCorner, sOffset, huffmanFSTable, huffmanDSTable, huffmanDTTable, huffmanRDWTable, huffmanRDHTable, huffmanRDXTable, huffmanRDYTable, huffmanRSizeTable, template, symbolRegionAdaptiveTemplateX, symbolRegionAdaptiveTemplateY, decoder);

			if (inlineImage)
			{
				PageInformationSegment pageSegment = decoder.FindPageSegement(segmentHeader.GetPageAssociation());
				JBIG2Bitmap pageBitmap = pageSegment.GetPageBitmap();

				if (JBIG2StreamDecoder.debug)
					Console.WriteLine(pageBitmap + " " + bitmap);

				int externalCombinationOperator = regionFlags.GetFlagValue(RegionFlags.EXTERNAL_COMBINATION_OPERATOR);
				pageBitmap.Combine(bitmap, regionBitmapXLocation, regionBitmapYLocation, externalCombinationOperator);
			}
			else
			{
				bitmap.SetBitmapNumber(GetSegmentHeader().GetSegmentNumber());
				decoder.AppendBitmap(bitmap);
			}

			decoder.ConsumeRemainingBits();
		}

		private void ReadTextRegionFlags()
		{
			/** extract text region Segment flags */
			short[] textRegionFlagsField = new short[2];
			decoder.Readbyte(textRegionFlagsField);

			int flags = BinaryOperation.GetInt16(textRegionFlagsField);
			textRegionFlags.SetFlags(flags);

			if (JBIG2StreamDecoder.debug)
				Console.WriteLine("text region Segment flags = " + flags);

			bool sbHuff = textRegionFlags.GetFlagValue(TextRegionFlags.SB_HUFF) != 0;
			if (sbHuff)
			{
				/** extract text region Segment Huffman flags */
				short[] textRegionHuffmanFlagsField = new short[2];
				decoder.Readbyte(textRegionHuffmanFlagsField);

				flags = BinaryOperation.GetInt16(textRegionHuffmanFlagsField);
				textRegionHuffmanFlags.SetFlags(flags);

				if (JBIG2StreamDecoder.debug)
					Console.WriteLine("text region segment Huffman flags = " + flags);
			}

			bool sbRefine = textRegionFlags.GetFlagValue(TextRegionFlags.SB_REFINE) != 0;
			int sbrTemplate = textRegionFlags.GetFlagValue(TextRegionFlags.SB_R_TEMPLATE);
			if (sbRefine && sbrTemplate == 0)
			{
				symbolRegionAdaptiveTemplateX[0] = ReadATValue();
				symbolRegionAdaptiveTemplateY[0] = ReadATValue();
				symbolRegionAdaptiveTemplateX[1] = ReadATValue();
				symbolRegionAdaptiveTemplateY[1] = ReadATValue();
			}
		}

		public TextRegionFlags GetTextRegionFlags()
		{
			return textRegionFlags;
		}

		public TextRegionHuffmanFlags GetTextRegionHuffmanFlags()
		{
			return textRegionHuffmanFlags;
		}
	}
}
