using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using easyrsa;
using NUnit.Framework;
using RSAExtensions;


namespace ScuTests
{
    public class Tests
    {
        private RSAHelper rsa;
        private String priKey;
        private String pubKey;

        private string jPub =
            "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAs481Bq+edtr8uYiu9btRML4SNW01BAlVeATQSz+4c+HKSEyfhLsHn7/WcOX+Q6tEdiHhTVZouJCkZRhkSaeERfMst/x2JhER3IUfsHKBCSZiuKPuxv6QVE1fKAjYNIpbH5on7zV8/uE3RTXti9s4wkRW99gtYtziftxXGOjJYNqqlQJ/iZiNTY9UiCffDXodBpYl3PXbbomNhlQygXhkm0O92947qufquVcVGnw+wU0eq7qfvNmp0P5UdWDy72Q41hfF50Tgemq7n5Qwmvj8tKkqSfuREIRE5IEaUtIb2QrctDH1sdclTQFUkjsJS2gqEs87wz0tiNfvQPtd3z+HLwIDAQAB";


        [SetUp]
        public void StartUp()
        {
            rsa = new RSAHelper();
            pubKey = rsa.PublicKey();
            priKey = rsa.PrivatecKey();
            
           
        }

        [TearDown]
        public void Clearup()
        {
            rsa.Dispose();
        }

        [Test]
        public void EncrypAndDecrypted()
        {
         
            
            string str = "博客园 http://www.cnblogs.com/";

            byte[] strData = Encoding.UTF8.GetBytes(str);

            Console.WriteLine("原始字符串：" + str);


            var encrypted = rsa.Encrypt(strData);

            var decrypted = rsa.Decrypt(encrypted);

            Console.WriteLine($"Leng:{encrypted.Length}");
            var decryptedText = Encoding.UTF8.GetString(decrypted);
            Assert.True(decryptedText.Equals(str));
        }


        [Test]
        public void ImportExport()
        {
           

            using var t2 = new RSAHelper(priKey);

            var pubKey2 = t2.PublicKey();
            var priKey2 = t2.PrivatecKey(); 
            Assert.AreEqual(pubKey, pubKey2);
            Assert.AreEqual(priKey, priKey2);
            var txt = "YQpXS9E0XT8dEJI3oyORQMnF0NzRRgdmyS5JeSVMxCL/DvSaxpxA1Go6w2TfY7q";

            var txtData = Encoding.UTF8.GetBytes(txt);

            var txtSign = t2.Sign(txtData);

            Assert.True(t2.Verify(txtData, txtSign));
        }


        [Test]
        public void JavaPublicKey()
        {
            using var jrsa = RSA.Create();
            jrsa.ImportPkcs8PublicKey(Convert.FromBase64String(jPub));

            var dataX = Encoding.UTF8.GetBytes("lEOEB9D8u3uvyDYoAewm/7BsAkKC7w+Co");

            var dataEncrypted = jrsa.Encrypt(dataX, RSAEncryptionPadding.Pkcs1);

            Assert.True(dataEncrypted != null);

            Console.WriteLine($"Jpub:{Encoding.UTF8.GetString(dataX)}");
            Console.WriteLine($"Jpub:{Convert.ToBase64String(dataEncrypted)}");
        }
    }
}