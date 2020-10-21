using System;
using System.Collections.Generic;
using System.Text;

namespace JBig2Decoder.NETCore
{
	public class RefinementRegionFlags : Flags
	{

		public const string GR_TEMPLATE = "GR_TEMPLATE";
		public const string TPGDON = "TPGDON";

		public override void SetFlags(int flagsAsInt)
		{
			this.flagsAsInt = flagsAsInt;

			/** extract GR_TEMPLATE */
			flags[GR_TEMPLATE] = flagsAsInt & 1;

			/** extract TPGDON */
			flags[TPGDON] = (flagsAsInt >> 1) & 1;

			if (JBIG2StreamDecoder.debug)
				Console.WriteLine(flags);
		}
	}
}
