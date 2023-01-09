namespace JBig2Decoder.NETStandard
{
    public class ExtensionSegment : Segment
    {

        public ExtensionSegment(JBIG2StreamDecoder streamDecoder) : base(streamDecoder) { }

        public override void ReadSegment()
        {
            for (int i = 0; i < GetSegmentHeader().GetSegmentDataLength(); i++)
            {
                decoder.Readbyte();
            }
        }
    }
}
