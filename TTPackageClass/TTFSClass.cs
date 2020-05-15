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
using System.Net;

namespace TTPackageClass
{
    /// <summary>
    /// The class containing all actions can be done with <see cref="TTFSDataClass"/>.
    /// </summary>
    public static class TTFSClass
    {
        /// <summary>
        /// Saves the specified <see cref="TTFSDataClass"/> into a file.
        /// </summary>
        /// <param name="Dat">Instance of <see cref="TTFSDataClass"/> which will be saved.</param>
        /// <param name="FileLoc">Path to the target file.</param>
        /// <exception cref="TTFSParserException">Occurs when a general saving exception thrown wrapping the original exception data.</exception>
        public static void Save(TTFSDataClass Dat, string FileLoc) => File.WriteAllBytes(FileLoc, Save(Dat).ToArray());

        /// <summary>
        /// Saves the specified <see cref="TTFSDataClass"/> into a <see cref="MemoryStream"/>
        /// </summary>
        /// <param name="Dat">Instance of <see cref="TTFSDataClass"/> which will be saved.</param>
        /// <returns>An instance of <see cref="MemoryStream"/> saved into Renegade-readable format.</returns>
        /// <exception cref="TTFSParserException">Occurs when a general saving exception thrown wrapping the original exception data.</exception>
        public static MemoryStream Save(TTFSDataClass Dat)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (StreamWriter Writer = new StreamWriter(ms))
                    {
                        foreach (TPIPackageClass TPI in Dat.Packages)
                        {
                            using (var Stream = TPIClass.Save(TPI))
                            {
                                Writer.Write("GKCP");
                                Writer.Write(Expressions.Flatten(BitConverter.GetBytes((uint)Stream.Length)));
                                Stream.CopyTo(ms);
                            }
                        }
                    }

                    return ms;
                }
            }
            catch(Exception ex)
            {
                if (ex is TPIParserException)
                {
                    throw ex;
                }
                else
                {
                    throw new TTFSParserException("Failed to save TTFS Data.", ex); //Wrapping the exception to our TTFSParserException class.
                }
            }
        }

        /// <summary>
        /// Parses the TTFS Data from specified remote host.
        /// </summary>
        /// <param name="Link">URL of remote TTFS root.</param>
        /// <returns>Parsed data of remote TTFS host as <see cref="TTFSDataClass"/>.</returns>
        /// <exception cref="TTFSParserException">Occurs when a general parsing exception thrown wrapping the original exception data.</exception>
        public static TTFSDataClass FromTTFS(Uri Link)
        {
            using (WebClient wc = new WebClient())
            {
                byte[] DAT = wc.DownloadData($"{Link.OriginalString}/packages.dat"); //Downloading a copy of "packages.dat" into memory.
                return FromBytes(DAT); //Fetch the data from another call to prevent mess.
            }
        }

        /// <summary>
        /// Parses the TTFS data from a local file.
        /// </summary>
        /// <param name="Location">Path to the TTFS data file "packages.dat".</param>
        /// <returns>Parsed data of TTFS data as <see cref="TTFSDataClass"/>.</returns>
        /// <exception cref="TTFSParserException">Occurs when a general parsing exception thrown wrapping the original exception data.</exception>
        public static TTFSDataClass FromFile(string Location)
        {
            if (!File.Exists(Location))
                throw new FileNotFoundException("DAT file could not be found."); //Boink

            byte[] DAT = File.ReadAllBytes(Location); //Read whole TPI file.
            return FromBytes(DAT); //Fetch the data from another call to prevent mess.
        }

        /// <summary>
        /// Parses the TTFS data from array of bytes.
        /// </summary>
        /// <param name="Data">Bytes of the TTFS data.</param>
        /// <returns>Parsed data of TTFS data as <see cref="TTFSDataClass"/>.</returns>
        /// <exception cref="TTFSParserException">Occurs when a general parsing exception thrown wrapping the original exception data.</exception>
        public static TTFSDataClass FromBytes(byte[] Data)
        {
            byte[] DAT = Data; //Read whole DAT file.
            return FromStream(new MemoryStream(DAT)); //Fetch the data from another call to prevent mess.
        }

        /// <summary>
        /// Parses the TTFS data from specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="Stream">Instance of <see cref="MemoryStream"/> containing the TTFS data.</param>
        /// <returns>Parsed data of TTFS data as <see cref="TTFSDataClass"/>.</returns>
        /// <exception cref="TTFSParserException">Occurs when a general parsing exception thrown wrapping the original exception data.</exception>
        public static TTFSDataClass FromStream(MemoryStream Stream)
        {
            try
            {
                TTFSDataClass Class = new TTFSDataClass();

                using (Stream Bytes = new MemoryStream(Stream.ToArray()))
                {
                    while (TPIClass.ConditionsOK(Bytes))
                    {
                        uint Header = Expressions.GetUNumber(Expressions.GetBytes(Bytes, 0, 4));

                        if (Header == (uint)ChunkType.Package) //Doc: GKCP<Weird 4 Bytes (Length of TPI)><Content>
                        {
                            //Processing the GKCP. Reading first 4 bytes to find out length.
                            //Every PCKG starts with length, but the length string is terminated with "€" everytime. And it's HEX value is 20AC.
                            //So we're ignoring it as it still works without it and it messes up the stuff.

                            var ABytes = Expressions.GetBytes(Bytes, 0, 4);
                            ABytes[3] = (byte)0x00;
                            uint Next = Expressions.GetUNumber(ABytes);

                            //The thing we just read must be a whole TPI now.
                            byte[] Data = Expressions.GetBytes(Bytes, 0, (int)Next);
                            try
                            {
                                Class.Packages.Add(TPIClass.FromBytes(Data)); //Parsing the TPI and adding output to enumeration.
                            }
                            catch (Exception Ex)
                            {
                                //Uh-oh! TPI parser failed.
                                throw new FormatException("Failed to parse TTFS Package Data.", Ex);
                            }
                        }
                        else //We don't expect to receive something else from this. Throwing an exception...
                        {
                            throw new FormatException("Received an invalid header from file.");
                        }
                    }
                }

                //Return when everything is good to go.
                return Class;
            }
            catch(Exception ex)
            {
                if(ex is TPIParserException)
                {
                    throw ex;
                }
                else
                {
                    throw new TTFSParserException("Failed to parse TTFS Data.", ex); //Wrapping the exception to our TTFSParserException class.
                }
            }
        }
    }
}
