using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using easyrsa;
using NUnit.Framework;
using RSAExtensions;

namespace ScuTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            Console.WriteLine("StartupTests!");
        }

    

        [Test]
        public void TestRsaSignVerify()
        {
            //2048 公钥
            var filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            string json = File.ReadAllText(filePath);
            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            var scuPrikey = $"{jsonObj["RsaKey"]["PrivateKey"]}";
            var scuPubkey = $"{jsonObj["RsaKey"]["PublicKey"]}";
            var appid = $"{jsonObj["RsaKey"]["ApplicationID"]}";
            var appKey = $"{jsonObj["RsaKey"]["ApplicationKey"]}";
            var clientId = $"{jsonObj["RsaKey"]["AppID"]}";
            
            
            var rsa = new RSAHelper(    scuPrikey, scuPubkey);

            string str = "博客园 http://www.cnblogs.com/";

            byte[] strData = Encoding.UTF8.GetBytes(str);

            Console.WriteLine("原始字符串：" + str);

            //加密
            byte[] enStrData = rsa.Encrypt(strData);

            Console.WriteLine("加密字符串：" + Convert.ToBase64String(enStrData));

            //解密
            byte[] deStr = rsa.Decrypt(enStrData);

            Console.WriteLine("解密字符串：" + Encoding.UTF8.GetString(deStr));

            //私钥签名
            byte[] signStrData = rsa.Sign(strData);

         

            Assert.AreEqual(str ,Encoding.UTF8.GetString(deStr) );
            //公钥验证签名
            bool signVerify = rsa.Verify(strData, signStrData);

            Console.WriteLine("验证签名：" + signVerify);
            
            Assert.AreEqual(signVerify ,true);

          
        }

      
    }
}