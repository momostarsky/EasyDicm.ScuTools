using System;
using Dicom.Log;

namespace easyscu
{
    class ScuLog : Dicom.Log.Logger
    {
        private readonly log4net.ILog logger;

        public ScuLog(String name)
        {
            logger = log4net.LogManager.GetLogger(name, typeof(ScuLog));
        }

        
        public override void Log(LogLevel level, string msg, params object[] args)
        {
            var ordinalFormattedMessage = NameFormatToPositionalFormat(msg);

            switch (level)
            {
                case LogLevel.Debug:
                    this.logger.DebugFormat(ordinalFormattedMessage, args);
                    break;
                case LogLevel.Info:
                    this.logger.InfoFormat(ordinalFormattedMessage, args);
                    break;
                case LogLevel.Warning:
                    this.logger.WarnFormat(ordinalFormattedMessage, args);
                    break;
                case LogLevel.Error:
                    this.logger.ErrorFormat(ordinalFormattedMessage, args);
                    break;
                case LogLevel.Fatal:
                    this.logger.FatalFormat(ordinalFormattedMessage, args);
                    break;
                default:
                    this.logger.InfoFormat(ordinalFormattedMessage, args);
                    break;
            }
     
        }
    }

    public class ScuLogManager : LogManager
    {
        protected override Logger GetLoggerImpl(string name)
        {
            return new ScuLog(name);
        }
        
        public static readonly   ScuLogManager  Instance =new ScuLogManager();
    }
}