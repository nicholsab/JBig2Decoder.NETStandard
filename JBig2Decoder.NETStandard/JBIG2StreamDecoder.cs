using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JBig2Decoder.NETStandard
{
    public enum ImageFormat { JPEG, TIFF, PNG }
    public class JBIG2StreamDecoder
    {
        public static bool debug = false;
        private Big2StreamReader reader;
        private ArithmeticDecoder arithmeticDecoder;
        private HuffmanDecoder huffmanDecoder;
        private MMRDecoder mmrDecoder;
        private bool noOfPagesKnown;
        private bool randomAccessOrganisation;
        private int noOfPages = -1;
        private List<Segment> segments = new List<Segment>();
        private List<JBIG2Bitmap> bitmaps = new List<JBIG2Bitmap>();
        private byte[] globalData;

        public void MovePointer(int i)
        {
            reader.MovePointer(i);
        }
        public void SetGlobalData(byte[] data)
        {
            globalData = data;
        }

        /// <summary>
        /// This section is new for .net standard 2.0.  This is basically
        /// implementing WriteableBitmap since it is in .net core only.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="stride"></param>
        /// <returns></returns>
        private byte[] BitsToImage(byte[] input, int width, int height, int stride, ImageFormat format)
        {
            int n = 0;
            var bmp = new Bitmap(width, height);
            for (int y = 0; y < height; y++)
            {
                int x = 0;
                for (int s = 0; s < stride; s++)
                {
                    if (x >= width)
                    {
                        break;
                    }

                    var b = input[n++];
                    for (int b0 = 7; b0 >= 0; b0--)
                    {
                        var bit = b & (1 << b0);
                        if (bit == 0)
                        {
                            bmp.SetPixel(x, y, Color.Black);
                        }
                        else
                        {
                            bmp.SetPixel(x, y, Color.White);
                        }

                        x++;
                        if (x >= width)
                        {
                            break;
                        }
                    }
                }
            }

            var memory = new MemoryStream();
            if (format == ImageFormat.JPEG)
            {
                bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            else if (format == ImageFormat.PNG)
            {
                bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
            }
            else
            {
                bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Tiff);
            }

            return memory.ToArray();
        }

        public byte[] DecodeJBIG2(byte[] data, ImageFormat format = ImageFormat.TIFF, int NewWidth = 0, int NewHeight = 0)
        {
            reader = new Big2StreamReader(data);
            ResetDecoder();
            bool validFile = CheckHeader();
            if (JBIG2StreamDecoder.debug)
                Console.WriteLine("validFile = " + validFile);
            if (!validFile)
            {
                /**
                 * Assume this is a stream from a PDF so there is no file header,
                 * end of page segments, or end of file segments. Organisation must
                 * be sequential, and the number of pages is assumed to be 1.
                 */
                noOfPagesKnown = true;
                randomAccessOrganisation = false;
                noOfPages = 1;
                /** check to see if there is any global data to be read */
                if (globalData != null)
                {
                    /** set the reader to read from the global data */
                    reader = new Big2StreamReader(globalData);

                    huffmanDecoder = new HuffmanDecoder(reader);
                    mmrDecoder = new MMRDecoder(reader);
                    arithmeticDecoder = new ArithmeticDecoder(reader);

                    /** read in the global data segments */
                    ReadSegments();

                    /** set the reader back to the main data */
                    reader = new Big2StreamReader(data);
                }
                else
                {
                    /**
                     * There's no global data, so move the file pointer back to the
                     * start of the stream
                     */
                    reader.MovePointer(-8);
                }
            }
            else
            {
                /**
                 * We have the file header, so assume it is a valid stand-alone
                 * file.
                 */

                if (JBIG2StreamDecoder.debug)
                    Console.WriteLine("==== File Header ====");

                SetFileHeaderFlags();

                if (JBIG2StreamDecoder.debug)
                {
                    Console.WriteLine("randomAccessOrganisation = " + randomAccessOrganisation);
                    Console.WriteLine("noOfPagesKnown = " + noOfPagesKnown);
                }

                if (noOfPagesKnown)
                {
                    noOfPages = GetNoOfPages();

                    if (JBIG2StreamDecoder.debug)
                        Console.WriteLine("noOfPages = " + noOfPages);
                }
            }

            huffmanDecoder = new HuffmanDecoder(reader);
            mmrDecoder = new MMRDecoder(reader);
            arithmeticDecoder = new ArithmeticDecoder(reader);

            /** read in the main segment data */
            ReadSegments();

            //Create Image
            var rawimage = FindPageSegement(1).GetPageBitmap();
            int width = (int)rawimage.GetWidth();
            int height = (int)rawimage.GetHeight();
            var dataStream = rawimage.GetData(true);
            int stride = (width * 1 + 7) / 8;

            // This section is new for .net standard 2.0
            var bmp = BitsToImage(dataStream, width, height, stride, format);
            if (NewWidth != 0 && NewHeight != 0)
            {
                var newbitmap = ResizeHelpers.ScaleImage(bmp, NewWidth, NewHeight);
                return newbitmap;
            }

            return bmp;
        }

        public HuffmanDecoder GetHuffmanDecoder()
        {
            return huffmanDecoder;
        }
        public MMRDecoder GetMMRDecoder()
        {
            return mmrDecoder;
        }
        public ArithmeticDecoder GetArithmeticDecoder()
        {
            return arithmeticDecoder;
        }
        private void ResetDecoder()
        {
            noOfPagesKnown = false;
            randomAccessOrganisation = false;

            noOfPages = -1;
            segments.Clear();
            bitmaps.Clear();
        }
        private void ReadSegments()
        {
            bool finished = false;
            while (!reader.IsFinished() && !finished)
            {

                SegmentHeader segmentHeader = new SegmentHeader();
                ReadSegmentHeader(segmentHeader);

                // read the Segment data
                Segment segment = null;

                int segmentType = segmentHeader.GetSegmentType();
                int[] referredToSegments = segmentHeader.GetReferredToSegments();
                int noOfReferredToSegments = segmentHeader.GetReferredToSegmentCount();

                switch (segmentType)
                {
                    case Segment.SYMBOL_DICTIONARY:

                        segment = new SymbolDictionarySegment(this);

                        segment.SetSegmentHeader(segmentHeader);

                        break;

                    case Segment.INTERMEDIATE_TEXT_REGION:

                        segment = new TextRegionSegment(this, false);

                        segment.SetSegmentHeader(segmentHeader);

                        break;

                    case Segment.IMMEDIATE_TEXT_REGION:

                        segment = new TextRegionSegment(this, true);

                        segment.SetSegmentHeader(segmentHeader);

                        break;

                    case Segment.IMMEDIATE_LOSSLESS_TEXT_REGION:

                        segment = new TextRegionSegment(this, true);

                        segment.SetSegmentHeader(segmentHeader);

                        break;

                    case Segment.PATTERN_DICTIONARY:

                        segment = new PatternDictionarySegment(this);

                        segment.SetSegmentHeader(segmentHeader);

                        break;

                    case Segment.INTERMEDIATE_HALFTONE_REGION:

                        segment = new HalftoneRegionSegment(this, false);

                        segment.SetSegmentHeader(segmentHeader);

                        break;

                    case Segment.IMMEDIATE_HALFTONE_REGION:

                        segment = new HalftoneRegionSegment(this, true);

                        segment.SetSegmentHeader(segmentHeader);

                        break;

                    case Segment.IMMEDIATE_LOSSLESS_HALFTONE_REGION:

                        segment = new HalftoneRegionSegment(this, true);

                        segment.SetSegmentHeader(segmentHeader);

                        break;

                    case Segment.INTERMEDIATE_GENERIC_REGION:

                        segment = new GenericRegionSegment(this, false);

                        segment.SetSegmentHeader(segmentHeader);

                        break;

                    case Segment.IMMEDIATE_GENERIC_REGION:

                        segment = new GenericRegionSegment(this, true);

                        segment.SetSegmentHeader(segmentHeader);

                        break;

                    case Segment.IMMEDIATE_LOSSLESS_GENERIC_REGION:

                        segment = new GenericRegionSegment(this, true);

                        segment.SetSegmentHeader(segmentHeader);

                        break;

                    case Segment.INTERMEDIATE_GENERIC_REFINEMENT_REGION:

                        segment = new RefinementRegionSegment(this, false, referredToSegments, noOfReferredToSegments);

                        segment.SetSegmentHeader(segmentHeader);

                        break;

                    case Segment.IMMEDIATE_GENERIC_REFINEMENT_REGION:

                        segment = new RefinementRegionSegment(this, true, referredToSegments, noOfReferredToSegments);

                        segment.SetSegmentHeader(segmentHeader);

                        break;

                    case Segment.IMMEDIATE_LOSSLESS_GENERIC_REFINEMENT_REGION:

                        segment = new RefinementRegionSegment(this, true, referredToSegments, noOfReferredToSegments);

                        segment.SetSegmentHeader(segmentHeader);

                        break;

                    case Segment.PAGE_INFORMATION:

                        segment = new PageInformationSegment(this);

                        segment.SetSegmentHeader(segmentHeader);

                        break;

                    case Segment.END_OF_PAGE:
                        continue;

                    case Segment.END_OF_STRIPE:

                        segment = new EndOfStripeSegment(this);

                        segment.SetSegmentHeader(segmentHeader);
                        break;

                    case Segment.END_OF_FILE:

                        finished = true;

                        continue;

                    case Segment.PROFILES:
                        break;

                    case Segment.TABLES:
                        break;

                    case Segment.EXTENSION:

                        segment = new ExtensionSegment(this);

                        segment.SetSegmentHeader(segmentHeader);

                        break;

                    default:
                        break;
                }

                if (!randomAccessOrganisation)
                {
                    segment.ReadSegment();
                }
                segments.Add(segment);
            }

            if (randomAccessOrganisation)
            {
                foreach (Segment segment in segments)
                {
                    segment.ReadSegment();
                }
            }
        }

        public PageInformationSegment FindPageSegement(int page)
        {
            foreach (Segment segment in segments)
            {
                SegmentHeader segmentHeader = segment.GetSegmentHeader();
                if (segmentHeader.GetSegmentType() == Segment.PAGE_INFORMATION && segmentHeader.GetPageAssociation() == page)
                {
                    return (PageInformationSegment)segment;
                }
            }

            return null;
        }
        public Segment FindSegment(int segmentNumber)
        {
            foreach (Segment segment in segments)
            {
                if (segment.GetSegmentHeader().GetSegmentNumber() == segmentNumber)
                {
                    return segment;
                }
            }
            return null;
        }
        private void ReadSegmentHeader(SegmentHeader segmentHeader)
        {
            HandleSegmentNumber(segmentHeader);

            HandleSegmentHeaderFlags(segmentHeader);

            HandleSegmentReferredToCountAndRententionFlags(segmentHeader);

            HandleReferedToSegmentNumbers(segmentHeader);

            HandlePageAssociation(segmentHeader);

            if (segmentHeader.GetSegmentType() != Segment.END_OF_FILE)
                HandleSegmentDataLength(segmentHeader);
        }
        private void HandlePageAssociation(SegmentHeader segmentHeader)
        {
            int pageAssociation;

            bool isPageAssociationSizeSet = segmentHeader.IsPageAssociationSizeSet();
            if (isPageAssociationSizeSet)
            { // field is 4 bytes long
                short[] buf = new short[4];
                reader.Readbyte(buf);
                pageAssociation = BinaryOperation.GetInt32(buf);
            }
            else
            { // field is 1 byte long
                pageAssociation = reader.Readbyte();
            }

            segmentHeader.SetPageAssociation(pageAssociation);

            if (JBIG2StreamDecoder.debug)
                Console.WriteLine("pageAssociation = " + pageAssociation);
        }
        private void HandleSegmentNumber(SegmentHeader segmentHeader)
        {
            short[] segmentbytes = new short[4];
            reader.Readbyte(segmentbytes);

            int segmentNumber = BinaryOperation.GetInt32(segmentbytes);

            if (JBIG2StreamDecoder.debug)
                Console.WriteLine("SegmentNumber = " + segmentNumber);
            segmentHeader.SetSegmentNumber(segmentNumber);
        }
        private void HandleSegmentHeaderFlags(SegmentHeader segmentHeader)
        {
            short segmentHeaderFlags = reader.Readbyte();
            // System.out.println("SegmentHeaderFlags = " + SegmentHeaderFlags);
            segmentHeader.SetSegmentHeaderFlags(segmentHeaderFlags);
        }
        private void HandleSegmentReferredToCountAndRententionFlags(SegmentHeader segmentHeader)
        {
            short referedToSegmentCountAndRetentionFlags = reader.Readbyte();

            int referredToSegmentCount = (referedToSegmentCountAndRetentionFlags & 224) >> 5; // 224
                                                                                              // =
                                                                                              // 11100000
            short[] retentionFlags = null;
            /** take off the first three bits of the first byte */
            short firstbyte = (short)(referedToSegmentCountAndRetentionFlags & 31); // 31 =
                                                                                    // 00011111

            if (referredToSegmentCount <= 4)
            { // short form

                retentionFlags = new short[1];
                retentionFlags[0] = firstbyte;

            }
            else if (referredToSegmentCount == 7)
            { // long form

                short[] longFormCountAndFlags = new short[4];
                /** add the first byte of the four */
                longFormCountAndFlags[0] = firstbyte;

                for (int i = 1; i < 4; i++)
                    // add the next 3 bytes to the array
                    longFormCountAndFlags[i] = reader.Readbyte();

                /** get the count of the referred to Segments */
                referredToSegmentCount = BinaryOperation.GetInt32(longFormCountAndFlags);

                /** calculate the number of bytes in this field */
                int noOfbytesInField = (int)Math.Ceiling(4 + ((referredToSegmentCount + 1) / 8d));
                // System.out.println("noOfbytesInField = " + noOfbytesInField);

                int noOfRententionFlagbytes = noOfbytesInField - 4;
                retentionFlags = new short[noOfRententionFlagbytes];
                reader.Readbyte(retentionFlags);

            }
            else
            { // error
              //throw new JBIG2Exception("Error, 3 bit Segment count field = " + referredToSegmentCount);
            }

            segmentHeader.SetReferredToSegmentCount(referredToSegmentCount);

            if (JBIG2StreamDecoder.debug)
                Console.WriteLine("referredToSegmentCount = " + referredToSegmentCount);

            segmentHeader.SetRententionFlags(retentionFlags);

            if (JBIG2StreamDecoder.debug)
                Console.WriteLine("retentionFlags = ");

            if (JBIG2StreamDecoder.debug)
            {
                for (int i = 0; i < retentionFlags.Length; i++)
                    Console.WriteLine(retentionFlags[i] + " ");
                Console.WriteLine("");
            }
        }
        private void HandleReferedToSegmentNumbers(SegmentHeader segmentHeader)
        {
            int referredToSegmentCount = segmentHeader.GetReferredToSegmentCount();
            int[] referredToSegments = new int[referredToSegmentCount];

            int segmentNumber = segmentHeader.GetSegmentNumber();

            if (segmentNumber <= 256)
            {
                for (int i = 0; i < referredToSegmentCount; i++)
                    referredToSegments[i] = reader.Readbyte();
            }
            else if (segmentNumber <= 65536)
            {
                short[] buf = new short[2];
                for (int i = 0; i < referredToSegmentCount; i++)
                {
                    reader.Readbyte(buf);
                    referredToSegments[i] = BinaryOperation.GetInt16(buf);
                }
            }
            else
            {
                short[] buf = new short[4];
                for (int i = 0; i < referredToSegmentCount; i++)
                {
                    reader.Readbyte(buf);
                    referredToSegments[i] = BinaryOperation.GetInt32(buf);
                }
            }

            segmentHeader.SetReferredToSegments(referredToSegments);

            if (JBIG2StreamDecoder.debug)
            {
                Console.WriteLine("referredToSegments = ");
                for (int i = 0; i < referredToSegments.Length; i++)
                    Console.WriteLine(referredToSegments[i] + " ");
                Console.WriteLine("");
            }
        }

        private int GetNoOfPages()
        {
            short[] noOfPages = new short[4];
            reader.Readbyte(noOfPages);
            return BinaryOperation.GetInt32(noOfPages);
        }
        private void HandleSegmentDataLength(SegmentHeader segmentHeader)
        {
            short[] buf = new short[4];
            reader.Readbyte(buf);

            int dateLength = BinaryOperation.GetInt32(buf);
            segmentHeader.SetDataLength(dateLength);

            if (JBIG2StreamDecoder.debug)
                Console.WriteLine("dateLength = " + dateLength);
        }
        private void SetFileHeaderFlags()
        {
            short headerFlags = reader.Readbyte();

            if ((headerFlags & 0xfc) != 0)
            {
                Console.WriteLine("Warning, reserved bits (2-7) of file header flags are not zero " + headerFlags);
            }

            int fileOrganisation = headerFlags & 1;
            randomAccessOrganisation = fileOrganisation == 0;

            int pagesKnown = headerFlags & 2;
            noOfPagesKnown = pagesKnown == 0;
        }
        private bool CheckHeader()
        {
            short[] controlHeader = new short[] { 151, 74, 66, 50, 13, 10, 26, 10 };
            short[] actualHeader = new short[8];
            reader.Readbyte(actualHeader);

            return controlHeader.SequenceEqual(actualHeader);
        }
        public int ReadBits(long num)
        {
            return reader.ReadBits(num);
        }
        public int ReadBit()
        {
            return reader.ReadBit();
        }
        public void Readbyte(short[] buff)
        {
            reader.Readbyte(buff);
        }
        public void ConsumeRemainingBits()
        {
            reader.ConsumeRemainingBits();
        }
        public short Readbyte()
        {
            return reader.Readbyte();
        }
        public void AppendBitmap(JBIG2Bitmap bitmap)
        {
            bitmaps.Add(bitmap);
        }

        public JBIG2Bitmap FindBitmap(int bitmapNumber)
        {
            foreach (JBIG2Bitmap bitmap in bitmaps)
            {
                if (bitmap.GetBitmapNumber() == bitmapNumber)
                {
                    return bitmap;
                }
            }

            return null;
        }
        public JBIG2Bitmap GetPageAsJBIG2Bitmap(int i)
        {
            JBIG2Bitmap pageBitmap = FindPageSegement(1).GetPageBitmap();
            return pageBitmap;
        }
        public bool IsNumberOfPagesKnown()
        {
            return noOfPagesKnown;
        }
        public int GetNumberOfPages()
        {
            return noOfPages;
        }
        public bool IsRandomAccessOrganisationUsed()
        {
            return randomAccessOrganisation;
        }
        public List<Segment> GetAllSegments()
        {
            return segments;
        }


    }
}
