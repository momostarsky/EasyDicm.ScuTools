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
using LogManager = log4net.LogManager;

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
            Startup.Intance.Start();
            // var client = new DicomClient("127.0.0.1", 12345, false, "SCU", "ANY-SCP");
            // await client.AddRequestAsync(new DicomCStoreRequest(@"test.dcm"));
            // await client.SendAsync();
         
           
          

         

 
            Parser.Default.ParseArguments<EchoOptons, StoreOptions>(args)
                .MapResult(
                    (EchoOptons opt) => CEchoScu(opt),
                    (StoreOptions opt) => CStoreScu(opt),
                    _ => 1
                );
            //   
            // // This will shutdown the log4net system
            // 
            Startup.Intance.Stop();
        }
    }
}