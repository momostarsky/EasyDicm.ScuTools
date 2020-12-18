using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using CommandLine;

namespace easyscu
{

    public abstract class MyOptions
    {

        protected MyOptions()
        {
            
        }
        public abstract string OptionText();
    }

    /// <summary>
    /// CXXXScu 命令选项
    /// </summary>
    public abstract class  ScuOptions :MyOptions
    {

        protected ScuOptions() : base()
        {
            
        }
        
        [Option("ae", Required = true, HelpText = "SCP Service Provider  AETitle")]
        public string RemoteAE
        {
            get;
            set;
        }

        [Option("port", Required = true, HelpText = "SCP Service Provider    PortNum")]
        public int Port
        {
            get;
            set;
        }
        
        [Option("host", Required = true, HelpText = "SCP Service Provider    Ip Or Host Name")]
        public string Host 
        {
            get;
            set;
        }

        [Option("myae", Required = true, HelpText = "LocalAE Title")]
        public string MyAE
        {
            get;
            set;
        }

        public override string   OptionText()
        {
           var sb=new StringBuilder(1024);
           sb.AppendFormat("{0}={1} ", "MyAE", MyAE);
           sb.AppendFormat("{0}={1} ", "RemoteAE", RemoteAE);
           sb.AppendFormat("{0}={1} ", "Port", Port);
           sb.AppendFormat("{0}={1} ", "Host", Host);
           return sb.ToString();
        }
 
    }
    
    [Verb("cstore", false, HelpText = "CStoreScu")]
    public class StoreOptions : ScuOptions
    {

        public StoreOptions() : base()
        {
            
        }
        
        [Option("src", Required = true, HelpText = "Dicom Files Folder To Be Send !")]
        public string DicomSrc
        {
            get;
            set;
        }

        [Option("batch", Required = true, Min  = 1, Default = 100  ,HelpText = "How much  DicomFiles Send in one Request !")]
         
        public int  BatchSize
        {
            get;
            set;
        }
        
  

        public override string OptionText()
        {
            return $"{base.OptionText()} DicomSrc={DicomSrc}";
        }
    }

    [Verb("cecho", false, HelpText = "CEchoScu")]
    public class EchoOptons : ScuOptions
    {
        public EchoOptons() : base()
        {
            
        }
    }
    [Verb("rsakey", false, HelpText = "RSAKey Generator")]
    public class RsaOptions : MyOptions
    {
        public RsaOptions() : base()
        {
            
        }
        [Option("Days", Required = false ,Default = 360, HelpText = "how much days  validated !")]
        public int Days
        {
            get;
            set;
        }
        
       
        
        
        [Option("appid", Required = true, HelpText = "application unique identifier!")]
        public String  AppId 
        {
            get;
            set;
        }

        [Option("appname", Required = true, HelpText = "application  unique name")]
        public string AppName
        {
            get;
            set;
        }
        public override string OptionText()
        {
            var sb=new StringBuilder(1024);
            sb.AppendFormat("{0}={1} ", "AppId", AppId);
            sb.AppendFormat("{0}={1} ", "AppName", AppName);
            sb.AppendFormat("{0}={1} ", "Days", Days); 
            return sb.ToString();
        }
    }
}