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
    /// List of package types which is used by Package Edit and Renegade to set it's privacy and visibility.
    /// Secret: Packages that are always enabled and never displayed.
    /// Hidden: Packages that are hidden but can be shown.
    /// Normal: Packages that are always shown.
    /// </summary>
    public enum PackageType
    {
        Secret = 0,
        Hidden = 1,
        Normal = 2
    }
}
