﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
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
    public class RSAHelper : IDisposable
    {
        public static readonly Encoding Encoding = Encoding.UTF8;
        public static readonly int KeySize = 1688;

        private readonly RSA rsa;

        /// <summary>
        /// 实例化RSAHelper
        /// </summary> 
        /// <param name="privateKey">私钥</param>
        /// <param name="publicKey">公钥</param>
        public RSAHelper(string privateKey )
        {
            rsa = RSA.Create( );
            rsa.ImportPkcs8PrivateKey( Convert.FromBase64String(privateKey), out _);
           
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

        public void Dispose()
        {
            rsa?.Dispose();
        }
    }
}