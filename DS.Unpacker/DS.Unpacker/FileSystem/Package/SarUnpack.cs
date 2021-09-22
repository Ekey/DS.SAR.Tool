using System;
using System.IO;
using System.Text;

namespace DS.Unpacker
{
    class SarUnpack
    {
        public static void iDoIt(String m_Archive, String m_DstFolder)
        {
            bool bUseDictionary = false;
            UInt32 dwEncryptionSeed = 0;

            FileStream TFileStream = new FileStream(m_Archive, FileMode.Open);

            Int32 dwMagic = TFileStream.ReadInt32(); // 55935 (7FDA0000)
            if (dwMagic != 0x0000DA7F)
            {
                Utils.iSetError("[ERROR]: Invalid magic of PKG archive file");
                return;
            }

            Int32 dwVersion = TFileStream.ReadInt32();
            Int64 dwArchiveSize = TFileStream.ReadInt64();
            Int64 dwTableOffset = TFileStream.ReadInt64();
            Int32 dwTotalFiles = TFileStream.ReadInt32();
            Int32 dwGameDirectory = TFileStream.ReadInt32();
            Int32 dwTableZSize = TFileStream.ReadInt32();
            Int16 wBuildVersionMinor = TFileStream.ReadInt16();
            Int16 wBuildVersionMajor = TFileStream.ReadInt16();
            Int32 dwBuildChangeList = TFileStream.ReadInt32();
            Int16 wDirectoryQueries = TFileStream.ReadInt16();
            Int32 bObfuscated = TFileStream.ReadByte();
            Int32 bPlatform = TFileStream.ReadByte();

            //Build encryption key
            String lpEncryptionKey = String.Format("{0}{1}", wBuildVersionMajor, dwBuildChangeList);

            //Reading entry table
            TFileStream.Seek(dwTableOffset, SeekOrigin.Begin);
            var lpSrcEntryTable = TFileStream.ReadBytes(dwTableZSize);

            Byte[] lpDictionary = null;
            UInt32 dwCompressionMagic = 0; //ZSTD, LZ4C or LZHAM (without magic)
            UInt32 dwEncryptionHashMain = 0x54007B47;
            UInt32 dwEncryptionHashScripts = 0xB29F8D49;

            //Decrypt entry table
            dwEncryptionSeed = DS_Hash.iGetHash(lpEncryptionKey.ToLower(), dwEncryptionHashMain);
            DS_Cipher.iDecryptData(lpSrcEntryTable, dwEncryptionSeed, dwTableZSize);

            //Decompress entry table
            var lpDstEntryTable = ZSTD.iDecompress(lpSrcEntryTable, dwTableZSize);

            using (var TMemoryReader = new MemoryStream(lpDstEntryTable))
            {
                for (Int32 i = 0; i < dwTotalFiles; i++)
                {
                    Int64 dwOffset = TMemoryReader.ReadInt64();
                    Int64 dwZSize = TMemoryReader.ReadInt64();
                    Int64 dwSize = TMemoryReader.ReadInt64();
                    Int64 dwTimeStamp = TMemoryReader.ReadInt64();
                    Int32 dwCRC32Pre = TMemoryReader.ReadInt32();
                    Int32 dwCRC32Post = TMemoryReader.ReadInt32();
                    Int32 dwFileNameLength = TMemoryReader.ReadInt32();
                    var lpFileNane = TMemoryReader.ReadBytes(dwFileNameLength);
                    Array.Resize(ref lpFileNane, dwFileNameLength - 1);
                    String m_FileName = Encoding.ASCII.GetString(lpFileNane);
                    String m_FullPath = m_DstFolder + @"\" + m_FileName;

                    Console.WriteLine("[UNPACKING]: {0}", m_FileName);

                    Utils.iCreateDirectory(m_FullPath);

                    TFileStream.Seek(dwOffset, SeekOrigin.Begin);
                    var lpSrcBuffer = TFileStream.ReadBytes((Int32)dwZSize);

                    String m_Extension = Path.GetExtension(m_FileName.ToLower());
                    String m_DictionaryFile = Path.GetFileName(m_FileName);

                    if (m_Extension == ".lbc")
                    {
                        dwEncryptionSeed = DS_Hash.iGetHash(Path.GetFileNameWithoutExtension(m_FileName.ToLower()), dwEncryptionHashScripts);
                        DS_Cipher.iDecryptData(lpSrcBuffer, dwEncryptionSeed, (Int32)dwZSize);
                    }
                    else if (m_Extension == ".json" || m_Extension == ".dat" || m_Extension == ".pem")
                    {
                        dwEncryptionSeed = DS_Hash.iGetHash(m_FileName.ToLower(), dwEncryptionHashMain);
                        DS_Cipher.iDecryptData(lpSrcBuffer, dwEncryptionSeed, (Int32)dwZSize);
                    }

                    if (dwZSize <= 40)
                    {
                        File.WriteAllBytes(m_FullPath, lpSrcBuffer);
                    }
                    else if (dwZSize != dwSize && bUseDictionary == false)
                    {
                        var lpDstBuffer = ZSTD.iDecompress(lpSrcBuffer, (Int32)dwZSize);
                        if (m_DictionaryFile.ToLower() == "pkgcdict_pc.dat" || m_DictionaryFile.ToLower() == "pkgcdict_ios.dat" || m_DictionaryFile.ToLower() == "pkgcdict_android.dat")
                        {
                            Array.Resize(ref lpDictionary, lpDstBuffer.Length);
                            Array.Copy(lpDstBuffer, 0, lpDictionary, 0, lpDstBuffer.Length);
                            bUseDictionary = true;
                        }

                        File.WriteAllBytes(m_FullPath, lpDstBuffer);
                    }
                    else
                    {
                        dwCompressionMagic = BitConverter.ToUInt32(lpSrcBuffer, 0);
                        dwSize = BitConverter.ToUInt32(lpSrcBuffer, 4);

                        if (dwCompressionMagic == 0x4454535A) //ZSTD
                        {
                            if (bUseDictionary == true)
                            {
                                var lpDstBuffer = ZSTD.iDecompressDictionary(lpSrcBuffer, lpDictionary, (int)dwZSize);
                                File.WriteAllBytes(m_FullPath, lpDstBuffer);
                            }
                            else
                            {
                                var lpDstBuffer = ZSTD.iDecompress(lpSrcBuffer, (Int32)dwZSize);
                                File.WriteAllBytes(m_FullPath, lpDstBuffer);
                            }
                        }
                        else if (dwCompressionMagic == 0x43345A4C) //LZ4C
                        {
                            Byte[] lpDstBuffer = new Byte[dwSize];
                            lpDstBuffer = LZ4C.iDecompress(lpSrcBuffer, lpDstBuffer, (Int32)dwZSize);
                            File.WriteAllBytes(m_FullPath, lpDstBuffer);
                         }
                        else
                        {
                            File.WriteAllBytes(m_FullPath, lpSrcBuffer);
                        }
                    }
                }
            }
        }
    }
}
