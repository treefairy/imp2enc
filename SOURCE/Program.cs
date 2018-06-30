using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Text.RegularExpressions;
using FreeImageAPI;

// THIS SOFTWARE IS LICENSED UNDER THE LGPL 3.0. SEE COPYING and COPYING.LESSER FOR DETAILS.
// SOME COMPONENTS USED UNDER OTHER LICENSES.
// PLEASE RETAIN ALL LICENSING INFORMATION IN YOUR SOURCE CODE IF YOU MODIFY THIS SOFTWARE.

namespace IMP2ENC
{
    // Utilities class, from TargaImage.cs
    // ==========================================================
    // TargaImage
    //
    // Design and implementation by
    // - David Polomis (paloma_sw@cox.net)
    //
    //
    // This source code, along with any associated files, is licensed under
    // The Code Project Open License (CPOL) 1.02
    // A copy of this license can be found in the CPOL.html file 
    // which was downloaded with this source code
    // or at http://www.codeproject.com/info/cpol10.aspx
    //
    // 
    // COVERED CODE IS PROVIDED UNDER THIS LICENSE ON AN "AS IS" BASIS,
    // WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED,
    // INCLUDING, WITHOUT LIMITATION, WARRANTIES THAT THE COVERED CODE IS
    // FREE OF DEFECTS, MERCHANTABLE, FIT FOR A PARTICULAR PURPOSE OR
    // NON-INFRINGING. THE ENTIRE RISK AS TO THE QUALITY AND PERFORMANCE
    // OF THE COVERED CODE IS WITH YOU. SHOULD ANY COVERED CODE PROVE
    // DEFECTIVE IN ANY RESPECT, YOU (NOT THE INITIAL DEVELOPER OR ANY
    // OTHER CONTRIBUTOR) ASSUME THE COST OF ANY NECESSARY SERVICING,
    // REPAIR OR CORRECTION. THIS DISCLAIMER OF WARRANTY CONSTITUTES AN
    // ESSENTIAL PART OF THIS LICENSE. NO USE OF ANY COVERED CODE IS
    // AUTHORIZED HEREUNDER EXCEPT UNDER THIS DISCLAIMER.
    //
    // Use at your own risk!
    //
    // ==========================================================


    public struct Color555 : IEquatable<Color555>, IComparable<Color555>, IEquatable<ushort>, IComparable<ushort>
    {
        public static readonly Color555 MinValue = ushort.MinValue;
        public static readonly Color555 MaxValue = ushort.MaxValue;

        private readonly ushort _Value;

        public Color555(Color value)
        {
            uint c = (uint)value.ToArgb();
            _Value = (ushort)(((c >> 16) & 0x8000 | (c >> 9) & 0x7C00 | (c >> 6) & 0x03E0 | (c >> 3) & 0x1F));
        }

        public Color555(ushort value)
        {
            _Value = value;
        }

        public ushort getValue()
        {
            return _Value;
        }

        public override int GetHashCode()
        {
            return _Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return (obj is ushort && Equals((ushort)obj)) || (obj is Color555 && Equals((Color555)obj));
        }


        public bool Equals(ushort other)
        {
            return _Value == other;
        }

        public bool Equals(Color555 other)
        {
            return _Value == other._Value;
        }

        public int CompareTo(Color555 other)
        {
            return _Value.CompareTo(other._Value);
        }

        public int CompareTo(ushort other)
        {
            return _Value.CompareTo(other);
        }

        public override string ToString()
        {
            return String.Format("{0}", _Value);
        }

        public string ToString(string format)
        {
            return String.Format(format, _Value);
        }

        public string ToString(IFormatProvider provider)
        {
            return String.Format(provider, "{0}", _Value);
        }

        public string ToString(string format, IFormatProvider provider)
        {
            return String.Format(provider, format, _Value);
        }

        public int ToArgb()
        {
            return ToColor().ToArgb();
        }

        public Color ToColor()
        {
            int a = _Value & 0x8000;
            int r = _Value & 0x7C00;
            int g = _Value & 0x03E0;
            int b = _Value & 0x1F;
            int rgb = (r << 9) | (g << 6) | (b << 3);

            return Color.FromArgb((a * 0x1FE00) | rgb | ((rgb >> 5) & 0x070707));
        }

        public static bool operator ==(Color555 l, Color555 r)
        {
            return l.Equals(r);
        }

        public static bool operator !=(Color555 l, Color555 r)
        {
            return !l.Equals(r);
        }

        public static implicit operator Color555(Color value)
        {
            return new Color555(value);
        }

        public static implicit operator Color555(ushort value)
        {
            return new Color555(value);
        }

        public static implicit operator ushort(Color555 value)
        {
            return value._Value;
        }
    }


    static class Utilities
    {
        /// <summary>
        /// Gets an int value representing the subset of bits from a single Byte.
        /// </summary>
        /// <param name="b">The Byte used to get the subset of bits from.</param>
        /// <param name="offset">The offset of bits starting from the right.</param>
        /// <param name="count">The number of bits to read.</param>
        /// <returns>
        /// An int value representing the subset of bits.
        /// </returns>
        /// <remarks>
        /// Given -> b = 00110101 
        /// A call to GetBits(b, 2, 4)
        /// GetBits looks at the following bits in the byte -> 00{1101}00
        /// Returns 1101 as an int (13)
        /// </remarks>
        internal static int GetBits(byte b, int offset, int count)
        {
            return (b >> offset) & ((1 << count) - 1);
        }

        

        /// <summary>
        /// Reads ARGB values from the 16 bits of two given Bytes in a 1555 format.
        /// </summary>
        /// <param name="one">The first Byte.</param>
        /// <param name="two">The Second Byte.</param>
        /// <returns>A System.Drawing.Color with a ARGB values read from the two given Bytes</returns>
        /// <remarks>
        /// Gets the ARGB values from the 16 bits in the two bytes based on the below diagram
        /// |   BYTE 1   |  BYTE 2   |
        /// | A RRRRR GG | GGG BBBBB |
        /// </remarks>
        internal static Color GetColorFrom2Bytes(byte one, byte two)
        {
            // get the 5 bits used for the RED value from the first byte
            int r1 = Utilities.GetBits(one, 2, 5);
            int r = r1 << 3;

            // get the two high order bits for GREEN from the from the first byte
            int bit = Utilities.GetBits(one, 0, 2);
            // shift bits to the high order
            int g1 = bit << 6;

            // get the 3 low order bits for GREEN from the from the second byte
            bit = Utilities.GetBits(two, 5, 3);
            // shift the low order bits
            int g2 = bit << 3;
            // add the shifted values together to get the full GREEN value
            int g = g1 + g2;

            // get the 5 bits used for the BLUE value from the second byte
            int b1 = Utilities.GetBits(two, 0, 5);
            int b = b1 << 3;

            // get the 1 bit used for the ALPHA value from the first byte
            int a1 = Utilities.GetBits(one, 7, 1);
            int a = a1 * 255;

            // return the resulting Color
            return Color.FromArgb(a, r, g, b);
        }


        internal static void printIntBits(int value) {

            string s = Convert.ToString(value, 2).PadLeft(8, '0');
            Console.WriteLine(s);
            return;
        }

        internal static void printUShortBits(ushort value)
        {

            string s = Convert.ToString(value, 2).PadLeft(16, '0');
            Console.WriteLine(s);
            return;
        }

        internal static ushort Get2BytesFromColor(Color color)
        {

            int red = 0;
            int green = 0;
            int blue = 0;
            int alpha = 0;

            /*if ((color.R >= 248) && (color.G >= 248) && (color.B >= 248))
            {

                red = 255 >> 3;
                green = 255 >> 3;
                blue = 255 >> 3;
                alpha = 255;
                //Console.ReadKey();
            }
            else
            {*/


                red = color.R >> 3;
                green = color.G >> 3;
                blue = color.B >> 3;
                alpha = color.A;
            //}
            

            int red_mask = 0x7C00;
            int green_mask = 0x3E0;
            int blue_mask = 0x1F;
            

            ushort value = (ushort)(
                    (((red)) << FreeImage.FI16_555_RED_SHIFT) |
                    (((green)) << FreeImage.FI16_555_GREEN_SHIFT) |
                    (((blue)) << FreeImage.FI16_555_BLUE_SHIFT) |
                    (((alpha)) << 11) );

            if ((color.R >= 248) && (color.G >= 248) && (color.B >= 248))
            {
                //Console.WriteLine("val = " + value);
                //Console.ReadKey();
            }

                /*printUShortBits(value);
                Console.WriteLine(alpha);
                Console.WriteLine(red);
                Console.WriteLine(green);
                Console.WriteLine(blue);
                Console.WriteLine(color.A);
                Console.WriteLine(color.R);
                Console.WriteLine(color.G);
                Console.WriteLine(color.B);
                Console.WriteLine(value);
                Console.ReadKey();*/
                return value;

        }

        /// <summary>
        /// Gets a 32 character binary string of the specified Int32 value.
        /// </summary>
        /// <param name="n">The value to get a binary string for.</param>
        /// <returns>A string with the resulting binary for the supplied value.</returns>
        /// <remarks>
        /// This method was used during debugging and is left here just for fun.
        /// </remarks>
        internal static string GetIntBinaryString(Int32 n)
        {
            char[] b = new char[32];
            int pos = 31;
            int i = 0;

            while (i < 32)
            {
                if ((n & (1 << i)) != 0)
                {
                    b[pos] = '1';
                }
                else
                {
                    b[pos] = '0';
                }
                pos--;
                i++;
            }
            return new string(b);
        }

        /// <summary>
        /// Gets a 16 character binary string of the specified Int16 value.
        /// </summary>
        /// <param name="n">The value to get a binary string for.</param>
        /// <returns>A string with the resulting binary for the supplied value.</returns>
        /// <remarks>
        /// This method was used during debugging and is left here just for fun.
        /// </remarks>
        internal static string GetInt16BinaryString(Int16 n)
        {
            char[] b = new char[16];
            int pos = 15;
            int i = 0;

            while (i < 16)
            {
                if ((n & (1 << i)) != 0)
                {
                    b[pos] = '1';
                }
                else
                {
                    b[pos] = '0';
                }
                pos--;
                i++;
            }
            return new string(b);
        }

        internal static int ReverseBytes(long val)
        {
            byte[] intAsBytes = BitConverter.GetBytes(val);
            Array.Reverse(intAsBytes);
            return BitConverter.ToInt32(intAsBytes, 0);
        }

        internal static string IntToBinaryString(long v)
        {
            string s = Convert.ToString(v, 2);
            string t = s.PadLeft(32, '0');
            string res = "";
            for (int i = 0; i < t.Length; ++i)
            {
                if (i > 0 && i % 8 == 0)
                    res += " ";
                res += t[i];
            }
            return res;
        }

        internal static uint ColorToRGB565Bytes(uint pixel) {

            ushort red_mask = 0xF800;
            ushort green_mask = 0x7E0;
            ushort blue_mask = 0x1F;


            uint red_value = (pixel & red_mask) >> 11;
            uint green_value = (pixel & green_mask) >> 5;
            uint blue_value = (pixel & blue_mask);


            uint pixel565 = (red_value << 11) | (green_value << 5) | blue_value;
            return pixel565;
        }
    }


    public class MyFilenameComparer : Comparer<string>
    {
        public override int Compare(string x, string y)
        {
            var xs = x.Split('\\');
            var ys = y.Split('\\');

            var xsi = xs[1];
            var ysi = ys[1];

            var xss = xsi.Split('_');
            var yss = ysi.Split('_');

            var xsii = xss[0];
            var ysii = yss[0];

            int xNumber;
            int yNumber;
            var xIsNumber = int.TryParse(xsii, out xNumber);
            var yIsNumber = int.TryParse(ysii, out yNumber);

            if (xIsNumber && yIsNumber)
            {
                return xNumber.CompareTo(yNumber);
            }
            if (xIsNumber)
            {
                return -1;
            }
            if (yIsNumber)
            {
                return 1;
            }
            return x.CompareTo(y);
        }
    }

    public struct RecordContentHeaderStruct
    {
        public Int32 contentHeaderLength;
        public Int32 imageWidth;
        public Int32 desiredImageWidth;
        public Int32 imageHeight;
        public Int16 frameCount; //not sure on this one; tends to be 1
        public Int16 colorDepthInBits; //not sure on this one
        public Int32 lengthOfImageDataFor8BitImages; //not sure on this one
        public Int32 paletteSizeFor8BitImages;
        public Int32 mysteriousNumber1; //tends to be 2835
        public Int32 mysteriousNumber2;//tends to be 2835
        public Int32 endDataHeader;
    }

    public struct RecordHeaderStruct
    {
        public string blockType;
        public Int32 recordID;
        public Int32 startOffset;
        public Int32 dataSize;
        public Int32 originalPosition;
        public string imageBitDepth;
        public int bitDepth;
        public RecordContentHeaderStruct recordContentHeader;
    }

    public struct FileRecordStruct
    {
        public RecordHeaderStruct rhs;
        public RecordContentHeaderStruct rchs;
        public string filename;
        public string extension;
        public string filenameWithPath;
    }


    class Program
    {
        static void Main(string[] args)
        {
            //byte version1;
            //byte version2;
            //byte numRecordsAsByte;
            Int32 numRecords;
            int headerSizeInBytes = 16;
            //string filename;
            IEnumerable<string> filenames;
            List<string> approvedFilenames = new List<string>();
            List<string> sortedFilenames = new List<string>();
            string foldername;
            string outputFilename;
            string outputFilenameWithExt;
            //string outputFolderName;
            const int RSRC_HEADER_LENGTH = 16;
            const int ENTRY_HEADER_LENGTH = 16;
            
            RecordHeaderStruct[] recHeads;
            FIBITMAP[] theBitmaps = null;


            Console.WriteLine();
            Console.WriteLine("IMPERIALISM II PC .RSRC FILE PACKER");
            Console.WriteLine("");
            Console.WriteLine("------------------------------------");
            Console.WriteLine("");
            Console.WriteLine("Version 1.1 released in 2017 by n64gamer");
            Console.WriteLine("");
            Console.WriteLine("This software is released under the LGPL.");
            Console.WriteLine("Portions of this software use code licensed under the CPOL.");
            Console.WriteLine("Please include acknowledgements below if you make changes to the code.");
            Console.WriteLine("");
            Console.WriteLine("Made with Utilities class from TargaImage.cs by David Polomis");
            Console.WriteLine("(paloma_sw@cox.net) which is licensed under Code Project Open License");
            Console.WriteLine("(CPOL) 1.02.");
            Console.WriteLine("");
            Console.WriteLine("Parts of this software use modified code portions from Zachtronics.com");
            Console.WriteLine("Yoda Stories reverse engineering tutorial:");
            Console.WriteLine("http://www.zachtronics.com/yoda-stories/");
            Console.WriteLine("");
            Console.WriteLine("Other portions obtained from various programming forums discussing C++ and C#.");
            Console.WriteLine();
            Console.WriteLine("------------------------------------");
            if (args.GetLength(0) == 0)
            {
                Console.WriteLine("Press ENTER to continue...");
                Console.ReadKey();
                Console.WriteLine("------------------------------------");
            }
            Console.WriteLine();
            Console.WriteLine("Note: This tool won't pack Imperialism II for Mac .RSRC files. Use a Resource");
            Console.WriteLine("Fork Editor on Mac to modify the contents of Mac .RSRC files for Imperialism II");
            Console.WriteLine();
            
            Console.WriteLine("------------------------------------");
            Console.WriteLine();
            if (args.GetLength(0) == 0) {
                Console.WriteLine("Usage Instructions:");
                Console.WriteLine("IMP2ENC.EXE [FOLDERNAME] [/oef]");
                Console.WriteLine("Optional Parameters: /oef - Overwrites .rsrc file if it already exists");
                Console.WriteLine();
                Console.WriteLine("------------------------------------");
                Console.WriteLine();
                Console.WriteLine("Error: No FOLDERNAME provided!");
                Console.WriteLine("Please provide the name of a folder whose contents you wish to pack into an Imperialism II .RSRC file.");
            }
            else 
            {
                foldername = args[0];
                if (Directory.Exists(args[0]))
                {
                    
                    outputFilename = Path.GetFileNameWithoutExtension(foldername);
                    bool continuePacking = true;
                    outputFilenameWithExt = outputFilename + ".rsrc";
                    
                    if ((((File.Exists(outputFilenameWithExt)) && (args.Length > 1)) && (args[1] != "/oef")) || ((args.Length == 1) && (File.Exists(outputFilenameWithExt))))
                    {
                        continuePacking = false;
                        Console.WriteLine("Could not pack to " + outputFilenameWithExt + " - file already exists!");
                        Console.WriteLine("Use optional /oef parameter to force overwriting of existing file.");
                        Console.WriteLine();
                        Console.WriteLine("Press any key to exit...");
                        Console.ReadKey();
                        Environment.Exit(0);
                    }
                    else if (((File.Exists(outputFilenameWithExt)) && (args.Length == 2)) && (args[1] == "/oef"))
                    {
                        //continue
                        Console.WriteLine("OUTPUT FILE: " + outputFilenameWithExt + " already exists - overwriting... (/oef switch used)");
                    }
                    else
                    {
                        //continue
                        Console.WriteLine("OUTPUT FILE: " + outputFilenameWithExt + " does not exist - file will be created.");
                    }
                    
                    if (continuePacking == true)
                    {
                        Console.WriteLine("FOLDER " + foldername + " exists. Good!");
                        Console.WriteLine("Packing to file: " + outputFilenameWithExt);
                        Console.WriteLine();
                        Console.WriteLine("Press any key to exit...");
                        Console.ReadKey();
                    }


                    try
                    {
                        //FileStream fs = File.OpenWrite(outputFilenameWithExt);
                        FileStream fs = File.Create(outputFilenameWithExt);
                        BinaryWriter binaryWriter = new BinaryWriter(fs);
                        

                        string headerCode = "rsrc";
                        Byte[] headerBytes = new UTF8Encoding(true).GetBytes(headerCode);
                        binaryWriter.Write(headerBytes, 0, headerBytes.Length);
                        uint headerMajorVersion = 3;
                        uint headerMinorVersion = 16;
                        binaryWriter.Write(headerMajorVersion);
                        binaryWriter.Write(headerMinorVersion);
                            
                        filenames = Directory.EnumerateFiles(foldername, "*.*");
                        int numFilenames = filenames.Count();

                        for (int i=0; i<numFilenames; i++)
                        {
                            string ext = Path.GetExtension(filenames.ElementAt(i));
                            //bool isApproved = false;
                            Console.WriteLine("EXT = " + ext);
                            if (((ext == ".png") || (ext == ".PNG")) || ((ext == ".bmp") || (ext == ".BMP"))  || (((ext == ".wav") || (ext == ".WAV")) || ((ext == ".raw") || (ext == ".RAW")))) //supported file types
                            {
                                approvedFilenames.Add(filenames.ElementAt(i));
                                //approvedFilenames.
                            }
                            Console.WriteLine(filenames.ElementAt(i));
                        }

                        //approvedFilenames.CustomSort();
                        var sortedFiles = approvedFilenames.OrderBy(s => s, new MyFilenameComparer());
                        sortedFilenames = sortedFiles.ToList();

                        //Console.ReadKey();
                            
                        numRecords = sortedFilenames.Count;
                        Console.WriteLine("Number of Files to add to RSRC File:" + numRecords);
                            

                        binaryWriter.Write(numRecords);
                        //binaryWriter.Write(headerCode);
                        int recordHeaderLength = numRecords * ENTRY_HEADER_LENGTH;
                        int rsrcHeaderLength = RSRC_HEADER_LENGTH;
                        int dataStartOffset = rsrcHeaderLength + recordHeaderLength;
                        Console.WriteLine("Length of Entry Header:" + recordHeaderLength);
                        Console.WriteLine("Data Start Offset:" + dataStartOffset);
                        //Console.ReadKey();

                        recHeads = new RecordHeaderStruct[numRecords];
                        string chunktype = "";

                        Int32 startOffset = 0;
                        Int32 dataCountSoFar = 0;

                        //theBitmaps = new FIBITMAP[sortedFilenames.Count];

                        /* create preliminary headers for each file */
                        for (int i = 0; i < sortedFilenames.Count; i++)
                        {
                            RecordHeaderStruct recHead = default(RecordHeaderStruct);
                            recHead.recordContentHeader = default(RecordContentHeaderStruct);
                            //recHead.recordContentHeader = default(RecordContentHeaderStruct);
                            //RecordContentHeaderStruct contHead = default(RecordContentHeaderStruct);

                            string theFilename = sortedFilenames.ElementAt(i);
                            string ext = Path.GetExtension(theFilename);
                            string afilename = Path.GetFileNameWithoutExtension(theFilename);
                            //split the filename up into pieces wherever an underscore is present
                            string[] filenameparts = afilename.Split('_');
                            int originalPosition = int.Parse(filenameparts[0]);
                            Int32 recordID = Int32.Parse(filenameparts[1]);
                            string imageBitDepth = "";
                            short imageBitDepthAsNumber = 0;
                            string imageWidth = "";
                            short imageWidthAsNumber = 0;
                            string imageHeight = "";
                            short imageHeightAsNumber = 0;
                            if (filenameparts.Length > 5)
                            {
                                imageWidth = filenameparts[3];
                                imageHeight = filenameparts[5];
                                imageWidthAsNumber = Convert.ToInt16(imageWidth);
                                imageHeightAsNumber = Convert.ToInt16(imageHeight);
                            }
                            if (filenameparts.Length > 2) {
                                imageBitDepth = filenameparts[2];
                            }
                            Console.WriteLine("original position: " + originalPosition);
                            Console.WriteLine("original recordid: " + recordID);
                            if (imageBitDepth != "")
                            {
                                Console.WriteLine("original bitdepth: " + imageBitDepth);
                                if (imageBitDepth == "16bit") {
                                    imageBitDepthAsNumber = 16;
                                }
                                else if (imageBitDepth == "8bit")
                                {
                                    imageBitDepthAsNumber = 8;
                                }
                                else if (imageBitDepth == "24bit")
                                {
                                    imageBitDepthAsNumber = 24;
                                }
                                else if (imageBitDepth == "32bit")
                                {
                                    imageBitDepthAsNumber = 32;
                                }
                            }
                            //Console.ReadKey();

                            recHead.originalPosition = originalPosition; //(i + 1);
                                

                            if (((ext == ".png") || (ext == ".PNG")) || ((ext == ".bmp") || (ext == ".BMP"))) //png picture file
                            {
                                chunktype = "TCIP";
                            }
                            else if ((ext == ".wav") || (ext == ".WAV")) //wave sound file
                            {
                                chunktype = " dns";
                            }
                            else if ((ext == ".raw") || (ext == ".RAW")) //raw sound file
                            {
                                chunktype = " dns";
                            }

                            recHead.blockType = chunktype;
                            recHead.imageBitDepth = imageBitDepth;
                            recHead.recordContentHeader.colorDepthInBits = imageBitDepthAsNumber;

                            Int32 dataSize = 0;

                            FIBITMAP dib = new FIBITMAP();

                            if (chunktype.Equals("TCIP"))
                            {
                                    
                                if ((ext == ".png") || (ext == ".PNG")) {
                                        
                                    dib = FreeImage.Load(FREE_IMAGE_FORMAT.FIF_PNG, theFilename, FREE_IMAGE_LOAD_FLAGS.DEFAULT);
                                    
                                }
                                else if ((ext == ".bmp") || (ext == ".BMP"))
                                {
                                        
                                    dib = FreeImage.Load(FREE_IMAGE_FORMAT.FIF_BMP, theFilename, FREE_IMAGE_LOAD_FLAGS.DEFAULT);
                                    

                                }
                                FreeImage.FlipHorizontal(dib);
                                //Image theImage = Bitmap.FromFile(theFilename);
                                Console.WriteLine("Loaded image : " + theFilename);
                                //Console.WriteLine(theImage.PixelFormat.ToString()); //Format32bppArgb

                                //rotate image by 180 deg and flip on x axis
                                //theImage.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipX);
                                FreeImage.Rotate(dib, 180);

                                //read pixel data from theImage
                                recHead.recordContentHeader.imageWidth = (int)(FreeImage.GetWidth(dib)); // theImage.Width;
                                recHead.recordContentHeader.imageHeight = (int)(FreeImage.GetHeight(dib)); // theImage.Height;
                                //overwrite read image width with described imagewidth
                                recHead.recordContentHeader.desiredImageWidth = recHead.recordContentHeader.imageWidth;
                                if (imageWidthAsNumber != 0) { recHead.recordContentHeader.desiredImageWidth = imageWidthAsNumber; }

                                //recHead.recordContentHeader.colorDepthInBits = recHead.bitDepth;
                                Console.WriteLine("Img Size : " + recHead.recordContentHeader.imageWidth + " x " + recHead.recordContentHeader.imageHeight + " @ " + recHead.recordContentHeader.colorDepthInBits + "-bit");

                                Int32 imWidthEven = recHead.recordContentHeader.imageWidth;
                                Int32 imWidthActual = recHead.recordContentHeader.desiredImageWidth;
                                Boolean widthHackApplied = false;


                                /* handle binary file padding quirks */
                                if (imWidthEven % 2 == 1)
                                {
                                    bool divisbleBy2 = false;
                                    do
                                    {
                                        widthHackApplied = true;
                                        imWidthEven++;

                                        if (recHead.recordContentHeader.colorDepthInBits == 16)
                                        {
                                            if ((imWidthEven % 2) == 0) { divisbleBy2 = true; }
                                        }
                                        else if (recHead.recordContentHeader.colorDepthInBits == 8)
                                        {
                                            if ((imWidthEven % 4) == 0) { divisbleBy2 = true; }
                                        }
                                    }
                                    while (divisbleBy2 == false);
                                } //round up to nearest divisor of 2; quirk of the file format
                                else if ((imWidthEven % 4 != 0) && (recHead.recordContentHeader.colorDepthInBits == 8))
                                {
                                    bool divisbleBy4 = false;
                                    do
                                    {
                                        widthHackApplied = true;
                                        int difference = imWidthEven % 4;
                                        imWidthEven = imWidthEven + difference;

                                        if (recHead.recordContentHeader.colorDepthInBits == 16)
                                        {
                                            if ((imWidthEven % 4) == 0) { divisbleBy4 = true; }
                                        }
                                        else if (recHead.recordContentHeader.colorDepthInBits == 8)
                                        {
                                            if ((imWidthEven % 4) == 0) { divisbleBy4 = true; }
                                        }
                                        else if (recHead.recordContentHeader.colorDepthInBits == 24)
                                        {
                                            if ((imWidthEven % 4) == 0) { divisbleBy4 = true; }
                                        }
                                    }
                                    while (divisbleBy4 == false);
                                } //round up to nearest divisor of 4; quirk of the file format

                                Int32 totalPixels;

                                if (widthHackApplied == true)
                                {
                                    totalPixels = imWidthEven * recHead.recordContentHeader.imageHeight;
                                }
                                else
                                {
                                    totalPixels = recHead.recordContentHeader.imageWidth * recHead.recordContentHeader.imageHeight;
                                }

                                Int32 totalImageDataBytes = 0;
                                Int32 total8BitImagePaletteBytes = 1024; //ie. 256 x 4 - 4th byte is zero on all palette entries
                                Int32 totalImageHeaderBytes = 40;
                                Int32 totalImageBytes = 0;

                                if (recHead.recordContentHeader.colorDepthInBits == 24)
                                {
                                    totalImageDataBytes = totalPixels * 3;
                                    totalImageBytes = totalImageDataBytes + totalImageHeaderBytes;
                                }
                                else if (recHead.recordContentHeader.colorDepthInBits == 16)
                                {
                                    totalImageDataBytes = totalPixels * 2;
                                    totalImageBytes = totalImageDataBytes + totalImageHeaderBytes;
                                }
                                else if (recHead.recordContentHeader.colorDepthInBits == 8)
                                {
                                    totalImageDataBytes = totalPixels * 1;
                                    totalImageBytes = totalImageDataBytes + total8BitImagePaletteBytes + totalImageHeaderBytes;
                                }
                                Console.WriteLine("Total Pixel Count : " + totalPixels);
                                Console.WriteLine("Total Image Byte Count : " + totalImageBytes);
                                Console.WriteLine("* Total Image Data Byte Count: " + totalImageDataBytes);
                                Console.WriteLine("* Total Image Header Byte Count: " + totalImageHeaderBytes);
                                Console.WriteLine("* Total 8-Bit Image Palette Byte Count (if applicable): " + total8BitImagePaletteBytes);
                                dataSize = totalImageBytes;
                                    
                            }

                                

                            if ((chunktype.Equals("TCIP")) || (chunktype.Equals(" dns")))
                            {
                                    
                                //write the recordID
                                //Int32 recordID = Int32.Parse(filenameparts[1]);
                                startOffset = 0;
                                startOffset = headerSizeInBytes + (numRecords * 16) + dataCountSoFar;
                                    

                                recHead.recordID = recordID;
                                recHead.startOffset = startOffset;
                                recHead.dataSize = dataSize;
                                recHeads[i] = recHead;
                                Console.WriteLine("SLOT: " + recHeads[i].originalPosition + " REC#: " + recordID + " StartAt: " + startOffset + "  DataSize: " + dataSize);

                                //write the file header for this entry
                                if (chunktype.Equals("TCIP")) {
                                    char[] chunkTypeAsChars = chunktype.ToCharArray(); 
                                    binaryWriter.Write(chunkTypeAsChars);
                                    binaryWriter.Write(recHead.recordID);
                                    binaryWriter.Write(recHead.startOffset);
                                    binaryWriter.Write(recHead.dataSize);
                                }

                                dataCountSoFar += dataSize;
                            }
                            else
                            {
                                Console.WriteLine("Unknown BLOCKTYPE or FILETYPE in ENTRY# " + (i + 1) + " : " + theFilename);
                            }
                                
                        }

                        /* create detailed headers for each file */
                        for (int i=0; i<sortedFilenames.Count; i++)
                        {
                            Console.WriteLine("BWBSP = "+binaryWriter.BaseStream.Position);
                            FIBITMAP dib = new FIBITMAP();
                            /* write record header */
                            recHeads[i].recordContentHeader.contentHeaderLength = 40;
                            if (recHeads[i].blockType == "TCIP") { 
                                //recHeads[i].recordContentHeader.colorDepthInBits = (short)recHeads[i].bitDepth;
                                recHeads[i].recordContentHeader.frameCount = (short)1;
                                recHeads[i].recordContentHeader.mysteriousNumber1 = 2835;
                                recHeads[i].recordContentHeader.mysteriousNumber2 = 2835;
                                recHeads[i].recordContentHeader.endDataHeader = 0;
                                binaryWriter.Write(recHeads[i].recordContentHeader.contentHeaderLength);
                                binaryWriter.Write(recHeads[i].recordContentHeader.desiredImageWidth);
                                binaryWriter.Write(recHeads[i].recordContentHeader.imageHeight);
                                binaryWriter.Write(recHeads[i].recordContentHeader.frameCount);
                                binaryWriter.Write(recHeads[i].recordContentHeader.colorDepthInBits);
                                Int32 gapA = 0;
                                binaryWriter.Write(gapA);
                                //recHeads[i].recordContentHeader.lengthOfImageDataFor8BitImages = recHeads[i].recordContentHeader.imageWidth * recHeads[i].recordContentHeader.imageHeight;
                                recHeads[i].recordContentHeader.lengthOfImageDataFor8BitImages = recHeads[i].dataSize - 40;
                                /*if (recHeads[i].recordContentHeader.colorDepthInBits == 16)
                                {
                                    recHeads[i].recordContentHeader.lengthOfImageDataFor8BitImages *= 2;
                                }
                                else if (recHeads[i].recordContentHeader.colorDepthInBits == 24)
                                {
                                    recHeads[i].recordContentHeader.lengthOfImageDataFor8BitImages *= 3;
                                }
                                */
                                

                                binaryWriter.Write(recHeads[i].recordContentHeader.lengthOfImageDataFor8BitImages);
                                //recHeads[i].recordContentHeader.paletteSizeFor8BitImages = recHeads[j].dataSize - recHeads[j].recordContentHeader.contentHeaderLength - recHeads[j].recordContentHeader.lengthOfImageDataFor8BitImages
                                //binaryWriter.Write(recHeads[i].recordContentHeader.paletteSizeFor8BitImages);
                                binaryWriter.Write(recHeads[i].recordContentHeader.mysteriousNumber1);
                                binaryWriter.Write(recHeads[i].recordContentHeader.mysteriousNumber2);
                                Int32 gapB = 0;
                                binaryWriter.Write(gapB);
                                binaryWriter.Write(recHeads[i].recordContentHeader.endDataHeader);

                                

                                string theFilename = sortedFilenames.ElementAt(i);
                                string ext = Path.GetExtension(theFilename);

                                if ((ext == ".png") || (ext == ".PNG"))
                                {
                                    dib = FreeImage.Load(FREE_IMAGE_FORMAT.FIF_PNG, theFilename, FREE_IMAGE_LOAD_FLAGS.DEFAULT);
                                }
                                else if ((ext == ".bmp") || (ext == ".BMP"))
                                {
                                    dib = FreeImage.Load(FREE_IMAGE_FORMAT.FIF_BMP, theFilename, FREE_IMAGE_LOAD_FLAGS.DEFAULT);
                                        
                                }
                                FreeImage.FlipHorizontal(dib);
                                //Image theImage = Bitmap.FromFile(theFilename);
                                Console.WriteLine("Loaded image again : " + theFilename);
                                //Console.WriteLine(theImage.PixelFormat.ToString()); //Format32bppArgb

                                if (recHeads[i].recordContentHeader.colorDepthInBits == 8)
                                {
                                    //recHeads[i].recordContentHeader.lengthOfImageDataFor8BitImages *= 1;
                                    /* write the palette for 8-bit image to file */
                                    Palette palette2 = FreeImage.GetPaletteEx(dib);
                                    Color theColor;
                                    byte palR = 0;
                                    byte palG = 0;
                                    byte palB = 0;
                                    byte palX = 0;
                                    int palSize = palette2.Count;
                                    RGBQUAD indexColor = new RGBQUAD();
                                    for (int m = 0; m < palSize; m++)
                                    {
                                        //indexColor = palette2[m];
                                        //theColor = indexColor.Color;
                                        theColor = palette2[m].Color;
                                        palR = theColor.R;
                                        palG = theColor.G;
                                        palB = theColor.B;
                                        binaryWriter.Write(palB);
                                        binaryWriter.Write(palG);
                                        binaryWriter.Write(palR);
                                        binaryWriter.Write(palX);
                                    }
                                }

                                //rotate image by 180 deg and flip on x axis
                                //theImage.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipX);
                                //dib = FreeImage.ConvertTo16Bits555(dib);
                                //dib = FreeImage.Rotate(dib, 180);
                                //dib = FreeImage.ConvertTo16Bits555(dib);
                                //FreeImage.FlipHorizontal(dib);




                                uint pixWide = (uint)recHeads[i].recordContentHeader.imageWidth;
                                uint pixHigh = (uint)recHeads[i].recordContentHeader.imageHeight;
                                Console.WriteLine("ImageSave "+pixWide + " x " +pixHigh);
                                /* write bitmap data */
                                for (uint y = 0; y < pixHigh; y++)
                                {
                                    //Console.WriteLine(y);
                                    //Console.WriteLine("ImageSave " + x);
                                    for (uint x = (pixWide-1); x >= 0; x--)
                                    {
                                        //get pixel color
                                        //Console.WriteLine(x);
                                            

                                        if (recHeads[i].recordContentHeader.colorDepthInBits == 16)
                                        {
                                            //uint bpp = FreeImage.GetBPP(dib);

                                            //Console.WriteLine("BPP = " + bpp);
                                            //Console.ReadKey();
                                            RGBQUAD pixColor = default(RGBQUAD);
                                            FreeImage.GetPixelColor(dib, x, y, out pixColor);
                                            //Color col = Color.FromArgb(pixColor.uintValue);

                                            //Console.WriteLine(pixColor.rgbRed);
                                                
                                            // Console.ReadKey();
                                            ushort a = Utilities.Get2BytesFromColor(pixColor.Color);
                                            // Console.WriteLine(a);
                                            //Console.ReadKey();

                                            binaryWriter.Write(a);
                                            //Color555 c = new Color555(pixColor.Color);
                                            //ushort theVal = c.getValue();

                                            //uint val565 = Utilities.ColorToRGB565Bytes(pixColor.uintValue);

                                            //binaryWriter.Write(val565);
                                            //IntPtr t = FreeImage.GetBits(dib);


                                            //convert 3 byte rgb to 2 byte 16-bit rgb 555
                                            //binaryWriter.Write(pixColor.rgbRed);
                                            //binaryWriter.Write(pixColor.rgbGreen);
                                            //binaryWriter.Write(pixColor.rgbBlue);
                                        }
                                        else if (recHeads[i].recordContentHeader.colorDepthInBits == 24)
                                        {
                                            RGBQUAD pixColor;
                                            FreeImage.GetPixelColor(dib, x, y, out pixColor);
                                            binaryWriter.Write(pixColor.rgbRed);
                                            binaryWriter.Write(pixColor.rgbGreen);
                                            binaryWriter.Write(pixColor.rgbBlue);
                                        }
                                        else if (recHeads[i].recordContentHeader.colorDepthInBits == 8)
                                        {
                                            byte theIndex = 0;
                                            FreeImage.GetPixelIndex(dib, x, y, out theIndex);
                                            binaryWriter.Write(theIndex);
                                        }
                                        else
                                        {
                                            Console.WriteLine("Unsupported color depth for SLOT #" + recHeads[i].recordID);
                                        }
                                        if (x==0) { break;  }
                                        //pixColor.
                                    }
                                    if (y == (pixHigh -1)) { break; }
                                }
                            }
                            
                            
                            
                        }
                        binaryWriter.Close();
                        fs.Close();
                    }
                    catch (Exception Ex)
                    {
                        Console.WriteLine("ERROR handling file!!!");
                        Console.WriteLine(Ex.ToString());
                    }
                }
                else
                {
                    Console.WriteLine("FOLDER "+foldername+" does not exist!");
                    Console.WriteLine();
                    Console.WriteLine("Please provide the name of a folder that does exist.");
                }
                //Console.WriteLine();
                //Console.WriteLine("Press a key to exit...");
                //Console.ReadKey();
                //Console.WriteLine();
                //Environment.Exit(0);
                return;
            }
        }
    }
}
