using System;
using System.IO;

using LZ4Sharp;

namespace DS.Unpacker
{
    class LZ4C
    {
        //Decompress LZ4C data
        public static byte[] iDecompress(byte[] lpSrcBuffer, byte[] lpDstBuffer, int dwZSize)
        {
            using (MemoryStream TMemoryStream = new MemoryStream(lpSrcBuffer))
            {
                int dwHeaderSize = 8;
                int dwSignature = TMemoryStream.ReadInt32(); //LZ4C
                int dwSize = TMemoryStream.ReadInt32();

                byte[] lpTempBuffer = new Byte[dwZSize - dwHeaderSize];
                Array.Copy(lpSrcBuffer, dwHeaderSize, lpTempBuffer, 0, dwZSize - dwHeaderSize);

                LZ4Decompressor64 TLZ4Decompressor64 = new LZ4Decompressor64();
                TLZ4Decompressor64.Decompress(lpTempBuffer, lpDstBuffer);
            }
            return lpDstBuffer;
        }
    }
}
