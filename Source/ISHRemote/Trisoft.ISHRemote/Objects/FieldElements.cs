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

namespace Trisoft.ISHRemote.Objects
{
    /// <summary>
    /// FieldElements groups all constants to access the fields with an element.
    ///  Duplication of \Dev\Server.App\Common\Trisoft.InfoShare\Trisoft.InfoShare.Configuration\Common\FieldElements.cs
    /// </summary>
    internal class FieldElements
    {
        #region General Fields
        /// <summary>
        /// The element of the Field NAME.
        /// This mandatory field contains the label of the card.
        /// </summary>
        public const string Name = "NAME";
        /// <summary>
        /// The element of the Field CREATED-ON
        /// </summary>
        public const string CreationDate = "CREATED-ON";
        /// <summary>
        /// The element of the Field MODIFIED-ON
        /// </summary>
        public const string ModificationDate = "MODIFIED-ON";
        /// <summary>
        /// The element of the Field CARD-TYPE.
        /// This mandatory field contains an LovValue with the CardType of the card.
        /// </summary>
        public const string CardType = "CARD-TYPE";

        /// <summary>
        /// The element of the field FANCESTOR
        /// </summary>
        public const string Ancestor = "FANCESTOR";
        /// <summary>
        /// The element of the field FMAPID
        /// Used on language objects to store the LogicalId
        /// </summary>
        public const string MapId = "FMAPID";
        /// <summary>
        /// The element of the field FISHOBJECTACTIVE
        /// Used on cardtypes which are linked with an LOV (e.g. CTOUTPUTFORMAT, USER, CTUSERROLE and CTUSERGROUP) to allow hiding or showing values in the UI. 
        /// Note that the value is still valid input.
        /// </summary>
        public const string ObjectActive = "FISHOBJECTACTIVE";
        #endregion

        #region Security Fields
        /// <summary>
        /// The element of the Field READ-ACCESS 
        /// </summary>
        public const string ReadAccess = "READ-ACCESS";
        /// <summary>
        /// The element of the Field MODIFY-ACCESS 
        /// </summary>
        public const string ModifyAccess = "MODIFY-ACCESS";
        /// <summary>
        /// The element of the Field DELETE-ACCESS 
        /// </summary>
        public const string DeleteAccess = "DELETE-ACCESS";
        /// <summary>
        /// The element of the Field OWNER 
        /// </summary>
        public const string Owner = "OWNER";
        /// <summary>
        /// The element of the Field RIGHTS
        /// </summary>
        public const string Rights = "RIGHTS";
        /// <summary>
        /// The element of the Field FUSERGROUP
        /// </summary>
        public const string Usergroup = "FUSERGROUP";
        #endregion

        #region User Fields
        /// <summary>
        /// The element of the Field USERNAME
        /// </summary>
        public const string UserName = "USERNAME";
        /// <summary>
        /// The element of the Field FISHUSERDISPLAYNAME
        /// </summary>
        public const string UserDisplayName = "FISHUSERDISPLAYNAME";
        /// <summary>
        /// The element of the Field PASSWORD
        /// </summary>
        public const string Password = "PASSWORD";
        /// <summary>
        /// The element of the Field FISHUSERROLES (new element name of FISHCAPABILITIES)
        /// </summary>
        public const string UserRoles = "FISHUSERROLES";
        /// <summary>
        /// The element of the Field FISHUSERLANGUAGE
        /// </summary>
        public const string UserLanguage = "FISHUSERLANGUAGE";
        /// <summary>
        /// The element of the Field FISHFAVORITES
        /// This field contains the folder card with the 'favorite' objects
        /// </summary>
        public const string Favorites = "FISHFAVORITES";
        /// <summary>
        /// The element of the Field FISHUSERTYPE
        /// </summary>
        public const string UserType = "FISHUSERTYPE";
        /// <summary>
        /// The element of the Field FISHEXTERNALID
        /// This field maps an external authenticated user to a Trisoft user
        /// </summary>
        public const string ExternalId = "FISHEXTERNALID";
        /// <summary>
        /// The element of the Field FISHUSERDISABLED
        /// A disabled user is no longer allowed to log in.
        /// </summary>
        public const string UserDisabled = "FISHUSERDISABLED";
        /// <summary>
        /// The element of the Field FISHPASSWORDSALT
        /// Used on USER to store the password salt
        /// </summary>
        public const string UserPasswordSalt = "FISHPASSWORDSALT";
        /// <summary>
        /// The element of the Field FISHPASSWORDMODIFIEDON
        /// Used on USER to store the date when the password was last modified
        /// </summary>
        public const string UserPasswordModifiedOn = "FISHPASSWORDMODIFIEDON";
        /// <summary>
        /// The element of the Field FISHLASTLOGINON
        /// Used on USER to store the date on when user last log in 
        /// </summary>
        public const string UserLastLoginOn = "FISHLASTLOGINON";
        /// <summary>
        /// The element of the Field FISHFAILEDATTEMPTS
        /// Used on USER to store the count of failed attemtps
        /// </summary>
        public const string UserFailedAttempts = "FISHFAILEDATTEMPTS";
        /// <summary>
        /// The element of the Field FISHLOCKEDSINCE
        /// Used on USER to store the date since when the account had been locked
        /// </summary>
        public const string UserLockedSince = "FISHLOCKEDSINCE";
        /// <summary>
        /// The element of the Field FISHLOCKED
        /// Used on USER to store the account locked status
        /// </summary>
        public const string UserLocked = "FISHLOCKED";
        /// <summary>
        /// The element of the Field FISHPASSWORDHISTORY
        /// Used on USER to store the password history
        /// </summary>
        public const string UserPasswordHistory = "FISHPASSWORDHISTORY";

        #endregion

        #region UserGroup Fields
        /// <summary>
        /// The element of the Field FISHUSERGROUPNAME
        /// Used on usergroups to link the card with the LovValue of DUSERGROUP which contains the label/name of the usergroup
        /// </summary>
        public const string UserGroupName = "FISHUSERGROUPNAME";
        #endregion

        #region UserRole Fields
        /// <summary>
        /// The element of the Field FISHUSERROLENAME
        /// Used on userroles to link the card with the LovValue of DUSERROLE which contains the label/name of the userrole
        /// </summary>
        public const string UserRoleName = "FISHUSERROLENAME";
        #endregion

        #region Configuration Fields
        /// <summary>
        /// The element of the Field FSTATECONFIGURATION
        /// Used on Configuration Card to store the active status configuration containing the status definitions, status transitions and initial statuses.
        /// </summary>
        public const string StatusConfiguration = "FSTATECONFIGURATION";
        /// <summary>
        /// The element of the Field FISHPUBSTATECONFIG
        /// Used on Configuration Card to store the publication status configuration containing the different status definitions and transitions of the publication.
        /// </summary>
        public const string PublicationStatusConfiguration = "FISHPUBSTATECONFIG";
        /// <summary>
        /// The element of the Field FTRANSLATIONCONFIGURATION
        /// Used on Configuration Card to store the active translation configuration
        /// </summary>
        public const string TranslationConfiguration = "FTRANSLATIONCONFIGURATION";
        /// <summary>
        /// The element of the Field FINBOXCONFIGURATION
        /// Used on Configuration Card to store the inbox configuration xml
        /// </summary>
        public const string InboxConfiguration = "FINBOXCONFIGURATION";
        /// <summary>
        /// The element of the Field FISHPLUGINCONFIGXML
        /// Used on Configuration Card to store the active plugin/OnDocStore configuration.
        /// </summary>
        public const string PluginConfiguration = "FISHPLUGINCONFIGXML";
        /// <summary>
        /// The element of the Field FISHCHANGETRACKERCONFIG
        /// This field contains the Changetracker configuration, used when publishing or previewing the changes between the content of 2 objects.
        /// </summary>
        public const string ChangeTrackerConfiguration = "FISHCHANGETRACKERCONFIG";
        /// <summary>
        /// The element of the Field FISHTRANSJOBSTATECONFIG
        /// Used on Configuration Card to store the translation job status configuration 
        /// containing the different status definitions and transitions of the translation job.
        /// </summary>
        public const string TranslationJobStatusConfiguration = "FISHTRANSJOBSTATECONFIG";
        /// <summary>
        /// The element of the Field FISHPLUGINCONFIGXML
        /// Used on Configuration Card to store the active plugin/OnDocStore configuration.
        /// </summary>
        public const string WritePluginConfiguration = "FISHWRITEOBJPLUGINCFG";
        /// <summary>
        /// The element of the Field FISHPUBLISHPLUGINCONFIG
        /// Used on Configuration Card to store the active publish plugin configuration.
        /// </summary>
        public const string PublishPluginConfiguration = "FISHPUBLISHPLUGINCONFIG";
        /// <summary>
        /// The element of the Field FISHBACKGROUNDTASKCONFIG
        /// Used on Configuration Card to store the configuration for the background tasks.
        /// </summary>
        public const string BackgroundTaskConfiguration = "FISHBACKGROUNDTASKCONFIG";
        /// <summary>
        /// The element of the Field FISHEXTENSIONCONFIG
        /// Used on Configuration Card to store the configuration for the extensions.
        /// </summary>
        public const string ExtensionConfiguration = "FISHEXTENSIONCONFIG";
        /// <summary>
        /// The element of the Field FISHLCURI
        /// Used to link Architect with Reach systems
        /// </summary>
        public const string LiveContentReachUri = "FISHLCURI";
        /// <summary>
        /// The element of the Field FISHENRICHURI
        /// Used to link Architect with Enrich systems
        /// </summary>
        public const string LiveContentEnrichUri = "FISHENRICHURI";
        /// <summary>
        /// The element of the Field FISHSYSTEMRESOLUTION.
        /// The system wide default resolution.
        /// </summary>
        public const string SystemResolution = "FISHSYSTEMRESOLUTION";

        #endregion

        #region DocumentCard Fields
        /// <summary>
        /// The element of the Field that links the blob/page with the ED/Document Card
        /// </summary>
        public const string Page = "IS_PAGE";
        #endregion

        #region EDT Fields
        /// <summary>
        /// The element of the Field FISHEDTNAME.
        /// This field links the EDT card with the corresponding LovValue.
        /// </summary>
        public const string EDTName = "FISHEDTNAME";
        /// <summary>
        /// The element of the Field that indicates the mimetype of the document
        /// </summary>
        public const string EDMimeType = "EDT-MIME-TYPE";        
        /// <summary>
        /// The element of the Field that indicates the file extension of the document
        /// </summary>
        public const string EDFileExtension = "EDT-FILE-EXTENSION";
        /// <summary>
        /// The element of the Field that contains the possible file extensions for the EDT
        /// </summary>
        public const string EDTCandidate = "EDT-CANDIDATE";
        /// <summary>
        /// The element of the Field EDT
        /// </summary>
        public const string EDType = "EDT";
        #endregion

        #region Baseline Fields
        /// <summary>
        /// The element of the Field FISHLABELRELEASED.
        /// This field indicates that the baseline is released/frozen or not.
        /// </summary>
        public const string BaselineReleased = "FISHLABELRELEASED";
        /// <summary>
        /// The element of the Field FISHBASELINEACTIVE.
        /// This field indicates that the baseline is active or not.
        /// </summary>
        public const string BaselineActive = "FISHBASELINEACTIVE";
        /// <summary>
        /// The element of the Field FISHDOCUMENTRELEASE.
        /// This field links the baseline card with the corresponding LovValue.
        /// </summary>
        public const string BaselineLovValue = "FISHDOCUMENTRELEASE";
        /// <summary>
        /// The element of the Field FISHDOCUMENTRELEASE.
        /// This field links the baseline card with the corresponding LovValue which contains the label of the baseline.
        /// </summary>
        public const string BaselineName = BaselineLovValue;
        #endregion

        #region OutputFormat Fields
        /// <summary>
        /// The element of the Field FISHOUTPUTFORMATNAME.
        /// This field links the OutputFormat card with the corresponding LovValue.
        /// </summary>
        public const string OutputFormatName = "FISHOUTPUTFORMATNAME";
        /// <summary>
        /// The element of the Field FISHRESOLUTIONS.
        /// The resolutions that can be used for this OutputFormat (only these resolutions are exported in the Publish)
        /// </summary>
        public const string OutputFormatResolutions = "FISHRESOLUTIONS";
        /// <summary>
        /// The element of the Field FISHSINGLEFILE.
        /// Used on the OutputFormat to indicate if the output is one file or not.
        /// </summary>
        public const string OutputFormatSingleFile = "FISHSINGLEFILE";
        /// <summary>
        /// The element of the Field FISHCLEANUP.
        /// Used on the OutputFormat to indicate if working files must be removed after publishing.
        /// </summary>
        public const string OutputFormatCleanup = "FISHCLEANUP";
        /// <summary>
        /// The element of the Field FISHKEEPDTDSYSTEMID.
        /// Used on the OutputFormat to indicate if the resulting files must have SystemIds for the DTD
        /// </summary>
        public const string OutputFormatKeepDTDSystemId = "FISHKEEPDTDSYSTEMID";
        /// <summary>
        /// The element of the Field FISHOUTPUTEDT.
        /// Used on the OutputFormat to indicate the EDT of the output
        /// </summary>
        public const string OutputEDType = "FISHOUTPUTEDT";
        /// <summary>
        /// The element of the Field FISHDITADLVRCLIENTSECRET.
        /// The client secret used for oAuth authentication to connect to Dita Delivery services.
        /// </summary>
        public const string DitaDeliveryClientSecret = "FISHDITADLVRCLIENTSECRET";
        #endregion

        #region Revision fields
        /// <summary>
        /// The element of the field FISHREVISIONS
        /// Used on language objects to store the list with the revision objects
        /// </summary>
        public const string Revisions = "FISHREVISIONS";
        /// <summary>
        /// The element of the field FISHREVCOUNTER
        /// Used on language objects to store the sequence of the last revision
        /// </summary>
        public const string RevisionCounter = "FISHREVCOUNTER";
        /// <summary>
        /// The element of the field FISHREVISIONLOG
        /// Used on language objects to store the XML with the information of the revisions (CreationDate, Author and status of a new revision)
        /// </summary>
        public const string RevisionLog = "FISHREVISIONLOG";
        /// <summary>
        /// The element of the field FHISTORYLOGGING [deprecated]
        /// Used on various language card types to store the historic status transition information. Since InfoShare 3.3.x this way of storing information is replaced by FISHREVISIONLOG.
        /// </summary>
        public const string RevisionHistoryLogging = "FHISTORYLOGGING";
        #endregion

        #region ContentObject Fields

        #region ContentObject Fields - Logical object
        /// <summary>
        /// The element of the field FTITLE
        /// </summary>
        public const string Title = "FTITLE";
        /// <summary>
        /// The element of the field FDESCRIPTION
        /// </summary>
        public const string Description = "FDESCRIPTION";
        /// <summary>
        /// The element of the field FISHNOREVISIONS
        /// </summary>
        public const string DisableRevisions = "FISHNOREVISIONS";
        /// <summary>
        /// The element of the field DOC-VERSION-MULTI-LNG
        /// This field links the logical level with the version level of an object.
        /// </summary>
        public const string DocVersionMultiLng = "DOC-VERSION-MULTI-LNG";

        /// <summary>
        /// The element of the field FISHCONTEXTS
        /// Used on CTMASTER and CTPUBLICATIONV to list the saved CTCONTEXTs.
        /// </summary>
        public const string SavedContexts = "FISHCONTEXTS";
        /// <summary>
        /// The element of the field FISHEDITTMPLTICONNAME.
        /// Used for serving the icon of an editor template in the web
        /// </summary>
        public const string EditorTemplateIconName = "FISHEDITTMPLTICONNAME";


        #region ContentObjects Fields - Logical object - Translation Management
        /// <summary>
        /// The element of the Field FREQUESTEDLANGUAGES.
        /// Used on various logical card types to store the languages that are requested for this object, matching values from DLANGUAGE.
        /// </summary>
        public const string RequestedLanguages = "FREQUESTEDLANGUAGES";
        /// <summary>
        /// The element of the Field FINHERITEDLANGUAGES.
        /// Used on various logical card types to store the languages which are indicated as requested languages by objects holding a reference to this object at the time Translation Management was executed.
        /// </summary>
        public const string InheritedLanguages = "FINHERITEDLANGUAGES";
        /// <summary>
        /// The element of the Field FNOTRANSLATIONMGMT.
        /// Used on various logical card types to indicate if TranslationManagement is activated or not
        /// </summary>
        public const string DisableTranslationManagement = "FNOTRANSLATIONMGMT";
        #endregion

        #endregion

        #region ContentObject Fields - Version Object
        /// <summary>
        /// The element of the field VERSION
        /// </summary>
        public const string Version = "VERSION";
        /// <summary>
        /// The element of the field FISHBRANCHNR
        /// </summary>
        public const string BranchNumber = "FISHBRANCHNR";
        /// <summary>
        /// The element of the field LNG-VERSION
        /// This field links the version level with the language level of an object.
        /// </summary>
        public const string LngVersion = "LNG-VERSION";
        /// <summary>
        /// The element of the field FISHRELEASECANDIDATE.
        /// This field indicates that the current version object is a candidate for the specified baseline
        /// </summary>
        public const string CandidateForBaseline = "FISHRELEASECANDIDATE";
        /// <summary>
        /// The element of the field FISHRELEASELABEL.
        /// Used on various version card types to store a reference to a CTBASELINE that this particular version is part of the referenced frozen baseline. When freezing a baseline, this field is set automatically.
        /// </summary>
        public const string ReferencedFrozenBaselines = "FISHRELEASELABEL";
        /// <summary>
        /// The element of the field FREUSEDINVERSION.
        /// Used on various version card types to store a lists of pairs of block id and reusable object sequence number of all start reuse operations that occurred on this version. For example "warning-id-06£360" means that start reuse on element with id “warning-id-06” resulted in the creation of reusable object with logical id "IS_REUSED_OBJECT_360".
        /// </summary>
        public const string ReusedInVersion = "FREUSEDINVERSION";
        #endregion

        #region ContentObject Fields - Language Object
        /// <summary>
        /// The element of the field DOC-LANGUAGE
        /// </summary>
        public const string DocumentLanguage = "DOC-LANGUAGE";
        /// <summary>
        /// The element of the field FSOURCELANGUAGE
        /// </summary>
        public const string SourceLanguage = "FSOURCELANGUAGE";
        /// <summary>
        /// The element of the field FRESOLUTION
        /// </summary>
        public const string Resolution = "FRESOLUTION";
        /// <summary>
        /// The element of the field FISHSTATUSTYPE.
        /// This field contains the mapping of the FISHSTATUS / FISHPUBSTATUS with its enum values:
        /// DrafT(10), Release Candidate(15), Released(20), ...
        /// </summary>
        public const string StatusType = "FISHSTATUSTYPE";
        /// <summary>
        /// The element of the field FSTATUS.
        /// Used on various language card types to store the current status, matching a value from DSTATUS.
        /// </summary>
        public const string Status = "FSTATUS";
        /// <summary>
        /// The element of the field FISHLASTMODIFIEDBY.
        /// Used on various language card types to automatically store the user which has done the last modification to the document
        /// </summary>
        public const string LastModifiedBy = "FISHLASTMODIFIEDBY";
        /// <summary>
        /// The element of the field FISHLASTMODIFIEDON.
        /// Used on various language card types to automatically store the modification date indicating the last modification to the document
        /// </summary>
        public const string LastModifiedOn = "FISHLASTMODIFIEDON";
        
        /// <summary>
        /// The element of the field FISHCONDITIONS.
        /// Used on various language card types to store the condition name:value pairs present in the attached document.
        /// </summary>
        public const string Conditions = "FISHCONDITIONS";

        /// <summary>
        /// The element of the field FISHPLUGINS.
        /// Used on various language card types to store the succesfully ran plugins. Can be used by status transitions to verify if a plugin was ran succesfully (eg. valid xml)
        /// </summary>
        public const string SuccessfulPlugins = "FISHPLUGINS";

        /// <summary>
        /// The element of the field FSYSTEMLOCK.
        /// Used on various language card types to indicate that an asynchrone/background process is triggered to generate a reusable object.
        /// </summary>
        public const string ReusableObjectSystemLock = "FSYSTEMLOCK";

        /// <summary>
        /// The element of the field FTRANSLSTARTDATE.
        /// Used on various language card types to indicate that Translation Management is started on this language.  Translation Management will in background set the current date automatically in this field.
        /// </summary>
        public const string TranslationManagementStartDate = "FTRANSLSTARTDATE";

        /// <summary>
        /// The element of the field FISHWORDCOUNT.
        /// Used on various language card types to store the total number of words using space as delimiter in the current language attached xml file.
        /// </summary>
        public const string WordCount = "FISHWORDCOUNT";
        /// <summary>
        /// The element of the field FISHTRANSLATIONLOAD.
        /// Used on various language card types to store the number of untranslated words (excluding words within preTranslation elements) using the space as delimiter in the current language attached xml file.
        /// </summary>
        public const string TranslationLoad = "FISHTRANSLATIONLOAD";
        /// <summary>
        /// The element of the field FISHTOBETRANSLWC.
        /// Used on various language card types to store the number of untranslated words (excluding words within preTranslation elements) using the space as delimiter in the current language attached xml file.
        /// </summary>
        public const string ToBeTranslatedWordCount = "FISHTOBETRANSLWC";
        /// <summary>
        /// The element of the field FISHEDITTMPLTDESCRIPTION.
        /// Used for serving the description of an editor template in the web
        /// </summary>
        public const string EditorTemplateDescription = "FISHEDITTMPLTDESCRIPTION";
        /// <summary>
        /// The element of the field FISHTHUMBNAIL.
        /// Holds a reference on the logical level to the latest version ED card holding the Thumbnail Image data
        /// </summary>
        public const string Thumbnail = "FISHTHUMBNAIL";

        #region ContentObjects Fields - Language Object - Links
        /// <summary>
        /// The element of the Field FISHIMAGELINKS.
        /// Used on various language card types to store the illustration references present in the attached document.
        /// </summary>
        public const string ImageLinks = "FISHIMAGELINKS";
        /// <summary>
        /// The element of the Field FISHLINKS.
        /// Used on various language card types to store the object references (includes links and conref without anchor) present in the attached document.
        /// </summary>
        public const string Links = "FISHLINKS";
        /// <summary>
        /// The element of the field FISHHYPERLINKS.
        /// Used on various language card types to store the hyperlink references present in the attached document.
        /// </summary>
        public const string HyperLinks = "FISHHYPERLINKS";
        /// <summary>
        /// The element of the field FISHREUSABLEOBJECTS
        /// </summary>
        public const string ReusableObjects = "FISHREUSABLEOBJECTS";
        /// <summary>
        /// The element of the field FISHVARASSIGNED
        /// Used on various language card types to store all variable assignments in the attached document.
        /// </summary>
        public const string VariableAssignments = "FISHVARASSIGNED";
        /// <summary>
        /// The element of the field FISHVARINUSE
        /// Used on various language card types to store all variables that are used in the attached document.
        /// </summary>
        public const string VariablesInUse = "FISHVARINUSE";
        /// <summary>
        /// The element of the field FISHTARGETS
        /// Used on various language card types to store possible anchors in the attached document that can be used as a target for a document fragment link (eg. conref, hyperlink). 
        /// </summary>
        public const string Targets = "FISHTARGETS";
        /// <summary>
        /// The element of the field FISHFRAGMENTLINKS
        /// Used on various language card types to store the full document fragment references used in the attached document (eg. conref including anchor).
        /// </summary>
        public const string FragmentLinks = "FISHFRAGMENTLINKS";
        /// <summary>
        /// The element of the ConRefs (see FragmentLinks)
        /// </summary>
        public const string ConRefs = FragmentLinks;
        #endregion

        #region ContentObjects Fields - Language Object - Document
        /// <summary>
        /// The element of the field ED
        /// </summary>
        public const string ED = "ED";
        /// <summary>
        /// The element of the field CHECKED-OUT.
        /// Used on CTCONTEXT, CTBASELINE and all language level cards to store a boolean indicating whether the object is checked out or not.
        /// </summary>
        public const string CheckedOut = "CHECKED-OUT";
        /// <summary>
        /// The element of the field CHECKED-OUT-BY.
        /// Used on CTCONTEXT, CTBASELINE and all language level cards to store the reference to the USER card indicated as user who checked out this object.
        /// </summary>
        public const string CheckedOutBy = "CHECKED-OUT-BY";
        #endregion

        #endregion

        #endregion

        #region Publication Fields

        #region Publication Fields - Version Object
        /// <summary>
        /// The element of the field FISHBASELINE.
        /// This field links the version level of the Publication with the baseline card.
        /// </summary>
        public const string Baseline = "FISHBASELINE";
        /// <summary>
        /// The element of the field FISHMASTERREF.
        /// This field contains the master for this version level of the Publication
        /// </summary>
        public const string MasterReference = "FISHMASTERREF";
        /// <summary>
        /// The element of the field FISHRESOURCES.
        /// This field contains the resources for this version level of the Publication
        /// </summary>
        public const string Resources = "FISHRESOURCES";
        /// <summary>
        /// The element of the field FISHPUBSOURCELANGUAGES.
        /// This field contains the (required) source language(s) for this version level of the Publication
        /// </summary>
        public const string PublicationSourceLanguages = "FISHPUBSOURCELANGUAGES";
        /// <summary>
        /// The element of the field FISHREQUIREDRESOLUTIONS.
        /// This field contains the required resolution(s) for this version level of the Publication
        /// </summary>
        public const string RequiredResolutions = "FISHREQUIREDRESOLUTIONS";
        /// <summary>
        /// The element of the field FISHPUBCONTEXT.
        /// This field contains the context for this version level of the Publication
        /// </summary>
        public const string PublicationContext = "FISHPUBCONTEXT";
        /// <summary>
        /// The element of the field FISHISRELEASED.
        /// This field indicates if this version of the Publication is released or not.
        /// </summary>
        public const string PublicationReleased = "FISHISRELEASED";
        /// <summary>
        /// The element of the field FISHMODIFIEDON.
        /// This field indicates when this version of the Publication was modified.
        /// The version of the Publication is modified, when the baseline or publication context is changed. 
        /// The version of the Publication is NOT modified, when an extra Publication Output is added.
        /// </summary>
        public const string PublicationModifiedOn = "FISHMODIFIEDON";
        /// <summary>
        /// The element of the field FISHBASELINECOMPLETEMODE.
        /// </summary>
        public const string BaselineCompleteMode = "FISHBASELINECOMPLETEMODE";
        #endregion

        #region Publication Fields - Publication Output
        /// <summary>
        /// The element of the field FISHPUBLNGCOMBINATION.
        /// Used on the PublicationOutput to indicate the language combination of the publicaiton output
        /// </summary>
        public const string PublicationLanguageCombination = "FISHPUBLNGCOMBINATION";
        /// <summary>
        /// The element of the field FISHOUTPUTFORMATREF.
        /// This field links the PublicationOutput with the OutputFormat card.
        /// </summary>
        public const string OutputFormatReference = "FISHOUTPUTFORMATREF";
        /// <summary>
        /// The element of the field FISHPUBSTATUS.
        /// Used on the publication version to store the current publication status, matching a value from DPUBSTATUS.
        /// </summary>
        public const string PublicationOutputStatus = "FISHPUBSTATUS";
        /// <summary>
        /// The element of the field FISHEVENTID.
        /// Unique identifier of the event that was started to run this publication. If it is empty, there is no publication attached yet, or the publication has been reset.
        /// </summary>
        public const string PublishEventId = "FISHEVENTID";
        /// <summary>
        /// The element of the field FISHPUBLISHER.
        /// The user that published this publication. Contains a reference to an user.
        /// </summary>
        public const string Publisher = "FISHPUBLISHER";
        /// <summary>
        /// The element of the field FISHPUBSTARTDATE.
        /// Used on the publication language to indicate the start date of the publication process.
        /// </summary>
        public const string PublishStartDate = "FISHPUBSTARTDATE";
        /// <summary>
        /// The element of the field FISHPUBENDDATE.
        /// Used on the publication language to indicate the end date of the publication process.
        /// </summary>
        public const string PublishEndDate = "FISHPUBENDDATE";
        /// <summary>
        /// The element of the field FISHPUBREPORT.
        /// The publication report contains information or messages that where logged during the publication process. E.g. It will contain the reason why the publication was draft or why it failed.
        /// </summary>
        public const string PublishReport = "FISHPUBREPORT";
        /// <summary>
        /// The element of the field FISHJOBSPECDESTINATION.
        /// Used on PublicationOutput and links with system LOV DJOBSPECDESTINATION. It contains Output destination values Watch and OnHold.
        /// </summary>
        public const string JobSpecificationDestination = "FISHJOBSPECDESTINATION";
        /// <summary>
        /// The element of the field FISHPUBREVIEWENDDATE
        /// DateTime field which specifies an end date for reviewing. An empty value means the review time is infinite.
        /// </summary>
        public const string ReviewEndDate = "FISHPUBREVIEWENDDATE";
        /// <summary>
        /// The element of the field FPUBINCLUDECOMMENTS
        /// Publication language setting that allows XML comments to be included as part of a publication.
        /// </summary>
        public const string PublishIncludeComments = "FPUBINCLUDECOMMENTS";
        /// <summary>
        /// The element of the field FPUBINCLUDEMETADATA
        /// Publication language setting that allows metadata to be included as part of a publication.
        /// </summary>
        public const string PublishIncludeMetadata = "FPUBINCLUDEMETADATA";
        /// <summary>
        /// The element of the field FPUBWATERMARK
        /// Publication language setting that allows a string to be displayed as watermark as part of a publication.
        /// </summary>
        public const string PublishWaterMark = "FPUBWATERMARK";
        /// <summary>
        /// The element of the field FISHFALLBACKLNGDEFAULT
        /// Publish fallback languages for documents
        /// </summary>
        public const string PublishDocumentFallbackLanguages = "FISHFALLBACKLNGDEFAULT";
        /// <summary>
        /// The element of the field FISHFALLBACKLNGIMAGES
        /// Publish fallback languages for images
        /// </summary>
        public const string PublishImagesFallbackLanguages = "FISHFALLBACKLNGIMAGES";
        /// <summary>
        /// The element of the field FISHFALLBACKLNGRESOURCES
        /// Publish fallback languages for resources
        /// </summary>
        public const string PublishResourcesFallbackLanguages = "FISHFALLBACKLNGRESOURCES";
        #endregion

        #endregion

        #region Folder Fields
        /// <summary>
        /// The element of the field FDOCUMENTS
        /// Used on CTDOCMAP to store the list of logical cards available in this folder.
        /// </summary>
        public const string FolderContents = "FDOCUMENTS";
        /// <summary>
        /// The element of the field FDOCUMENTTYPE
        /// Used on CTDOCMAP to indicate which type of objects can be stored in this folder.
        /// </summary>
        public const string FolderContentType = "FDOCUMENTTYPE";
        /// <summary>
        /// The element of the field FFOLDERS
        /// Used on CTDOCMAP to store the list of all subfolders of this folder.
        /// </summary>
        public const string FolderSubFolders = "FFOLDERS";
        /// <summary>
        /// The element of the field FISHDOCUMENTREFERENCES
        /// Used on CTDOCMAP for folders of the type "Reference" to store the shortcuts to chosen logical cards (eg. My Favorites).
        /// </summary>
        public const string FolderContentReferences = "FISHDOCUMENTREFERENCES";
        /// <summary>
        /// The element of the field FISHQUERY
        /// Used on CTFOLDER for folders of the type "Query" to store the FolderQuery XML structure containing the metadata query to find the content of this folder.
        /// </summary>
        public const string FolderContentQuery = "FISHQUERY";
        /// <summary>
        /// The element of the field FNAME
        /// Used on CTDOCMAP to store the name of the folder
        /// </summary>
        public const string FolderName = "FNAME";
        /// <summary>
        /// Reference to the user that created/ owned the objects
        /// --> The owner of the Folder is stored in the FUSERGROUP
        /// </summary>
        public const string FolderOwner = Usergroup;
        /// <summary>
        /// The element of the field FISHFOLDERPATH
        /// Used on CTDOCMAP to store the folder path on the folders
        /// </summary>
        public const string FolderPath = "FISHFOLDERPATH";
        #endregion

        #region Translation Job Fields

        /// <summary>
        /// The element of the Field FNAME.
        /// The name of the translation job.
        /// </summary>
        public const string TranslationJobName = "FNAME";
        /// <summary>
        /// The element of the Field FISHTRANSJOBSTATUS.
        /// The status of the translation job.
        /// </summary>
        public const string TranslationJobStatus = "FISHTRANSJOBSTATUS";
        /// <summary>
        /// The element of the Field FISHTRANSJOBSENTOUTBY.
        /// The user who sent out the translation job.
        /// </summary>
        public const string TranslationJobSentOutBy = "FISHTRANSJOBSENTOUTBY";
        /// <summary>
        /// The element of the Field FISHTRANSJOBSRCLNG.
        /// The source language of the translation job.
        /// </summary>
        public const string TranslationJobSourceLanguage = "FISHTRANSJOBSRCLNG";
        /// <summary>
        /// The element of the Field FISHTRANSJOBTRANSTEMPLID.
        /// The id of the template used in the translation job.
        /// </summary>
        public const string TranslationJobTemplateId = "FISHTRANSJOBTRANSTEMPLID";
        /// <summary>
        /// The element of the Field FISHTRANSJOBREQUIREDDATE.
        /// The required date of the translation job.
        /// </summary>
        public const string TranslationJobRequiredDate = "FISHTRANSJOBREQUIREDDATE";
        /// <summary>
        /// The element of the Field FISHTRANSJOBINCLTRANSLTD.
        /// Specifies whether the translation job should include already translated items.
        /// </summary>
        public const string TranslationJobIncludeTranslated = "FISHTRANSJOBINCLTRANSLTD";
        /// <summary>
        /// The element of the Field FISHTRANSJOBTARGETFIELDS.
        /// The target fields of the translation job.
        /// </summary>
        public const string TranslationJobTargetFields = "FISHTRANSJOBTARGETFIELDS";
        /// <summary>
        /// The element of the Field FISHTRANSJOBPROGRESSID.
        /// The id of the event in the event monitor related to the translation job.
        /// </summary>
        public const string TranslationJobProgressId = "FISHTRANSJOBPROGRESSID";
        /// <summary>
        /// The element of the Field FISHPUSHTRANSPROGRESSIDS.
        /// The id of the event in the event monitor related to the translation job.
        /// </summary>
        public const string TranslationJobPushTranslationProgressIds = "FISHPUSHTRANSPROGRESSIDS";
        /// <summary>
        /// The element of the Field FISHTRANSJOBTYPE.
        /// The type of the translation job.
        /// </summary>
        public const string TranslationJobType = "FISHTRANSJOBTYPE";
        /// <summary>
        /// The element of the Field FDESCRIPTION.
        /// The description of the translation job.
        /// </summary>
        public const string TranslationJobDescription = "FDESCRIPTION";
        /// <summary>
        /// Reference to the user that created/ owned the objects
        /// --> The owner of the TranslationJob is stored in the FUSERGROUP
        /// </summary>
        public const string TranslationJobOwner = Usergroup;
        /// <summary>
        /// The element of the Field FISHLEASEDBY.
        /// The id of the processing step which is currently processing the translation job.
        /// </summary>
        public const string TranslationJobLeasedBy = "FISHLEASEDBY";
        /// <summary>
        /// The element of the Field FISHLEASEDON.
        /// The date and the time when the FISHLEASEDBY field was last changed.
        /// </summary>
        public const string TranslationJobLeasedOn = "FISHLEASEDON";
		/// <summary>
		/// The element of the Field FISHTRANSJOBALIAS.
		/// The alias of the translation job.
		/// </summary>
		public const string TranslationJobAlias = "FISHTRANSJOBALIAS";
        /// <summary>
        /// The element of the Field FISHTRANSJOBLEASES.
        /// The leases of the translation job.
        /// </summary>
        public const string TranslationJobLeases = "FISHTRANSJOBLEASES";

        #endregion
    }
}
