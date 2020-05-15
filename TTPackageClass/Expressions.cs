/*  
    TT Package / TPI File Parser
    Copyright (c) 2020 Unstoppable

    You can redistribute or modify this code under GNU General Public License v3.0.
    The permission given to run this code in a closed source project modified.
    But, you have to release the source code using this library must be released.
    Or, you have to add original owner's name into your project.
*/

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace TTPackageClass
{
    internal static class Expressions
    {
        public static TPIPackageClass CreateTPIPackage()
        {
            return new TPIPackageClass()
            {
                Files = new List<TTFileClass>(),
                PackageID = "00000000",
                PackageName = "",
                PackageOwner = "",
                PackageVer = "",
                Type = PackageType.Normal
            };
        }

        public static TTFSDataClass CreateTTFSData()
        {
            return new TTFSDataClass()
            {
                Packages = new List<TPIPackageClass>()
            };
        }

        public static TTFileClass CreateTTFile()
        {
            return new TTFileClass()
            {
                CRC = "00000000",
                FileName = "",
                FileSize = 0,
            };
        }

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
            if (Bytes.Length != 4)
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
            if (h.Length < 2)
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

            int mag = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }

        public static string Flatten(byte[] Array)
        {
            string Return = string.Empty;
            foreach (byte b in Array)
                Return += (char)b;
            return Return;
        }

        public static void Write(this MemoryStream Stream, string Str)
        {
            byte[] b = new byte[Str.Length];
            for (int i = 0; i < Str.Length; ++i)
                b[i] = (byte)Str[i];

            Stream.Write(b, 0, b.Length);
        }

        public static void Write(this MemoryStream Stream, byte[] arr)
        {
            Stream.Write(arr, 0, arr.Length);
        }

        public static void Write(this MemoryStream Stream, byte b)
        {
            Stream.Write(new byte[] { b }, 0, 1);
        }
    }
}