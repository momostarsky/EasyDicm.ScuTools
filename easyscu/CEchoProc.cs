using System;
using System.Threading.Tasks;
using Dicom.Network;
using DicomClient = Dicom.Network.Client.DicomClient;

namespace easyscu
{
    /// <summary>
    /// Command :   cecho --ae CONQUESTSRV1 --port 56781 --host 192.168.1.155 --myae JPAIBox
    /// </summary>
    public class CEchoProc : ScuProc<EchoOptons>
    {
        public CEchoProc(EchoOptons opt) : base(opt)
        {
        }
 

        public override void Start()
        {
            var r = run();
            r.Wait(3000); 
        }

        private async Task  run()
        {
            var dicomReq = new DicomCEchoRequest
            {
                 OnResponseReceived = (request, response) => { Log.Info(response.Status); }
            };

            var client = new DicomClient(Opt.Host, Opt.Port, false, Opt.MyAE, Opt.RemoteAE);
            client.NegotiateAsyncOps(); 
            await client.AddRequestAsync(dicomReq); 
            await client.SendAsync(); 
        }
 
    }
}