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

namespace TTPackageClass
{
    /// <summary>
    /// The class containing all actions can be done with <see cref="TPIPackageClass"/>.
    /// </summary>
    public class TPIClass
    {
        internal static bool ConditionsOK(Stream str)
        {
            if (str.Position != str.Length) //I'm afraid this might cause a freeze...
                return true;
            else
                return false;
        }

        /// <summary>
        /// Saves the specified <see cref="TPIPackageClass"/> into a file.
        /// </summary>
        /// <param name="TPI">Instance of <see cref="TPIPackageClass"/> which will be saved.</param>
        /// <param name="FileLoc">Path to the target file.</param>
        public static void Save(TPIPackageClass TPI, string FileLoc) => File.WriteAllBytes(FileLoc, Save(TPI).ToArray());

        /// <summary>
        /// Saves the specified <see cref="TPIPackageClass"/> into a <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="Dat">Instance of <see cref="TPIPackageClass"/> which will be saved.</param>
        /// <returns>An instance of <see cref="MemoryStream"/> saved into Renegade-readable format.</returns>
        public static MemoryStream Save(TPIPackageClass TPI)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write("DAEH"); //Starting with HEAD because why not.
                    ms.Write(BitConverter.GetBytes(8)); //Length of HEAD.
                    for (int i = 6; i >= 0; i -= 2)
                        ms.Write((byte)Convert.ToInt32(TPI.PackageID.Substring(i, 2), 16)); //Writing CRC.
                    ms.Write(BitConverter.GetBytes(TPI.FileCount));

                    ms.Write("ATAD"); //DATA now.
                    string Data = string.Empty;

                    Data += Expressions.Flatten(BitConverter.GetBytes((ushort)TPI.PackageName.Length)); //Write package name length + content.
                    Data += TPI.PackageName;
                    Data += Expressions.Flatten(BitConverter.GetBytes((ushort)TPI.PackageVer.Length)); //Write package version length + content.
                    Data += TPI.PackageVer;
                    Data += Expressions.Flatten(BitConverter.GetBytes((ushort)TPI.PackageOwner.Length)); //Write package owner length + content.
                    Data += TPI.PackageOwner;
                    Data += Expressions.Flatten(BitConverter.GetBytes((int)TPI.Type));

                    ms.Write(BitConverter.GetBytes(Data.Length));
                    ms.Write(Data);

                    foreach (TTFileClass TTFile in TPI.Files)
                    {
                        ms.Write("ELIF");
                        string File = string.Empty;
                        for (int i = 6; i >= 0; i -= 2)
                            File += (char)((byte)Convert.ToInt32(TTFile.CRC.Substring(i, 2), 16)); //Writing CRC.

                        File += Expressions.Flatten(BitConverter.GetBytes(TTFile.FileSize));
                        File += Expressions.Flatten(BitConverter.GetBytes(TTFile.OriginalNameLength));
                        File += TTFile.FileName;

                        ms.Write(BitConverter.GetBytes(File.Length));
                        ms.Write(File);
                    }

                    MemoryStream Str = new MemoryStream();
                    ms.WriteTo(Str);
                    return Str;
                }
            }
            catch(Exception ex)
            {
                throw new TPIParserException("Failed to save TPI.", ex); //Wrapping the exception to our TPIParserException class.
            }
        }

        /// <summary>
        /// Parses the TPI data from a local file.
        /// </summary>
        /// <param name="Location">Path to the TPI file.</param>
        /// <returns>Parsed data of TPI as <see cref="TPIPackageClass"/>.</returns>
        public static TPIPackageClass FromFile(string Location)
        {
            if (!File.Exists(Location))
                throw new FileNotFoundException("TPI file could not be found."); //Boink

            byte[] TPI = File.ReadAllBytes(Location); //Read whole TPI file.
            return FromBytes(TPI); //Fetch the data from another call to prevent mess.
        }

        /// <summary>
        /// Parses the TPI from array of bytes.
        /// </summary>
        /// <param name="Data">Bytes of the TPI file.</param>
        /// <returns>Parsed data of TPI as <see cref="TPIPackageClass"/></returns>
        public static TPIPackageClass FromBytes(byte[] Data)
        {
            byte[] TPI = Data; //Read whole TPI file.
            return FromStream(new MemoryStream(TPI)); //Fetch the data from another call to prevent mess.
        }

        /// <summary>
        /// Parses the TPI from specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="Stream">Instance of <see cref="MemoryStream"/> containing the TPI file data.</param>
        /// <returns>Parsed data of TPI as <see cref="TPIPackageClass"/>.</returns>
        public static TPIPackageClass FromStream(MemoryStream Stream)
        {
            using (Stream Bytes = new MemoryStream(Stream.ToArray()))
            {
                try
                {
                    TPIPackageClass Pack = Expressions.CreateTPIPackage();
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

                            //Next 4 bytes are indicating total files used by this TPI. But it is read-only because it uses the ones
                            //actually parsed by the parser.

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
                        else if (Header == (uint)ChunkType.File)
                        {
                            TTFileClass File = Expressions.CreateTTFile();
                            int Next = Expressions.GetNumber(Expressions.GetBytes(Bytes, 0, 4));
                            File.CRC = Expressions.GetCRCID(Expressions.GetBytes(Bytes, 0, 4));
                            File.FileSize = Expressions.GetUNumber(Expressions.GetBytes(Bytes, 0, 4));

                            int namelen = Expressions.GetUNumber16(Expressions.GetBytes(Bytes, 0, 2));
                            File.FileName = Expressions.GetText(Bytes, 0, namelen);

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
                catch(Exception ex)
                {
                    throw new TPIParserException("Failed to parse TPI.", ex); //Wrapping the exception to our TPIParserException class.
                }
            }
        }
    }
}
