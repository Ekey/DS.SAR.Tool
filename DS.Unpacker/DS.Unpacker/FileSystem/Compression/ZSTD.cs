using System;
using System.IO;
using System.IO.Compression;

using Zstandard.Net;

namespace DS.Unpacker
{
    class ZSTD
    {
        //Decompress ZSTD data
        public static byte[] iDecompress(byte[] lpSrcBuffer, int dwZSize)
        {
            byte[] lpDstBuffer;
            using (MemoryStream TMemoryStream = new MemoryStream(lpSrcBuffer))
            {
                int dwHeaderSize = 8;
                int dwSignature = TMemoryStream.ReadInt32(); //ZSTD
                int dwSize = TMemoryStream.ReadInt32();

                byte[] lpTempBuffer = new Byte[dwZSize - dwHeaderSize];
                Array.Copy(lpSrcBuffer, dwHeaderSize, lpTempBuffer, 0, dwZSize - dwHeaderSize);

                using (var TMemoryStream2 = new MemoryStream(lpTempBuffer))
                using (var TZstandardStream = new ZstandardStream(TMemoryStream2, CompressionMode.Decompress))
                using (var lpTemp = new MemoryStream())
                {
                    TZstandardStream.CopyTo(lpTemp);
                    lpDstBuffer = lpTemp.ToArray();
                }
            }
            return lpDstBuffer;
        }

        //Decompress data with using dictionary
        public static byte[] iDecompressDictionary(byte[] lpSrcBuffer, byte[] lpDictionary, int dwZSize)
        {
            byte[] lpDstBuffer;
            using (MemoryStream TMemoryStream = new MemoryStream(lpSrcBuffer))
            {
                int dwHeaderSize = 8;
                int dwSignature = TMemoryStream.ReadInt32(); //ZSTD
                int dwSize = TMemoryStream.ReadInt32();

                byte[] lpTempBuffer = new Byte[dwZSize - dwHeaderSize];
                Array.Copy(lpSrcBuffer, dwHeaderSize, lpTempBuffer, 0, dwZSize - dwHeaderSize);

                using (var TMemoryStream2 = new MemoryStream(lpTempBuffer))
                using (var TZstandardStream = new ZstandardStream(TMemoryStream2, CompressionMode.Decompress))
                using (var TZstandardDictionary = new ZstandardDictionary(lpDictionary))
                using (var lpTemp = new MemoryStream())
                {
                    TZstandardStream.CompressionDictionary = TZstandardDictionary;
                    TZstandardStream.CopyTo(lpTemp);
                    lpDstBuffer = lpTemp.ToArray();
                }
            }
            return lpDstBuffer;
        }
    }
}
