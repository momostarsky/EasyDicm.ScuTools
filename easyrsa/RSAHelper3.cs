using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace easyrsa
{
     /// <summary>
    /// RSA加解密 使用OpenSSL的公钥加密/私钥解密
    /// 作者：李志强
    /// 创建时间：2017年10月30日15:50:14
    /// QQ:501232752
    /// https://www.cnblogs.com/caolingyi/p/12395379.html
    /// </summary>
    public class RSAHelper3
    {
        private readonly RSA _privateKeyRsaProvider;
        private readonly RSA _publicKeyRsaProvider;
        private readonly HashAlgorithmName _hashAlgorithmName  = HashAlgorithmName.SHA1;
        private readonly Encoding _encoding = Encoding.UTF8;

        /// <summary>
        /// 实例化RSAHelper  RSA  秘钥长度定死了为2048 ,  签名为SHA256
        /// </summary> 
        /// <param name="privateKey">私钥</param>
        /// <param name="publicKey">公钥</param>
        public RSAHelper3(  string privateKey, string publicKey = null)
        {
           
            if (!string.IsNullOrEmpty(privateKey))
            {
                _privateKeyRsaProvider = CreateRsaProviderFromPrivateKey(privateKey);
            }

            if (!string.IsNullOrEmpty(publicKey))
            {
                _publicKeyRsaProvider = CreateRsaProviderFromPublicKey(publicKey);
            }
             
           
        }

        public RSAHelper3( )
        {
            
        }

        #region 使用私钥签名

        /// <summary>
        /// 使用私钥签名
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <returns></returns>
        public string Sign(string data)
        {
            byte[] dataBytes = _encoding.GetBytes(data);

            var signatureBytes = _privateKeyRsaProvider.SignData(dataBytes, _hashAlgorithmName, RSASignaturePadding.Pkcs1);

            return Convert.ToBase64String(signatureBytes);
        }

        #endregion

        #region 使用公钥验证签名

        /// <summary>
        /// 使用公钥验证签名
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <param name="sign">签名</param>
        /// <returns></returns>
        public bool Verify(string data, string sign)
        {
            byte[] dataBytes = _encoding.GetBytes(data);
            var dataSign = sign.Replace(" ", "+");
            int mod4 = dataSign.Length % 4;
            if (mod4 > 0)
            {
                dataSign += new string('=', 4 - mod4);
            }

            byte[] signBytes = Convert.FromBase64String(dataSign);

            var verify = _publicKeyRsaProvider.VerifyData(dataBytes, signBytes, _hashAlgorithmName, RSASignaturePadding.Pkcs1);

            return verify;
        }

        #endregion

        #region 解密

        public string Decrypt(string cipherText)
        {
            if (_privateKeyRsaProvider == null)
            {
                throw new Exception("_privateKeyRsaProvider is null");
            }
            return Encoding.UTF8.GetString(_privateKeyRsaProvider.Decrypt(Convert.FromBase64String(cipherText), RSAEncryptionPadding.Pkcs1));
        }

        public string DecryptBigData(string dataStr)
        {
            //privateKey = RsaPrivateKeyJava2DotNet(privateKey);
            //FromXmlStringExtensions(rsa, privateKey);

            var data = dataStr.Split(new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
            var byteList = new List<byte>();

            foreach (var item in data)
            {
                byteList.AddRange(_privateKeyRsaProvider.Decrypt(Convert.FromBase64String(item), RSAEncryptionPadding.Pkcs1));
            }
            return Encoding.UTF8.GetString(byteList.ToArray());
        }

        public string DecryptBigDataJava(string dataStr)
        {
            //privateKey = RsaPrivateKeyJava2DotNet(privateKey);
            //FromXmlStringExtensions(rsa, privateKey);

            dataStr = dataStr.Replace(" ", "+");
            int mod4 = dataStr.Length % 4;
            if (mod4 > 0)
            {
                dataStr += new string('=', 4 - mod4);
            }
            var byteList = new List<byte>();
            var data = Convert.FromBase64String(dataStr);

            var splitLength = 128;
            var splitsNumber = Convert.ToInt32(Math.Ceiling(data.Length * 1.0 / splitLength));

            var pointer = 0;
            for (int i = 0; i < splitsNumber; i++)
            {
                if (pointer + splitLength < data.Length)
                {
                    byte[] current = data.Skip(pointer).Take(splitLength).ToArray();
                    byteList.AddRange(_privateKeyRsaProvider.Decrypt(current, RSAEncryptionPadding.Pkcs1));
                    pointer += splitLength;
                }
                else
                {
                    byte[] current = data.Skip(pointer).Take(splitLength).ToArray();
                    byteList.AddRange(_privateKeyRsaProvider.Decrypt(current, RSAEncryptionPadding.Pkcs1));
                    pointer += splitLength;
                }
                //byte[] current = pointer + splitLength < data.Length ? data[pointer..(pointer + splitLength)] : data[pointer..];
            }
            return Encoding.UTF8.GetString(byteList.ToArray());
        }

        #endregion

        #region 加密

        public string Encrypt(string text)
        {
            if (_publicKeyRsaProvider == null)
            {
                throw new Exception("_publicKeyRsaProvider is null");
            }
            return Convert.ToBase64String(_publicKeyRsaProvider.Encrypt(Encoding.UTF8.GetBytes(text), RSAEncryptionPadding.Pkcs1));
        }

        public string EncryptBigData(string dataStr)
        {
            char connChar = '$';
            //publicKey = RsaPublicKeyJava2DotNet(publicKey);
            //FromXmlStringExtensions(rsa, publicKey);

            var data = Encoding.UTF8.GetBytes(dataStr);
            //var modulusLength = _publicKeyRsaProvider.KeySize / 8;
            //var splitLength = modulusLength - PaddingLimitDic[padding];
            var splitLength = 117;
            var sb = new StringBuilder();

            var splitsNumber = Convert.ToInt32(Math.Ceiling(data.Length * 1.0 / splitLength));

            var pointer = 0;
            for (int i = 0; i < splitsNumber; i++)
            {
                if (pointer + splitLength < data.Length)
                {
                    byte[] current = data.Skip(pointer).Take(splitLength).ToArray();
                    sb.Append(Convert.ToBase64String(_publicKeyRsaProvider.Encrypt(current, RSAEncryptionPadding.Pkcs1)));
                    sb.Append(connChar);
                    pointer += splitLength;
                }
                else
                {
                    byte[] current = data.Skip(pointer).Take(data.Length - pointer).ToArray();
                    sb.Append(Convert.ToBase64String(_publicKeyRsaProvider.Encrypt(current, RSAEncryptionPadding.Pkcs1)));
                    sb.Append(connChar);
                    pointer += splitLength;
                }
                //byte[] current = pointer + splitLength < data.Length ? data[pointer..(pointer + splitLength)] : data[pointer..];
            }

            return sb.ToString();
        }

        public string EncryptBigDataJava(string dataStr)
        {
            //publicKey = RsaPublicKeyJava2DotNet(publicKey);
            //FromXmlStringExtensions(rsa, publicKey);

            var data = Encoding.UTF8.GetBytes(dataStr);
            //var modulusLength = _publicKeyRsaProvider.KeySize / 8;
            //var splitLength = modulusLength - PaddingLimitDic[padding];
            var splitLength = 117;
            var byteList = new List<byte>();

            var splitsNumber = Convert.ToInt32(Math.Ceiling(data.Length * 1.0 / splitLength));

            var pointer = 0;
            for (int i = 0; i < splitsNumber; i++)
            {
                if (pointer + splitLength < data.Length)
                {
                    byte[] current = data.Skip(pointer).Take(splitLength).ToArray();
                    byteList.AddRange(_publicKeyRsaProvider.Encrypt(current, RSAEncryptionPadding.Pkcs1));
                    pointer += splitLength;
                }
                else
                {
                    byte[] current = data.Skip(pointer).Take(data.Length - pointer).ToArray();
                    byteList.AddRange(_publicKeyRsaProvider.Encrypt(current, RSAEncryptionPadding.Pkcs1));
                    pointer += splitLength;
                }
                //byte[] current = pointer + splitLength < data.Length ? data[pointer..(pointer + splitLength)] : data[pointer..];
            }
            return Convert.ToBase64String(byteList.ToArray());
        }
        #endregion

        #region 使用私钥创建RSA实例

        public   RSA CreateRsaProviderFromPrivateKey(string privateKey)
        {
            var privateKeyBits = Convert.FromBase64String(privateKey);

            var rsa = RSA.Create();
            var rsaParameters = new RSAParameters();

            using (BinaryReader binr = new BinaryReader(new MemoryStream(privateKeyBits)))
            {
                byte bt = 0;
                ushort twobytes = 0;
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)
                    binr.ReadByte();
                else if (twobytes == 0x8230)
                    binr.ReadInt16();
                else
                    throw new Exception("Unexpected value read binr.ReadUInt16()");

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)
                    throw new Exception("Unexpected version");

                bt = binr.ReadByte();
                if (bt != 0x00)
                    throw new Exception("Unexpected value read binr.ReadByte()");

                rsaParameters.Modulus = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.Exponent = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.D = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.P = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.Q = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.DP = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.DQ = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.InverseQ = binr.ReadBytes(GetIntegerSize(binr));
            }

            rsa.ImportParameters(rsaParameters);
            return rsa;
        }

        #endregion

        #region 使用公钥创建RSA实例

        public   RSA CreateRsaProviderFromPublicKey(string publicKeyString)
        {
            // encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
            byte[] seqOid = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] seq = new byte[15];

            var x509Key = Convert.FromBase64String(publicKeyString);

            // ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------
            using (MemoryStream mem = new MemoryStream(x509Key))
            {
                using (BinaryReader binr = new BinaryReader(mem))  //wrap Memory Stream with BinaryReader for easy reading
                {
                    byte bt = 0;
                    ushort twobytes = 0;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        return null;

                    seq = binr.ReadBytes(15);       //read the Sequence OID
                    if (!CompareBytearrays(seq, seqOid))    //make sure Sequence for OID is correct
                        return null;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8103) //data read as little endian order (actual data order for Bit String is 03 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8203)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        return null;

                    bt = binr.ReadByte();
                    if (bt != 0x00)     //expect null byte next
                        return null;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        return null;

                    twobytes = binr.ReadUInt16();
                    byte lowbyte = 0x00;
                    byte highbyte = 0x00;

                    if (twobytes == 0x8102) //data read as little endian order (actual data order for Integer is 02 81)
                        lowbyte = binr.ReadByte();  // read next bytes which is bytes in modulus
                    else if (twobytes == 0x8202)
                    {
                        highbyte = binr.ReadByte(); //advance 2 bytes
                        lowbyte = binr.ReadByte();
                    }
                    else
                        return null;
                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };   //reverse byte order since asn.1 key uses big endian order
                    int modsize = BitConverter.ToInt32(modint, 0);

                    int firstbyte = binr.PeekChar();
                    if (firstbyte == 0x00)
                    {   //if first byte (highest order) of modulus is zero, don't include it
                        binr.ReadByte();    //skip this null byte
                        modsize -= 1;   //reduce modulus buffer size by 1
                    }

                    byte[] modulus = binr.ReadBytes(modsize);   //read the modulus bytes

                    if (binr.ReadByte() != 0x02)            //expect an Integer for the exponent data
                        return null;
                    int expbytes = (int)binr.ReadByte();        // should only need one byte for actual exponent data (for all useful values)
                    byte[] exponent = binr.ReadBytes(expbytes);

                    // ------- create RSACryptoServiceProvider instance and initialize with public key -----
                    var rsa = RSA.Create();
                    RSAParameters rsaKeyInfo = new RSAParameters
                    {
                        Modulus = modulus,
                        Exponent = exponent
                    };
                    rsa.ImportParameters(rsaKeyInfo);

                    return rsa;
                }

            }
        }

        #endregion

        #region 导入密钥算法

        private int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte();
            else
            if (bt == 0x82)
            {
                var highbyte = binr.ReadByte();
                var lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;
            }

            while (binr.ReadByte() == 0x00)
            {
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);
            return count;
        }

        private bool CompareBytearrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            int i = 0;
            foreach (byte c in a)
            {
                if (c != b[i])
                    return false;
                i++;
            }
            return true;
        }

        #endregion

        static readonly Dictionary<RSAEncryptionPadding, int> PaddingLimitDic = new Dictionary<RSAEncryptionPadding, int>()
        {
            [RSAEncryptionPadding.Pkcs1] = 11,
            [RSAEncryptionPadding.OaepSHA1] = 42,
            [RSAEncryptionPadding.OaepSHA256] = 66,
            [RSAEncryptionPadding.OaepSHA384] = 98,
            [RSAEncryptionPadding.OaepSHA512] = 130,
        };

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
            var privateKeyParam = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateKey));
            return
                $"<RSAKeyValue><Modulus>{Convert.ToBase64String(privateKeyParam.Modulus.ToByteArrayUnsigned())}</Modulus><Exponent>{Convert.ToBase64String(privateKeyParam.PublicExponent.ToByteArrayUnsigned())}</Exponent><P>{Convert.ToBase64String(privateKeyParam.P.ToByteArrayUnsigned())}</P><Q>{Convert.ToBase64String(privateKeyParam.Q.ToByteArrayUnsigned())}</Q><DP>{Convert.ToBase64String(privateKeyParam.DP.ToByteArrayUnsigned())}</DP><DQ>{Convert.ToBase64String(privateKeyParam.DQ.ToByteArrayUnsigned())}</DQ><InverseQ>{Convert.ToBase64String(privateKeyParam.QInv.ToByteArrayUnsigned())}</InverseQ><D>{Convert.ToBase64String(privateKeyParam.Exponent.ToByteArrayUnsigned())}</D></RSAKeyValue>";
        }

        /// <summary>
        /// 扩展FromXmlString
        /// </summary>
        /// <param name="rsa"></param>
        /// <param name="xmlString"></param>
        private static void FromXmlStringExtensions(RSA rsa, string xmlString)
        {
            var parameters = new RSAParameters();

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            if (xmlDoc.DocumentElement.Name.Equals("RSAKeyValue"))
            {
                foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "Modulus":
                            parameters.Modulus = (string.IsNullOrEmpty(node.InnerText)
                                ? null
                                : Convert.FromBase64String(node.InnerText));
                            break;
                        case "Exponent":
                            parameters.Exponent = (string.IsNullOrEmpty(node.InnerText)
                                ? null
                                : Convert.FromBase64String(node.InnerText));
                            break;
                        case "P":
                            parameters.P = (string.IsNullOrEmpty(node.InnerText)
                                ? null
                                : Convert.FromBase64String(node.InnerText));
                            break;
                        case "Q":
                            parameters.Q = (string.IsNullOrEmpty(node.InnerText)
                                ? null
                                : Convert.FromBase64String(node.InnerText));
                            break;
                        case "DP":
                            parameters.DP = (string.IsNullOrEmpty(node.InnerText)
                                ? null
                                : Convert.FromBase64String(node.InnerText));
                            break;
                        case "DQ":
                            parameters.DQ = (string.IsNullOrEmpty(node.InnerText)
                                ? null
                                : Convert.FromBase64String(node.InnerText));
                            break;
                        case "InverseQ":
                            parameters.InverseQ = (string.IsNullOrEmpty(node.InnerText)
                                ? null
                                : Convert.FromBase64String(node.InnerText));
                            break;
                        case "D":
                            parameters.D = (string.IsNullOrEmpty(node.InnerText)
                                ? null
                                : Convert.FromBase64String(node.InnerText));
                            break;
                    }
                }
            }
            else
            {
                throw new Exception("Invalid XML RSA key.");
            }

            rsa.ImportParameters(parameters);
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

            var publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
            return string.Format(
                "<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
                Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned()),
                Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned())
            );
        }
    }
 
}