using System;
using System.Collections.Generic;
using System.Text;

namespace JBig2Decoder.NETCore
{
	public class Big2StreamReader
	{
		private byte[] data;

		private int bitPointer = 7;

		private int bytePointer = 0;

		public Big2StreamReader(byte[] data)
		{
			this.data = data;
		}

		public short Readbyte()
		{
			short bite = (short)(data[bytePointer++] & 255);

			return bite;
		}

		public void Readbyte(short[] buf)
		{
			for (int i = 0; i < buf.Length; i++)
			{
				buf[i] = (short)(data[bytePointer++] & 255);
			}
		}

		public int ReadBit()
		{
			short buf = Readbyte();
			short mask = (short)(1 << bitPointer);

			int bit = (buf & mask) >> bitPointer;

			bitPointer--;
			if (bitPointer == -1)
			{
				bitPointer = 7;
			}
			else
			{
				MovePointer(-1);
			}

			return bit;
		}

		public int ReadBits(long num)
		{
			int result = 0;

			for (int i = 0; i < num; i++)
			{
				result = (result << 1) | ReadBit();
			}

			return result;
		}

		public void MovePointer(int ammount)
		{
			bytePointer += ammount;
		}

		public void ConsumeRemainingBits()
		{
			if (bitPointer != 7)
				ReadBits(bitPointer + 1);
		}

		public bool IsFinished()
		{
			return bytePointer == data.Length;
		}
	}
}
