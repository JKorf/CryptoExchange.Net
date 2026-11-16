# Working on CryptoExchange.Net

This repository contains shared exchange-client infrastructure and contracts. Consumer guidance lives in the [library skill](.claude/skills/cryptoexchange-net/SKILL.md); read the relevant reference when changing consumer-facing behavior. [llms.txt](llms.txt) is the documentation index.

## Source layout

- [Clients](CryptoExchange.Net/Clients) and [client interfaces](CryptoExchange.Net/Interfaces/Clients): base REST and socket clients, not concrete exchange endpoints.
- [Shared API V2](CryptoExchange.Net/SharedApis/V2), [capability references](CryptoExchange.Net/SharedApis/CapabilityReferences), and [shared models](CryptoExchange.Net/SharedApis/Models): portable contracts, discovery, and data representations.
- [Authentication](CryptoExchange.Net/Authentication), [requests](CryptoExchange.Net/Requests), [rate limiting](CryptoExchange.Net/RateLimiting), and [sockets](CryptoExchange.Net/Sockets): signing and transport infrastructure.
- [Order books](CryptoExchange.Net/OrderBook), [trackers](CryptoExchange.Net/Trackers), and [token management](CryptoExchange.Net/TokenManagement): stateful helpers.
- [Unit tests](CryptoExchange.Net.UnitTests), [examples](Examples/README.md), and [Web3 project](CryptoExchange.Net.Web3): tests, consumers, and optional signing functionality.

## Implementation

Follow neighboring code for public API shape, XML documentation, nullable annotations, cancellation, logging, and asynchronous disposal. Use `ConfigureAwait(false)` in library awaits where required by the analyzer. Preserve public compatibility across the target frameworks in the [project](CryptoExchange.Net/CryptoExchange.Net.csproj).

Changes here affect multiple independently released exchange libraries. Keep transport infrastructure independent of exchange-specific endpoints, symbols, credentials, and account semantics. Shared capability contracts must express accurate request support, quantity units, trading modes, authentication requirements, and transport-specific results. Update descriptors, options, and relevant tests together when adding or changing a capability. Retain V1 compatibility where existing consumers rely on it.

Do not edit generated build output. Keep package versions, target frameworks, signing, and release configuration outside unrelated changes.

## Validation

Run commands from the repository root using the SDK selected by CI:

```powershell
dotnet restore CryptoExchange.Net.sln
dotnet build CryptoExchange.Net.sln --no-restore
dotnet test CryptoExchange.Net.UnitTests/CryptoExchange.Net.UnitTests.csproj --no-build
```

Choose focused tests while iterating; complete the checks relevant to the change. The core project builds all of its declared target frameworks in both Debug and Release. To check the distributable library and skill:

```powershell
dotnet build CryptoExchange.Net/CryptoExchange.Net.csproj -c Release
dotnet pack CryptoExchange.Net/CryptoExchange.Net.csproj -c Release --no-build
```

Compile changed consumer snippets with the concrete exchange dependencies they use. Build the [complete Shared API example](Examples/SharedClients/SharedClients.csproj) without running it to validate API usage. Running examples can contact exchanges; compilation needs no credentials or exchange operations.

The authoritative skill is under `.claude/skills/cryptoexchange-net`. Its five files must appear under `skills/cryptoexchange-net/` in the NuGet archive, with identical contents and no duplicated path segments. Validate relative links and compile changed snippets when updating it. Keep checkout-only links labeled, since repository sources are absent from the packaged skill.
