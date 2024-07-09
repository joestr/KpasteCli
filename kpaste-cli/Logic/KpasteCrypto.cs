using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System.IO.Compression;
using System.Numerics;
using System.Text;

namespace kpaste_cli.Logic
{
    public class KPasteCrypto
    {
        // WHO THE HELL???
        private static string PseudoBase58 = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

        private static int iterationCount = 100000;
        private static int keySize = 256;
        private static int tagSize = 128;

        private byte[] key;
        private byte[] vector;
        private byte[] salt;

        public class KpasteEncryptionResultDto
        {
            public string Key { get; set; }
            public string Vector { get; set; }
            public string Salt { get; set; }
            public string Message { get; set; }
        }

        public KPasteCrypto(string key, string vector, string salt)
        {
            this.key = FromPseudoBase58(key);
            this.vector = Convert.FromBase64String(vector);
            this.salt = Convert.FromBase64String(salt);
        }
        
        public KPasteCrypto(byte[]? key, byte[]? vector, byte[]? salt, bool crypt)
        {
            if (key == null)
            {
                var randomBytes = new byte[32];
                Random.Shared.NextBytes(randomBytes);
                this.key = randomBytes;
            }
            else
            {
                this.key = key;
            }

            if (vector == null)
            {
                var randomBytes = new byte[16];
                Random.Shared.NextBytes(randomBytes);
                this.vector = randomBytes;
            }
            else
            {
                this.vector = vector;
            }

            if (salt == null)
            {
                var randomBytes = new byte[8];
                Random.Shared.NextBytes(randomBytes);
                this.salt = randomBytes;
            }
            else
            {
                this.salt = salt;
            }
        }

        public KpasteEncryptionResultDto Encrypt(string plainText, string password)
        {
            var derivedKey = DeriveKey(password);
            var message = Encoding.UTF8.GetBytes(ArrayBufferToString(Aes256GcmEncrypt(plainText, derivedKey)));

            var result = new KpasteEncryptionResultDto()
            {
                Key = ToPseudoBase58(derivedKey),
                Vector = Convert.ToBase64String(this.vector),
                Salt = Convert.ToBase64String(this.salt),
                Message = Convert.ToBase64String(message)
            };
            return result;
        }

        public string Decrypt(string cipherText, string password)
        {
            var derivedKey = DeriveKey(password);
            var message = Aes256GcmDecrypt(cipherText, derivedKey);

            return Encoding.UTF8.GetString(message);
        }

        private byte[] Aes256GcmEncrypt(string plainText, byte[] derivedKey)
        {
            var plainTextBytes = StringToArrayBuffer(plainText);
            var plainTextBytesMemoryStream = new MemoryStream(plainTextBytes);

            var compressedPlainTextByesMemoryStream = new MemoryStream(plainTextBytes.Length + 1024); //set to estimate of compression ratio
            using (GZipStream gZipStream = new GZipStream(compressedPlainTextByesMemoryStream, CompressionMode.Compress))
            {
                plainTextBytesMemoryStream.CopyTo(gZipStream);
            }

            var compressedPlainTextBytes = compressedPlainTextByesMemoryStream.ToArray();
            byte[] encryptedBytes;

            //var aes256Gcm = new AesGcm(derivedKey, tagSize);
            //aes256Gcm.Encrypt(this.vector, compressedStream.ToArray(), encryptedBytes, tagBytes);

            IBlockCipher cipher = new AesEngine();
            KeyParameter keyParam = new KeyParameter(derivedKey);
            AeadParameters keyParamAead = new AeadParameters(keyParam, tagSize, this.vector, new byte[0]);
            GcmBlockCipher cipherMode = new GcmBlockCipher(cipher);
            cipherMode.Init(true, keyParamAead);
            int outputSize = cipherMode.GetOutputSize(compressedPlainTextBytes.Length);
            byte[] cipherTextData = new byte[outputSize];
            int result = cipherMode.ProcessBytes(compressedPlainTextBytes, 0, compressedPlainTextBytes.Length, cipherTextData, 0);
            cipherMode.DoFinal(cipherTextData, result);
            encryptedBytes = cipherTextData;

            return encryptedBytes;
        }

        private byte[] Aes256GcmDecrypt(string cipherText, byte[] derivedKey)
        {
            var cipherTextBytes = StringToArrayBuffer(Encoding.UTF8.GetString(Convert.FromBase64String(cipherText)));
            byte[] compressedPlainTextBytes;

            //var aes256Gcm = new AesGcm(derivedKey, tagSize);
            //aes256Gcm.Decrypt(this.vector, cipherBytes, tagBytes, unencryptedBytes);

            IBlockCipher cipher = new AesEngine();
            KeyParameter keyParam = new KeyParameter(derivedKey);
            AeadParameters keyParamAead = new AeadParameters(keyParam, tagSize, this.vector, new byte[0]);
            GcmBlockCipher cipherMode = new GcmBlockCipher(cipher);
            cipherMode.Init(false, keyParamAead);
            var outputSize = cipherMode.GetOutputSize(cipherTextBytes.Length);
            var plainTextData = new byte[outputSize];
            var result = cipherMode.ProcessBytes(cipherTextBytes, 0, cipherTextBytes.Length, plainTextData, 0);
            cipherMode.DoFinal(plainTextData, result);
            compressedPlainTextBytes = plainTextData;

            var compressedPlainTextBytesMemoryStream = new MemoryStream(compressedPlainTextBytes);
            var plainTextBytesMemoryStream = new MemoryStream(compressedPlainTextBytesMemoryStream.Capacity * 2 + 1024);

            using (GZipStream gZipStream = new GZipStream(compressedPlainTextBytesMemoryStream, CompressionMode.Decompress))
            {
                gZipStream.CopyTo(plainTextBytesMemoryStream);
            }

            var plainTextBytes = plainTextBytesMemoryStream.ToArray();

            return plainTextBytes;
        }

        private byte[] DeriveKey(string password)
        {
            byte[] newKeyBytes;
            if (password.Length > 0)
            {
                var passwordBytes = StringToArrayBuffer(password);
                newKeyBytes = new byte[this.key.Length + passwordBytes.Length];
                this.key.CopyTo(newKeyBytes, 0);
                passwordBytes.CopyTo(newKeyBytes, this.key.Length);
            }
            else
            {
                newKeyBytes = new byte[this.key.Length];
            }

            var pdb = new Pkcs5S2ParametersGenerator(new Org.BouncyCastle.Crypto.Digests.Sha256Digest());
            pdb.Init(newKeyBytes, salt,
                iterationCount);
            var derivedKey = (KeyParameter)pdb.GenerateDerivedMacParameters(keySize);
            return derivedKey.GetKey();
        }

        private string ToPseudoBase58(byte[] derivedKey)
        {
            var result = "";

            foreach (var derivedKeyByte in derivedKey)
            {
                result += ToBaseX(derivedKeyByte, PseudoBase58);
            }

            return result;
        }

        private byte[] FromPseudoBase58(string derivedKey)
        {
            List<byte> result = new();

            foreach (char character in derivedKey)
            {
                result.Add(FromBaseX(character.ToString(), PseudoBase58).ToByteArray().First());
            }

            var size = Math.Ceiling(result.Count / 32d) * 32;

            for (var i = result.Count; i < 32; i++)
            {
                result.Insert(0, (byte)0);
            }

            return result.ToArray();
        }

        public static string ToBaseX(BigInteger number, string baseX)
        {
            int l = baseX.Length;
            string result = "";
            while (number > 0)
            {
                BigInteger remainder = number % l;
                int index = (int)remainder;
                if (index >= l)
                {
                    throw new ArgumentException($"Cannot convert {number} ToBaseX {baseX}");
                }
                result += baseX[index];
                number /= l;
            }
            return result;
        }

        public static BigInteger FromBaseX(string input, string baseX)
        {
            int l = baseX.Length;
            BigInteger result = -1;
            int pow = 0;
            foreach (char c in input)
            {
                int index = baseX.IndexOf(c);
                if (index < 0)
                {
                    throw new ArgumentException($"Cannot convert {input} FromBaseX {baseX}");
                }
                BigInteger additions = BigInteger.Pow(l, pow) * index;
                result += additions;
                pow++;
            }
            return result;
        }
        
        private string ArrayBufferToString(byte[] messageArray)
        {
            var message = "";
            for (var i = 0; i < messageArray.Length; i += 1) {
                message += ((char)messageArray[i]);
            }
            return message;
        }

        private byte[] StringToArrayBuffer(string message) {
            byte[] messageArray = new byte[message.Length];
            for (var i = 0; i < message.Length; i += 1) {
                messageArray[i] = (byte)message[i];
            }
            return messageArray;
        }
    }
}
