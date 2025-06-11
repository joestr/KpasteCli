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
        public void TestEncryptAndDecryptWithAsciiCharacters()
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
        public void TestEncryptAndDecryptWithSpecialCharacters()
        {
            var message = "Well, let's test some Äöü. How about some ß or ẞ? Maybe even ¯\\_(ツ)_/¯? How do you like that?";
            var password = "1234";

            var kPasteCrypto = new KPasteCrypto();
            var res = kPasteCrypto.Encrypt(message, password);

            var kPasteCryptoDecrypt = new KPasteCrypto(res.Key, res.Vector, res.Salt);
            var decryptedText =
                kPasteCryptoDecrypt.Decrypt(res.Message, password);

            Assert.AreEqual(message, decryptedText);
        }

        [Test]
        public void TestSendAndReceiveWithAsciiCharacters()
        {
            var plainText = "text only text and just text";

            var paste = new Paste();

            var kPasteCrypto = new KPasteCrypto();
            var res = kPasteCrypto.Encrypt(plainText, "1234");

            var kPaste = new Paste.NewPasteRequestDto()
            {
                Burn = true,
                Vector = res.Vector,
                Salt = res.Salt,
                Data = res.Message,
                Password = true,
                Validity = "1d"
            };

            Paste.NewPasteResponseDto response = null;

            Assert.DoesNotThrow(() =>
            {
                response = paste.SendPaste(kPaste);
            });

            Paste.GetPasteResponseDto getResponse = null;
            getResponse = paste.ReceivePaste(response.Data);

            var decryptedText = kPasteCrypto.Decrypt(getResponse.Data.Data, "1234");

            Assert.AreEqual(plainText, decryptedText);
        }

        [Test]
        public void TestSendAndReceiveWithSpecialCharacters()
        {
            var plainText = "text üß ẞ ʃ ¯\\_(ツ)_/¯ lel";

            var paste = new Paste();

            var kPasteCrypto = new KPasteCrypto();
            var res = kPasteCrypto.Encrypt(plainText, "1234");

            var kPaste = new Paste.NewPasteRequestDto()
            {
                Burn = true,
                Vector = res.Vector,
                Salt = res.Salt,
                Data = res.Message,
                Password = true,
                Validity = "1d"
            };

            Paste.NewPasteResponseDto response = null;

            Assert.DoesNotThrow(() =>
            {
                response = paste.SendPaste(kPaste);
            });

            Paste.GetPasteResponseDto getResponse = null;
            getResponse = paste.ReceivePaste(response.Data);

            var decryptedText = kPasteCrypto.Decrypt(getResponse.Data.Data, "1234");

            Assert.AreEqual(plainText, decryptedText);
        }
    }
}