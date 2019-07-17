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
    //Last compiled at 2019/07/17 Wednesday 12:21:02 GMT+03.
    public abstract class TTPackageClass
    {
        public string PackageID = "00000000";
        public string PackageName = "Unknown";
        public string PackageVer = "N/A";
        public string PackageOwner = "Unknown";
        public int FileCount = 0;
        public List<TTFileClass> Files = new List<TTFileClass>();
    }

    public class TTPackage : TTPackageClass
    {
        private static bool ConditionsOK(Stream str)
        {
            if(str.Position != str.Length) //I'm afraid this might cause a freeze...
                return true;
            else
                return false;
        }

        public static TTPackageClass FromFile(string Location)
        {
            if (!File.Exists(Location))
                throw new FileNotFoundException("TPI file could not be found."); //Boink

            byte[] TPI = File.ReadAllBytes(Location); //Read whole TPI file.
            using (Stream Bytes = new MemoryStream(TPI))
            {
                TTPackageClass Pack = new TTPackage();
                while (ConditionsOK(Bytes))
                {
                    string Header = Expressions.GetText(Bytes, 0, 4).ToUpper();
                    if (Header == "DAEH")
                    {
                        //Processing the HEAD.
                        int Next = Expressions.GetByte(Bytes, 0);
                        byte[] Data = Expressions.GetBytes(Bytes, 0, Next);
                        Data = Data.SubArray(3, Data.Length - 3);
                        Pack.PackageID = Expressions.GetCRCID(Data.SubArray(0, 4));
                        Pack.FileCount = Data[Data.Length - 1];

                        //We're done with HEAD, now let's read next 3 null characters.
                        Expressions.GetBytes(Bytes, 0, 3);

                        //Now, we should have DATA. Ending this if else.
                    }
                    else if (Header == "ATAD")
                    {
                        //Processing the DATA.
                        int Next = Expressions.GetByte(Bytes, 0);
                        int Current = 0;
                        Expressions.GetBytes(Bytes, 0, 3); //Wasting the NULs.

                        //Fun part ;)
                        int NameLen = Expressions.GetByte(Bytes, 0) + 1;
                        Pack.PackageName = Expressions.GetText(Bytes, 0, NameLen).Substring(1, NameLen - 1);
                        int VerLen = Expressions.GetByte(Bytes, 0) + 1;
                        Pack.PackageVer = Expressions.GetText(Bytes, 0, VerLen).Substring(1, VerLen - 1);
                        int OwnerLen = Expressions.GetByte(Bytes, 0) + 1;
                        Pack.PackageOwner = Expressions.GetText(Bytes, 0, OwnerLen).Substring(1, OwnerLen - 1);

                        //Unfortunately, I don't know what that next byte are here for. So, I'll discard it.
                        Expressions.GetBytes(Bytes, 0, 1);

                        //We're done with DATA, now let's read next 3 null characters.
                        Expressions.GetBytes(Bytes, 0, 3);

                        //Now, we should have FILE. Ending this if else and continuing with a while loop.
                    }
                    else if(Header == "ELIF")
                    { 
                        TTFileClass File = new TTFileClass();
                        File.FullNameLength = Expressions.GetByte(Bytes, 0) - 1;
                        Expressions.GetBytes(Bytes, 0, 3); // 3 NULs...
                        File.CRC = Expressions.GetCRCID(Expressions.GetBytes(Bytes, 0, 4));

                        //Unfortunately, I don't know what these next 2 bytes are here for. So, I'll discarding them.
                        //Note: A friend says it's file size. But multiplation, addition and XOR did not worked to get original file sizes.
                        Expressions.GetBytes(Bytes, 0, 2);

                        //Skipping 2 NULs...
                        Expressions.GetBytes(Bytes, 0, 2);

                        File.OriginalNameLength = Expressions.GetByte(Bytes, 0);
                        File.FileName = Expressions.GetText(Bytes, 0, File.OriginalNameLength + 1).Substring(1, File.OriginalNameLength);

                        //No NULs after file name, so let while loop read next FILE is exist or Stream not ended.
                        //Before ending while, let's add file to collection...
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
        public int OriginalNameLength;
        public int FullNameLength;

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
            Return += Bytes[3].ToString("X");
            Return += Bytes[2].ToString("X");
            Return += Bytes[1].ToString("X");
            Return += Bytes[0].ToString("X");
            return Return;
        }

        //An extension for arrays like Substring. Taken from https://stackoverflow.com/questions/943635/getting-a-sub-array-from-an-existing-array.
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
}
