using System;
using System.Collections.Generic;
using System.Text;

namespace JBig2Decoder.NETCore
{
	public class HalftoneRegionFlags : Flags
	{

		public const string H_MMR = "H_MMR";
		public const string H_TEMPLATE = "H_TEMPLATE";
		public const string H_ENABLE_SKIP = "H_ENABLE_SKIP";
		public const string H_COMB_OP = "H_COMB_OP";
		public const string H_DEF_PIXEL = "H_DEF_PIXEL";

		public override void SetFlags(int flagsAsInt)
		{
			this.flagsAsInt = flagsAsInt;

			/** extract H_MMR */
			flags[H_MMR] = flagsAsInt & 1;

			/** extract H_TEMPLATE */
			flags[H_TEMPLATE] = (flagsAsInt >> 1) & 3;

			/** extract H_ENABLE_SKIP */
			flags[H_ENABLE_SKIP] = (flagsAsInt >> 3) & 1;

			/** extract H_COMB_OP */
			flags[H_COMB_OP] = (flagsAsInt >> 4) & 7;

			/** extract H_DEF_PIXEL */
			flags[H_DEF_PIXEL] = (flagsAsInt >> 7) & 1;


			if (JBIG2StreamDecoder.debug)
				Console.WriteLine(flags);
		}
	}
}
