using System;
using System.IO;
using Dicom;
using Dicom.Imaging.Codec;
using Dicom.IO;
using Dicom.Network;
using log4net;
using log4net.Config;
using log4net.Repository;

namespace easyscu
{
    public class Startup
    {
     //   private readonly ILoggerRepository LoggerRepository;
        private readonly ILoggerRepository dicomNetworkLoggerRepository;
        
        
     
        private Startup()
        {
          //  LoggerRepository = LogManager.CreateRepository("Dicom.Log.Log4NetManager");
            dicomNetworkLoggerRepository = LogManager.CreateRepository("Dicom.Network");
           
        }
        
        public  static  readonly  Startup  Intance =new Startup();

      //  public string LogName => LoggerRepository.Name;

        public String DicomNetworkName => dicomNetworkLoggerRepository.Name;

        public void Start()
        {
        //    XmlConfigurator.Configure(LoggerRepository, new FileInfo("log4net.config"));
            XmlConfigurator.Configure(dicomNetworkLoggerRepository, new FileInfo("log4net.config"));
            DicomDictionary.EnsureDefaultDictionariesLoaded();
            Dicom.Log.Log4NetManager.SetImplementation(ScuLogManager.Instance);
            Dicom.IO.IOManager.SetImplementation(DesktopIOManager.Instance);
            Dicom.Network.NetworkManager.SetImplementation(DesktopNetworkManager .Instance); // if you want to run dicom services
          //  Dicom.Imaging.Codec.TranscoderManager.SetImplementation( Efferent.Native.Codec.NativeTranscoderManager.Instance); // if you want to run dicom services
        }

        public void Stop()
        {
            log4net.LogManager.Shutdown();
        }
    }
}