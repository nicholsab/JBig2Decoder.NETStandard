using System;
using System.Collections.Generic;
using System.Text;

namespace JBig2Decoder.NETCore
{
	public class BinaryOperation
	{

		public const int LEFT_SHIFT = 0;
		public const int RIGHT_SHIFT = 1;

		public const long LONGMASK = 0xffffffffl; // 1111 1111 1111 1111 1111 1111 1111 1111
		public const int INTMASK = 0xff; // 1111 1111

		public static int GetInt32(short[] number)
		{
			return (number[0] << 24) | (number[1] << 16) | (number[2] << 8) | number[3];
		}

		public static int GetInt16(short[] number)
		{
			return (number[0] << 8) | number[1];
		}

		public static long Bit32ShiftL(long number, int shift)
		{
			//return (number << shift) & LONGMASK;
			return number << shift;
		}

		public static long Bit32ShiftR(long number, int shift)
		{
			//return (number >> shift) & LONGMASK;
			return number >> shift;
		}

		public static int Bit8Shift(int number, int shift, int direction)
		{
			if (direction == LEFT_SHIFT)
				number <<= shift;
			else
				number >>= shift;

			return (number & INTMASK);
		}

		public static int GetInt32(byte[] number)
		{
			return (number[0] << 24) | (number[1] << 16) | (number[2] << 8) | number[3];
		}
	}
}
