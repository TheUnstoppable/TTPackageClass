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
    /// The chunk header data of all TTFS-used chunks.
    /// </summary>
    public enum ChunkType
    {
        Head = 0x48454144,
        Data = 0x44415441,
        File = 0x46494C45,
        Package = 0x50434B47
    }
}
