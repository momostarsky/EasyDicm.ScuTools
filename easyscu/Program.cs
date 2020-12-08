using System;
using CommandLine;
using log4net;

namespace easyscu
{
    class Program
    {
        static int CEchoScu(EchoOptons opt)
        {
            CEchoProc proc = new CEchoProc(opt);
            int extCode = 0;
            try
            {
                proc.Start();
            }
            catch
            {
                extCode = 1;
            }

            return extCode;
        }

        static int CStoreScu(StoreOptions opt)
        {
            CStoreProc proc = new CStoreProc(opt);
            int extCode = 0;
            try
            {
                proc.Start();
            }
            catch
            {
                extCode = 1;
            }

            return extCode;
        }

        static void Main(string[] args)
        {
        
            Parser.Default.ParseArguments<EchoOptons, StoreOptions>(args)
                .MapResult(
                    (EchoOptons opt) => CEchoScu(opt),
                    (StoreOptions opt) => CStoreScu(opt),
                    _ => 1
                ); 
        }
    }
}