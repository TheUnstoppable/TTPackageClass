/*  
    TT Package / TPI File Parser
    Copyright (c) 2020 Unstoppable

    You can redistribute or modify this code under GNU General Public License v3.0.
    The permission given to run this code in a closed source project modified.
    But, you have to release the source code using this library must be released.
    Or, you have to add original owner's name into your project.
*/


using System.Collections.Generic;

namespace TTPackageClass
{
    /// <summary>
    /// Contains parsed TPI datas.
    /// </summary>
    public struct TPIPackageClass
    {
        /// <summary>
        /// Package CRC.
        /// </summary>
        public string PackageID { get; internal set; }

        /// <summary>
        /// Package/Map name.
        /// </summary>
        public string PackageName { get; internal set; }

        /// <summary>
        /// Package/Map version.
        /// </summary>
        public string PackageVer { get; internal set; }

        /// <summary>
        /// Package/Map owner.
        /// </summary>
        public string PackageOwner { get; internal set; }

        /// <summary>
        /// Privacy type of this package.
        /// </summary>
        public PackageType Type { get; internal set; }

        /// <summary>
        /// Files used by this package.
        /// </summary>
        public List<TTFileClass> Files { get; internal set; }

        /// <summary>
        /// File count.
        /// </summary>
        public int FileCount { get => Files.Count; }
    }
}
