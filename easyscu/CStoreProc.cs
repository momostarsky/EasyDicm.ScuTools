using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using Dicom.Network;
using DicomClient = Dicom.Network.Client.DicomClient;


namespace easyscu
{
    /// <summary>
    /// Command :   cstore --ae CONQUESTSRV1 --port 56781 --host 192.168.1.155 --myae JPAIBox  --src /home/dhz/dcmdata
    /// </summary>
    public class CStoreProc : ScuProc<StoreOptions>
    {
        public CStoreProc(StoreOptions opt) : base(opt)
        {
        }

        private async Task SendSubSize(string[] dicomFiles)
        {
            var client = new DicomClient(Opt.Host, Opt.Port, false, Opt.MyAE, Opt.RemoteAE);
            client.NegotiateAsyncOps();


            int end = dicomFiles.Length;
            for (int i = 0; i < end; i++)
            {
                Log.Info(dicomFiles[i]);
                var request = new DicomCStoreRequest(dicomFiles[i]);

                request.OnResponseReceived += (req, response) =>
                    Console.WriteLine("C-Store Response Received, Status: " + response.Status);

                await client.AddRequestAsync(request);
            }

            await client.SendAsync();
        }

        public override async Task Start()
        {
            String[] ie = System.IO.Directory.GetFiles(Opt.DicomSrc, "*", SearchOption.AllDirectories);


            int mg = ie.Length / Opt.BatchSize;
            int lf = ie.Length % Opt.BatchSize;

            if (lf > 0)
            {
                mg += 1;
            }
            var   grups = new List<string[]>(mg);
            for (int i = 0; i < mg; i++)
            {
                String[] arr = null;
                if (lf > 0 && i == mg - 1)
                {
                    arr = new string[lf];
                }
                else
                {
                    arr = new string[Opt.BatchSize];
                }
                Array.Copy(ie, i * Opt.BatchSize, arr, 0, arr.Length);
                grups.Add( arr);
            }
            foreach (var grup in grups)
            {
                await SendSubSize(grup);
            }
            
 
        }
    }
}