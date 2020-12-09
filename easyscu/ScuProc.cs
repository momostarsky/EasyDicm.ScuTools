using System;
using System.Threading.Tasks;
using log4net;

namespace easyscu
{
    public abstract class ScuProc<T> where T : ScuOptions
    {
        protected ScuProc(T option)
        {
            Log = LogManager.GetLogger(Startup.Intance.DicomNetworkName, this.GetType());
            Opt = option;
        }

        public abstract   Task Start();
        

        protected T Opt { get; }

        protected ILog Log { get; }
    }
}