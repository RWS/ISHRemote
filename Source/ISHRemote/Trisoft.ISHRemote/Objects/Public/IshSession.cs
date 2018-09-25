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
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security;
using System.Text;
using Trisoft.ISHRemote.HelperClasses;
using Trisoft.ISHRemote.Interfaces;

namespace Trisoft.ISHRemote.Objects.Public
{
    /// <summary>
    /// <para type="description">Client session object to the InfoShare server instance required for every remote operation as it holds the web service url and authentication.</para>
    /// <para type="description">Furthermore it tracks your security token, provides direct client access to the web services API.</para>
    /// <para type="description">Gives access to contract parameters like seperators, date formats, batch and chunk sizes.</para>
    /// </summary>
    public class IshSession : IDisposable
    {
        private readonly ILogger _logger;

        private readonly Uri _webServicesBaseUri;
        private readonly Uri _wsTrustIssuerUri;
        private readonly Uri _wsTrustIssuerMexUri;
        private string _ishUserName;
        private string _userName;
        private readonly SecureString _ishSecurePassword;
        private readonly string _seperator = ", ";
        private readonly string _folderPathSeparator = @"\";

        private IshVersion _serverVersion;
        private IshVersion _clientVersion;
        private IshTypeFieldSetup _ishTypeFieldSetup;
        private Enumerations.StrictMetadataPreference _strictMetadataPreference = Enumerations.StrictMetadataPreference.Continue;

        private int _chunkSize = 10485760;
        private int _metadataBatchSize = 1000;
        private int _blobBatchSize = 50;
        private TimeSpan _timeout = new TimeSpan(0, 0, 20);  // up to 15s for a DNS lookup according to https://msdn.microsoft.com/en-us/library/system.net.http.httpclient.timeout%28v=vs.110%29.aspx
        private TimeSpan _timeoutIssue = TimeSpan.MaxValue;
        private readonly TimeSpan _timeoutService = TimeSpan.MaxValue;
        private readonly bool _ignoreSslPolicyErrors = false;
        private readonly bool _explicitIssuer = false;

        private InfoShareWcfConnection _connection;

        private Application25ServiceReference.Application _application25;
        private DocumentObj25ServiceReference.DocumentObj _documentObj25;
        private Folder25ServiceReference.Folder _folder25;
        private User25ServiceReference.User _user25;
        private UserRole25ServiceReference.UserRole _userRole25;
        private UserGroup25ServiceReference.UserGroup _userGroup25;
        private ListOfValues25ServiceReference.ListOfValues _listOfValues25;
        private PublicationOutput25ServiceReference.PublicationOutput _publicationOutput25;
        private OutputFormat25ServiceReference.OutputFormat _outputFormat25;
        private Settings25ServiceReference.Settings _settings25;
        private EDT25ServiceReference.EDT _EDT25;
        private EventMonitor25ServiceReference.EventMonitor _eventMonitor25;
        private Baseline25ServiceReference.Baseline _baseline25;
        private MetadataBinding25ServiceReference.MetadataBinding _metadataBinding25;
        private Search25ServiceReference.Search _search25;
        private TranslationJob25ServiceReference.TranslationJob _translationJob25;
        private TranslationTemplate25ServiceReference.TranslationTemplate _translationTemplate25;
        private BackgroundTask25ServiceReference.BackgroundTask _backgroundTask25;

        /// <summary>
        /// Creates a session object holding contracts and proxies to the web services API. Takes care of username/password and 'Active Directory' authentication (NetworkCredential) to the Secure Token Service.
        /// </summary>
        /// <param name="logger">Instance of the ILogger interface to allow some logging although Write-* is not very thread-friendly.</param>
        /// <param name="webServicesBaseUrl">The url to the web service API. For example 'https://example.com/ISHWS/'</param>
        /// <param name="ishUserName">InfoShare user name. For example 'Admin'</param>
        /// <param name="ishSecurePassword">Matching password as SecureString of the incoming user name. When null is provided, a NetworkCredential() is created instead.</param>
        /// <param name="timeout">Timeout to control Send/Receive timeouts of HttpClient when downloading content like connectionconfiguration.xml</param>
        /// <param name="timeoutIssue">Timeout to control Send/Receive timeouts of WCF when issuing a token</param>
        /// <param name="timeoutService">Timeout to control Send/Receive timeouts of WCF for InfoShareWS proxies</param>
        /// <param name="ignoreSslPolicyErrors">IgnoreSslPolicyErrors presence indicates that a custom callback will be assigned to ServicePointManager.ServerCertificateValidationCallback. Defaults false of course, as this is creates security holes! But very handy for Fiddler usage though.</param>
        public IshSession(ILogger logger, string webServicesBaseUrl, string ishUserName, SecureString ishSecurePassword, TimeSpan timeout, TimeSpan timeoutIssue, TimeSpan timeoutService, bool ignoreSslPolicyErrors)
        {
            _logger = logger;
            _explicitIssuer = false;
            _ignoreSslPolicyErrors = ignoreSslPolicyErrors;
            if (_ignoreSslPolicyErrors)
            {
                CertificateValidationHelper.OverrideCertificateValidation();
            }
            ServicePointManagerHelper.RestoreCertificateValidation();
            // webServicesBaseUrl should have trailing slash, otherwise .NET throws unhandy "Reference to undeclared entity 'raquo'." error
            _webServicesBaseUri = (webServicesBaseUrl.EndsWith("/")) ? new Uri(webServicesBaseUrl) : new Uri(webServicesBaseUrl + "/");
            _ishUserName = ishUserName == null ? Environment.UserName : ishUserName;
            _ishSecurePassword = ishSecurePassword;
            _timeout = timeout;
            _timeoutIssue = timeoutIssue;
            _timeoutService = timeoutService;
            CreateConnection();
        }

        /// <summary>
        /// Creates a session object holding contracts and proxies to the web services API. Takes care of username/password and 'Active Directory' authentication (NetworkCredential) to the Secure Token Service.
        /// </summary>
        /// <param name="logger">Instance of the ILogger interface to allow some logging although Write-* is not very thread-friendly.</param>
        /// <param name="webServicesBaseUrl">The url to the web service API. For example 'https://example.com/ISHWS/'</param>
        /// <param name="wsTrustIssuerUrl">The url to the security token service wstrust endpoint. For example 'https://example.com/ISHSTS/issue/wstrust/mixed/username'</param>
        /// <param name="wsTrustIssuerMexUrl">The binding for the wsTrustEndpoint url</param>
        /// <param name="ishUserName">InfoShare user name. For example 'Admin'</param>
        /// <param name="ishSecurePassword">Matching password as SecureString of the incoming user name. When null is provided, a NetworkCredential() is created instead.</param>
        /// <param name="timeout">Timeout to control Send/Receive timeouts of HttpClient when downloading content like connectionconfiguration.xml</param>
        /// <param name="timeoutIssue">Timeout to control Send/Receive timeouts of WCF when issuing a token</param>
        /// <param name="timeoutService">Timeout to control Send/Receive timeouts of WCF for InfoShareWS proxies</param>
        /// <param name="ignoreSslPolicyErrors">IgnoreSslPolicyErrors presence indicates that a custom callback will be assigned to ServicePointManager.ServerCertificateValidationCallback. Defaults false of course, as this is creates security holes! But very handy for Fiddler usage though.</param>
        public IshSession(ILogger logger, string webServicesBaseUrl, string wsTrustIssuerUrl, string wsTrustIssuerMexUrl, string ishUserName, SecureString ishSecurePassword, TimeSpan timeout, TimeSpan timeoutIssue, TimeSpan timeoutService, bool ignoreSslPolicyErrors)
        {
            _logger = logger;
            _explicitIssuer = true;
            _ignoreSslPolicyErrors = ignoreSslPolicyErrors;
            if (_ignoreSslPolicyErrors)
            {
                CertificateValidationHelper.OverrideCertificateValidation();
            }
            ServicePointManagerHelper.RestoreCertificateValidation();
            // webServicesBaseUrl should have trailing slash, otherwise .NET throws unhandy "Reference to undeclared entity 'raquo'." error
            _webServicesBaseUri = (webServicesBaseUrl.EndsWith("/")) ? new Uri(webServicesBaseUrl) : new Uri(webServicesBaseUrl + "/");
            _wsTrustIssuerUri = new Uri(wsTrustIssuerUrl);
            _wsTrustIssuerMexUri = new Uri(wsTrustIssuerMexUrl);

            _ishUserName = ishUserName == null ? Environment.UserName : ishUserName;
            _ishSecurePassword = ishSecurePassword;
            _timeout = timeout;
            _timeoutIssue = timeoutIssue;
            _timeoutService = timeoutService;
            CreateConnection();
        }

        private void CreateConnection()
        {
            //prepare connection for authentication/authorization
            var connectionParameters = new InfoShareWcfConnectionParameters
            {
                Credential = _ishSecurePassword == null ? null : new NetworkCredential(_ishUserName, SecureStringConversions.SecureStringToString(_ishSecurePassword)),
                Timeout = _timeout,
                IssueTimeout = _timeoutIssue,
                ServiceTimeout = _timeoutService
            };

            if (_explicitIssuer)
            {
                _connection = new InfoShareWcfConnection(_logger, _webServicesBaseUri, _wsTrustIssuerUri, _wsTrustIssuerMexUri, connectionParameters);
            }
            else
            {
                _connection = new InfoShareWcfConnection(_logger, _webServicesBaseUri, connectionParameters);
            }

            // application proxy to get server version or authentication context init is a must as it also confirms credentials, can take up to 1s
            _logger.WriteDebug("CreateConnection _serverVersion GetApplication25Channel");
            var application25Proxy = _connection.GetApplication25Channel();
            _logger.WriteDebug("CreateConnection _serverVersion GetApplication25Channel.GetVersion");
            _serverVersion = new IshVersion(application25Proxy.GetVersion());

        }

        internal IshTypeFieldSetup IshTypeFieldSetup
        {
            get
            {
                if (_ishTypeFieldSetup == null)
                {
                    if (_serverVersion.MajorVersion >= 13)
                    {
                        _logger.WriteDebug($"Loading Settings25.RetrieveFieldSetupByIshType...");
                        _ishTypeFieldSetup = new IshTypeFieldSetup(_logger, Settings25.RetrieveFieldSetupByIshType(null));
                        _ishTypeFieldSetup.StrictMetadataPreference = _strictMetadataPreference;
                    }
                    else
                    {
                        _logger.WriteDebug($"Loading TriDKXmlSetupFullExport_12_00_01...");
                        var triDKXmlSetupHelper = new TriDKXmlSetupHelper(_logger, Properties.Resouces.ISHTypeFieldSetup.TriDKXmlSetupFullExport_12_00_01);
                        _ishTypeFieldSetup = new IshTypeFieldSetup(_logger, triDKXmlSetupHelper.IshTypeFieldDefinition);
                        _ishTypeFieldSetup.StrictMetadataPreference = Enumerations.StrictMetadataPreference.Off;    // Otherwise custom metadata fields are always removed as they are unknown for the default TriDKXmlSetup Resource
                    }
                }
                return _ishTypeFieldSetup;
            }
        }

        public string WebServicesBaseUrl
        {
            get { return _webServicesBaseUri.ToString(); }
        }

        /// <summary>
        /// The user name used to authenticate to the serice, is initialized to Environment.UserName in case of Windows Authentication through NetworkCredential()
        /// </summary>
        public string IshUserName
        {
            get { return _ishUserName; }
            set { _ishUserName = value; }
        }

        /// <summary>
        /// The user name as available on the InfoShare User Profile in the CMS under field 'USERNAME'
        /// </summary>
        public string UserName
        {
            get
            {
                if (_userName == null)
                {
                    //TODO [Could] IshSession could initialize the current IshUser completely based on all available user metadata and store it on the IshSession
                    string requestedMetadata = "<ishfields><ishfield name='USERNAME' level='none'/></ishfields>";
                    string xmlIshObjects = User25.GetMyMetadata(requestedMetadata);
                    IshObjects ishObjects = new IshObjects(xmlIshObjects);
                    _userName = ishObjects.Objects[0].IshFields.GetFieldValue("USERNAME", Enumerations.Level.None, Enumerations.ValueType.Value);
                }
                return _userName;
            }
        }

        public string ServerVersion
        {
            get { return _serverVersion.ToString(); }
        }

        /// <summary>
        /// Retrieving assembly file version, actually can take up to 500 ms to get this initialized, so moved code to JIT property
        /// </summary>
        public string ClientVersion
        {
            get
            {
                if (_clientVersion == null)
                {
                    _clientVersion = new IshVersion(FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVer‌​sion);
                }
                return _clientVersion.ToString();
            }
        }

        public List<IshTypeFieldDefinition> IshTypeFieldDefinition
        {
            get
            {
                return IshTypeFieldSetup.IshTypeFieldDefinition;
            }
            internal set
            {
                _ishTypeFieldSetup = new IshTypeFieldSetup(_logger, value);
            }
        }

        public string AuthenticationContext
        {
            get
            {
                VerifyTokenValidity();

                var application25Proxy = _connection.GetApplication25Channel();
                return application25Proxy.Authenticate2();
            }
        }

        public string Seperator
        {
            get { return _seperator; }
        }

        public string FolderPathSeparator
        {
            get { return _folderPathSeparator; }
        }

        /// <summary>
        /// Timeout to control Send/Receive timeouts of HttpClient when downloading content like connectionconfiguration.xml
        /// </summary>
        public TimeSpan Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        /// <summary>
        /// Timeout to control Send/Receive timeouts of WCF when issuing a token
        /// </summary>
        public TimeSpan TimeoutIssue
        {
            get { return _timeoutIssue; }
            set { _timeoutIssue = value; }
        }

        /// <summary>
        /// Timeout to control Send/Receive timeouts of WCF for InfoShareWS proxies
        /// </summary>
        public TimeSpan TimeoutService
        {
            get { return _timeoutService; }
            // set { _timeoutService = value; }  // requires reset of all proxies
        }

        /// <summary>
        /// Web Service Retrieve batch size, if implemented, expressed in number of Ids/Objects for usage in metadata calls
        /// </summary>
        public int MetadataBatchSize
        {
            get { return _metadataBatchSize; }
            set { _metadataBatchSize = value; }
        }

        /// <summary>
        /// Client side filtering of nonexisting or unallowed metadata can be done silently, with warning or not at all. 
        /// </summary>
        public Enumerations.StrictMetadataPreference StrictMetadataPreference
        {
            get { return _strictMetadataPreference; }
            set
            {
                _strictMetadataPreference = value;
                IshTypeFieldSetup.StrictMetadataPreference = value;
            }
        }

        /// <summary>
        /// Web Service Retrieve batch size, if implemented, expressed in number of Ids/Objects for usage in blob/ishdata calls
        /// </summary>
        public int BlobBatchSize
        {
            get { return _blobBatchSize; }
            set { _blobBatchSize = value; }
        }

        /// <summary>
        /// Web Service Retrieve chunk size, if implemented, expressed in bytes
        /// </summary>
        public int ChunkSize
        {
            get { return _chunkSize; }
            set { _chunkSize = value; }
        }

        #region Web Services Getters

        public Application25ServiceReference.Application Application25
        {
            get
            {
                VerifyTokenValidity();

                if (_application25 == null)
                {
                    _application25 = _connection.GetApplication25Channel();
                }
                return _application25;
            }
        }

        public User25ServiceReference.User User25
        {
            get
            {
                VerifyTokenValidity();

                if (_user25 == null)
                {
                    _user25 = _connection.GetUser25Channel();
                }
                return _user25;
            }
        }

        public UserRole25ServiceReference.UserRole UserRole25
        {
            get
            {
                VerifyTokenValidity();

                if (_userRole25 == null)
                {
                    _userRole25 = _connection.GetUserRole25Channel();
                }
                return _userRole25;
            }
        }

        public UserGroup25ServiceReference.UserGroup UserGroup25
        {
            get
            {
                VerifyTokenValidity();

                if (_userGroup25 == null)
                {
                    _userGroup25 = _connection.GetUserGroup25Channel();
                }
                return _userGroup25;
            }
        }

        public DocumentObj25ServiceReference.DocumentObj DocumentObj25
        {
            get
            {
                VerifyTokenValidity();

                if (_documentObj25 == null)
                {
                    _documentObj25 = _connection.GetDocumentObj25Channel();
                }
                return _documentObj25;
            }
        }

        public PublicationOutput25ServiceReference.PublicationOutput PublicationOutput25
        {
            get
            {
                VerifyTokenValidity();

                if (_publicationOutput25 == null)
                {
                    _publicationOutput25 = _connection.GetPublicationOutput25Channel();
                }
                return _publicationOutput25;
            }
        }

        public Settings25ServiceReference.Settings Settings25
        {
            get
            {
                VerifyTokenValidity();

                if (_settings25 == null)
                {
                    _settings25 = _connection.GetSettings25Channel();
                }
                return _settings25;
            }
        }

        public EventMonitor25ServiceReference.EventMonitor EventMonitor25
        {
            get
            {
                VerifyTokenValidity();

                if (_eventMonitor25 == null)
                {
                    _eventMonitor25 = _connection.GetEventMonitor25Channel();
                }
                return _eventMonitor25;
            }
        }

        public Baseline25ServiceReference.Baseline Baseline25
        {
            get
            {
                VerifyTokenValidity();

                if (_baseline25 == null)
                {
                    _baseline25 = _connection.GetBaseline25Channel();
                }
                return _baseline25;
            }
        }

        public MetadataBinding25ServiceReference.MetadataBinding MetadataBinding25
        {
            get
            {
                VerifyTokenValidity();

                if (_metadataBinding25 == null)
                {
                    _metadataBinding25 = _connection.GetMetadataBinding25Channel();
                }
                return _metadataBinding25;
            }
        }

        public Folder25ServiceReference.Folder Folder25
        {
            get
            {
                VerifyTokenValidity();

                if (_folder25 == null)
                {
                    _folder25 = _connection.GetFolder25Channel();
                }
                return _folder25;
            }
        }

        public ListOfValues25ServiceReference.ListOfValues ListOfValues25
        {
            get
            {
                VerifyTokenValidity();

                if (_listOfValues25 == null)
                {
                    _listOfValues25 = _connection.GetListOfValues25Channel();
                }
                return _listOfValues25;
            }
        }

        public OutputFormat25ServiceReference.OutputFormat OutputFormat25
        {
            get
            {
                VerifyTokenValidity();

                if (_outputFormat25 == null)
                {
                    _outputFormat25 = _connection.GetOutputFormat25Channel();
                }
                return _outputFormat25;
            }
        }

        public EDT25ServiceReference.EDT EDT25
        {
            get
            {
                VerifyTokenValidity();

                if (_EDT25 == null)
                {
                    _EDT25 = _connection.GetEDT25Channel();
                }
                return _EDT25;
            }
        }

        public TranslationJob25ServiceReference.TranslationJob TranslationJob25
        {
            get
            {
                VerifyTokenValidity();

                if (_translationJob25 == null)
                {
                    _translationJob25 = _connection.GetTranslationJob25Channel();
                }
                return _translationJob25;
            }
        }

        public TranslationTemplate25ServiceReference.TranslationTemplate TranslationTemplate25
        {
            get
            {
                VerifyTokenValidity();

                if (_translationTemplate25 == null)
                {
                    _translationTemplate25 = _connection.GetTranslationTemplate25Channel();
                }
                return _translationTemplate25;
            }
        }

        public Search25ServiceReference.Search Search25
        {
            get
            {
                VerifyTokenValidity();

                if (_search25 == null)
                {
                    _search25 = _connection.GetSearch25Channel();
                }
                return _search25;
            }
        }

        public BackgroundTask25ServiceReference.BackgroundTask BackgroundTask25
        {
            get
            {
                VerifyTokenValidity();

                if (_backgroundTask25 == null)
                {
                    _backgroundTask25 = _connection.GetBackgroundTask25Channel();
                }
                return _backgroundTask25;
            }
        }

        #endregion

        private void VerifyTokenValidity()
        {
            if (_connection.IsValid) return;

            // Not valid...
            // ...dispose connection
            _connection.Dispose();
            // ...discard all channels
            _application25 = null;
            _baseline25 = null;
            _documentObj25 = null;
            _EDT25 = null;
            _eventMonitor25 = null;
            _folder25 = null;
            _listOfValues25 = null;
            _metadataBinding25 = null;
            _outputFormat25 = null;
            _publicationOutput25 = null;
            _search25 = null;
            _settings25 = null;
            _translationJob25 = null;
            _translationTemplate25 = null;
            _user25 = null;
            _userGroup25 = null;
            _userRole25 = null;
            // ...and re-create connection
            CreateConnection();
        }

        public void Dispose()
        {
            _connection.Dispose();
            if (_ignoreSslPolicyErrors)
            {
                CertificateValidationHelper.RestoreCertificateValidation();
            }
        }
        public void Close()
        {
            Dispose();
        }
    }
}
