using System;
using System.Threading.Tasks;
using Dicom.Network;
using DicomClient = Dicom.Network.Client.DicomClient;

namespace easyscu
{
    
     
    /// <summary>
    /// Usage :
    /// cecho --ae CONQUESTSRV1 --port 5678 --host 192.168.1.155 --myae EasySCU
    /// </summary>
    public class CEchoProc : ScuProc<EchoOptons>
    {
        public CEchoProc(EchoOptons opt) : base(opt)
        {
        }


        public override async Task Start()
        {

         
            var dicomReq = new DicomCEchoRequest
            {
                OnResponseReceived = (request, response) => { Log.Info(response.Status); }
            };
            var exts = DicmAppInfo.Instance.DicomExtendeds.Value;
            var client = new DicomClient(Opt.Host, Opt.Port, false, Opt.MyAE, Opt.RemoteAE);
            client.NegotiateAsyncOps();
           client.AdditionalExtendedNegotiations.AddRange( exts);
            await client.AddRequestAsync(dicomReq); 
            await client.SendAsync();
        }
    }
}