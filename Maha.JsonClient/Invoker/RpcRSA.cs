using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Maha.JsonClient.Invoker
{
    /// <summary>
    /// Rpc RSA签名
    /// </summary>
    public class RpcRSA
    {
        /// <summary>
        /// 基于证书进行 RSA 签名
        /// </summary>
        /// <param name="data">要签名的文本</param>
        /// <param name="privateKeyPemString">私钥内容(PEM格式的，即base64格式的证书文件内容)</param>
        /// <returns>十六进制格式文本</returns>
        public static string CreateSignature(string data, string privateKeyPemString)
        {
            privateKeyPemString = privateKeyPemString.Replace("-----BEGIN RSA PRIVATE KEY-----", "")
                .Replace("-----END RSA PRIVATE KEY-----", "");
            var rsaCryptoServiceProvider = new RSACryptoServiceProvider();
            rsaCryptoServiceProvider.ImportParameters(CreateRsaParametersFromPrivateKey(privateKeyPemString));
            RSAPKCS1SignatureFormatter formatter = new RSAPKCS1SignatureFormatter(rsaCryptoServiceProvider);
            formatter.SetHashAlgorithm("MD5");
            var plainTextBytes = Encoding.UTF8.GetBytes(data);
            HashAlgorithm md5 = HashAlgorithm.Create("MD5");
            var hashBytes = md5.ComputeHash(plainTextBytes);
            var signatureBytes = formatter.CreateSignature(hashBytes);
            return BytesToHexString(signatureBytes);
        }

        /// <summary>
        /// 基于证书对 RSA 签名校验
        /// </summary>
        /// <param name="data">要签名的文本</param>
        /// <param name="signatureText">签名（十六进制格式文本）</param>
        /// <param name="publicKeyPemString">公钥内容(PEM格式的，即base64格式的证书文件内容)</param>
        /// <returns></returns>
        public static bool VerifySignature(string data, string signatureText, string publicKeyPemString)
        {
            publicKeyPemString = publicKeyPemString.Replace("-----BEGIN PUBLIC KEY-----", "")
                .Replace("-----END PUBLIC KEY-----", "");
            var signatureBytes = HexStringToBytes(signatureText);
            var rsaCryptoServiceProvider = new System.Security.Cryptography.RSACryptoServiceProvider();
            rsaCryptoServiceProvider.ImportParameters(CreateRsaParametersFromPublicKey(publicKeyPemString));
            var RSADeformatter = new RSAPKCS1SignatureDeformatter(rsaCryptoServiceProvider);
            RSADeformatter.SetHashAlgorithm("MD5");
            var plainTextBytes = Encoding.UTF8.GetBytes(data);
            HashAlgorithm md5 = HashAlgorithm.Create("MD5");
            var hashBytes = md5.ComputeHash(plainTextBytes);

            return RSADeformatter.VerifySignature(hashBytes, signatureBytes);
        }

        /// <summary>
        /// 创建私钥
        /// </summary>
        /// <param name="privateKeyPemString"></param>
        /// <returns></returns>
        private static RSAParameters CreateRsaParametersFromPrivateKey(string privateKeyPemString)
        {
            var privateKeyBits = System.Convert.FromBase64String(privateKeyPemString);
            var RSAparams = new RSAParameters();

            using (var binr = new BinaryReader(new MemoryStream(privateKeyBits)))
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

                RSAparams.Modulus = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.Exponent = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.D = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.P = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.Q = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.DP = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.DQ = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.InverseQ = binr.ReadBytes(GetIntegerSize(binr));
            }

            return RSAparams;
        }

        /// <summary>
        /// 创建公钥
        /// </summary>
        /// <param name="publicKeyPemString"></param>
        /// <returns></returns>
        private static RSAParameters CreateRsaParametersFromPublicKey(string publicKeyPemString)
        {
            var rsaKeyInfo = new RSAParameters();
            byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] x509key;
            byte[] seq = new byte[15];
            int x509size;

            x509key = Convert.FromBase64String(publicKeyPemString);
            x509size = x509key.Length;

            using (var mem = new MemoryStream(x509key))
            {
                using (var binr = new BinaryReader(mem))
                {
                    byte bt = 0;
                    ushort twobytes = 0;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130)
                        binr.ReadByte();
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();
                    else
                        return rsaKeyInfo;

                    seq = binr.ReadBytes(15);
                    if (!CompareByteArrays(seq, SeqOID))
                        return rsaKeyInfo;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8103)
                        binr.ReadByte();
                    else if (twobytes == 0x8203)
                        binr.ReadInt16();
                    else
                        return rsaKeyInfo;

                    bt = binr.ReadByte();
                    if (bt != 0x00)
                        return rsaKeyInfo;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130)
                        binr.ReadByte();
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();
                    else
                        return rsaKeyInfo;

                    twobytes = binr.ReadUInt16();
                    byte lowbyte = 0x00;
                    byte highbyte = 0x00;

                    if (twobytes == 0x8102)
                        lowbyte = binr.ReadByte();
                    else if (twobytes == 0x8202)
                    {
                        highbyte = binr.ReadByte();
                        lowbyte = binr.ReadByte();
                    }
                    else
                        return rsaKeyInfo;
                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                    int modsize = BitConverter.ToInt32(modint, 0);

                    int firstbyte = binr.PeekChar();
                    if (firstbyte == 0x00)
                    {
                        binr.ReadByte();
                        modsize -= 1;
                    }

                    byte[] modulus = binr.ReadBytes(modsize);

                    if (binr.ReadByte() != 0x02)
                        return rsaKeyInfo;
                    int expbytes = (int)binr.ReadByte();
                    byte[] exponent = binr.ReadBytes(expbytes);

                    var rsa = RSA.Create();
                    rsaKeyInfo.Modulus = modulus;
                    rsaKeyInfo.Exponent = exponent;
                    return rsaKeyInfo;
                }
            }
        }

        /// <summary>
        /// 获取Integer大小
        /// </summary>
        /// <param name="binr"></param>
        /// <returns></returns>
        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
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
                highbyte = binr.ReadByte();
                lowbyte = binr.ReadByte();
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

        /// <summary>
        /// 比较字节数组
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static bool CompareByteArrays(byte[] a, byte[] b)
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

        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string BytesToHexString(byte[] input)
        {
            StringBuilder hexString = new StringBuilder(64);
            for (int i = 0; i < input.Length; i++)
            {
                hexString.Append(String.Format("{0:X2}", input[i]));
            }
            return hexString.ToString();
        }

        /// <summary>
        /// 16进制转字节数组
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        private static byte[] HexStringToBytes(string hex)
        {
            if (hex.Length == 0)
            {
                return new byte[] { 0 };
            }

            if (hex.Length % 2 == 1)
            {
                hex = "0" + hex;
            }

            byte[] result = new byte[hex.Length / 2];

            for (int i = 0; i < hex.Length / 2; i++)
            {
                result[i] = byte.Parse(hex.Substring(2 * i, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            }
            return result;
        }
    }
}
