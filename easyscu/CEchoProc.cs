namespace easyscu
{
    public class CEchoProc:ScuProc<EchoOptons>
    {
        public CEchoProc(EchoOptons opt) : base(opt)
        {
            
        }

        public override void Start()
        {
            base.Start();
            Log.Info("CEchoProcess Over!");
        }
    }
}