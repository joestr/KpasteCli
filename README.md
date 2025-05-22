# kpaste-cli

Pasting files to [infomaniak's]([https://](https://www.infomaniak.com/en)) service [kPaste](https://kpaste.infomaniak.com/).

## Build

1. Install .NET Core
2. Clone this repository
3. Run `dotnet build`

## Usage

Create new paste with file:
```
> .\kpaste-cli.exe new --filename .\somefile
https://kpaste.infomaniak.com/???#???
>
```

Get paste and print it to standard output:
```
> .\kpaste-cli.exe get --url "https://kpaste.infomaniak.com/???#???"
text
>
```

## Caveats
It isn't possible to load a paste which has been pasted encrypted with the Web UI.

This has something to do with the fact that the JavaScript isn't using a MAC during the encryption.

It is possible to view pastes on the Web UI, since the JavaScript can handle encrypted messages with a MAC.
