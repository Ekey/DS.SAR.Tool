using System;
using System.IO;
using System.Reflection;

namespace DS.Unpacker
{
    class Program
    {
        static void Main(String[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Demiurge Studios SAR Unpacker");
            Console.WriteLine("(c) 2021 Ekey (h4x0r) / v{0}\n", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Console.ResetColor();

            if (args.Length != 2)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    DS.Unpacker <m_File> <m_Directory>\n");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Example]");
                Console.WriteLine("    DS.Unpacker D:\\Android_Config.sar D:\\Unpacked\\Android_Config");
                Console.ResetColor();
                return;
            }

            if (!File.Exists("Zstandard.Net.dll") || !File.Exists("libzstd.dll"))
            {
                Utils.iSetError("[ERROR]: Unable to find ZSTD modules");
                return;
            }

            String m_Input = args[0];
            String m_Output = Utils.iCheckArgumentsPath(args[1]);

            if (!File.Exists(m_Input))
            {
                Utils.iSetError("[ERROR]: Input file -> " + m_Input + " <- does not exist!");
                return;
            }

            SarUnpack.iDoIt(m_Input, m_Output);
        }
    }
}
