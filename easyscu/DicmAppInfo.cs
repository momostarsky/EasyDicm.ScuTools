using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Dicom;
using Dicom.Network;
using easyrsa;
using RSAExtensions;

namespace easyscu
{
    public class DicmAppInfo
    {
        private readonly String scuPrikey;
        private readonly String scuPubkey;
        private readonly String scuClientId;
        private readonly String jApplicationID;
        private readonly String jPubKey;

        private DicmAppInfo()
        {
            // //----用DOTNET 的进行签名数据
            var filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            string json = File.ReadAllText(filePath);
            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            scuPrikey = $"{jsonObj["RsaKey"]["PrivateKey"]}";
            scuPubkey = $"{jsonObj["RsaKey"]["PublicKey"]}";
            scuClientId = $"{jsonObj["RsaKey"]["AppID"]}";
            jApplicationID = $"{jsonObj["RsaKey"]["ApplicationID"]}";
            jPubKey = $"{jsonObj["RsaKey"]["ApplicationKey"]}";

            DicomExtendeds = new Lazy<DicomExtendedNegotiation[]>(creator, true);
        }

        public static readonly DicmAppInfo Instance = new DicmAppInfo();
        
        public Lazy<DicomExtendedNegotiation[]> DicomExtendeds { get; private set; }

        private DicomExtendedNegotiation[] creator()
        {
            var txt = $"{jApplicationID}";
            var data = Encoding.UTF8.GetBytes(txt);
            var jPubRsa = RSA.Create();
            jPubRsa.ImportPublicKey(RSAKeyType.Pkcs8, jPubKey, false); 
            var dataEnc = jPubRsa.Encrypt(data, RSAEncryptionPadding.Pkcs1); 
            var rsa = new RSAHelper(scuPrikey);
            //私钥签名
            byte[] signStrData = rsa.Sign(data); 
            DicomExtendedNegotiation[] ext4 = new DicomExtendedNegotiation[4]; 
            ext4[0] = new DicomExtendedNegotiation(DicomUID.dicomHostname,
                new DicomServiceApplicationInfo(Encoding.UTF8.GetBytes(scuClientId)));
            ext4[1] = new DicomExtendedNegotiation(DicomUID.dicomDescription, new DicomServiceApplicationInfo(data));
            ext4[2] = new DicomExtendedNegotiation(DicomUID.dicomDevice, new DicomServiceApplicationInfo(dataEnc));
            ext4[3] = new DicomExtendedNegotiation(DicomUID.dicomDeviceName,
                new DicomServiceApplicationInfo(signStrData));
            return ext4;
        }
 
    }
}