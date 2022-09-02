# The Plan of ISHRemote v7.0

This plan brings together input from several stakeholders and outlines where and how we intend to move with ISHRemote. This plan is not set-in-stone and will evolve as we work on the release based on what we learn. This learning includes feedback from people like you, so please let us know what you think!

## TLDR (Too Long; Didn't Read)...
The plan is to work on an ISHRemote v7+ that will work on PowerShell (Core) cross platform, so Windows/Linux/Mac. It will work on all existing Tridion Docs versions as it will use ASMX-SOAP with internal authentication - so Internal CMS User Profiles as the CMS owns the password. Later it can move to OpenAPI-REST with modern authentication. The below text shares the current way of thinking and provides a framework for upcoming GitHub source changes.

_Update 20220902: Internal authentication is still true, but Microsoft released an WCF-SOAP that offers WS-Trust protocol support that matches ISHRemote's needs. The below text referencing ASMX-SOAP is outdated._

![ISHRemote-7.0--ThePlan 1024x512](./Images/ISHRemote-7.0--ThePlan.gif)


## General...
ISHRemote is a PowerShell module on Tridion Docs Content Manager. Its goal is business automation on top of the Component Content Management System (Knowledge Center Content Manager, LiveContent Architect, Trisoft InfoShare). This library is constructed close to the "Web Services API" to:
- allow business logic automation ranging from triggering publishing into the continuous integration pipeline over legacy data correction up to provisioning
- show case code examples and API best practices

Its goal is still to be a valued toolbox. Besides open-sourced code samples on how to work with the API and the business code.


## The problem...
ISHRemote v0.x relies on the WCF-SOAP public API web services of the CMS product. These WCF-SOAP web services are protected by OASIS WS-Trust/WS-Federation authentication. This way ISHRemote respects exactly the same security paradigm as its peers, the rich client tools like Publication Manager.

Windows PowerShell, up to version 5.1, relies on Microsoft .NET Framework 4.5+ availability to offer WS-Trust claims-based support.

PowerShell (Core) 7.1+ and up relies on .NET (Core) 5+ as engine. Through various channels Microsoft has indicated that .NET (Core) will not make the necessary [WS-Trust artefacts](https://stackoverflow.com/questions/56739200/porting-servicedescription-code-to-net-core/56745112) (WsdlImporter, EndpointAddress, WSTrustChannelFactory,...) available from .NET Framework into .NET (Core) - so in turn for PowerShell (Core) to enable ISHRemote to work.


## The plan...
The plan is to work on an ISHRemote v7 that will work on PowerShell (Core) v7 - notice the starting version alignment. This module should run cross platform, so Windows/Linux/Mac. It will work on all existing Tridion Docs versions as it will use ASMX-SOAP with internal authentication - so Internal CMS User Profiles as the CMS owns the password.


## Milestone - Enable PowerShell (Core)...DONE
Branch within the ISHRemote project to a .NET Standard project (so [PowerShell Standard.Library 5.1](https://github.com/PowerShell/PowerShellStandard)). Thereby all .NET Framework artefacts will be dropped - most notably WCF-SOAP and WS-Trust, so in turn also the `IShSession.Application25` pre-authenticated WCF proxies.

Compatibility of the cmdlets is top priority, so that your scripts would need no or minimal adjustment. So cmdlet definitions like parameter groups, help and samples remain the same.

End result is an ISHRemote v7 version that can run in PowerShell 7.1+ and works on all existing Tridion Docs CMS versions using internal authentication (AuthenticationContext). Notice that ISHRemote version range 1-6 are there for continuation of the current ISHRemote based on Windows PowerShell if desired; picking up the latest version is as simple as: `Install-Module ISHRemote -Repository PSGallery -Scope CurrentUser -MaximumVersion 6`


## Milestone - Enable cross-platform PowerShell (Core)...
ISHRemote v7+ does some file handling, so code changes to make ISHRemote v7+ work on all PowerShell (Core) supported operating systems could be a next step. Make ISHRemote run on Linux, Mac and Windows.


## Milestone - Rewire to OpenAPI-REST...
When the CMS introduces its next public API successor based on OpenAPI-REST, a rewire will proof the mapping from ASMX-SOAP (and WCF-SOAP) into OpenAPI-REST.


## Milestone - Reintroduce Modern Authentication...
Initially we dropped WCF-SOAP protected by claims-based OASIS WS-Trust/WS-Federation authentication. ISHRemote changes are required to step into the Modern Authentication world (OpenIDConnect, OAuth, passive and active scenarios,...) using the new public OpenAPI-REST. See [Tridion Docs Architectural Runway (TXS2020) - Community](https://community.sdl.com/product-groups/sdl-tridion/tridion-docs/b/weblog/posts/sdl-tridion-docs-architectural-runway-txs2020)


## Suggestions...
Your feedback on planning is important. The best way to indicate the importance of an issue is to vote (👍) for that issue on GitHub. This data will then feed into the planning process for the next release.

In addition, please comment on this post if you believe we are missing something that is critical, or are focusing on the wrong areas.