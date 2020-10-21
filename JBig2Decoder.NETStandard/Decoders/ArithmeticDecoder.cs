using System;
using System.Collections.Generic;
using System.Text;

namespace JBig2Decoder.NETCore
{
	public class ArithmeticDecoder
	{

		private Big2StreamReader reader;

		public ArithmeticDecoderStats genericRegionStats, refinementRegionStats;

		public ArithmeticDecoderStats iadhStats, iadwStats, iaexStats, iaaiStats, iadtStats, iaitStats, iafsStats, iadsStats, iardxStats, iardyStats, iardwStats, iardhStats, iariStats, iaidStats;

		int[] contextSize = { 16, 13, 10, 10 }, referredToContextSize = { 13, 10 };

		long buffer0, buffer1;
		long c, a;
		long previous;

		int counter;

		private ArithmeticDecoder() { }

		public ArithmeticDecoder(Big2StreamReader reader)
		{
			this.reader = reader;

			genericRegionStats = new ArithmeticDecoderStats(1 << 1);
			refinementRegionStats = new ArithmeticDecoderStats(1 << 1);

			iadhStats = new ArithmeticDecoderStats(1 << 9);
			iadwStats = new ArithmeticDecoderStats(1 << 9);
			iaexStats = new ArithmeticDecoderStats(1 << 9);
			iaaiStats = new ArithmeticDecoderStats(1 << 9);
			iadtStats = new ArithmeticDecoderStats(1 << 9);
			iaitStats = new ArithmeticDecoderStats(1 << 9);
			iafsStats = new ArithmeticDecoderStats(1 << 9);
			iadsStats = new ArithmeticDecoderStats(1 << 9);
			iardxStats = new ArithmeticDecoderStats(1 << 9);
			iardyStats = new ArithmeticDecoderStats(1 << 9);
			iardwStats = new ArithmeticDecoderStats(1 << 9);
			iardhStats = new ArithmeticDecoderStats(1 << 9);
			iariStats = new ArithmeticDecoderStats(1 << 9);
			iaidStats = new ArithmeticDecoderStats(1 << 1);
		}

		public void ResetIntStats(int symbolCodeLength)
		{
			iadhStats.Reset();
			iadwStats.Reset();
			iaexStats.Reset();
			iaaiStats.Reset();
			iadtStats.Reset();
			iaitStats.Reset();
			iafsStats.Reset();
			iadsStats.Reset();
			iardxStats.Reset();
			iardyStats.Reset();
			iardwStats.Reset();
			iardhStats.Reset();
			iariStats.Reset();

			if (iaidStats.GetContextSize() == 1 << (symbolCodeLength + 1))
			{
				iaidStats.Reset();
			}
			else
			{
				iaidStats = new ArithmeticDecoderStats(1 << (symbolCodeLength + 1));
			}
		}

		public void ResetGenericStats(int template, ArithmeticDecoderStats previousStats)
		{
			int size = contextSize[template];

			if (previousStats != null && previousStats.GetContextSize() == size)
			{
				if (genericRegionStats.GetContextSize() == size)
				{
					genericRegionStats.Overwrite(previousStats);
				}
				else
				{
					genericRegionStats = previousStats.Copy();
				}
			}
			else
			{
				if (genericRegionStats.GetContextSize() == size)
				{
					genericRegionStats.Reset();
				}
				else
				{
					genericRegionStats = new ArithmeticDecoderStats(1 << size);
				}
			}
		}

		public void ResetRefinementStats(int template, ArithmeticDecoderStats previousStats)
		{
			int size = referredToContextSize[template];
			if (previousStats != null && previousStats.GetContextSize() == size)
			{
				if (refinementRegionStats.GetContextSize() == size)
				{
					refinementRegionStats.Overwrite(previousStats);
				}
				else
				{
					refinementRegionStats = previousStats.Copy();
				}
			}
			else
			{
				if (refinementRegionStats.GetContextSize() == size)
				{
					refinementRegionStats.Reset();
				}
				else
				{
					refinementRegionStats = new ArithmeticDecoderStats(1 << size);
				}
			}
		}

		public void Start()
		{
			buffer0 = reader.Readbyte();
			buffer1 = reader.Readbyte();

			c = BinaryOperation.Bit32ShiftL((buffer0 ^ 0xff), 16);
			Readbyte();
			c = BinaryOperation.Bit32ShiftL(c, 7);
			counter -= 7;
			a = 0x80000000l;
		}

		public DecodeIntResult DecodeInt(ArithmeticDecoderStats stats)
		{
			long value;

			previous = 1;
			int s = DecodeIntBit(stats);
			if (DecodeIntBit(stats) != 0)
			{
				if (DecodeIntBit(stats) != 0)
				{
					if (DecodeIntBit(stats) != 0)
					{
						if (DecodeIntBit(stats) != 0)
						{
							if (DecodeIntBit(stats) != 0)
							{
								value = 0;
								for (int i = 0; i < 32; i++)
								{
									value = BinaryOperation.Bit32ShiftL(value, 1) | DecodeIntBit(stats);
								}
								value += 4436;
							}
							else
							{
								value = 0;
								for (int i = 0; i < 12; i++)
								{
									value = BinaryOperation.Bit32ShiftL(value, 1) | DecodeIntBit(stats);
								}
								value += 340;
							}
						}
						else
						{
							value = 0;
							for (int i = 0; i < 8; i++)
							{
								value = BinaryOperation.Bit32ShiftL(value, 1) | DecodeIntBit(stats);
							}
							value += 84;
						}
					}
					else
					{
						value = 0;
						for (int i = 0; i < 6; i++)
						{
							value = BinaryOperation.Bit32ShiftL(value, 1) | DecodeIntBit(stats);
						}
						value += 20;
					}
				}
				else
				{
					value = DecodeIntBit(stats);
					value = BinaryOperation.Bit32ShiftL(value, 1) | DecodeIntBit(stats);
					value = BinaryOperation.Bit32ShiftL(value, 1) | DecodeIntBit(stats);
					value = BinaryOperation.Bit32ShiftL(value, 1) | DecodeIntBit(stats);
					value += 4;
				}
			}
			else
			{
				value = DecodeIntBit(stats);
				value = BinaryOperation.Bit32ShiftL(value, 1) | DecodeIntBit(stats);
			}

			int decodedInt;
			if (s != 0)
			{
				if (value == 0)
				{
					return new DecodeIntResult((int)value, false);
				}
				decodedInt = (int)-value;
			}
			else
			{
				decodedInt = (int)value;
			}

			return new DecodeIntResult(decodedInt, true);
		}

		public long DecodeIAID(long codeLen, ArithmeticDecoderStats stats)
		{
			previous = 1;
			for (long i = 0; i < codeLen; i++)
			{
				int bit = DecodeBit(previous, stats);
				previous = BinaryOperation.Bit32ShiftL(previous, 1) | bit;
			}

			return previous - (1 << (int)codeLen);
		}

		public int DecodeBit(long context, ArithmeticDecoderStats stats)
		{
			int iCX = BinaryOperation.Bit8Shift(stats.GetContextCodingTableValue((int)context), 1, BinaryOperation.RIGHT_SHIFT);
			int mpsCX = stats.GetContextCodingTableValue((int)context) & 1;
			int qe = qeTable[iCX];

			a -= qe;

			int bit;
			if (c < a)
			{
				if ((a & 0x80000000) != 0)
				{
					bit = mpsCX;
				}
				else
				{
					if (a < qe)
					{
						bit = 1 - mpsCX;
						if (switchTable[iCX] != 0)
						{
							stats.SetContextCodingTableValue((int)context, (nlpsTable[iCX] << 1) | (1 - mpsCX));
						}
						else
						{
							stats.SetContextCodingTableValue((int)context, (nlpsTable[iCX] << 1) | mpsCX);
						}
					}
					else
					{
						bit = mpsCX;
						stats.SetContextCodingTableValue((int)context, (nmpsTable[iCX] << 1) | mpsCX);
					}
					do
					{
						if (counter == 0)
						{
							Readbyte();
						}

						a = BinaryOperation.Bit32ShiftL(a, 1);
						c = BinaryOperation.Bit32ShiftL(c, 1);

						counter--;
					} while ((a & 0x80000000) == 0);
				}
			}
			else
			{
				c -= a;

				if (a < qe)
				{
					bit = mpsCX;
					stats.SetContextCodingTableValue((int)context, (nmpsTable[iCX] << 1) | mpsCX);
				}
				else
				{
					bit = 1 - mpsCX;
					if (switchTable[iCX] != 0)
					{
						stats.SetContextCodingTableValue((int)context, (nlpsTable[iCX] << 1) | (1 - mpsCX));
					}
					else
					{
						stats.SetContextCodingTableValue((int)context, (nlpsTable[iCX] << 1) | mpsCX);
					}
				}
				a = qe;

				do
				{
					if (counter == 0)
					{
						Readbyte();
					}

					a = BinaryOperation.Bit32ShiftL(a, 1);
					c = BinaryOperation.Bit32ShiftL(c, 1);

					counter--;
				} while ((a & 0x80000000) == 0);
			}
			return bit;
		}

		private void Readbyte()
		{
			if (buffer0 == 0xff)
			{
				if (buffer1 > 0x8f)
				{
					counter = 8;
				}
				else
				{
					buffer0 = buffer1;
					buffer1 = reader.Readbyte();
					c = c + 0xfe00 - (BinaryOperation.Bit32ShiftL(buffer0, 9));
					counter = 7;
				}
			}
			else
			{
				buffer0 = buffer1;
				buffer1 = reader.Readbyte();
				c = c + 0xff00 - (BinaryOperation.Bit32ShiftL(buffer0, 8));
				counter = 8;
			}
		}

		private int DecodeIntBit(ArithmeticDecoderStats stats)
		{

			int bit = DecodeBit(previous, stats);
			if (previous < 0x100)
			{
				previous = BinaryOperation.Bit32ShiftL(previous, 1) | bit;
			}
			else
			{
				previous = (((BinaryOperation.Bit32ShiftL(previous, 1)) | bit) & 0x1ff) | 0x100;
			}
			return bit;
		}

		readonly int[] qeTable = { 0x56010000, 0x34010000, 0x18010000, 0x0AC10000, 0x05210000, 0x02210000, 0x56010000, 0x54010000, 0x48010000, 0x38010000, 0x30010000, 0x24010000, 0x1C010000, 0x16010000, 0x56010000, 0x54010000, 0x51010000, 0x48010000, 0x38010000, 0x34010000, 0x30010000, 0x28010000, 0x24010000, 0x22010000, 0x1C010000, 0x18010000, 0x16010000, 0x14010000, 0x12010000, 0x11010000, 0x0AC10000, 0x09C10000, 0x08A10000, 0x05210000, 0x04410000, 0x02A10000, 0x02210000, 0x01410000, 0x01110000, 0x00850000, 0x00490000, 0x00250000, 0x00150000, 0x00090000, 0x00050000, 0x00010000,
			0x56010000 };
		readonly int[] nmpsTable = { 1, 2, 3, 4, 5, 38, 7, 8, 9, 10, 11, 12, 13, 29, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 45, 46 };
		readonly int[] nlpsTable = { 1, 6, 9, 12, 29, 33, 6, 14, 14, 14, 17, 18, 20, 21, 14, 14, 15, 16, 17, 18, 19, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 46 };
		readonly int[] switchTable = { 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
	}
}
