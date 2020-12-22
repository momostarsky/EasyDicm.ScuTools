using System;
using System.IO;
using Dicom;
using Dicom.Imaging;
using Dicom.Imaging.Codec;
using Dicom.IO;
using Dicom.Network;
using log4net;
using log4net.Config;
using log4net.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace easyscu
{
    public interface ICustomService
    {
    }

    public class CustomService : ICustomService
    {
        
        public CustomService(IConfigurationRoot config)
        {
            Console.WriteLine(config["test"]);
        }
    }

    public class Startup
    {
       
        //   private readonly ILoggerRepository LoggerRepository;
        private ILoggerRepository dicomNetworkLoggerRepository;


        private IConfigurationRoot configuration { get; set; }


        private IConfigurationRoot ReadFromAppSettings()
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT")}.json",
                    optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public static readonly Startup Intance = new Startup();
        private IServiceCollection services;
        private IServiceProvider   provider;

        public void Start()
        {
            services = new ServiceCollection();
          

            configuration = ReadFromAppSettings();
            dicomNetworkLoggerRepository = LogManager.CreateRepository("Dicom.Network");
            XmlConfigurator.Configure(dicomNetworkLoggerRepository, new FileInfo("log4net.config"));
            DicomDictionary.EnsureDefaultDictionariesLoaded();
            Dicom.Log.Log4NetManager.SetImplementation(ScuLogManager.Instance);
            ImageManager.SetImplementation(new ImageSharpImageManager());
            Dicom.IO.IOManager.SetImplementation(DesktopIOManager.Instance);
            Dicom.Network.NetworkManager.SetImplementation(DesktopNetworkManager
                .Instance); // if you want to run dicom services
           
            services.AddSingleton<IConfigurationRoot>(configuration);
            services.AddSingleton<ICustomService, CustomService>();
            
            provider = services.BuildServiceProvider();
          
        }


        public T Get<T>()
        {
            return provider.GetRequiredService<T>();
        }


        //  public string LogName => LoggerRepository.Name;

        public String DicomNetworkName => dicomNetworkLoggerRepository.Name;


        public void Stop()
        {
            log4net.LogManager.Shutdown();
        }
    }
}