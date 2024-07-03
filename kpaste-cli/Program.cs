using kpaste_cli.Logic;

namespace kpaste_cli;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        var kpasecrypto = new KPasteCrypto(null, null, null);
        var res = kpasecrypto.crypt("ultrasicher", "strenggeheim");

        var kPaste = new Paste.NewPasteRequestDto()
        {
            Burn = false,
            Vector = res.Vector,
            Salt = res.Salt,
            Data = res.Message,
            Password = true,
            Validity = "1w"
        };

        var paste = new Paste();
        var pasteRes = paste.sendPaste(kPaste);

        // generated from the official site
        // https://kpaste.infomaniak.com/Fr8KL5A51-1_qSQ1CmeQVNHJBRdAdz3x#4zzsVPDgnuTKH7cj7vMshEtCLN7Ak9CSKbjuAhaLLEzM
        /*
            {
                "result": "success",
                "data": {
                    "id": "Fr8KL5A51-1_qSQ1CmeQVNHJBRdAdz3x",
                    "data": "\/WI2YMrF+Ufx\/W0lPXtz6XoMs+t78MnsumPaJcPaE+bRMNQ=",
                    "burn": false,
                    "password": true,
                    "vector": "7S3XKXcy8JxckY73FGc05A==",
                    "salt": "KQStFXXvzOo=",
                    "created_at": 1720018510,
                    "updated_at": 1720018510,
                    "expirated_at": 1720623310,
                    "deleted_at": null
                }
            }
        */

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

        Console.WriteLine($"https://kpaste.infomaniak.com/{pasteRes.Data}#{res.Key}");

        return;
    }
}