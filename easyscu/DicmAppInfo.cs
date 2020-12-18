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
 
 

        // var txt = "CJ10000:123";
        // var data = Encoding.UTF8.GetBytes(txt);
        // //---用JAVA的Publick 进行加密
        // var jPub =
        //     "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAtC0Pt1y/kPOiUVtbJbWwBCfF45iEO9/S9wAu+UO89KHFZqI8dTKpsJaNoXNGFFv4I0eOq7dko/JBseZ13HVHoXoBsKpaisafBW+cuDcExyzeNX6CwTwEWyhr5x3c07xrxxhDBGH/AbFoUR6YpFBiY3eKLzgKVzenAr9auJS39NXupK7hy7zp4OL4RYoIz3Uz1aJa0QScq8ntGl7TzFs73s3csJJ6IBsPn2QUe+2IM3AbQLXVB/ng7n3AYzrMx16r/WayFx2MESFbwBeZbwyiDSpk2XOeXo1Rx0uZPgZgXcPfpnGzXiFoHOWcMzEpVPxh9/YRgluToRHORWjeMZu1jQIDAQAB";
        //
        // var jPubKey =  RSA.Create(2048);
        // jPubKey.ImportPublicKey(RSAKeyType.Pkcs8, jPub,false); 
        // var dataEnc = jPubKey.Encrypt( data, RSAEncryptionPadding.Pkcs1);
        // var bEnc = Convert.ToBase64String(dataEnc);
        // Console.WriteLine($"{ bEnc.Length}={ bEnc }");
        //     
        // //----用DOTNET 的进行签名数据
        // var filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        // string json = File.ReadAllText(filePath);
        // dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
        // var scuPrikey = $"{jsonObj["RsaKey"]["PrivateKey"]}";
        // var scuPubkey = $"{jsonObj["RsaKey"]["PublicKey"]}";
        //  
        // Console.WriteLine(scuPubkey);
        //     
        //
        // var rsa = new RSAHelper(scuPrikey, scuPubkey); 
        // //私钥签名
        // byte[] signStrData = rsa.Sign(dataEnc); 
        // var signB64Enc = Convert.ToBase64String(signStrData);
        //     
        // Console.WriteLine($"signData:{ signB64Enc.Length}={ signB64Enc }");
        //
        //     
        // //公钥验证签名
        // bool signVerify = rsa.Verify(dataEnc, signStrData);
        //
        // Console.WriteLine("验证签名：" + signVerify);
    }
}