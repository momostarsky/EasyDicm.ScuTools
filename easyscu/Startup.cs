using System.IO;
using log4net;
using log4net.Config;
using log4net.Repository;

namespace easyscu
{
    public class Startup
    {
        private readonly ILoggerRepository LoggerRepository;
     
        private Startup()
        {
            LoggerRepository = LogManager.CreateRepository("easyDicmLog");
            XmlConfigurator.Configure(LoggerRepository, new FileInfo("log4net.config"));
        }
        
        public  static  readonly  Startup  Intance =new Startup();

        public string LogName => LoggerRepository.Name; 
        
    }
}