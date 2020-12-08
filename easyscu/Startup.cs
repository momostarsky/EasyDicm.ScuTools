using System;
using System.IO;
using log4net;
using log4net.Config;
using log4net.Repository;

namespace easyscu
{
    public class Startup
    {
        private readonly ILoggerRepository LoggerRepository;
        private readonly ILoggerRepository dicomNetworkLoggerRepository;
        
        
     
        private Startup()
        {
            LoggerRepository = LogManager.CreateRepository("Dicom.Log.Log4NetManager");
            dicomNetworkLoggerRepository = LogManager.CreateRepository("Dicom.Network");
            XmlConfigurator.Configure(LoggerRepository, new FileInfo("log4net.config"));
            XmlConfigurator.Configure(dicomNetworkLoggerRepository, new FileInfo("log4net.config"));
        }
        
        public  static  readonly  Startup  Intance =new Startup();

        public string LogName => LoggerRepository.Name;

        public String DicomNetworkName => dicomNetworkLoggerRepository.Name;
    }
}