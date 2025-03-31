using CommandLine;

namespace kpaste_cli.Objects;

[Verb("get", HelpText = "Get a paste.")]
public class CliVerbGet
{
    [Option("url", Required = true, HelpText = "The URL of the paste.")]
    public string Url { get; set; }

    [Option("password", Required = false, Default = "", HelpText = "The password the paste has been encrypted with.")]
    public string Password { get; set; } = "";

    [Option("filename", Required = false, Default = "", HelpText = "Where to output the file.")]
    public string FileName { get; set; } = "";
}