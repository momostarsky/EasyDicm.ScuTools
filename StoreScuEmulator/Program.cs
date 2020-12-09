using System;
using System.Threading.Tasks;
using Dicom.IO;
using Dicom.Log;
using Dicom.Network;

namespace StoreScuEmulator
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Dicom.Log.Log4NetManager.SetImplementation(Log4NetManager.Instance);  
           
            Dicom.IO.IOManager.SetImplementation(DesktopIOManager.Instance);
            Dicom.Network.NetworkManager.SetImplementation(DesktopNetworkManager .Instance); // if you want to run dicom services
             

 

            var t = Task.Factory.StartNew(async 
                () =>
            {
                var client = new Dicom.Network.Client.DicomClient("192.168.1.92", 11112, false, "JpAIBOX", "DicmQRSCP");
                client.NegotiateAsyncOps();
                await client.AddRequestAsync(
                    new DicomCStoreRequest("/home/dhz/dcmdata/CR.1.3.46.670589.26.902153.4.20190502.90700.714171.0"));
                await client.SendAsync();
            });
            t.Wait();
            
        }
    }
}