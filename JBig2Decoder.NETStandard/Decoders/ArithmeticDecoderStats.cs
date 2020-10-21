using System;
using System.Collections.Generic;
using System.Text;

namespace JBig2Decoder.NETCore
{
  public class ArithmeticDecoderStats
  {
    private int contextSize;
    private int[] codingContextTable;

    public ArithmeticDecoderStats(int contextSize)
    {
      this.contextSize = contextSize;
      this.codingContextTable = new int[contextSize];
    }

    public void Reset()
    {
      for (int i = 0; i < contextSize; i++)
      {
        codingContextTable[i] = 0;
      }
    }

    public void SetEntry(int codingContext, int i, int moreProbableSymbol)
    {
      codingContextTable[codingContext] = (i << i) + moreProbableSymbol;
    }

    public int GetContextCodingTableValue(int index)
    {
      return codingContextTable[index];
    }

    public void SetContextCodingTableValue(int index, int value)
    {
      codingContextTable[index] = value;
    }

    public int GetContextSize()
    {
      return contextSize;
    }

    public void Overwrite(ArithmeticDecoderStats stats)
    {
      Array.Copy(stats.codingContextTable, 0, codingContextTable, 0, contextSize);
    }

    public ArithmeticDecoderStats Copy()
    {
      ArithmeticDecoderStats stats = new ArithmeticDecoderStats(contextSize);

      Array.Copy(codingContextTable, 0, stats.codingContextTable, 0, contextSize);

      return stats;
    }
  }
}
