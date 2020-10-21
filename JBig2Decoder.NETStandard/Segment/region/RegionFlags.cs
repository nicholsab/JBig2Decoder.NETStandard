using System;
using System.Collections.Generic;
using System.Text;

namespace JBig2Decoder.NETCore
{
  public class RegionFlags : Flags
  {
    public static string EXTERNAL_COMBINATION_OPERATOR = "EXTERNAL_COMBINATION_OPERATOR";
    public override void SetFlags(int flagsAsInt)
    {
      this.flagsAsInt = flagsAsInt;

      /** extract EXTERNAL_COMBINATION_OPERATOR */
      flags[EXTERNAL_COMBINATION_OPERATOR] = flagsAsInt & 7;

      if (JBIG2StreamDecoder.debug)
        Console.WriteLine(flags);
    }
  }
}
