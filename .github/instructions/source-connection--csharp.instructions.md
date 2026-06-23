---
applyTo: "Source/ISHRemote/Trisoft.ISHRemote/Connection/**/*.cs"
description: "Structure and intent of the ISHRemote Connection layer: a deliberately self-contained authentication + proxy stack (WS-Trust/WS-Federation, OpenIdConnect over SOAP and over REST/OpenAPI, with Client Credentials or interactive System Browser) that should be copyable to other C# projects with minimal change — and why changes must stay inside this folder."
---

# ISHRemote Connection Layer Conventions

The `Connection/` folder is the **authentication + service-proxy stack** that sits underneath
`IshSession`. It discovers how a Tridion Docs server is configured, obtains the right credential
(WS-Trust SAML token, or an OpenIdConnect access/bearer token), and hands back ready-to-call SOAP
proxies and OpenAPI clients. `IshSession` (in `Objects/Public/IshSession.cs`) is the **only**
intended consumer: it builds one `HttpClient`, fills a parameters bag, news up one of the three
connection front-ends, and then calls the `GetXxx25Channel()` / `GetOpenApi…Client()` accessors.

> **Legacy vs future (investment).** All three flavours must keep working — ISHRemote still runs on
> older InfoShare (e.g. 13.0.2, 14.0.4) over **`WcfSoapWithWsTrust`**, which is therefore
> **maintain-only / bug-fix-only** here (the product removes it in 16.0.0). The SOAP proxies are
> **deprecated by the product**: `WcfSoapWithOpenIdConnect` stays broadly useful on 15.x and remains
> in 16.0.0 but is deprecated, while **`OpenApiWithOpenIdConnect` (REST) is the future** (parity with
> OIDC-over-SOAP at 15.3.0). Keep the WS-Trust front-end healthy, but **don't pour new feature work
> into it** — see the repo-wide `.github/copilot-instructions.md` "Legacy & where to invest less".

## 0. Portability is the whole point — keep changes inside this folder
These files are written to be **standalone**. Copy the folder into another C# project and, with only
a handful of adaptations (see §7), you get **all authentication flavours** against the InfoShare /
Access Management stack:
- **WS-Trust / WS-Federation (active)** SOAP — `WindowsMixed` and `UserNameMixed`.
- **OpenIdConnect on top of SOAP/WCF** — the OIDC bearer token is wrapped so it rides over WCF.
- **OpenIdConnect on top of REST/OpenAPI** — the bearer token goes on the `Authorization` header.
- Each OIDC variant works with **Client Credentials** (`ClientId` + `ClientSecret`, non-interactive)
  **or** an **interactive System Browser** flow (Authorization Code + PKCE).

**Golden rule:** make every change *within* `Connection/`. The layer is intentionally decoupled from
the cmdlets and from `IshSession`. If a change seems to require editing anything **outside** this
folder — `IshSession`, cmdlets, `Trisoft.ISHRemote.csproj`, the generated proxies — **stop and ask
the implementer first.** A leak past this boundary means the abstraction is breaking and the design
owner needs to decide. Likewise, don't pull cmdlet/business concepts *into* this folder.

## 1. License header (mandatory, verbatim)
Every `.cs` starts with the Apache 2.0 header exactly as in the neighbouring files (historical
`Copyright (c) 2014 ... SDL Group` text — copy it, don't "modernize" it; tooling checks it, see
[Add-SDLOpenSourceHeader.ps1](../../Source/Tools/PowerShell/Add-SDLOpenSourceHeader.ps1) /
[Test-SDLOpenSourceHeader.ps1](../../Source/Tools/PowerShell/Test-SDLOpenSourceHeader.ps1)).

## 2. Shared conventions across every file
- Namespace is always `Trisoft.ISHRemote.Connection`.
- Concrete connections, parameter bags, the tokens DTO and the bearer-credentials helpers are
  `internal sealed`; the shared base is `internal abstract`. **Exceptions to know:**
  `InfoShareOpenIdConnectSystemBrowser` is `public` (it implements Duende's `IBrowser`),
  `InfoShareOpenIdConnectLocalHttpEndpoint` and `IshConnectionConfiguration` are `internal` (not
  sealed). Don't widen visibility without a reason.
- Constructor signature for the three connection front-ends is uniform:
  `(ILogger logger, HttpClient httpClient, <…ConnectionParameters> parameters)`. The `HttpClient` is
  **created once by `IshSession` and reused** (TLS/SSL already initialized) — never new up your own
  `HttpClient` here.
- Log through `Trisoft.ISHRemote.Interfaces.ILogger` only (`_logger.WriteDebug` / `WriteVerbose`);
  no `Console`, no `Write-Host`. Keep secrets out of logs — log `ClientSecret.Length` or a masked
  `new string('*', …)`, never the secret or a raw token.
- Triple-slash `///` summaries on classes and members (this project builds XML docs and treats
  warnings as errors in Release).
- URL hygiene matches the existing setters: normalize to a trailing `/`, and for OpenAPI strip the
  `OWcf`/`OCoreWcf` segment off `InfoShareWSUrl` before composing the `…/api` base.
- `IgnoreSslPolicyErrors` is honoured everywhere for self-signed dev servers; preserve that hook.

## 3. Multi-targeting: `#if` pragmas are load-bearing
The module ships `net48;net6.0;net10.0` and the WCF surface differs between .NET Framework and
CoreCLR, so the SOAP files are split with `#if NET48 / #else / #endif` (plus `#if NET6_0_OR_GREATER`
and `#if NET10_0_OR_GREATER` for narrower cases). Both arms must compile **and behave the same**:
- WS-Trust binding: `WS2007FederationHttpBinding` + manual `WSTrustChannel` on `net48` vs
  `WSFederationHttpBinding` (`System.ServiceModel.Federation`) on `net6.0+`.
- OIDC-over-SOAP token attach: `CreateChannelWithIssuedToken(WrapJwt(...))` on `net48` vs adding
  `BearerCredentials` endpoint behaviour on `net6.0+`.
- `InfoShareWcfSoapBearerCredentials.cs` is **entirely** `#if NET6_0_OR_GREATER` (the SAML2 wrapper
  uses APIs absent from `net48`). `net48` reaches the same result through its own code path.
- `net48` also needs the catch-all `ServerCertificateCustomValidationCallback`/`BackchannelHandler`
  for `/.well-known/openid-configuration` discovery. Keep these platform shims — don't "simplify"
  one arm away. If you touch one arm, update the other to match.

## 4. What each file is (and where to make a given change)
**Discovery**
- `IshConnectionConfiguration.cs` — parses `/ISHWS/connectionconfiguration.xml` (and the 15+
  `/ISHWS/owcf/…` variant) into `SoftwareVersion`, `ApplicationName`, `InfoShareWSUrl`,
  `AuthenticationType`, `IssuerUrl`. This drives protocol/flavour selection upstream.

**State (POCOs — no behaviour)**
- `InfoShareWcfSoapWithWsTrustConnectionParameters.cs` — inputs for the WS-Trust SOAP path
  (`NetworkCredential`, URLs, timeouts, `IgnoreSslPolicyErrors`).
- `InfoShareOpenIdConnectConnectionParameters.cs` — inputs **shared** by *both* OIDC connections
  (SOAP and OpenAPI): `ClientAppId`, `Scope`, `RedirectUri`, `ClientId`/`ClientSecret`, `Tokens`,
  timeouts incl. `SystemBrowserTimeout`. Adding an OIDC knob? It belongs here so both front-ends see
  it.
- `InfoShareOpenIdConnectTokens.cs` — small DTO holding `AccessToken`/`IdentityToken`/`RefreshToken`
  + `AccessTokenExpiration`.

**OIDC token engine**
- `InfoShareOpenIdConnectConnectionBase.cs` — the single source of token logic shared by the two
  OIDC connections: `GetTokensOverClientCredentialsAsync` (Client Credentials),
  `GetTokensOverSystemBrowserAsync` (interactive Authorization Code + PKCE via Duende `OidcClient`),
  `RefreshTokensAsync`, and `GetAccessToken()` which transparently refreshes using the
  `RefreshBeforeExpiration` skew (default 3 min) and decides refresh-vs-client-credentials by whether
  `ClientId`/`ClientSecret` are set. **Put shared token behaviour here**, not in a front-end.
- `InfoShareOpenIdConnectSystemBrowser.cs` — `IBrowser` implementation; launches the OS default
  browser cross-platform (Windows/Linux/macOS) for the interactive flow.
- `InfoShareOpenIdConnectLocalHttpEndpoint.cs` — the `127.0.0.1` `HttpListener` that catches the
  OIDC redirect callback (kept alive briefly to render the "you are signed in" page).
- `InfoShareWcfSoapBearerCredentials.cs` (`net6.0+` only) — wraps the OIDC JWT into a self-signed
  SAML2 token (`BearerCredentials` + token manager/provider/serializer) so a bearer token passes
  cleanly over WCF.

**Connection front-ends (what `IshSession` news up — one per protocol)**
- `InfoShareWcfSoapWithWsTrustConnection.cs` — dynamic (no `app.config`) WCF proxy generation
  secured by WS-Trust; exposes `GetXxx25Channel()` accessors and lazy issued token. Does **not**
  inherit the OIDC base.
- `InfoShareWcfSoapWithOpenIdConnectConnection.cs` — same dynamic WCF proxies, but secured by an
  OIDC bearer token (wrapped per §3); **inherits** `InfoShareOpenIdConnectConnectionBase`.
- `InfoShareOpenApiWithOpenIdConnectConnection.cs` — wraps the NSwag OpenAPI clients
  (`OpenApiISH30Client`, `OpenApiAM10Client`), setting the `Bearer` header from `GetAccessToken()`;
  **inherits** the OIDC base; `IDisposable`.

**Diagram**
- `__ConnectionClassDiagram.cd` — Visual Studio class diagram. **Keep it in sync** when you add,
  remove, or rename a class in this folder.

## 5. Adding / changing a connection class — checklist
- New auth *flavour*? Add a front-end that follows the `(ILogger, HttpClient, …Parameters)` ctor
  shape and (for OIDC) inherits `InfoShareOpenIdConnectConnectionBase`; reuse the existing parameters
  bag rather than inventing a parallel one.
- New SOAP service proxy? Follow the established `private … _xxxClient;` field +
  `GetXxx25Channel()` accessor + `const string Xxx25 = "Xxx25";` naming, and add it to **both**
  `#if` arms where the WS-Trust/OIDC SOAP files diverge.
- Shared token/refresh behaviour goes in the **base**, not duplicated across front-ends.
- Update `__ConnectionClassDiagram.cd` and keep triple-slash docs warning-clean.

## 6. Don't
- Don't edit anything outside `Connection/` to make a connection change work — ask the implementer
  (see §0).
- Don't create or dispose your own `HttpClient`; use the injected one.
- Don't drop or one-sidedly diverge a `#if NET48` / `#else` arm.
- Don't log tokens, passwords, or client secrets.
- Don't hand-edit the generated SOAP `*25ServiceReference` proxies or the NSwag OpenAPI clients from
  here.

## 7. Reusing this folder in another C# project (the standalone contract)
When you copy `Connection/` out, these are the only external seams to satisfy — by design:
- `Trisoft.ISHRemote.Interfaces.ILogger` — swap for your own logging abstraction.
- `Trisoft.ISHRemote.Exceptions` — used by `IshConnectionConfiguration`; bring it or replace it.
- The generated SOAP `*25ServiceReference` proxies (the WCF front-ends new these up) — point at your
  own generated service references.
- The NSwag OpenAPI clients `Trisoft.ISHRemote.OpenApiISH30` / `…OpenApiAM10`.
- NuGet: `Duende.IdentityModel` + `Duende.IdentityModel.OidcClient`, and `Newtonsoft.Json`.

If you keep those seams thin, the WS-Trust, OIDC-over-SOAP and OIDC-over-OpenAPI flavours — with
Client Credentials or interactive browser auth — all come across intact.
