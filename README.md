This is the first part of the Febris OSS release. It is safe to call this version 4 of the Febris platform. Many aspects of Version 3 had to be stripped out (The central hub, marketplace, developer system, accreditation system, micro-credentialing, CRM, LMS components that added centralized truth, and there may be a few parts that are now gone that previously existed that I cannot recall right this second) and I used Claude to create and cut that seam. If there are lingering parts, I apologize and I will fix it as soon as I can. I feel like I stretched Claude's capabilities while working on this project. AI was not used on any of the other version of Febris so some of these cuts may seem a little ragged but the entire system was built by one person so, please cut me a little slack.

Claude is far better at documenting code than I have ever been and I suspect between my naming conventions and Claude's documentation, this release will be easy to follow.

# febris-shared

**The shared kernel of the Febris platform. Three .NET 8 libraries that every tier compiles
against: enums, view models, and infrastructure services.**

This repository exists so that shared code has a neutral home. It is consumed by the self-hostable
training node, by the Windows desktop client suite, and by services that are not public. None of
those owns it.

---

## What is in here

| Project | Package | What it is |
|---|---|---|
| `shared/FebrisEnumLibrary` | `Febris.EnumLibrary` | the platform's enums, a leaf with no dependencies |
| `shared/FebrisModelLibrary` | `Febris.ModelLibrary` | view models and DTOs |
| `shared/FebrisSharedServices` | `Febris.SharedServices` | transport hardening, storage seam, mail, JWT signing-key provision, caching, CORS policy |
| `tests/FebrisSharedServicesTests` | not packaged | the suite covering all three |

The dependency graph is small and acyclic:

```
Febris.EnumLibrary     leaf
Febris.ModelLibrary    -> EnumLibrary, Febris.XApi.Models
Febris.SharedServices  -> ModelLibrary, EnumLibrary
```

## One external dependency that matters

`Febris.ModelLibrary` depends on **`Febris.XApi.Models`**, the xAPI contract package, which lives
in its own repository and is **Apache-2.0**. It is consumed here as a `PackageReference`, never
vendored.

Both this repository and the keystone are Apache-2.0, so consuming it as a package is not a
licensing requirement. It is a source-of-truth one: the keystone is a contract with its own
release line and its own consumers, and vendoring it here would fork it.

`Febris.XApi.Models` must be live on nuget.org before this repository can publish.

## Why this is its own repository

It was not always. These three libraries previously lived inside the training node and were
published from that repository's CI as a by-product of its release. That was reversed for three
reasons, in increasing order of force:

- **The consumer count was wrong.** The decision to house them in the node rested on there being
  one cross-repository consumer. Counted properly, `EnumLibrary` has 24 consuming projects,
  `ModelLibrary` 22 and `SharedServices` 22, and the node accounts for 4 of them.
- **A private product depended on a public repository's release schedule.** Shared code that a
  proprietary service consumes should not have to be changed in, and released from, the node's
  repository first.
- **The dependency graph never required it.** These libraries have zero edges into the node.
  Nothing made the node publish first, or twice. That ordering was an artefact of where the
  packaging lived, not of what depends on what.

## Versioning

Pre-1.0. These packages version on their own line and are not tied to any consumer's release
cadence, which is the main practical gain from the split. Once the surface stabilises they follow
semantic versioning, and a breaking change lands in a MAJOR.

Treat the public surface as a contract. More than one tier compiles against it, and several of
them are released separately from this one.

## Licence

**Apache-2.0.** See [LICENSE](LICENSE).

`Febris.XApi.Models`, the keystone this depends on, is Apache-2.0 as well. Depending on any of
these four packages puts no copyleft obligation on your code. Note that the Febris node itself is
AGPL-3.0, and that is a separate matter from these libraries.

## Security

Report vulnerabilities privately through this repository's Security tab. See
[SECURITY.md](SECURITY.md). Please do not open a public issue for a security bug.

`Febris.SharedServices` carries JWT signing-key provision, transport hardening and CORS policy, so
it is the part of this repository where a report matters most.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). A change here reaches every tier, so the bar for the
public surface is higher than the size of the codebase suggests.
