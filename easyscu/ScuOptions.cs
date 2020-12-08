using System;
using System.Text;
using CommandLine;

namespace easyscu
{

    /// <summary>
    /// CXXXScu 命令选项
    /// </summary>
    public abstract class  ScuOptions
    {
        
        
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

        public virtual string   OptionText()
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
        
        [Option("src", Required = true, HelpText = "Dicom Files Folder To Be Send !")]
        public string DicomSrc
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
        
    }
}