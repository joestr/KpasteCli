# KpasteCli

Pasting and receiving files to [infomaniak's](https://www.infomaniak.com/en) service [kPaste](https://kpaste.infomaniak.com/).

<a href="https://apps.microsoft.com/detail/9mxzj94hcc4f?referrer=appbadge&mode=direct" target="_blank" rel="noopener">
	<img src="https://get.microsoft.com/images/en-us%20light.svg" alt="Download it from Microsoft Store" width="200"/>
</a>

## Build

1. Install .NET Core
2. Clone this repository with `git clone https://github.com/joestr/KpasteCli.git`
3. Run `dotnet build`

## Usage

Create a new paste with the content of a file:
```
> KpasteCli.exe new --filename .\somefile
https://kpaste.infomaniak.com/???#???
>
```

Get a paste and print it to the standard output:
```
> KpasteCli.exe get --url "https://kpaste.infomaniak.com/???#???"
text
>
```

## Contributing

See [contributing guide](./CONTRIBUTING.md).

## License

See [license file](./LICENSE.txt).
