/*  
    Copyright (c) 2019 MasterCan

    You can redistribute or modify this code under GNU General Public License v3.0.
    The permission given to run this code in a closed source project modified.
    But, you have to release the source code using this library must be released.
    Or, you have to add original owner's name into your project.
*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Package
{
    public enum ChunkType
    {
        Head = 0x48454144,
        Data = 0x44415441,
        File = 0x46494C45
    }

    public enum PackageType
    {
        Secret = 0,
        Hidden = 1,
        Normal = 2
    }

    //Last compiled at 2019/07/17 Wednesday 18:56:00 GMT+03.
    public abstract class TPIPackageClass
    {
        public string PackageID = "00000000";
        public string PackageName = "Unknown";
        public string PackageVer = "N/A";
        public string PackageOwner = "Unknown";
        public int FileCount = 0;
        public PackageType Type = PackageType.Normal;
        public List<TTFileClass> Files = new List<TTFileClass>();
    }

    public class TPIClass : TPIPackageClass
    {
        private static bool ConditionsOK(Stream str)
        {
            if(str.Position != str.Length) //I'm afraid this might cause a freeze...
                return true;
            else
                return false;
        }

        public static TPIPackageClass FromFile(string Location)
        {
            if (!File.Exists(Location))
                throw new FileNotFoundException("TPI file could not be found."); //Boink

            byte[] TPI = File.ReadAllBytes(Location); //Read whole TPI file.
            using (Stream Bytes = new MemoryStream(TPI))
            {
                TPIClass Pack = new TPIClass();
                while (ConditionsOK(Bytes))
                {
                    uint Header = Expressions.GetUNumber(Expressions.GetBytes(Bytes, 0, 4));
                    if (Header == (uint)ChunkType.Head)
                    {
                        //Processing the HEAD. Reading first 4 bytes to find out length.
                        int Next = Expressions.GetNumber(Expressions.GetBytes(Bytes, 0, 4));

                        //Buaa getting all bytes now.
                        byte[] Data = Expressions.GetBytes(Bytes, 0, Next);
                        Pack.PackageID = Expressions.GetCRCID(Data.SubArray(0, 4));
                        Pack.FileCount = Expressions.GetNumber(Data.SubArray(4, Data.Length - 4));

                        //Now, we should have DATA. Ending this if else.
                    }
                    else if (Header == (uint)ChunkType.Data)
                    {
                        //Processing the DATA.
                        int Next = Expressions.GetNumber(Expressions.GetBytes(Bytes, 0, 4));

                        //Fun part ;)
                        int NameLen = Expressions.GetUNumber16(Expressions.GetBytes(Bytes, 0, 2));
                        Pack.PackageName = Expressions.GetText(Bytes, 0, NameLen);
                        int VerLen = Expressions.GetUNumber16(Expressions.GetBytes(Bytes, 0, 2));
                        Pack.PackageVer = Expressions.GetText(Bytes, 0, VerLen);
                        int OwnerLen = Expressions.GetUNumber16(Expressions.GetBytes(Bytes, 0, 2));
                        Pack.PackageOwner = Expressions.GetText(Bytes, 0, OwnerLen);

                        Pack.Type = (PackageType)Expressions.GetNumber(Expressions.GetBytes(Bytes, 0, 4));

                        //Now, we should have FILEs. Ending this if else and letting while continue it's work.
                    }
                    else if(Header == (uint)ChunkType.File)
                    { 
                        TTFileClass File = new TTFileClass();
                        int Next = Expressions.GetNumber(Expressions.GetBytes(Bytes, 0, 4));
                        File.CRC = Expressions.GetCRCID(Expressions.GetBytes(Bytes, 0, 4));
                        File.FileSize = Expressions.GetUNumber(Expressions.GetBytes(Bytes, 0, 4));

                        File.OriginalNameLength = Expressions.GetUNumber16(Expressions.GetBytes(Bytes, 0, 2));
                        File.FileName = Expressions.GetText(Bytes, 0, File.OriginalNameLength);

                        //No NULs after file name, so let while loop read next header if Stream not ended.
                        //Let's add file to collection...
                        Pack.Files.Add(File);
                    }
                    else
                    {
                        //Hmm...
                    }
                }

                //Return when everything is good to go.
                return Pack;
            }
        }
    }

    public class TTFileClass
    {
        public string FileName;
        public string CRC;
        public ushort OriginalNameLength;
        public int FullNameLength;
        public uint FileSize;

        public string FullName { get { return CRC + "." + FileName; } }
    }

    public static class Expressions
    {
        //Gets bytes from a range, and converts it to string.
        public static string GetText(Stream arr, int start, int count)
        {
            return Encoding.UTF8.GetString(GetBytes(arr, start, count));
        }

        //Gets bytes after start index, and converts it to string.
        public static string GetText(Stream arr, int start)
        {
            return Encoding.UTF8.GetString(GetBytes(arr, start));
        }

        //Get next byte.
        public static byte GetByte(Stream arr, int start)
        {
            byte[] New = new byte[1];
            arr.Read(New, start, 1);
            return New[0];
        }

        //Gets bytes after start index.
        public static byte[] GetBytes(Stream arr, int start)
        {
            byte[] New = new byte[arr.Length - start];
            arr.Read(New, start, (int)(arr.Length - start));
            return New;
        }

        //Gets bytes from a range.
        public static byte[] GetBytes(Stream arr, int start, int count)
        {
            byte[] New = new byte[count];
            arr.Read(New, start, count);
            return New;
        }

        //Converts 4-byte CRC data into string and returns.
        public static string GetCRCID(byte[] Bytes)
        {
            if(Bytes.Length != 4)
                throw new ArgumentException("Invalid Package ID or formatting problem.");

            string Return = "";
            Return += ByteToHex(Bytes[3]);
            Return += ByteToHex(Bytes[2]);
            Return += ByteToHex(Bytes[1]);
            Return += ByteToHex(Bytes[0]);
            return Return;
        }

        //Converting byte to hex for CRC and Hash generation (simply, appending 0)
        private static string ByteToHex(byte b)
        {
            string h = b.ToString("X");
            if(h.Length < 2)
                h = "0" + h;
            return h;
        }

        //An extension for arrays like Substring. Taken from https://stackoverflow.com/questions/943635/getting-a-sub-array-from-an-existing-array.
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        //Getting Int32 of byte array.
        public static int GetNumber(byte[] Bytes)
        {
            return BitConverter.ToInt32(Bytes, 0);
        }

        //Getting UInt32 of byte array.
        public static uint GetUNumber(byte[] Bytes)
        {
            return BitConverter.ToUInt32(Bytes, 0);
        }

        //Getting UInt16 of byte array.
        public static ushort GetUNumber16(byte[] Bytes)
        {
            return BitConverter.ToUInt16(Bytes, 0);
        }


        //Array of size sufixes.
        private static readonly string[] SizeSuffixes =
                   { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        //Getting file size from bytes count.
        public static string GetFileSize(Int64 value, int decimalPlaces = 2)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + GetFileSize(-value); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }
    }
}
