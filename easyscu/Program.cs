using System;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using Dicom;
using Dicom.Imaging;
using Dicom.Imaging.Codec;
using Dicom.IO;
using Dicom.Log;
using Dicom.Network;
using log4net;
using log4net.Config;
using DicomClient = Dicom.Network.Client.DicomClient;
using LogManager = log4net.LogManager;

namespace easyscu
{
   internal static class Program
    {
        static int 
            CEchoScu(EchoOptons opt)
        {
            CEchoProc proc = new CEchoProc(opt);

            var t=  proc.Start();
            t.Wait();
            return 0;
        }
        
        static int 
            CKeyGen(RsaOptions opt)
        {
            KeyGen proc = new KeyGen(opt);

            var t=  proc.Start();
            t.Wait();
            return 0;
        }


        static int  CStoreScu(StoreOptions opt)
        {
            CStoreProc proc = new CStoreProc(opt);


            var t=  proc.Start();
            t.Wait();
            return 0;
        }
 

        static async Task Main(string[] args)
        {
            Startup.Intance.Start();


           
            Parser.Default.ParseArguments<EchoOptons, StoreOptions,RsaOptions>(args)
                .MapResult(
                    (EchoOptons opt) => CEchoScu(opt),
                    (StoreOptions opt) => CStoreScu(opt),
                    (RsaOptions opt) => CKeyGen(opt),
                     _ => 1 
                );
            //   
            // // This will shutdown the log4net system
            // 
            Startup.Intance.Stop();
        }
    }
}