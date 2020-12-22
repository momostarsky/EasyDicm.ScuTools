using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using RSAExtensions;

namespace easyrsa
{
    /// <summary>
    /// RSA加解密 使用OpenSSL的公钥加密/私钥解密
    /// 作者：李志强
    /// https://www.cnblogs.com/stulzq/p/7757915.html
    /// 创建时间：2017年10月30日15:50:14
    /// QQ:501232752
    /// </summary>
    public class RSAHelper : BaseDispose
    {
        public static readonly Encoding Encoding = Encoding.UTF8;
        private static readonly int KeySize = 1688;

        private readonly RSA rsa;

        /// <summary>
        /// 实例化RSAHelper
        /// </summary> 
        /// <param name="privateKey">私钥</param>
        /// <param name="publicKey">公钥</param>
        public RSAHelper(string privateKey)
        {
            rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKey), out _);
        }

        public RSAHelper()
        {
            rsa = RSA.Create(KeySize);
        }


        public String PublicKey()
        {
            return Convert.ToBase64String(rsa.ExportPkcs8PublicKey());
        }

        public String PrivatecKey()
        {
            return Convert.ToBase64String(rsa.ExportPkcs8PrivateKey());
        }

        #region 使用私钥签名

        /// <summary>
        /// 使用私钥签名
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <returns></returns>
        public byte[] Sign(byte[] data)
        {
            var signatureBytes = rsa.SignData(data, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);

            return signatureBytes;
        }

        #endregion

        #region 使用公钥验证签名

        /// <summary>
        /// 使用公钥验证签名
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <param name="sign">签名</param>
        /// <returns></returns>
        public bool Verify(byte[] data, byte[] sign)
        {
            var verify =
                rsa.VerifyData(data, sign, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);

            return verify;
        }

        #endregion

        #region 解密

        public byte[] Decrypt(byte[] cipherText)
        {
            return rsa.Decrypt(cipherText, RSAEncryptionPadding.Pkcs1);
        }

        #endregion

        #region 加密

        public byte[] Encrypt(byte[] text)
        {
            return rsa.Encrypt(text, RSAEncryptionPadding.Pkcs1);
        }

        #endregion


        public string EncryptBigDataJava(string dataStr)
        {
            return rsa.EncryptBigData(dataStr, RSAEncryptionPadding.Pkcs1);
        }

        public string DecryptBigDataJava(string dataStr)
        {
            return rsa.DecryptBigData(dataStr, RSAEncryptionPadding.Pkcs1);
        }


        /// <summary>
        /// private key ，java->.net
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static string RsaPrivateKeyJava2DotNet(string privateKey)
        {
            if (string.IsNullOrEmpty(privateKey))
            {
                return string.Empty;
            }

            var privateKeyParam =
                (RsaPrivateCrtKeyParameters) PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateKey));
            return
                $"<RSAKeyValue>" +
                $"<Modulus>{Convert.ToBase64String(privateKeyParam.Modulus.ToByteArrayUnsigned())}</Modulus>" +
                $"<Exponent>{Convert.ToBase64String(privateKeyParam.PublicExponent.ToByteArrayUnsigned())}</Exponent>" +
                $"<P>{Convert.ToBase64String(privateKeyParam.P.ToByteArrayUnsigned())}</P>" +
                $"<Q>{Convert.ToBase64String(privateKeyParam.Q.ToByteArrayUnsigned())}</Q>" +
                $"<DP>{Convert.ToBase64String(privateKeyParam.DP.ToByteArrayUnsigned())}</DP>" +
                $"<DQ>{Convert.ToBase64String(privateKeyParam.DQ.ToByteArrayUnsigned())}</DQ>" +
                $"<InverseQ>{Convert.ToBase64String(privateKeyParam.QInv.ToByteArrayUnsigned())}</InverseQ>" +
                $"<D>{Convert.ToBase64String(privateKeyParam.Exponent.ToByteArrayUnsigned())}</D>" +
                $"</RSAKeyValue>";
        }

        /// <summary>
        /// public key ，java->.net
        /// </summary>
        /// <param name="publicKey"></param>
        /// <returns>格式转换结果</returns>
        public static string RsaPublicKeyJava2DotNet(string publicKey)
        {
            if (string.IsNullOrEmpty(publicKey))
            {
                return string.Empty;
            }

            var publicKeyParam = (RsaKeyParameters) PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
            return
                $"<RSAKeyValue>" +
                $"<Modulus>{Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned())}</Modulus>" +
                $"<Exponent>{Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned())}</Exponent>" +
                $"</RSAKeyValue>";
        }

        protected override void ClearManagedObjects()
        {
            rsa.Dispose();
            base.ClearManagedObjects();
        }
    }
}