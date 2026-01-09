# The Execution of the plan of ISHRemote v8.2

This page will try to track work in progress. And because I work on it in free time, it will help trace how I got where I am in the first place plus what is next. Inspired by [ThePlan-ISHRemote-8.2.md](./ThePlan-ISHRemote-8.2.md) although that one does not mention [MCP](https://modelcontextprotocol.info) at all :)

# MCP - What if you can steer an LLM to use ISHRemote properly?

[Add cmdlet Start\-IshRemoteMcpServer and supporting resources for MCP experimentation · Issue \#213 · RWS/ISHRemote](https://github.com/RWS/ISHRemote/issues/213)

Allow the ISHRemote library to register itself as a local `stdio` transport Mcp server `ISHRemoteMcpServer`. Overall as a thin client layer that pushes the heavy lifting via PowerShell over ISHRemote and in turn HTTPS to a server. Hat tip to [dfinke/PSMCP](https://github.com/dfinke/PSMCP) offering a generic PowerShell library wrapper. The idea is to make ISHRemote offer a self-contained MCP solution.

Main tasks are
- [ ] Revisit all ISHRemote cmdlet help to be more self-contained and actionable so that an LLM knows what to do next. And perhaps a human as well :)
- [ ] Add experimental auxiliary scripts to support [Model Context Protocol](https://modelcontextprotocol.io/specification/versioning) like `Invoke-IshRemoteMcpHandleRequest.ps1`, `Register-IshRemoteMcpTool.ps1`, `Register-IshRemoteMcpResources.ps1` or alike `Start-IshRemoteMcpServer.ps1`


## Install

### From PowerShell Gallery

```
{
    "servers": {
        "IshRemoteMcpServer": {
            "type": "stdio",
            "command": "pwsh",
            "args": [                
                "-NoProfile",
                "-Command",
                "& { Start-IshRemoteMcpServer -CmdletsToRegister (Get-Command -Module ISHRemote -ListImported -CommandType Cmdlet).Name -LogFilePath \"D:\\GITHUB\\ISHRemote\\IshRemoteMcpServer.log\" }"
            ]
        }
    }
}
```

### While developing

```
{
    "servers": {
        "IshRemoteMcpServer": {
            "type": "stdio",
            "command": "pwsh",
            "args": [                
                "-NoProfile",
                "-Command",
                "${workspaceFolder}\\Debug-IShRemoteMCPServer.ps1 }"
            ]
        }
    }
}
```

## Experimented with PSMPC Module

```powershell
Install-PSResource -Name PSMCP -Repository PSGallery
CD D:\GITHUB\ISHRemote\MCP
New-MCP -Path .\ISHRemoteMcpServer -ServerName 'ISHRemoteMcpServer'
```
PSMCP `Register-MCPTool.ps1` does not implement 'description'. There is `Get-Help` that prefers .SYNOPSIS over .DESCRIPTION but eventually it puts the cmdlet name in the description for `tools/list` endpoint

Edit `C:\Users\ddemeyer\Documents\PowerShell\Modules\PSMCP\0.1.2\Public\Register-MCPTool.ps1`
1. Prefer Description over Synopsis
2. Add examples

Submitted [In v0\.12 Register\-MCPTool uses the cmdlet name as description for the tools/list endpoint · Issue \#7 · dfinke/PSMCP](https://github.com/dfinke/PSMCP/issues/7)


# Issues


## Extend ISHRemote help

This help rewrite would benefit [dfinke/PSMCP: PSMCP turns your PowerShell scripts into intelligent, conversational services—zero YAML, zero APIs, zero headaches\.](https://github.com/dfinke/PSMCP) usage for experimenting. And improves the `tools/list` that is advised by [Tools – Model Context Protocol （MCP）](https://modelcontextprotocol.info/docs/concepts/tools/)

- [ ] Extend description with a sentence per parameter set and its purpose
- [ ] Consistently remove the New-IshSession line in examples
- [ ] Every parameter should hold the default value, and mention which cmdlet to use to generate the parameter like Set-IshMetadataField
- [X] Handled over markdown file to be passed over `instructions`
    - [X] Set-IshMetadataField and Requested/Filter should hold in their description how to use Get-IshTypeFieldDefinition per object types. Exactly describe the name and level fields per object types and how types relate to cmdlets you can use. In some way logical-version-language and none-level needs to be explained.
    - [X] Set-IshMetadataFilterField like and clike operator use wildcard percentage(`%`)
    - [X] Get-IshTypeFieldDefinition should describe per object type (and synonyms like DocumentObj, content object, topic, map, template, other, etc) or Publication/PublicationOutput
    - [X] Get-IshTypeFieldDefinition should describe how to interpret the columns. So one sentence per column like 'MM' is short for mandatory and multi-value; the first 'M' indicates that the field is required and the second 'm' indicates that field is single value or can contain multiple values.
    - [X] `Get-IShUser` without parameters is the shallow whoami call that checks if you are still authenticated. If not do a New-IShession again.

## Duplicate PSMCP code into ISHRemote

Overall [PSMCP/Public at main · dfinke/PSMCP](https://github.com/dfinke/PSMCP/tree/main/Public) only contains three methods...
1. `Invoke-HandleRequest.ps1` that could become `Invoke-IshRemoteMcpHandleRequest.ps1`, where potenially a prompt/resource system can be hardcoded added as described on [Prompts – Model Context Protocol （MCP）](https://modelcontextprotocol.info/docs/concepts/prompts/)
2. `New-MCP.ps1` that could become `New-IshRemotehMcpServer.ps1` to .vscode, mcp.json holding 'ISHRemoteMcpServer' (perhaps just documentation)
3. `Register-MCPTool.ps1` that could become `Register-IshRemoteMcpTool.ps1` ... could be generated compile time from the xml documentation.
   1. Surpress all `-IshSession` parameters, not required to pass $ishSession every time, use the implicit one.
   2. Tools Annotations as specified in [Schema Reference](https://modelcontextprotocol.io/specification/2025-06-18/schema#toolannotations) like destructiveHint, idempotentHint, readOnlyHint, openWorldHint (false) could be added based on PowerShell verbs next to title of course.
4. Perhaps in turn `Register-IshRemoteMcpPrompt.ps1` and `Register-IshRemoteMcpResources.ps1`)
5. `Start-McpServer.ps1` can hold the dedicated edition for ISHRemote like `Start-IshRemoteMcpServer.ps1` which in essence loads ISHRemote module and registers the allowed cmdlets, so MCP Tools and perhaps prompts and resources. This will be called in the `mcp.json` and holds the while true loop.

Is 'inspect' required?

Debugging for now from D:\GITHUB\ISHREMOTE>Copy-Item -Path Source\ISHRemote\Trisoft.ISHRemote\Scripts -Destination Source\ISHRemote\Trisoft.ISHRemote\bin\Debug\ISHRemote\ -Force -Recurse; Import-Module D:\GITHUB\ISHRemote\Source\ISHRemote\Trisoft.ISHRemote\bin\Debug\ISHRemote\ISHRemote.psm1 -Force; start-ishremotemcpserver -ActivateWhileLoop $false

More debugging is: 
    1. pwsh -NoProfile -Command "& { Copy-Item -Path "D:\GITHUB\ISHRemote\Source\ISHRemote\Trisoft.ISHRemote\Scripts" -Destination "D:\GITHUB\\ISHRemote\Source\ISHRemote\Trisoft.ISHRemote\bin\Debug\ISHRemote\" -Force -Recurse;Import-Module "D:\GITHUB\\ISHRemote\Source\ISHRemote\Trisoft.ISHRemote\bin\Debug\ISHRemote\ISHRemote.psm1" -Verbose -Debug -Force;Import-Module "D:\GITHUB\\ISHRemote\Source\ISHRemote\Trisoft.ISHRemote\bin\Debug\ISHRemote\net6.0\Trisoft.ISHRemote.dll" -Verbose -Debug -Force;Get-IshVersion;(Get-Command -Module ISHRemote -ListImported -CommandType Cmdlet).Name;(Get-Help New-IShSession).Description.Text;(Get-Help New-IShSession).Description;Start-IshRemoteMcpServer -CmdletsToRegister (Get-Command -Module ISHRemote -ListImported -CommandType Cmdlet).Name -ActivateWhileLoop 0 -LogFilePath "D:\GITHUB\ISHRemote\IshRemoteMcpServer.log" }"
    2. pwsh -NoProfile -Command "& { $env:PSModulePath=$env:PSModulePath + \";\" +  \"D:\GITHUB\ISHRemote\Source\ISHRemote\Trisoft.ISHRemote\bin\Debug\ISHRemote\";Get-IshVersion;(Get-Command -Module ISHRemote -ListImported -CommandType Cmdlet).Name;(Get-Help New-IShSession).Description.Text;(Get-Help New-IShSession).Description;Start-IshRemoteMcpServer -CmdletsToRegister (Get-Command -Module ISHRemote -ListImported -CommandType Cmdlet).Name -ActivateWhileLoop 0 -LogFilePath "D:\GITHUB\ISHRemote\IshRemoteMcpServer.log" }"



## Add MCP Prompt

The one big prompt is New-IshSession as you need that to get started, asking for a url, but could it not also force loading the resources... this way the LLM knows the context.

## Add MCP Resource

So generate [Resources](https://modelcontextprotocol.io/specification/2025-06-18/server/resources) that hold InfoShare concepts and best practices. Could even 'describe' based on `Get-IshTypeFieldDefinition` for your connect system what the mandatory and sensible fields are per object type - almost creating forms over [Prompts](https://modelcontextprotocol.io/specification/2025-06-18/server/prompts).
This can be separated files for ease of the user, but the must-load (1.0) could be holding everything.

See also /FieldHandlingResource.cs?at=refs%2Fheads%2FCreate_TridionDocs_MCP_Solution

```json
{
  "uri": "file:///IshRemoteMcpGetStarted.md",
  "name": "IshRemoteMcpGetStarted.md",
  "title": "Explains requirement of New-IShSession (could be a prompt)",
  "description": "his can be used by clients to improve the LLM’s understanding of available resources. It can be thought of like a “hint” to the model.",
  "size": "This can be used by Hosts to display file sizes and estimate context window usage.",
  "mimeType": "text/markdown",
  "annotations": {
    "audience": ["assist"],
    "priority": 1.0,
    "lastModified": "2025-01-12T15:00:58Z"
  }
}
```
Where priority 1.0 means required, is hopefully always read by the host. Others could be
- 0.9 BestPractices.md that explains the PowerShell verbs, so read and write. Logical-Version-Language and None levels. That Find could have performance issues. Do not use -IshSession parameter. 
- 0.8 TypeFieldSetup.md which could be generated just in time holding typical create / update / read blocks... allows easier field+level autocompletion for the Set-IshMetadataField and alike cmdlets.

Resources could also be editor template like files as DITA Map or DITA Topic.

Clearly mention PowerShell so use the provided parameters for all tools and not some json object overload
Prefer table result over Select columns
Do not use parameter -IshSession to pass variable $ishSession, it is superfluous

Set-IshMetadataFilterField like and clike operator use wildcard percentage(`%`)

Instead of making up field names, I expected you to source it from that Get-IshTypeFieldDefinition cmdlet

Suggest this McpResources as a must read over `instructions`, see https://modelcontextprotocol.io/specification/2025-06-18/schema#initializeresult-instructions


## Create script Register-IshRemoteMcpTool at ISHRemote compile time

As the set of cmdlets of ISHRemote is fixed, the json to seed the LLM for McpTools (and probably also McpResources) can be generated up front. Currently that takes 40 seconds or so.

Instead of generated from Get-Help on ISHRemote, directly convert Trisoft.ISHRemote.dll-Help.xml into the requested JSON. Perhaps over .NET code instead of scripting.

- Add tag `[IgnoreForMcp]` or `[ScriptUasegeOnly]` to cmdlet description, parameters (like ishsession) and examples /remarks
- Also remove common parameters from mcp help 


Do make sure to add some good basic self-service cmdlets in the JSON like `Get-Help`.

## Only works on PS7+

Mcp cmdlets should check platform and only work on pwsh 7, warn on ps5.1 or preferably not available which include ice help.
Script cmdlets should have help 
So most simple way is to have psm1 export the extra scripts only on pwsh 7, perhaps better a begin() with platform check that throws or points to documentation what to do now.

## Invoke-IshRemoteMcpHandleRequest to hint McpResources upon error

When an McpTool call results in a (PowerShell) error like ParseError or failed cmdlets because cmdlet parameter combinations. Append an instruction 'see docs://tools for more information' to instruct the LLM to download this McpResource for proper usagelike 


## Known Issues

- Where are modules loaded from `$(Get-Module ISHRemote -ListAvailable | select-object).path`
- Very very painful, but `import-module ....psm1` or even `import-module ....Trisoft.InfoShare.dll` does not seem to 
    - properly load the cmdlets. The `Get-Command` gave irregular results.
    - properly load the help breaking `Get-Help` for cmdlet descriptions and sometimes only for parameter descriptions (like description of `IShSession` parameter).
    - This means that best way to resolve this was actually make the `ISHRemote` module properly available in `%USERPROFILE%\Documents\PowerShell\Modules\ISHRemote\8.2.0\ISHRemote.psd1` where a simple `import-module ISHRemote` does the job - it loads all cmdlets with its help.
    - In turn `Start-IshRemoteMcpServer.ps1` function takes the cmdlets list from outside as a parameter coming from a regular `.PS1` script, and loading within that function gave irregular results for `Get-Command` and `Get-Help`
- Script `.\.vscode\Debug-IShRemoteMCPServer.ps1` allows to debug by either a variation of `import-module` and the `-ActivateWhileLoop $true` which is required to have a *listening* MCP Server.
- Visual Studio Code shows a `Continue` button after every *MCP Tool*, even the non-destructive or idempotent ones like `Get-Help`. Seemingly it is an all-or-nothing in vscode for now as described on [Use agent mode in VS Code](https://code.visualstudio.com/docs/copilot/chat/chat-agent-mode#_manage-tool-approvals).

## References
- [MCP Server Best Practices: Production\-Grade Development Guide \| MCPcat](https://mcpcat.io/blog/mcp-server-best-practices/)
  - Instead of long list of cmdlets, so MCP Tools, sorted by verb-noun; you can prefix them with the noun again like ${category}/${tool} so `DocumentObj/Add-IShDocumentObj` or `FileProcessor/...`
- [Automatic MCP Resources inclusion to the context · Issue \#260689 · microsoft/vscode](https://github.com/microsoft/vscode/issues/260689)