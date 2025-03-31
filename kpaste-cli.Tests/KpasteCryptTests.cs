using kpaste_cli.Logic;

namespace kpaste_cli.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestEncryptAndDecrypt()
        {
            var message = "A text to test the encryption and decryption.";
            var password = "1234";

            var kPasteCrypto = new KPasteCrypto();
            var res = kPasteCrypto.Encrypt(message, password);

            var kPasteCryptoDecrypt = new KPasteCrypto(res.Key, res.Vector, res.Salt);
            var decryptedText =
                kPasteCryptoDecrypt.Decrypt(res.Message, password);

            Assert.AreEqual(message, decryptedText);
        }

        [Test]
        public void TestEncryptAndSend()
        {

            
            var kPasteCrypto = new KPasteCrypto();
            var res = kPasteCrypto.Encrypt("text", "1234");

            var kPaste = new Paste.NewPasteRequestDto()
            {
                Burn = true,
                Vector = res.Vector,
                Salt = res.Salt,
                Data = res.Message,
                Password = true,
                Validity = "1d"
            };

            Assert.DoesNotThrow(() =>
            {
                var paste = new Paste();
                paste.SendPaste(kPaste);
            });

            /*
                {
                    "result": "success",
                    "data": {
                        "id": "O3h3mVVblBmf5BrSLPHz3lR12s9H2Z_H",
                        "data": "For+lljeY+YbsOEBORpIzyd0xoIIUVKGsFCe0\/yLww==",
                        "burn": false,
                        "password": true,
                        "vector": "YRGPYfUhv57A3VPO",
                        "salt": "6B3D2PH6b0w=",
                        "created_at": 1720018620,
                        "updated_at": 1720018620,
                        "expirated_at": 1720623420,
                        "deleted_at": null
                    }
                }
            */
        }

        [Test]
        public void TestReceiveAndDecrypt()
        {
            /* https://kpaste.infomaniak.com/wQd61amxePIxxCjwEv97eje7O2XTFB3f#3pPBbpJ8MB3ELzGRKYAcPrGtgGR7rdJzVkUUKVUvRNpZ */
            /*
                {
                    "result": "success",
                    "data": {
                        "id": "wQd61amxePIxxCjwEv97eje7O2XTFB3f",
                        "data": "B80iPuwAoGKjBoRgRr765h+FdkaytaDycJOIew==",
                        "burn": true,
                        "password": true,
                        "vector": "YxfTX4mKLEs1g7fM6+BPxg==",
                        "salt": "xJ\/tMs\/jgq8=",
                        "created_at": 1743419212,
                        "updated_at": 1743419212,
                        "expirated_at": 1743505612,
                        "deleted_at": 1743419221
                    }
                }
            */

            var kPasteCryptoDecrypt = new KPasteCrypto("3pPBbpJ8MB3ELzGRKYAcPrGtgGR7rdJzVkUUKVUvRNpZ", "YxfTX4mKLEs1g7fM6+BPxg==", "xJ/tMs/jgq8=");
            var decryptedText =
                kPasteCryptoDecrypt.Decrypt("B80iPuwAoGKjBoRgRr765h+FdkaytaDycJOIew==", "strenggeheim");

            Assert.AreEqual("text", decryptedText);
        }
    }
}