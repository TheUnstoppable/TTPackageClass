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
    /// Data containing the TTFS information.
    /// </summary>
    public struct TTFSDataClass
    {
        /// <summary>
        /// TPI packages installed in this TTFS.
        /// </summary>
        public List<TPIPackageClass> Packages;

        /// <summary>
        /// Total packages installed in this TTFS.
        /// </summary>
        public int PackageCount { get { return Packages.Count; } }
    }
}
