# Contributing

Thanks for looking. This is a small repository with a large blast radius. Several independently
released products compile against these three libraries, so a change to the public surface is
reviewed against that rather than against the size of the diff.

## Prerequisites

- **.NET 8 SDK.** A `global.json` pins the 8.0 feature band, so a machine carrying only a newer
  SDK is told so by name rather than silently building against a different toolchain.
- **Network access to nuget.org** on first restore, for `Febris.XApi.Models` and the third-party
  packages.

No database, no Docker, no platform requirement. It builds on Linux, macOS and Windows.

## Build and test

```bash
dotnet restore
dotnet build -c Release
dotnet test -c Release
```

The suite lives in `tests/FebrisSharedServicesTests` and covers all three libraries. It travels
with them deliberately: a test project that lives apart from the code it tests is a test project
that stops being run when the two are released separately.

## The keystone is a package, not source

`Febris.ModelLibrary` takes a `PackageReference` on `Febris.XApi.Models`. Do not replace that with
a `ProjectReference`, and do not vendor the keystone's source into this repository.

Both are Apache-2.0, so this is not a relicensing hazard. It is a source-of-truth one. That
package is separate so other people can depend on the xAPI contract on its own release line, and
a vendored copy here would silently fork it.

If you need a change in the keystone, it goes in that repository and arrives here as a version
bump.

## Changing the public surface

- **Adding a type or a member** is usually fine.
- **Renaming or removing anything public** breaks every consumer at once, and they do not all
  release together. Open an issue first so the sequencing can be worked out.
- **A change to an enum's numeric values is a wire change**, not a rename. Values are persisted
  and travel between tiers. Adding a member at the end is safe. Reordering is not.

## Testing

Every change ships with the test that pins it. The suite is the only thing standing between a
change here and four downstream products discovering it at runtime.

If a change cannot be covered by a test, say so in the pull request and say what you ran instead.

## Style

- One logical change per pull request.
- Match the surrounding file. The codebase is not uniform and there is no formatter gate.
- New first-party source files carry the two-line SPDX header the existing files carry.

## Reporting a security issue

Do not open a public issue. See [SECURITY.md](SECURITY.md) for the private reporting channel.

## Licence

By contributing you agree that your contributions are licensed under Apache-2.0, the same
licence as the project. See [LICENSE](LICENSE).
