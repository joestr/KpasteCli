﻿using System.Text;
using CommandLine;
using KpasteCli.Logic;
using KpasteCli.Objects;
using Org.BouncyCastle.Crypto;

namespace KpasteCli;

class Program
{
    static void Main(string[] args)
    {
        Type[] types =
        {
            typeof(CliVerbNew),
            typeof(CliVerbGet)
        };

        Parser.Default.ParseArguments(args, types).WithParsed(Run).WithNotParsed(RunError);

        return;
    }

    private static void Run(object obj)
    {
        switch (obj)
        {
            case CliVerbNew options:
                RunCliVerbNew(options);
                break;
            case CliVerbGet options:
                RunCliVerbGet(options);
                break;
        }
    }

    private static void RunCliVerbNew(CliVerbNew options)
    {
        var validity = options.Validity;
        var password = options.Password;
        var isPasswordPresent = !string.IsNullOrEmpty(password);
        var burn = options.Burn;
        var fileName = options.FileName;
        var message = options.Message;
        var content = "";

        if (fileName != null)
        {
            if (File.Exists(fileName))
            {
                content = File.ReadAllText(fileName, Encoding.UTF8);
            }
            else
            {
                Console.Error.WriteLine("E1000: The file does not exist. (File location: \"{0}\")", fileName);
                Environment.Exit(1000);
            }
        }
        else if (message != null)
        {
            content = message;
        }
        else
        {
            Console.Error.WriteLine("E1001: There is no content to paste.");
            Environment.Exit(1001);
        }

        var kPasteCrypto = new KpasteCrypto();
        var encryptionResult = kPasteCrypto.Encrypt(content, password);

        var kPaste = new Paste.NewPasteRequestDto()
        {
            Burn = burn,
            Vector = encryptionResult.Vector,
            Salt = encryptionResult.Salt,
            Data = encryptionResult.Message,
            Password = isPasswordPresent,
            Validity = validity
        };

        var paste = new Paste();
        var pasteRes = paste.SendPaste(kPaste);

        Console.WriteLine($"https://kpaste.infomaniak.com/{pasteRes.Data}#{encryptionResult.Key}");
    }

    private static void RunCliVerbGet(CliVerbGet options)
    {
        var url = options.Url;
        var password = options.Password;
        var fileName = options.FileName;

        var pasteId = new UriBuilder(url).Path;
        pasteId = pasteId.Substring(1, pasteId.Length-1);
        var paste = new Paste().ReceivePaste(pasteId);

        var key = new UriBuilder(url).Fragment;
        key = key.Substring(1, key.Length-1);

        if (paste.Data.Password == true && password == "")
        {
            Console.Write("Password: ");
            password = Console.ReadLine();
        }

        if (password == null)
        {
            return;
        }

        var kPasteCrypto = new KpasteCrypto(key.Replace("\\/", "/"), paste.Data.Vector.Replace("\\/", "/"), paste.Data.Salt.Replace("\\/", "/"));
        var decryptionResult = "";

        try
        {
            decryptionResult = kPasteCrypto.Decrypt(paste.Data.Data.Replace("\\/", "/"), password);
        }
        catch (InvalidCipherTextException exception)
        {
            throw;
            Console.Error.WriteLine("E1002: The MAC (Message Authentication Code) check failed. This message is not properly encrypted.");
            Environment.Exit(1002);
        }

        if (fileName == "")
        {
            Console.WriteLine(decryptionResult);
        }
        else
        {
            File.WriteAllText(fileName, decryptionResult);
        }
    }

    /// <summary>
    /// Will be executed if an error occours whilst parsing the command line arguments.
    /// </summary>
    /// <param name="errors">The errors occoured during parsing.</param>
    /// <returns></returns>
    private static void RunError(IEnumerable<CommandLine.Error> errors)
    {
        if (errors.Any(x => x is not HelpRequestedError || x is not VersionRequestedError))
        {

        }
    }
}
