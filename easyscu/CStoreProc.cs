using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace easyscu
{
    /// <summary>
    /// Command :   cstore --ae CONQUESTSRV1 --port 56781 --host 192.168.1.155 --myae JPAIBox  --src /home/dhz/dcmdata
    /// </summary>
    public class CStoreProc :ScuProc<StoreOptions>
    {

        public CStoreProc(StoreOptions opt) : base(opt)
        {
            
        }

        public override void Start()
        {
            
            var r = run();
            r.Wait(); 
        }


        async Task run()
        {
            var ie = System.IO.Directory.GetFiles(Opt.DicomSrc, "*", SearchOption.AllDirectories);
            var sz = ie.Length;
            var step = Opt.BatchSize;
            var left =  sz % step == 0 ? 0 : 1;
            var size = sz / step  ;
            ParallelOptions maxp = new ParallelOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount * 2,
            };
                
            Parallel.For(0, size, maxp,idx =>
            {
                int sz = idx * step;
                int end = sz + step;
                for (int i = sz; i < end; i++)
                {
                    Console.WriteLine($"Batch{ idx }:{ie[i]}");
                }
            });
            
            Parallel.For(size * step, sz , maxp,idx =>
            {
                Console.WriteLine($"Batch  :{ie[idx]}"); 
            });
        }
    }
}