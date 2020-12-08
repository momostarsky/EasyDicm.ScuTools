using System;
using log4net;

namespace easyscu
{
    public abstract class ScuProc<T> where T : ScuOptions
    {
        protected ScuProc(T option)
        {
            Log = LogManager.GetLogger(Startup.Intance.LogName, this.GetType());
            Opt = option;
        }

        public virtual void Start()
        {
            Log.Info($"Started With Options :{Opt.OptionText()}");
        }

        public T Opt { get; }

        public ILog Log { get; }
    }
}