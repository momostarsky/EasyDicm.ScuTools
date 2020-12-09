using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dicom.Network;


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

        public override void Start()
        {
            var t = run();
            t.Wait();
        }


        async Task run()
        {
            Log.Info($"Start Process {Opt.DicomSrc}");
            var ie = System.IO.Directory.GetFiles(Opt.DicomSrc, "*", SearchOption.AllDirectories);
            var sz = ie.Length;
            var step = Opt.BatchSize;
            var left = sz % step == 0 ? 0 : 1;
            var size = sz / step;
            ParallelOptions maxp = new ParallelOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount * 2,
            };


            Parallel.For(0, size, maxp, idx =>
            {
                int sz = idx * step;
                int end = sz + step;
                var client = new DicomClient();
                client.NegotiateAsyncOps();
                for (int i = sz; i < end; i++)
                {
                    Log.Info(ie[i]);
                    client.AddRequest(new DicomCStoreRequest(ie[i]));
                }

                client.Send(Opt.Host, Opt.Port, false, Opt.MyAE, Opt.RemoteAE);
            });

            //
            Parallel.For(size * step, sz, maxp, async idx =>
            {
                var client = new DicomClient();
                client.NegotiateAsyncOps();
                for (int i = sz; i < sz; i++)
                {
                    Log.Info(ie[i]);
                    client.AddRequest(new DicomCStoreRequest(ie[i]));
                }

                client.Send(Opt.Host, Opt.Port, false, Opt.MyAE, Opt.RemoteAE);
            });
        }
    }
}