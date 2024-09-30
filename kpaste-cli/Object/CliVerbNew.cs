using CommandLine;

namespace kpaste_cli.Objects;

[Verb("new", HelpText = "Create a new paste.")]
public class CliVerbNew
{
    [Option("validity", Required = false, Default = "1d", HelpText = "How long the message should be accessible.")]
    public string Validity { get; set; } = "1d";

    [Option("burn", Required = false, Default = false, HelpText = "Burn the message after reading.")]
    public bool Burn { get; set; }

    [Option("password", Required = false, Default = "", HelpText = "An additional password to protect the message.")]
    public string Password { get; set; } = "";

    [Option("filename", Group = "Content", Required = true, HelpText = "An additional password to protect the message.")]
    public string? FileName { get; set; }

    [Option("message", Group = "Content", Required = true, HelpText = "The message.")]
    public string? Message { get; set; }
}