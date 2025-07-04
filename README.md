# KpasteCli

Pasting and receicing files to [infomaniak's]([https://](https://www.infomaniak.com/en)) service [kPaste](https://kpaste.infomaniak.com/).

## Build

1. Install .NET Core
2. Clone this repository with `git clone https://github.com/joestr/KpasteCli.git`
3. Run `dotnet build`

## Usage

Create new paste with file:
```
> .\KpasteCli.exe new --filename .\somefile
https://kpaste.infomaniak.com/???#???
>
```

Get paste and print it to standard output:
```
> .\KpasteCli.exe get --url "https://kpaste.infomaniak.com/???#???"
text
>
```

## Contributing

See [contributing guide](./CONTRIBUTING.md).

## License

See [license file](./LICENSE.txt).
