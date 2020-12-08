namespace easyscu
{
    public class CStoreProc :ScuProc<StoreOptions>
    {

        public CStoreProc(StoreOptions opt) : base(opt)
        {
            
        }

        public override void Start()
        {
            base.Start();
            Log.Info("CStoreScu Executed Over ");
        }
    }
}