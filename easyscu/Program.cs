using System;
using System.IO;
using CommandLine;
using Dicom.Imaging;
using Dicom.Imaging.Codec;
using Dicom.IO;
using Dicom.Log;
using Dicom.Network;
using log4net;
using log4net.Config;

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
            // 设置日志连接
            Dicom.Log.Log4NetManager.SetImplementation(LogMgr.Instance); 
            Dicom.IO.IOManager.SetImplementation(DesktopIOManager.Instance);
            Dicom.Network.NetworkManager.SetImplementation(DesktopNetworkManager .Instance); // if you want to run dicom services


            Parser.Default.ParseArguments<EchoOptons, StoreOptions>(args)
                .MapResult(
                    (EchoOptons opt) => CEchoScu(opt),
                    (StoreOptions opt) => CStoreScu(opt),
                    _ => 1
                );
        }
    }
}