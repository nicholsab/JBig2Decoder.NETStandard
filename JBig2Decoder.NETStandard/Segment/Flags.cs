using System;
using System.Collections.Generic;
using System.Text;

namespace JBig2Decoder.NETCore
{
  public abstract class Flags
  {
    protected int flagsAsInt;
    protected Dictionary<string, int> flags = new Dictionary<string, int>();

    public int GetFlagValue(string key)
    {
      int value = flags[key];
      return value;
    }
    public abstract void SetFlags(int flagsAsInt);
  }
}
