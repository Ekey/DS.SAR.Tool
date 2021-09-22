using System;

namespace DS.Unpacker
{
    class DS_Hash
    {
        public static UInt32 iGetHash(String lpString, UInt32 dwHash)
        {
            Int32 dwLength = lpString.Length;
            for (Int32 i = 0; i < dwLength; i++)
            {
                dwHash = lpString[i] + 33 * dwHash;
            }
            return dwHash;
        }
    }
}