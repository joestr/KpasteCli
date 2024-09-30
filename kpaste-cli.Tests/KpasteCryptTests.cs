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

            // we generate
            // https://kpaste.infomaniak.com/O3h3mVVblBmf5BrSLPHz3lR12s9H2Z_H#N3F4z3B3A4x3K4K2P5V2J4N4U25G4oz433T15y4N4o3c93A3XJHXw4D3
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

            Assert.Inconclusive("The official site does not provide a MAC tagged message. This would fail.");

            // generated from the official site
            // https://kpaste.infomaniak.com/Fr8KL5A51-1_qSQ1CmeQVNHJBRdAdz3x#4zzsVPDgnuTKH7cj7vMshEtCLN7Ak9CSKbjuAhaLLEzM
            /*
                {
                    "result": "success",
                    "data": {
                        "id": "vUIauzncyB-TUORxFRQzRZlLTYoNSvRq",
                        "data": "h\/KyG1eRpxt8mT0kA5KsGCzZLRK\/AR9ggaKT4VfSU6A=",
                        "burn": false,
                        "password": true,
                        "vector": "SNim0V9uxLEp0A7qHgYdBA==",
                        "salt": "k5Md47OdHZA=",
                        "created_at": 1727692384,
                        "updated_at": 1727692384,
                        "expirated_at": 1727695984,
                        "deleted_at": null
                    }
                }
            */

            var kPasteCryptoDecrypt = new KPasteCrypto("6USpJVMMUvZDCXz7oUqPA5UMwy9QuH76nXmjLoMn2TD5", "SNim0V9uxLEp0A7qHgYdBA==", "k5Md47OdHZA=");
            var decryptedText =
                kPasteCryptoDecrypt.Decrypt("h/KyG1eRpxt8mT0kA5KsGCzZLRK/AR9ggaKT4VfSU6A=", "strenggeheim");
        }
    }
}