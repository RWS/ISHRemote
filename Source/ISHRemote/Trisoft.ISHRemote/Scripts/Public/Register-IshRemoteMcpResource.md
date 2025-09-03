
# Introduction

IShRemoteMcpServer is a Model Context Provider implementation on top of PowerShell module IShRemote. The IShRemote module, and in turn all supporting PowerShell constructs, can be used to define an answer. Every IShRemote cmdlet has been made available as an MCP Tool to use; where the parameters of the cmdlet became the parameters of the MCP Tool.

The IShRemote module is used by many clients in PowerShell PS1 script files to achieve business automation on top of a Tridion Docs CMS (Content Management System). Such a Tridion Docs environment, often simplified to server, is typically identified by its `-WsBaseUrl "https://example.com/ISHWS/"` or variation.


# Getting Started

To execute these Mcp Tools, so IShRemote cmdlets, you need to create a `IshSession`. The cmdlet `New-IShSession` with provided `-WsBaseUrl` https url is triggered first. The result is an in-memory IShSession object. This means that you never have to explicitly mention parameter `-IShSession` or value `$ishSession` because every cmdlet takes that implicitly from memory.


# Rules

1. IShRemote PowerShell cmdlets can be recognized by their naming convention as the noun starts with `ISh`, so verb-`ISh`noun like `New-IShSession`. Most the cmdlets of IShRemote are made available as MCP Tools with the same or reduced parameter set - for example `-IshSession $ishSession` is superfluous.
2. Do not make up parameters for the cmdlets; and definitely not some JSON parameters. All parameters should exist and can be verified over the Mcp Tool description or by triggering a `Get-Help` on it.
3. All IShRemote cmdlets come with preformatted tables, there is no need to pipeline (`|`) these results in a `Format-Table`.
4. You always do a `Get-Help` for any ISHRemote cmdlet with option `-Detailed` so you get insights in its purpose, parameter sets which are allowed parameter combinations, the individual parameters explained and of course several typical examples. Do not ask for `Continue` for this Mcp Tool.


# Concepts


## Entities Hierarchy

Essentially the Tridion Docs, often called InfoShare, product uses the following entity types inside its database.
- Fields are in essence key value pairs, where the key is often prefixed with `F` short for field and `FISH` for product system fields. The value can be
    - `String` (`STRING`), where a string field is advised be lower than 1333 characters. Its database storage allows rich filtering over `like` (case-sensitive) and `cilike` (case-insensitive) operators.
    - `LongText` (`LONG TEXT`), is also a string field where the value can be over 1333 characters. However, the operators to filter are limited to `empty` or `notempty`
    - `ISHMetadataBinding`, is only a string field but holding identifiers from the remotely linked taxonomy system. 
    - `Number` (`NUMBER`), holds digits potentially with decimal separator. Many operators are available as listed under section `FilterOperator`.
    - `DateTime` (`DATE`), holds a date and time representation. Many operators are available as listed under section `FilterOperator`.
    - `ISHLov` (`LOV`), short for List-of-Values or sometimes called domain or permitted values, is a controlled list of allowed values. This enumeration has values identified by a `label` and an idempotent `element` identifier. The `ValueType` `label` as created through cmdlet `Add-IshLovValue` is allowed to change afterwards over cmdlet `Set-IshLovValue`, hence not advised to use in API calls. The `ValueType` `element`, once created is stable and outlives label renames.
    - `ISHType` (`CARD`), similar to List-of-Values, however this time the permitted values are of the type of link card type. Example is `IShUser` to indentity that a field like `FAUTHOR` links to an existing user instance. Same for an `IShFolder` that needs to link over `FUSERGROUP` to an existing `IShUserGroup`.
- A CARD like system, where CARD instance holds fields with values of the type LOV,	DATE, NUMBER, STRING or LONG TEXT.
- A single CARD instance holding fields is identified by `Level` `none`. This level is implicit as there is no confusion on which field to address on this card instance. Examples of this type are `IShUser`, `IShUserGroup`, `IShUserRole`, `IShConfiguration`, `IShOutputFormat`, `IShEDT` or `IShTranslationJob`. The typical identifier (property `IShRef`), is a generated somewhat readable identifier. For example `VUSERADMIN` which is prefix `V` for value, then `USER` for the object type and `ADMIN` is an encoding of the label provided at creation type.
- A multi CARD instance holding fields is identified by its `Level`. As there is potential confusion of having the same field name on multiple levels, like `MODIFIED-ON` (which holds the last modified date of that card level instance), it is advised to be explicit in specifying levels. The typical identifier (property `IShRef`), is a Global Unique Identifier (GUID), for the top entity like `logical`.
  - `IShPublication`, `IShDocumentObj` and their derived types use three levels: `logical`, `version` and `lng` (language).
  - `IShAnnotation` uses levels: `Annotation` or `Reply`
  - `ISHProject` uses levels:  `Project` and `ProjectAssignee`
- There is also a TABLE like system where the columns of the tables are expressed as field names and the alias names of the table are expressed as levels.
  - `IShBaseline` has a TABLE subsystem known as `IShBaselineItem`
  - `IShTranslationJob` has a TABLE subsystem known as `IShTranslationJobItem`
  - `IShEvent` has a TABLE subsystem with Level `Progress` and `Detail`
  - `IShBackgroundTask` has a TABLE subsystem with Level `Task` and `History`

These base entities surface over the business and api code to the ISHRemote library as the below Field Types and Object Types which can be interacted with.


## Field Types

There is a hierarchy where base class `IShField`, that represents the field `Name` and `Level`, is extended to
- `IShRequestedMetadataField` where `Name` and `Level` is reused and enriched with `ValueType` which highlights by `Value` the typical label of an entity or by `Element` the idempotent identifier of an entity.
- `IShMetadataField` where `Name` and `Level` is reused and enriched with `ValueType` which highlights by `Value` the typical label of an entity or by `Element` the idempotent identifier of an entity. Another property added here is `Value` which represent the value of the metadata field.
- `IShMetadataFilterField` where `Name` and `Level` is reused and enriched with `ValueType` which highlights by `Value` the typical label of an entity or by `Element` the idempotent identifier of an entity. Another property added here is `Value` which represent the value of the metadatafield. The last property is `FilterOperator` which an enumeration of allowed API operators.

## Object Types

Most object types are simple classes to return values like `IShSession` or `IShTypeFieldDefinition`. 

There is a hierarchy where base class `IShObject` is used for PowerShell pipeline actions, either as input or output for a cmdlet. Derived classes are
  - `IShAnnotation` holds an instance with fields (`IShField`) of level `Annotation` or `Reply` that refer to comments, annotations and suggestions.
  - `IShBaseline` holds an instance with fields (`IShField`) of level `None` and can be linked to `IShBaselineItem` entries that list exact versions per logical identifier.
  - `IShConfiguration` holds the singleton instance with fields (`IShField`) of level `None` that holds feature toggles, Xml Settings and more system configuration.
  - `IShEDT` holds an instance with fields (`IShField`) of level `None`. EDT is short for Electronic Document Type that links a typical file extension and mimetype of some binary. Well known EDT is `EDTXML` for xml files and `EDTPNG` for png files. This information is also offered when downloading Data (`IShData`).
  - `IShFolder` holds an instance with fields (`IShField`) of level `None`. Folders or sometimes Directories are nested structures to organize `IShDocumentObj` derived entities.
  - `IShDocumentObj`, also known as content objects. A denormalized structure of the `logical`, `version` and `lng` (language) level fields (`IShField`).
  - `IShIllustration` in turn is derived from `IShDocumentObj` and holds content objects known as images, illustrations, graphics. A unique language level identifier for this type is lng level field resolution (`FRESOLUTION`) which indicates the purpose of an image - for example `Low` for web resolution, `High` for print quality and `Master` could be the source vector graphic.
  - `IShLibrary` in turn is derived from `IShDocumentObj` and holds content objects known as library topics, conref libraries or variable libraries. Typically holding OASIS DITA topic xml files of type `EDTXML`.
  - `IShMasterDoc` in turn is derived from `IShDocumentObj` and holds content objects known as master documents, maps, bookmaps. Typically holding OASIS DITA map xml files of type `EDTXML`.
  - `IShModule` in turn is derived from `IShDocumentObj` and holds content objects known as  topics, concepts, tasks or reference topics. Typically holding OASIS DITA topic xml files of type `EDTXML`.
  - `IShTemplate` in turn is derived from `IShDocumentObj` and holds content objects known as  templates of type 'Other'. Not to be confused with 'Editor Templates'. Typically holding teh various binary files for supporting resources that need storing in the CMS.
  - `IShOutputFormat` holds an instance with fields (`IShField`) of level `None`. Output formats is a controlled list of rendering outputs indicating a certain layout or format. Typical examples are 'PDF A4' (so PDF files of page size A4) or 'Dynamic Delivery' (so downstream DXD or Genius web delivery format). Overall the output format is a key in identifying the language level of publication.
  - `IShProject` holds an instance with fields (`IShField`) of level `Project` and `ProjectAssignee`. A project is an entity in the publication hub to identify active publications to users.
  - `IShPublication`, also known as publication output. A denormalized structure of the `logical`, `version` and `lng` (language) level fields (`IShField`). The language level is commenly known as publication output as it uniquely identifies the logical, version and language level of a publication. This all the way to the output format.
  - `IShTranslationJob` holds an instance with fields (`IShField`) of level `None` and can be linked to `IShTranslationJobItem` entries that list exact versions per logical identifier.
  - `IShUser` holds an instance with fields (`IShField`) of level `None`. A user is the entity used to assign authoring (`FAUTHOR`) or reviewing (`FREVIEWER`) work to. When creating a `New-IShSession` you will be authenticated to a user instance, which in turn gives you authorization for CMS actions.
  - `IShUserGroup` holds an instance with fields (`IShField`) of level `None`. A user groups is assigned to protect `IShFolder` with read and write permission. This read and write security is saved on all `IShDocumentObj` derived entities that are organized in those folders. In turn an `IShUser` needs user groups assigned to be able to read or update folders or entities in those folders.
  - `IShUserRole` holds an instance with fields (`IShField`) of level `None`. A user role is assigned to an `IShUser` to authorize the user for activities in the CMS. The power user role is called `Administrator` and can be considered CMS root access. User Roles can have one or more priviliges, also known as permissions, assigned to enable users specific actions.

## Requested metadata, Get-IshTypeFieldDefinition and Set-IshRequestedMetadataField

ISHRemote offers defaults for the optional `-RequestedMetadata` parameter. The default is calculated as taking the `Get-IshTypeFieldDefinition` for the current Object Type. So cmdlets returning data like reading `Find-IShUser` or even creating `Add-IShUser` which returns the created instance will filter the table on column `ISHType` to `IShUser` and `SDB` column needs to hold `B` (`B` for Basic, and `D` for Descriptive so only object identifiers and `S` for product system fields).



Note properties for read only usage allowing easy table rendering and client side filtering and sorting

## Filter metadata, FilterOperator and Set-IshMetadataFilterField

Below table is replicated from public documentation [Understanding metadata filter operators](https://docs.rws.com/en-US/tridion-docs-main-documentation-1165616/understanding-metadata-filter-operators-68174). The first column holds the allowed filter operators of the `-FilterOperator` parameter that is used on the `Set-IshMetadataFilterField` cmdlet.

|     | ISHType | ISHLov | DateTime | Number | String | LongText |
| --- | --- | --- | --- | --- | --- | --- |
| `equal` | X | X | X | X | X |     |
| `not equal` | X | X |     | X | X |     |
| `in` | X | X | X | X | X |     |
| `not in` | X | X |     | X | X |     |
| `like` | X | X |     |     | X |     |
| `cilike` |     |     |     |     | X |     |
| `greater than` |     |     | X | X |     |     |
| `less than` |     |     | X | X |     |     |
| `greater than or equal` |     |     | X | X |     |     |
| `less than or equal` |     |     | X | X |     |     |
| `between` |     |     | X | X |     |     |
| `empty` | X | X | X | X | X | X |
| `not empty` | X | X | X | X | X | X |

Where `ISHMetadataBinding` typically is a multi-value `String` field holding the remote taxonomy identifiers (`element`), the CMS does not store the `label`s as they are retrieved live from the remote taxonomy.
    
## Setting metadata and Set-IshMetadataField

All Object Types mentioned as output of a cmdlet are client side in-memory representations. If you would update the fields of this object using `Set-IshMetadataField`, then you updated the client side in-memory representation, it would still require the matching `Set` operation (like object `IshUser` requires `Set-IshUser`).






# TODO

. Session 



..  
.. Server  operations like setting and filtering cannot be done over note properties but have to go over dedicated cmdlets 
..this is the short mapping convention from type field as the server understands it to client side only IshObject read only  properties 
.. Beware that there could be a multi gigabyte server and database connected so parameters like -recurse can result in long running operations. Also find on document obj and publication must not be used without one or preferably more server side filters (Set-IshMetadataFilterField)
The verb Search uses a full text index and is quite fast. The verb is a query on a relational database which can be slow 
. Concepts 
.. IshObject is the base object entity that defines a type as listed below and hold field as listed below 
.. 3-level entries having file /blob over ED. Typically denormalized hence repeated although single sourced on the remote server through its tree hierarchy structure..example on ftitle axis versions 
.. 1-level with superfluous none without files
attached 
.. Group by settings 
... User group
... User role with permission 
...user
... OutputFormat 
.. Group by content 
...documentobj have five sub types... Correct name and common synonyms 
... Folder using usergroup as security. Folder data type. Folder system type for settings 
.. Metadata 
... Type field definition explained column by Colin 
... Metadata for set, get which is implicit and filter with some examples. Also allowed filter operators per field type with guidance that server-side like can be slow and expensive. Name and level have to exist in typefielddef. Typically you do not need requested Metadata and hence requested Metadata parameter except for system fields as the tools (cmdlets) come with good defaults 
