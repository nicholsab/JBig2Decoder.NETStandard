using System;
using System.Collections.Generic;
using System.Text;

namespace JBig2Decoder.NETCore
{
	public class SymbolDictionarySegment : Segment
	{

		private int noOfExportedSymbols;
		private int noOfNewSymbols;

		short[] symbolDictionaryAdaptiveTemplateX = new short[4], symbolDictionaryAdaptiveTemplateY = new short[4];
		short[] symbolDictionaryRAdaptiveTemplateX = new short[2], symbolDictionaryRAdaptiveTemplateY = new short[2];

		private JBIG2Bitmap[] bitmaps;

		private SymbolDictionaryFlags symbolDictionaryFlags = new SymbolDictionaryFlags();

		private ArithmeticDecoderStats genericRegionStats;
		private ArithmeticDecoderStats refinementRegionStats;

		public SymbolDictionarySegment(JBIG2StreamDecoder streamDecoder) : base(streamDecoder) { }
		public override void ReadSegment()
		{

			if (JBIG2StreamDecoder.debug)
				Console.WriteLine("==== Read Segment Symbol Dictionary ====");

			/** read symbol dictionary flags */
			ReadSymbolDictionaryFlags();

			//List codeTables = new ArrayList();
			int numberOfInputSymbols = 0;
			int noOfReferredToSegments = segmentHeader.GetReferredToSegmentCount();
			int[] referredToSegments = segmentHeader.GetReferredToSegments();
			long i; // i = 0;
			for (i = 0; i < noOfReferredToSegments; i++)
			{
				Segment seg = decoder.FindSegment(referredToSegments[i]);
				int type = seg.GetSegmentHeader().GetSegmentType();

				if (type == Segment.SYMBOL_DICTIONARY)
				{
					numberOfInputSymbols += ((SymbolDictionarySegment)seg).noOfExportedSymbols;
				}
				else if (type == Segment.TABLES)
				{
					//codeTables.add(seg);
				}
			}

			int symbolCodeLength = 0;
			i = 1;
			while (i < numberOfInputSymbols + noOfNewSymbols)
			{
				symbolCodeLength++;
				i <<= 1;
			}

			JBIG2Bitmap[] bitmaps = new JBIG2Bitmap[numberOfInputSymbols + noOfNewSymbols];

			long j, k = 0;
			SymbolDictionarySegment inputSymbolDictionary = null;
			for (i = 0; i < noOfReferredToSegments; i++)
			{
				Segment seg = decoder.FindSegment(referredToSegments[i]);
				if (seg.GetSegmentHeader().GetSegmentType() == Segment.SYMBOL_DICTIONARY)
				{
					inputSymbolDictionary = (SymbolDictionarySegment)seg;
					for (j = 0; j < inputSymbolDictionary.noOfExportedSymbols; j++)
					{
						bitmaps[k++] = inputSymbolDictionary.bitmaps[j];
					}
				}
			}

			long[,] huffmanDHTable = null;
			long[,] huffmanDWTable = null;

			long[,] huffmanBMSizeTable = null;
			long[,] huffmanAggInstTable = null;

			bool sdHuffman = symbolDictionaryFlags.GetFlagValue(SymbolDictionaryFlags.SD_HUFF) != 0;
			int sdHuffmanDifferenceHeight = symbolDictionaryFlags.GetFlagValue(SymbolDictionaryFlags.SD_HUFF_DH);
			int sdHuffmanDiferrenceWidth = symbolDictionaryFlags.GetFlagValue(SymbolDictionaryFlags.SD_HUFF_DW);
			int sdHuffBitmapSize = symbolDictionaryFlags.GetFlagValue(SymbolDictionaryFlags.SD_HUFF_BM_SIZE);
			int sdHuffAggregationInstances = symbolDictionaryFlags.GetFlagValue(SymbolDictionaryFlags.SD_HUFF_AGG_INST);

			i = 0;
			if (sdHuffman)
			{
				if (sdHuffmanDifferenceHeight == 0)
				{
					huffmanDHTable = HuffmanDecoder.huffmanTableD;
				}
				else if (sdHuffmanDifferenceHeight == 1)
				{
					huffmanDHTable = HuffmanDecoder.huffmanTableE;
				}
				else
				{
					//huffmanDHTable = ((JBIG2CodeTable) codeTables.get(i++)).getHuffTable();
				}

				if (sdHuffmanDiferrenceWidth == 0)
				{
					huffmanDWTable = HuffmanDecoder.huffmanTableB;
				}
				else if (sdHuffmanDiferrenceWidth == 1)
				{
					huffmanDWTable = HuffmanDecoder.huffmanTableC;
				}
				else
				{
					//huffmanDWTable = ((JBIG2CodeTable) codeTables.get(i++)).getHuffTable();
				}

				if (sdHuffBitmapSize == 0)
				{
					huffmanBMSizeTable = HuffmanDecoder.huffmanTableA;
				}
				else
				{
					//huffmanBMSizeTable = ((JBIG2CodeTable) codeTables.get(i++)).getHuffTable();
				}

				if (sdHuffAggregationInstances == 0)
				{
					huffmanAggInstTable = HuffmanDecoder.huffmanTableA;
				}
				else
				{
					//huffmanAggInstTable = ((JBIG2CodeTable) codeTables.get(i++)).getHuffTable();
				}
			}

			int contextUsed = symbolDictionaryFlags.GetFlagValue(SymbolDictionaryFlags.BITMAP_CC_USED);
			int sdTemplate = symbolDictionaryFlags.GetFlagValue(SymbolDictionaryFlags.SD_TEMPLATE);

			if (!sdHuffman)
			{
				if (contextUsed != 0 && inputSymbolDictionary != null)
				{
					arithmeticDecoder.ResetGenericStats(sdTemplate, inputSymbolDictionary.genericRegionStats);
				}
				else
				{
					arithmeticDecoder.ResetGenericStats(sdTemplate, null);
				}
				arithmeticDecoder.ResetIntStats(symbolCodeLength);
				arithmeticDecoder.Start();
			}

			int sdRefinementAggregate = symbolDictionaryFlags.GetFlagValue(SymbolDictionaryFlags.SD_REF_AGG);
			int sdRefinementTemplate = symbolDictionaryFlags.GetFlagValue(SymbolDictionaryFlags.SD_R_TEMPLATE);
			if (sdRefinementAggregate != 0)
			{
				if (contextUsed != 0 && inputSymbolDictionary != null)
				{
					arithmeticDecoder.ResetRefinementStats(sdRefinementTemplate, inputSymbolDictionary.refinementRegionStats);
				}
				else
				{
					arithmeticDecoder.ResetRefinementStats(sdRefinementTemplate, null);
				}
			}

			long[] deltaWidths = new long[noOfNewSymbols];

			long deltaHeight = 0;
			i = 0;

			while (i < noOfNewSymbols)
			{

				long instanceDeltaHeight; // instanceDeltaHeight = 0;

				if (sdHuffman)
				{
					instanceDeltaHeight = huffmanDecoder.DecodeInt(huffmanDHTable).IntResult();
				}
				else
				{
					instanceDeltaHeight = arithmeticDecoder.DecodeInt(arithmeticDecoder.iadhStats).IntResult();
				}

				if (instanceDeltaHeight < 0 && -instanceDeltaHeight >= deltaHeight)
				{
					if (JBIG2StreamDecoder.debug)
						Console.WriteLine("Bad delta-height value in JBIG2 symbol dictionary");
				}

				deltaHeight += instanceDeltaHeight;
				long symbolWidth = 0;
				long totalWidth = 0;
				j = i;

				while (true)
				{

					long deltaWidth = 0;

					DecodeIntResult decodeIntResult;
					if (sdHuffman)
					{
						decodeIntResult = huffmanDecoder.DecodeInt(huffmanDWTable);
					}
					else
					{
						decodeIntResult = arithmeticDecoder.DecodeInt(arithmeticDecoder.iadwStats);
					}

					if (!decodeIntResult.BooleanResult())
						break;

					deltaWidth = decodeIntResult.IntResult();

					if (deltaWidth < 0 && -deltaWidth >= symbolWidth)
					{
						if (JBIG2StreamDecoder.debug)
							Console.WriteLine("Bad delta-width value in JBIG2 symbol dictionary");
					}

					symbolWidth += deltaWidth;

					if (sdHuffman && sdRefinementAggregate == 0)
					{
						deltaWidths[i] = symbolWidth;
						totalWidth += symbolWidth;

					}
					else if (sdRefinementAggregate == 1)
					{

						long refAggNum; //refAggNum = 0;

						if (sdHuffman)
						{
							refAggNum = huffmanDecoder.DecodeInt(huffmanAggInstTable).IntResult();
						}
						else
						{
							refAggNum = arithmeticDecoder.DecodeInt(arithmeticDecoder.iaaiStats).IntResult();
						}

						if (refAggNum == 1)
						{

							//long symbolID = 0, referenceDX = 0, referenceDY = 0;
							long symbolID, referenceDX, referenceDY;

							if (sdHuffman)
							{
								symbolID = decoder.ReadBits(symbolCodeLength);
								referenceDX = huffmanDecoder.DecodeInt(HuffmanDecoder.huffmanTableO).IntResult();
								referenceDY = huffmanDecoder.DecodeInt(HuffmanDecoder.huffmanTableO).IntResult();

								decoder.ConsumeRemainingBits();
								arithmeticDecoder.Start();
							}
							else
							{
								symbolID = (int)arithmeticDecoder.DecodeIAID(symbolCodeLength, arithmeticDecoder.iaidStats);
								referenceDX = arithmeticDecoder.DecodeInt(arithmeticDecoder.iardxStats).IntResult();
								referenceDY = arithmeticDecoder.DecodeInt(arithmeticDecoder.iardyStats).IntResult();
							}

							JBIG2Bitmap referredToBitmap = bitmaps[symbolID];

							JBIG2Bitmap bitmap = new JBIG2Bitmap(symbolWidth, deltaHeight, arithmeticDecoder, huffmanDecoder, mmrDecoder);
							bitmap.ReadGenericRefinementRegion(sdRefinementTemplate, false, referredToBitmap, referenceDX, referenceDY, symbolDictionaryRAdaptiveTemplateX,
									symbolDictionaryRAdaptiveTemplateY);

							bitmaps[numberOfInputSymbols + i] = bitmap;

						}
						else
						{
							JBIG2Bitmap bitmap = new JBIG2Bitmap(symbolWidth, deltaHeight, arithmeticDecoder, huffmanDecoder, mmrDecoder);
							bitmap.ReadTextRegion(sdHuffman, true, refAggNum, 0, numberOfInputSymbols + i, null, symbolCodeLength, bitmaps, 0, 0, false, 1, 0,
									HuffmanDecoder.huffmanTableF, HuffmanDecoder.huffmanTableH, HuffmanDecoder.huffmanTableK, HuffmanDecoder.huffmanTableO, HuffmanDecoder.huffmanTableO,
									HuffmanDecoder.huffmanTableO, HuffmanDecoder.huffmanTableO, HuffmanDecoder.huffmanTableA, sdRefinementTemplate, symbolDictionaryRAdaptiveTemplateX,
									symbolDictionaryRAdaptiveTemplateY, decoder);

							bitmaps[numberOfInputSymbols + i] = bitmap;
						}
					}
					else
					{
						JBIG2Bitmap bitmap = new JBIG2Bitmap(symbolWidth, deltaHeight, arithmeticDecoder, huffmanDecoder, mmrDecoder);
						bitmap.ReadBitmap(false, sdTemplate, false, false, null, symbolDictionaryAdaptiveTemplateX, symbolDictionaryAdaptiveTemplateY, 0);
						bitmaps[numberOfInputSymbols + i] = bitmap;
					}

					i++;
				}

				if (sdHuffman && sdRefinementAggregate == 0)
				{
					long bmSize = huffmanDecoder.DecodeInt(huffmanBMSizeTable).IntResult();
					decoder.ConsumeRemainingBits();

					JBIG2Bitmap collectiveBitmap = new JBIG2Bitmap(totalWidth, deltaHeight, arithmeticDecoder, huffmanDecoder, mmrDecoder);

					if (bmSize == 0)
					{

						long padding = totalWidth % 8;
						long bytesPerRow = (int)Math.Ceiling(totalWidth / 8d);

						//short[] bitmap = new short[totalWidth];
						//decoder.readbyte(bitmap);
						long size = deltaHeight * ((totalWidth + 7) >> 3);
						short[] bitmap = new short[size];
						decoder.Readbyte(bitmap);

						short[][] logicalMap = new short[deltaHeight][];
						int count = 0;
						for (int row = 0; row < deltaHeight; row++)
						{
							for (int col = 0; col < bytesPerRow; col++)
							{
								logicalMap[row][col] = bitmap[count];
								count++;
							}
						}

						int collectiveBitmapRow = 0, collectiveBitmapCol = 0;

						for (int row = 0; row < deltaHeight; row++)
						{
							for (int col = 0; col < bytesPerRow; col++)
							{
								if (col == (bytesPerRow - 1))
								{ // this is the last
									// byte in the row
									short currentbyte = logicalMap[row][col];
									for (int bitPointer = 7; bitPointer >= padding; bitPointer--)
									{
										short mask = (short)(1 << bitPointer);
										int bit = (currentbyte & mask) >> bitPointer;

										collectiveBitmap.SetPixel(collectiveBitmapCol, collectiveBitmapRow, bit);
										collectiveBitmapCol++;
									}
									collectiveBitmapRow++;
									collectiveBitmapCol = 0;
								}
								else
								{
									short currentbyte = logicalMap[row][col];
									for (int bitPointer = 7; bitPointer >= 0; bitPointer--)
									{
										short mask = (short)(1 << bitPointer);
										int bit = (currentbyte & mask) >> bitPointer;

										collectiveBitmap.SetPixel(collectiveBitmapCol, collectiveBitmapRow, bit);
										collectiveBitmapCol++;
									}
								}
							}
						}

					}
					else
					{
						collectiveBitmap.ReadBitmap(true, 0, false, false, null, null, null, bmSize);
					}

					long x = 0;
					while (j < i)
					{
						bitmaps[numberOfInputSymbols + j] = collectiveBitmap.GetSlice(x, 0, deltaWidths[j], deltaHeight);
						x += deltaWidths[j];

						j++;
					}
				}
			}

			this.bitmaps = new JBIG2Bitmap[noOfExportedSymbols];

			j = i = 0;
			bool export = false;
			while (i < numberOfInputSymbols + noOfNewSymbols)
			{

				//long run = 0;
				long run;

				if (sdHuffman)
				{
					run = huffmanDecoder.DecodeInt(HuffmanDecoder.huffmanTableA).IntResult();
				}
				else
				{
					run = arithmeticDecoder.DecodeInt(arithmeticDecoder.iaexStats).IntResult();
				}

				if (export)
				{
					for (int cnt = 0; cnt < run; cnt++)
					{
						this.bitmaps[j++] = bitmaps[i++];
					}
				}
				else
				{
					i += run;
				}

				export = !export;
			}

			int contextRetained = symbolDictionaryFlags.GetFlagValue(SymbolDictionaryFlags.BITMAP_CC_RETAINED);
			if (!sdHuffman && contextRetained == 1)
			{
				genericRegionStats = genericRegionStats.Copy();
				if (sdRefinementAggregate == 1)
				{
					refinementRegionStats = refinementRegionStats.Copy();
				}
			}

			/** consume any remaining bits */
			decoder.ConsumeRemainingBits();
		}

		private void ReadSymbolDictionaryFlags()
		{
			/** extract symbol dictionary flags */
			short[] symbolDictionaryFlagsField = new short[2];
			decoder.Readbyte(symbolDictionaryFlagsField);

			int flags = BinaryOperation.GetInt16(symbolDictionaryFlagsField);
			symbolDictionaryFlags.SetFlags(flags);

			if (JBIG2StreamDecoder.debug)
				Console.WriteLine("symbolDictionaryFlags = " + flags);

			// symbol dictionary AT flags
			int sdHuff = symbolDictionaryFlags.GetFlagValue(SymbolDictionaryFlags.SD_HUFF);
			int sdTemplate = symbolDictionaryFlags.GetFlagValue(SymbolDictionaryFlags.SD_TEMPLATE);
			if (sdHuff == 0)
			{
				if (sdTemplate == 0)
				{
					symbolDictionaryAdaptiveTemplateX[0] = ReadATValue();
					symbolDictionaryAdaptiveTemplateY[0] = ReadATValue();
					symbolDictionaryAdaptiveTemplateX[1] = ReadATValue();
					symbolDictionaryAdaptiveTemplateY[1] = ReadATValue();
					symbolDictionaryAdaptiveTemplateX[2] = ReadATValue();
					symbolDictionaryAdaptiveTemplateY[2] = ReadATValue();
					symbolDictionaryAdaptiveTemplateX[3] = ReadATValue();
					symbolDictionaryAdaptiveTemplateY[3] = ReadATValue();
				}
				else
				{
					symbolDictionaryAdaptiveTemplateX[0] = ReadATValue();
					symbolDictionaryAdaptiveTemplateY[0] = ReadATValue();
				}
			}

			// symbol dictionary refinement AT flags
			int refAgg = symbolDictionaryFlags.GetFlagValue(SymbolDictionaryFlags.SD_REF_AGG);
			int sdrTemplate = symbolDictionaryFlags.GetFlagValue(SymbolDictionaryFlags.SD_R_TEMPLATE);
			if (refAgg != 0 && sdrTemplate == 0)
			{
				symbolDictionaryRAdaptiveTemplateX[0] = ReadATValue();
				symbolDictionaryRAdaptiveTemplateY[0] = ReadATValue();
				symbolDictionaryRAdaptiveTemplateX[1] = ReadATValue();
				symbolDictionaryRAdaptiveTemplateY[1] = ReadATValue();
			}

			/** extract no of exported symbols */
			short[] noOfExportedSymbolsField = new short[4];
			decoder.Readbyte(noOfExportedSymbolsField);

			int noOfExportedSymbols = BinaryOperation.GetInt32(noOfExportedSymbolsField);
			this.noOfExportedSymbols = noOfExportedSymbols;

			if (JBIG2StreamDecoder.debug)
				Console.WriteLine("noOfExportedSymbols = " + noOfExportedSymbols);

			/** extract no of new symbols */
			short[] noOfNewSymbolsField = new short[4];
			decoder.Readbyte(noOfNewSymbolsField);

			int noOfNewSymbols = BinaryOperation.GetInt32(noOfNewSymbolsField);
			this.noOfNewSymbols = noOfNewSymbols;

			if (JBIG2StreamDecoder.debug)
				Console.WriteLine("noOfNewSymbols = " + noOfNewSymbols);
		}

		public int GetNoOfExportedSymbols()
		{
			return noOfExportedSymbols;
		}

		public void SetNoOfExportedSymbols(int noOfExportedSymbols)
		{
			this.noOfExportedSymbols = noOfExportedSymbols;
		}

		public int GetNoOfNewSymbols()
		{
			return noOfNewSymbols;
		}

		public void SetNoOfNewSymbols(int noOfNewSymbols)
		{
			this.noOfNewSymbols = noOfNewSymbols;
		}

		public JBIG2Bitmap[] GetBitmaps()
		{
			return bitmaps;
		}

		public SymbolDictionaryFlags GetSymbolDictionaryFlags()
		{
			return symbolDictionaryFlags;
		}

		public void SetSymbolDictionaryFlags(SymbolDictionaryFlags symbolDictionaryFlags)
		{
			this.symbolDictionaryFlags = symbolDictionaryFlags;
		}

		private ArithmeticDecoderStats GetGenericRegionStats()
		{
			return genericRegionStats;
		}

		private void SetGenericRegionStats(ArithmeticDecoderStats genericRegionStats)
		{
			this.genericRegionStats = genericRegionStats;
		}

		private void SetRefinementRegionStats(ArithmeticDecoderStats refinementRegionStats)
		{
			this.refinementRegionStats = refinementRegionStats;
		}

		private ArithmeticDecoderStats GetRefinementRegionStats()
		{
			return refinementRegionStats;
		}
	}
}
