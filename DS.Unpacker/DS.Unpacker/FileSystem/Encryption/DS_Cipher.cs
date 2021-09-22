using System;

namespace DS.Unpacker
{
    class DS_Cipher
    {
        public static void iDecryptData(Byte[] lpBuffer, UInt32 dwSeed, Int32 dwSize)
        {
            Int32 j = 0;
            var bTemp = 0;

            for (Int32 i = 0; i < dwSize; i++)
            {
                bTemp = (Byte)(j & 0x18);
                j += 8;
                lpBuffer[i] ^= (Byte)((dwSeed >> bTemp) + (101 * (i >> 2)));
            }
        }
    }
}
