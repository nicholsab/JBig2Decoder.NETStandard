using System;
using System.Collections.Generic;
using System.Text;

namespace JBig2Decoder.NETCore
{
  public class SegmentHeader
  {
    private int segmentNumber;
    private int segmentType;
    private bool pageAssociationSizeSet;
    private bool deferredNonRetainSet;
    private int referredToSegmentCount;
    private short[] rententionFlags;
    private int[] referredToSegments;
    private int pageAssociation;
    private int dataLength;

    public void SetSegmentNumber(int SegmentNumber)
    {
      this.segmentNumber = SegmentNumber;
    }
    public void SetSegmentHeaderFlags(short SegmentHeaderFlags)
    {
      segmentType = SegmentHeaderFlags & 63; // 63 = 00111111
      pageAssociationSizeSet = (SegmentHeaderFlags & 64) == 64; // 64 = // 01000000
      deferredNonRetainSet = (SegmentHeaderFlags & 80) == 80; // 64 = 10000000		
    }
    public void SetReferredToSegmentCount(int referredToSegmentCount)
    {
      this.referredToSegmentCount = referredToSegmentCount;
    }

    public void SetRententionFlags(short[] rententionFlags)
    {
      this.rententionFlags = rententionFlags;
    }

    public void SetReferredToSegments(int[] referredToSegments)
    {
      this.referredToSegments = referredToSegments;
    }

    public int[] GetReferredToSegments()
    {
      return referredToSegments;
    }

    public int GetSegmentType()
    {
      return segmentType;
    }

    public int GetSegmentNumber()
    {
      return segmentNumber;
    }

    public bool IsPageAssociationSizeSet()
    {
      return pageAssociationSizeSet;
    }

    public bool IsDeferredNonRetainSet()
    {
      return deferredNonRetainSet;
    }

    public int GetReferredToSegmentCount()
    {
      return referredToSegmentCount;
    }

    public short[] GetRententionFlags()
    {
      return rententionFlags;
    }

    public int GetPageAssociation()
    {
      return pageAssociation;
    }

    public void SetPageAssociation(int pageAssociation)
    {
      this.pageAssociation = pageAssociation;
    }

    public void SetDataLength(int dataLength)
    {
      this.dataLength = dataLength;
    }

    public void SetSegmentType(int type)
    {
      this.segmentType = type;
    }

    public int GetSegmentDataLength()
    {
      return dataLength;
    }
  }
}
