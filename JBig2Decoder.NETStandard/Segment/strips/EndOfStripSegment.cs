using System;
using System.Collections.Generic;
using System.Text;

namespace JBig2Decoder.NETCore
{
	public class EndOfStripeSegment : Segment
	{

		public EndOfStripeSegment(JBIG2StreamDecoder streamDecoder) : base(streamDecoder) { }

		public override void ReadSegment()
		{
			for (int i = 0; i < this.GetSegmentHeader().GetSegmentDataLength(); i++)
			{
				decoder.Readbyte();
			}
		}
	}
}
