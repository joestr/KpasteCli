﻿using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using SimpleBase;

namespace KpasteCli.Logic
{
    public class KpasteCrypto
    {
        private static readonly int IterationCount = 100000;
        private static readonly int KeySize = 256;
        private static readonly int TagSize = 128;

        private readonly string _key;
        private readonly string _vector;
        private readonly string _salt;

        public class KpasteEncryptionResultDto
        {
            public string Key { get; set; }
            public string Vector { get; set; }
            public string Salt { get; set; }
            public string Message { get; set; }
        }

        public KpasteCrypto(string key, string vector, string salt)
        {
            this._key = FromBase58(key);
            this._vector = ArrayBufferToString(Convert.FromBase64String(vector));
            this._salt = ArrayBufferToString(Convert.FromBase64String(salt));
        }
        
        public KpasteCrypto()
        {
            this._key = GetRandomBytes(32);
            this._vector = GetRandomBytes(16);
            this._salt = GetRandomBytes(8);
        }

        public KpasteEncryptionResultDto Encrypt(string plainText, string password)
        {
            var derivedKey = DeriveKey(password);
            var message = Aes256GcmEncrypt(plainText, derivedKey);

            var result = new KpasteEncryptionResultDto()
            {
                Key = ToBase58(this._key),
                Vector = Convert.ToBase64String(StringToArrayBuffer(this._vector)),
                Salt = Convert.ToBase64String(StringToArrayBuffer(this._salt)),
                Message = message
            };
            return result;
        }

        public string Decrypt(string cipherText, string password)
        {
            var derivedKey = DeriveKey(password);
            var message = Aes256GcmDecrypt(cipherText, derivedKey);

            return message;
        }

        private string Aes256GcmEncrypt(string plainText, byte[] derivedKey)
        {
            var plainTextBytes = StringToArrayBuffer(Utf16ToUtf8(plainText));
            var plainTextBytesMemoryStream = new MemoryStream(plainTextBytes);

            var compressedPlainTextByesMemoryStream = new MemoryStream(plainTextBytes.Length); //set to estimate of compression ratio
            using (ZLibStream zLibStream = new ZLibStream(compressedPlainTextByesMemoryStream, CompressionMode.Compress))
            {
                plainTextBytesMemoryStream.CopyTo(zLibStream);
            }

            var compressedPlainTextBytes = compressedPlainTextByesMemoryStream.ToArray();
            byte[] encryptedBytes;

            IBlockCipher cipher = new AesEngine();
            KeyParameter keyParam = new KeyParameter(derivedKey);
            AeadParameters keyParamAead = new AeadParameters(keyParam, TagSize, StringToArrayBuffer(this._vector), new byte[0]);
            GcmBlockCipher cipherMode = new GcmBlockCipher(cipher);
            cipherMode.Init(true, keyParamAead);
            int outputSize = cipherMode.GetOutputSize(compressedPlainTextBytes.Length);
            byte[] cipherTextData = new byte[outputSize];
            int result = cipherMode.ProcessBytes(compressedPlainTextBytes, 0, compressedPlainTextBytes.Length, cipherTextData, 0);
            cipherMode.DoFinal(cipherTextData, result);
            encryptedBytes = cipherTextData;

            var resultString = Convert.ToBase64String(encryptedBytes);

            return resultString;
        }

        private string Aes256GcmDecrypt(string cipherText, byte[] derivedKey)
        {
            var cipherTextBytes = Convert.FromBase64String(cipherText);
            byte[] compressedPlainTextBytes;

            IBlockCipher cipher = new AesEngine();
            KeyParameter keyParam = new KeyParameter(derivedKey);
            AeadParameters keyParamAead = new AeadParameters(keyParam, TagSize, StringToArrayBuffer(this._vector), new byte[0]);
            GcmBlockCipher cipherMode = new GcmBlockCipher(cipher);
            cipherMode.Init(false, keyParamAead);
            var outputSize = cipherMode.GetOutputSize(cipherTextBytes.Length);
            var plainTextData = new byte[outputSize];
            var result = cipherMode.ProcessBytes(cipherTextBytes, 0, cipherTextBytes.Length, plainTextData, 0);
            cipherMode.DoFinal(plainTextData, result);
            compressedPlainTextBytes = plainTextData;

            var compressedPlainTextBytesMemoryStream = new MemoryStream(compressedPlainTextBytes);
            var plainTextBytesMemoryStream = new MemoryStream(compressedPlainTextBytesMemoryStream.Capacity * 2 + 1024);

            using (ZLibStream zlibStream = new ZLibStream(compressedPlainTextBytesMemoryStream, CompressionMode.Decompress))
            {
                zlibStream.CopyTo(plainTextBytesMemoryStream);
            }

            var plainTextString = Utf8ToUtf16(ArrayBufferToString(plainTextBytesMemoryStream.ToArray()));

            return plainTextString;
        }

        private byte[] DeriveKey(string password)
        {
            byte[] newKeyBytes;
            if (password.Length > 0)
            {
                var passwordBytes = StringToArrayBuffer(password);
                var keyBytes = StringToArrayBuffer(this._key);
                newKeyBytes = new byte[keyBytes.Length + passwordBytes.Length];
                keyBytes.CopyTo(newKeyBytes, 0);
                passwordBytes.CopyTo(newKeyBytes, keyBytes.Length);
            }
            else
            {
                var keyBytes = StringToArrayBuffer(this._key);
                newKeyBytes = keyBytes;
            }

            var saltBytes = StringToArrayBuffer(this._salt);

            return Rfc2898DeriveBytes.Pbkdf2(newKeyBytes, saltBytes, IterationCount, HashAlgorithmName.SHA256,
                KeySize/8);
        }

        private string ToBase58(string input)
        {
            return Base58.Bitcoin.Encode(StringToArrayBuffer(input));
        }

        private string FromBase58(string input)
        {
            return ArrayBufferToString(Base58.Bitcoin.Decode(input));
        }
        
        private string ArrayBufferToString(byte[] messageArray)
        {
            return Encoding.UTF8.GetString(messageArray);

            // This somehow has problems with text conversion.
            //var message = "";
            //for (var i = 0; i < messageArray.Length; i += 1) {
            //    message += (char)messageArray[i];
            //}
            //return message;
        }

        private byte[] StringToArrayBuffer(string message)
        {
            return Encoding.UTF8.GetBytes(message);

            // This somehow has problems with text conversion.
            //byte[] messageArray = new byte[message.Length];
            //for (var i = 0; i < message.Length; i += 1) {
            //    messageArray[i] = (byte)message[i];
            //}
            //return messageArray;
        }

        private string Utf16ToUtf8(string message)
        {
            return message;
            // lol. This isn't required at all??
            //return Encoding.UTF8.GetString(Encoding.Unicode.GetBytes(message));
        }

        private string Utf8ToUtf16(string message)
        {
            return message;
            // lol. This isn't required at all??
            //return Encoding.Unicode.GetString(Encoding.UTF8.GetBytes(message));
        }

        private string GetRandomBytes(int length)
        {
            var result = "";

            var bytes = new byte[length];
            Random.Shared.NextBytes(bytes);

            for (var i = 0; i < bytes.Length; i += 1)
            {
                result += (char)bytes[i];
            }

            return result;
        }
    }
}
