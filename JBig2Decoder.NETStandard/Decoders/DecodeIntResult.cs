using System;
using System.Collections.Generic;
using System.Text;

namespace JBig2Decoder.NETCore
{
  public class DecodeIntResult
  {

    private long _intResult;
    private bool _booleanResult;

    public DecodeIntResult(long intResult, bool booleanResult)
    {
      this._intResult = intResult;
      this._booleanResult = booleanResult;
    }

    public long IntResult()
    {
      return _intResult;
    }

    public bool BooleanResult()
    {
      return _booleanResult;
    }
  }
}
