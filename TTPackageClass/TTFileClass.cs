/*  
    TT Package / TPI File Parser
    Copyright (c) 2020 Unstoppable

    You can redistribute or modify this code under GNU General Public License v3.0.
    The permission given to run this code in a closed source project modified.
    But, you have to release the source code using this library must be released.
    Or, you have to add original owner's name into your project.
*/

namespace TTPackageClass
{
    /// <summary>
    /// File data which is used by TPI.
    /// </summary>
    public struct TTFileClass
    {
        /// <summary>
        /// File name.
        /// </summary>
        public string FileName;

        /// <summary>
        /// File CRC.
        /// </summary>
        public string CRC;

        /// <summary>
        /// File size.
        /// </summary>
        public uint FileSize;

        /// <summary>
        /// Full name of the file with CRC.
        /// </summary>
        public string FullName { get => CRC + "." + FileName;  }

        /// <summary>
        /// Length of the full name.
        /// </summary>
        public int FullNameLength { get => FullName.Length; }

        /// <summary>
        /// Length of the original file name.
        /// </summary>
        public ushort OriginalNameLength { get => (ushort)FileName.Length; }
    }
}
