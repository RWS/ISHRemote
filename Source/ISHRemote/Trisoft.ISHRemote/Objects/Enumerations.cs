/*
* Copyright (c) 2014 All Rights Reserved by the SDL Group.
* 
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
* 
*     http://www.apache.org/licenses/LICENSE-2.0
* 
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Trisoft.ISHRemote.HelperClasses;

namespace Trisoft.ISHRemote.Objects
{
    /// <summary>
    /// <para type="description">Contains all enumerations and conversion required for this client library</para>
    /// </summary>
    public class Enumerations
    {
        /// <summary>
        /// <para type="description">IshSession Protocol tries to connect the communication protocol like ASMX (Soap11), WCF (Soap12), OpenAPI (rest) with the authentication protocol like WS-Trust (WCF-only), AuthenticationContext (Asxm-only), etc. Offering shorthand for working combinations.</para>
        /// </summary>
        public enum Protocol
        {
            /// <summary>
            /// <para type="description">Asmx (Soap11) endpoints exist since InfoShare 2.7, always authenticated through first parameter AuthenticationContext which only works for internal user profiles (so holding a password in the CMS).</para>
            /// </summary>
            AsmxAuthenticationContext = 2070,
            /// <summary>
            /// <para type="description">OpenApi (rest) endpoints exist since InfoShare 15.0.0 as private Api, always authenticated over Basic Authentication http header which only works for internal user profiles (so holding a password in the CMS).</para>
            /// </summary>
            OpenApiBasicAuthentication = 1500
            // <summary>
            // <para type="description">OpenApi (rest) endpoints exist since InfoShare 16.0.0 as public Api, always authenticated over OpenIDConnect/OAuth20 which only works for internal and external user profiles.</para>
            // </summary>
            //OpenApiOpenConnectId = 1600
        }

        /// <summary>
        /// <para type="description">Which card or table level should the field be present on. The int assignment allows sorting so Logical before Version before Language.</para>
        /// </summary>
        public enum Level
        {
            [StringValue("none")]
            None = 100,
            [StringValue("logical")]
            Logical = 200,
            [StringValue("version")]
            Version = 210,
            [StringValue("lng")]
            Lng = 220,
            [StringValue("progress")]
            Progress = 300,
            [StringValue("detail")]
            Detail = 310,
            [StringValue("data")]
            Data = 320,
            [StringValue("task")]
            Task = 400,
            [StringValue("history")]
            History = 410,
            [StringValue("annotation")]
            Annotation = 500,
            [StringValue("reply")]
            Reply = 510
        }

        /// <summary>
        /// <para type="description">Which data type to retrieve from the database for the specified field</para>
        /// </summary>
        public enum ValueType
        {
            /// <summary>
            /// The value of the ishfield.
            /// </summary>
            [StringValue("value")]
            Value,
            /// <summary>
            /// The GUID of the ishfield.
            /// </summary>
            [StringValue("element")]
            Element,
            /// <summary>
            /// The Id of the ishfield.
            /// </summary>
            [StringValue("id")]
            Id,
            /// <summary>
            /// All is used when the valuetype is irrelevant.
            /// For example when a field needs to be removed and you do not want to loop all different value types.
            /// </summary>
            /// TODO [Must] IshTypeFieldSetup - Remove confuzing value 'All' for ValueType usage, can be done once IshSession.IshTypeFieldSetup is implemented (see [ISHREMOTE-017])
            [StringValue("")]
            All
        }

        /// <summary>
        /// <para type="description">Allows tuning client-side metadata handling/warning since the IshTypeFieldSetup introduction.</para>
        /// </summary>
        public enum StrictMetadataPreference
        {
            /// <summary>
            /// Client-side silent filtering of nonexisting and unallowed fields. (e.g Nice for repository folder syncing with mismatching metadata setup)
            /// </summary>
            [StringValue("silentlycontinue")]
            SilentlyContinue,
            /// <summary>
            /// Client-side filtering of nonexisting and unallowed fields displaying a Write-Verbose message but still continues.
            /// </summary>
            [StringValue("continue")]
            Continue,
            /// <summary>
            ///  Client-side filtering of nonexisting and unallowed fields is turned off. No handling but simply executes the API call, most 
            ///  likely resulting in a Write-Error. This allows api/pester tests like IshUser PASSWORD field should not allowed to be read.
            /// </summary>
            [StringValue("off")]
            Off
        }

        /// <summary>
        /// <para type="description">Allows tuning client-side object enrichment like no wrapping (off) or PSObject-with-PSNoteProperty wrapping.</para>
        /// </summary>
        public enum PipelineObjectPreference
        {
            /// <summary>
            /// Wrap every possible pipeline object with PSObject and add PSNoteProperty for all IshFields
            /// </summary>
            [StringValue("psobjectnoteproperty")]
            PSObjectNoteProperty,
            /// <summary>
            /// Deprecated legacy behavior (0.6 and earlier), so no pipeline PSObject wrapping
            /// </summary>
            [StringValue("off")]
            Off
        }

        public enum RequestedMetadataGroup
        {
            /// <summary>
            /// performance-optimized, only primary keys
            /// </summary>
            [StringValue("descriptive")]
            Descriptive,
            /// <summary>
            /// user friendly fields used in tables
            /// </summary>
            [StringValue("basic")]
            Basic,
            // <summary>
            // not performant, tech fields only
            // </summary>
            //[StringValue("system")]
            //System,
            /// <summary>
            /// not performant
            /// </summary>
            [StringValue("all")]
            All
        }


        /// <summary>
        /// <para type="description">List of value like Events can be hidden upon Find/Retrieval with these filters</para>
        /// </summary>
        public enum ActivityFilter
        {
            [StringValue("none")]
            None,
            [StringValue("active")]
            Active,
            [StringValue("inactive")]
            Inactive
        }

        /// <summary>
        /// <para type="description">When filtering information having certain value(s), you need operators per field</para>
        /// </summary>
        public enum FilterOperator
        {
            [StringValue("equal")]
            Equal,
            [StringValue("notequal")]
            NotEqual,
            [StringValue("in")]
            In,
            [StringValue("notin")]
            NotIn,
            [StringValue("like")]
            Like,
            [StringValue("greaterthan")]
            GreaterThan,
            [StringValue("lessthan")]
            LessThan,
            [StringValue("greaterthanorequal")]
            GreaterThanOrEqual,
            [StringValue("lessthanorequal")]
            LessThanOrEqual,
            [StringValue("between")]
            Between,
            [StringValue("empty")]
            Empty,
            [StringValue("notempty")]
            NotEmpty
        }

        /// <summary>
        /// <para type="description">If two fields should be first, should the second value overwrite the first or prepend/append it</para>
        /// </summary>
        public enum ValueAction
        {
            [StringValue("overwrite")]
            Overwrite,
            [StringValue("prepend")]
            Prepend,
            [StringValue("append")]
            Append
        }

        /// <summary>
        /// <para type="description">Used by RemoveSystemFields to know which fields to filter. E.g. at creation time of a user PASSWORD is allowed, but not at retrieval time.</para>
        /// </summary>
        public enum ActionMode
        {
            Create,
            Read,
            Update,
            Delete,
            Find,
            Search
        }

        /// <summary>
        /// <para type="description">Used by IshObject/IshEvent/IshBackgroundTask to set all reference types on a card</para>
        /// </summary>
        public enum ReferenceType
        {
            [StringValue("ishlogicalref")]
            Logical,
            [StringValue("ishversionref")]
            Version,
            [StringValue("ishlngref")]
            Lng,
            [StringValue("ishbaselineref")]
            Baseline,
            [StringValue("ishoutputformatref")]
            OutputFormat,
            [StringValue("ishusergroupref")]
            UserGroup,
            [StringValue("ishuserref")]
            User,
            [StringValue("ishuserroleref")]
            UserRole,
            [StringValue("ishedtref")]
            EDT,
            [StringValue("ishprogressref")]
            EventProgress,
            [StringValue("ishdetailref")]
            EventDetail,
            [StringValue("ishtaskref")]
            BackgroundTask,
            [StringValue("ishhistoryref")]
            BackgroundTaskHistory,
            [StringValue("ishannotationref")]
            Annotation,
            [StringValue("ishreplyref")]
            AnnotationReply

        }

        /// <summary>
        /// <para type="description">Enumeration of all possible object types.</para>
        /// </summary>
        public enum ISHType
        {
            /// <summary>
            /// Initial value...we don't know what the ISHType is.
            /// </summary>
            ISHNone,
            /// <summary>
            /// The object does not exists
            /// </summary>
            ISHNotFound,
            /// <summary>
            /// InfoShare Module/IMAP-Map/DITA-Topic
            /// </summary>
            ISHModule,
            /// <summary>
            /// InfoShare MasterDocument/IMAP-Master/DITA-(Book)Map/DocumentOutline
            /// </summary>
            ISHMasterDoc,
            /// <summary>
            /// InfoShare Library
            /// </summary>
            ISHLibrary,
            /// <summary>
            /// InfoShare Template
            /// </summary>
            ISHTemplate,
            /// <summary>
            /// InfoShare Illustration/Image/Graphic
            /// </summary>
            ISHIllustration,
            /// <summary>
            /// InfoShare publication object
            /// </summary>
            ISHPublication,
            /// <summary>
            /// InfoShare user
            /// </summary>
            ISHUser,
            /// <summary>
            /// InfoShare usergroup (new name for department)
            /// </summary>
            ISHUserGroup,
            /// <summary>
            /// InfoShare user role
            /// </summary>
            ISHUserRole,
            /// <summary>
            /// The baseline
            /// </summary>
            ISHBaseline,
            /// <summary>
            /// InfoShare output format object
            /// </summary>
            ISHOutputFormat,
            /// <summary>
            /// Electronic Document Type
            /// </summary>
            ISHEDT,
            /// <summary>
            /// Folder/Directory
            /// </summary>
            ISHFolder,
            /// <summary>
            /// Translation job
            /// </summary>
            ISHTranslationJob,
            /// <summary>
            /// Configuration card
            /// </summary>
            ISHConfiguration,
            /// <summary>
            /// Electronic document/ED/Revision
            /// </summary>
            ISHRevision,
            /// <summary>
            /// Conditional Context, available on FISHCONTEXT (used to be saved on CTCONTEXT card type)
            /// </summary>
            ISHFeatures,
            /// <summary>
            /// Background Task table
            /// </summary>
            ISHBackgroundTask,
            /// <summary>
            /// Event Monitor table
            /// </summary>
            ISHEvent, 
            /// <summary>
            /// Annotations
            /// </summary>
            ISHAnnotation
        }


        /// <summary>
        /// <para type="description">Enumeration of folder types.</para>
        /// </summary>
        public enum IshFolderType
        {
            /// <summary>
            /// In this folder no objects allowed except subfolders
            /// </summary>
            ISHNone,
            /// <summary>
            /// Folder with InfoShare Module/IMAP-Map/DITA-Topic
            /// </summary>
            ISHModule,
            /// <summary>
            /// Folder with InfoShare MasterDocument/IMAP-Master/DITA-(Book)Map/DocumentOutline
            /// </summary>
            ISHMasterDoc,
            /// <summary>
            /// Folder with InfoShare Library
            /// </summary>
            ISHLibrary,
            /// <summary>
            /// Folder with InfoShare Template
            /// </summary>
            ISHTemplate,
            /// <summary>
            /// Folder with InfoShare Illustration/Image/Graphic
            /// </summary>
            ISHIllustration,
            /// <summary>
            /// Folder with InfoShare publication object
            /// </summary>
            ISHPublication,
            /// <summary>
            /// Folder with Document references
            /// </summary>
            ISHReference,
            /// <summary>
            /// Query Folders
            /// </summary>
            ISHQuery
        }

        /// <summary>
        /// /// <para type="description">Enumerations for controlled date types, used by IshTypeFieldDefinition. Holding base types like number, long, etc</para>
        /// </summary>
        public enum DataType
        {
            /// <summary>
            /// The field data type is a reference field pointing to a List Of Values (DOMAIN)
            /// </summary>
            ISHLov,
            /// <summary>
            /// The field data type is a reference field pointing to another ISHType (CARD REFERENCE)
            /// </summary>
            ISHType,
            /// <summary>
            /// The field data type is a simple text type, preferred for shorting string values with the most API operators
            /// </summary>
            String,
            /// <summary>
            /// The field data type is a simple text type, preferred for longer string values with less API operators (only empty/notempty)
            /// </summary>
            LongText,
            /// <summary>
            /// The field data type is a simple controlled decimal type
            /// </summary>
            Number,
            /// <summary>
            /// The field data type is a simple controlled date time type
            /// </summary>
            DateTime,
            /// <summary>
            /// The field data type used to describe metadata bound fields
            /// </summary>
            ISHMetadataBinding
        }

        /// <summary>
        /// <para type="description">Enumerations of basefolders</para>
        /// </summary>
        public enum BaseFolder
        {
            /// <summary>
            /// Indicates the Data folder as starting point. Also known as 'General', 'ISREPROOT', normal repository content,...
            /// </summary>
            Data,
            /// <summary>
            /// Indicates the System folder as starting point
            /// </summary>
            System,
            /// <summary>
            /// Indicates the User's favorite folder as starting point
            /// </summary>
            Favorites,
            /// <summary>
            /// Indicates the 'Editor Template' folder as starting point
            /// </summary>
            EditorTemplate,
        }

        /// <summary>
        /// <para type="description">Enumeration matching the API status filters for IshObjects</para>
        /// </summary>
        public enum StatusFilter
        {
            /// <summary>
            /// Released states only
            /// </summary>
            ISHReleasedStates,
            /// <summary>
            /// Released or Draft states
            /// </summary>
            ISHReleasedOrDraftStates,
            /// <summary>
            /// Out of date or Released states
            /// </summary>
            ISHOutOfDateOrReleasedStates,
            /// <summary>
            /// No status filter
            /// </summary>
            ISHNoStatusFilter
        }

        /// <summary>
        /// <para type="description">BackgroundTask Status Filter</para>
        /// </summary>
        public enum BackgroundTaskStatusFilter
        {
            /// <summary>
            /// Filtering on the status Busy will return all with status: VBACKGROUNDTASKSTATUSEXECUTING, VBACKGROUNDTASKSTATUSPENDING
            /// </summary>
            Busy,
            /// <summary>
            /// Filtering on the status Success will return all with status: VBACKGROUNDTASKSTATUSSUCCESS, VBACKGROUNDTASKSTATUSSKIPPED
            /// </summary>
            Success,
            /// <summary>
            /// Filtering on the status Failed will return all with status: VBACKGROUNDTASKSTATUSFAILED, VBACKGROUNDTASKSTATUSABORTED (NotUsedIn-13.0.1), VBACKGROUNDTASKSTATUSCANCELPENDING (NotUsedIn-13.0.1), VBACKGROUNDTASKSTATUSCANCELLED (NotUsedIn-13.0.1)
            /// </summary>
            Failed,
            /// <summary>
            /// No filtering on the status is applied 
            /// </summary>
            All
        }

        /// <summary>
        /// <para type="description">EventMonitor Events Status Filter</para>
        /// </summary>
        public enum ProgressStatusFilter
        {
            /// <summary>
            /// Filtering on the status Busy will return all events which are still busy 
            /// </summary>
            Busy,
            /// <summary>
            /// Filtering on the status Success will return all events which are completed successfully 
            /// </summary>
            Success,
            /// <summary>
            /// Filtering on the status Success will return all events with warnings 
            /// </summary>
            Warning,
            /// <summary>
            /// Filtering on the status Failed will return all failed events 
            /// </summary>
            Failed,
            /// <summary>
            /// No filtering on the status is applied 
            /// </summary>
            All
        }

        /// <summary>
        /// <para type="description">EventMonitor Events User Filter</para>
        /// </summary>
        public enum UserFilter
        {
            /// <summary>
            /// Used to indicate that only the events of the current user should be returned
            /// </summary>
            Current,
            /// <summary>
            /// Used to indicate that all events should be returned  
            /// </summary>
            All
        }

        /// <summary>
        /// <para type="description">Possible values for the level of an event detail</para>
        /// </summary>
        public enum EventLevel
        {
            /// <summary>
            /// Indicates that the event detail contains an error
            /// </summary>
            Exception,
            /// <summary>
            /// Indicates that the event detail contains a noncritical problem  
            /// </summary>
            Warning,
            /// <summary>
            /// Indicates that the event detail contains a configuration message  
            /// </summary>
            Configuration,
            /// <summary>
            /// Indicates that the event detail contains an informational message  
            /// </summary>
            Information,
            /// <summary>
            /// Indicates that the event detail contains a verbose message  
            /// </summary>
            Verbose,
            /// <summary>
            /// Indicates that the event detail contains a debug message  
            /// </summary>
            Debug
        }

        /// <summary>
        /// <para type="description">Possible values for the source enumeration of a baseline entry</para>
        /// </summary>
        public enum BaselineSourceEnumeration
        {
            SaveManual,
            SaveLatestAvailable,
            SaveLatestReleased,
            SaveByBaseline,
            SaveCandidate,
            SaveFirstVersion,
            SaveCopy,
            Manual,
            ExpandNone,
            ExpandLatestAvailable,
            ExpandLatestReleased,
            ExpandByBaseline,
            ExpandCandidate,
            ExpandFirstVersion,
        }

        /// <summary>
        /// Unique descriptive identifier of an IshTypeFieldDefinition concatenating type, level (respecting log/version/lng), and field name
        /// </summary>
        internal static string Key(Enumerations.ISHType ishType, Enumerations.Level level, string fieldName)
        {
            return ishType + "=" + (int)level + level + "=" + fieldName;
        }

        /// <summary>
        /// Extracts the TriDK CardType level from input like USER, CTMAPL,...
        /// </summary>
        internal static Level ToLevelFromCardType(string cardType)
        {
            switch (cardType)
            {
                case "CTMASTER":
                case "CTMAP":
                case "CTIMG":
                case "CTTEMPLATE":
                case "CTLIB":
                case "CTREUSEOBJ":  // obsolete card type, but added for completeness
                case "CTPUBLICATION":
                    return Level.Logical;
                case "CTMASTERV":
                case "CTMAPV":
                case "CTIMGV":
                case "CTTEMPLATEV":
                case "CTLIBV":
                case "CTREUSEOBJV":  // obsolete card type, but added for completeness
                case "CTPUBLICATIONV":
                    return Level.Version;
                case "CTMASTERL":
                case "CTMAPL":
                case "CTIMGL":
                case "CTTEMPLATEL":
                case "CTLIBL":
                case "CTREUSEOBJL":  // obsolete card type, but added for completeness
                case "CTPUBLICATIONOUTPUT":
                    return Level.Lng;
                default:
                    return Level.None;
            }
        }

        /// <summary>
        /// Extracts the TriDK CardType type from input like USER, CTMAPL,...
        /// </summary>
        internal static ISHType ToIshTypeFromCardType(string cardType)
        {
            switch (cardType)
            {
                case "CTMASTER":
                case "CTMASTERV":
                case "CTMASTERL":
                    return ISHType.ISHMasterDoc;
                case "CTMAP":
                case "CTMAPV":
                case "CTMAPL":
                    return ISHType.ISHModule;
                case "CTIMG":
                case "CTIMGV":
                case "CTIMGL":
                    return ISHType.ISHIllustration;
                case "CTTEMPLATE":
                case "CTTEMPLATEV":
                case "CTTEMPLATEL":
                    return ISHType.ISHTemplate;
                case "CTLIB":
                case "CTLIBV":
                case "CTLIBL":
                    return ISHType.ISHLibrary;
                case "CTPUBLICATION":
                case "CTPUBLICATIONV":
                case "CTPUBLICATIONOUTPUT":
                    return ISHType.ISHPublication;
                case "USER":
                    return ISHType.ISHUser;
                case "CTUSERROLE":
                    return ISHType.ISHUserRole;
                case "CTUSERGROUP":
                    return ISHType.ISHUserGroup;
                case "ELECTRONIC DOCUMENT":
                    return ISHType.ISHRevision;
                case "CTDOCMAP":
                    return ISHType.ISHFolder;
                case "CTCONFIGURATION":
                    return ISHType.ISHConfiguration;
                case "EDT":
                    return ISHType.ISHEDT;
                case "CTOUTPUTFORMAT":
                    return ISHType.ISHOutputFormat;
                case "CTBASELINE":
                    return ISHType.ISHBaseline;
                case "CTCONTEXT":  // obsolete card type, but added for completeness
                    return ISHType.ISHFeatures;
                case "CTTRANSJOB":
                    return ISHType.ISHTranslationJob;
                case "CTREUSEOBJ":  // obsolete card type, but added for completeness
                case "CTREUSEOBJV":  // obsolete card type, but added for completeness
                case "CTREUSEOBJL":  // obsolete card type, but added for completeness
                default:
                    return ISHType.ISHNotFound;
            }
        }

        /// <summary>
        /// Extracts the baseline source enumeration
        /// </summary>
        internal static BaselineSourceEnumeration ToBaselineSourceEnumeration(string source)
        {
            switch (source)
            {
                case "save:Manual":
                    return BaselineSourceEnumeration.SaveManual;
                case "save:LatestAvailable":
                    return BaselineSourceEnumeration.SaveLatestAvailable;
                case "save:LatestReleased":
                    return BaselineSourceEnumeration.SaveLatestReleased;
                case "save:ByBaseline":
                    return BaselineSourceEnumeration.SaveByBaseline;
                case "save:Candidate":
                    return BaselineSourceEnumeration.SaveCandidate;
                case "save:FirstVersion":
                    return BaselineSourceEnumeration.SaveFirstVersion;
                case "save:Copy":
                    return BaselineSourceEnumeration.SaveCopy;
                case "Manual":
                    return BaselineSourceEnumeration.Manual;
                case "expand:None":
                    return BaselineSourceEnumeration.ExpandNone;
                case "expand:LatestAvailable":
                    return BaselineSourceEnumeration.ExpandLatestAvailable;
                case "expand:LatestReleased":
                    return BaselineSourceEnumeration.ExpandLatestReleased;
                case "expand:ByBaseline":
                    return BaselineSourceEnumeration.ExpandByBaseline;
                case "expand:Candidate":
                    return BaselineSourceEnumeration.ExpandCandidate;
                case "expand:FirstVersion":
                    return BaselineSourceEnumeration.ExpandFirstVersion;
                default:
                    return BaselineSourceEnumeration.Manual;
            }
        }
    }
}
