using System;
using System.Collections.Generic;
using System.Text;

namespace JBig2Decoder.NETCore
{
	public class PatternDictionaryFlags : Flags
	{

		public const string HD_MMR = "HD_MMR";
		public const string HD_TEMPLATE = "HD_TEMPLATE";

		public override void SetFlags(int flagsAsInt)
		{
			this.flagsAsInt = flagsAsInt;

			/** extract HD_MMR */
			flags[HD_MMR] = flagsAsInt & 1;

			/** extract HD_TEMPLATE */
			flags[HD_TEMPLATE] = (flagsAsInt >> 1) & 3;

			if (JBIG2StreamDecoder.debug)
				Console.WriteLine(flags);
		}
	}
}
