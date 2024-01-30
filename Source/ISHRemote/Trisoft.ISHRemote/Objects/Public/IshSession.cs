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
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security;
using Trisoft.ISHRemote.Connection;
using Trisoft.ISHRemote.HelperClasses;
using Trisoft.ISHRemote.Interfaces;
using Trisoft.ISHRemote.OpenApiISH30;

namespace Trisoft.ISHRemote.Objects.Public
{
    /// <summary>
    /// <para type="description">Client session object to the InfoShare server instance required for every remote operation as it holds the web service url and authentication.</para>
    /// <para type="description">Furthermore it tracks your security token, provides direct client access to the web services API.</para>
    /// <para type="description">Gives access to contract parameters like separators, date formats, batch and chunk sizes.</para>
    /// </summary>
    public class IshSession : IDisposable
    {
        private readonly ILogger _logger;

        private readonly Uri _webServicesBaseUri;
        private string _ishUserName;
        private string _userName;
        private string _userLanguage;
        private readonly SecureString _ishSecurePassword;
        private string _clientId;
        private readonly SecureString _clientSecureSecret;
        private readonly string _separator = ", ";
        private readonly string _folderPathSeparator = @"\";

        private IshVersion _serverVersion;
        private IshVersion _clientVersion;
        private IshTypeFieldSetup _ishTypeFieldSetup;
        private Enumerations.StrictMetadataPreference _strictMetadataPreference = Enumerations.StrictMetadataPreference.Continue;
        private NameHelper _nameHelper;
        private Enumerations.PipelineObjectPreference _pipelineObjectPreference = Enumerations.PipelineObjectPreference.PSObjectNoteProperty;
        private Enumerations.RequestedMetadataGroup _defaultRequestedMetadata = Enumerations.RequestedMetadataGroup.Basic;

        /// <summary>
        /// Used by the SOAP API that retrieves files/blobs in multiple chunk, this parameter is the chunksize (10485760 bytes is 10Mb)
        /// </summary>
        private int _chunkSize = 10485760;
        /// <summary>
        /// Used to divide bigger data set retrievals in multiple API calls, 999 is the best optimization server-side (Oracle IN-clause only allows 999 values, so 1000 would mean 2x queries server-side)
        /// </summary>
        private int _metadataBatchSize = 999;
        private int _blobBatchSize = 50;
        private TimeSpan _timeout = new TimeSpan(0, 0, 20);  // up to 15s for a DNS lookup according to https://msdn.microsoft.com/en-us/library/system.net.http.httpclient.timeout%28v=vs.110%29.aspx
        private readonly bool _ignoreSslPolicyErrors = false;
        private Enumerations.Protocol _protocol;

        // one HttpClient per IshSession with potential certificate overwrites which can be reused across requests
        private readonly HttpClient _httpClient;
        /// <summary>
        /// OpenIdConnect Client Application Id that is typically configured in Access Management (ISHID) to allow a local redirect (http://127.0.0.1:SomePort/)
        /// This option is not typically used but allows validating other applications like Tridion_Docs_Content_Importer
        /// </summary>
        private string _clientAppId = "ISHRemote";
        private InfoShareOpenIdConnectConnectionParameters _infoShareOpenIdConnectConnectionParameters; 
        private InfoShareOpenApiWithOpenIdConnectConnection _infoShareOpenApiWithOpenIdConnectConnection;
        private InfoShareWcfSoapWithOpenIdConnectConnection _infoShareWcfSoapWithOpenIdConnectConnection;
        private InfoShareWcfSoapWithWsTrustConnectionParameters _infoShareWcfSoapWithWsTrustConnectionParameters;
        private InfoShareWcfSoapWithWsTrustConnection _infoShareWcfSoapWithWsTrustConnection;
        private Annotation25ServiceReference.Annotation _annotation25;
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
        /// <param name="clientId">Access Management (ISHAM) Client Identifier of a Service Account. For example 'c826e7e1-c35c-43fe-9756-e1b61f44bb40'</param>
        /// <param name="clientSecureSecret">Access Management (ISHAM) Client Secret is the matching password as SecureString of the clientId.For example 'ziKiGbx6N0G3m69/vWMZUTs2paVO1Mzqt6Y6TX7mnpPJyFVODsAAAA=='.</param>
        /// <param name="timeout">Timeout to control Send/Receive timeouts of HttpClient when downloading content like connectionconfiguration.xml</param>
        /// <param name="ignoreSslPolicyErrors">IgnoreSslPolicyErrors presence indicates that a custom callback will be assigned to ServicePointManager.ServerCertificateValidationCallback. Defaults false of course, as this is creates security holes! But very handy for Fiddler usage though.</param>
        /// <param name="protocol">Protocol indicates the preferred communication/authentication route.</param>
        public IshSession(ILogger logger, string webServicesBaseUrl, string ishUserName, SecureString ishSecurePassword, string clientId, SecureString clientSecureSecret, TimeSpan timeout, bool ignoreSslPolicyErrors, Enumerations.Protocol protocol)
        {
            _logger = logger;
            _ignoreSslPolicyErrors = ignoreSslPolicyErrors;
            _protocol = protocol;
            HttpClientHandler handler = new HttpClientHandler();
            _timeout = timeout;
#if NET48
            _logger.WriteDebug($"Enabling Tls, Tls11, Tls12 and Tls13 security protocols on AppDomain. Timeout[{_timeout}] IgnoreSslPolicyErrors[{_ignoreSslPolicyErrors}]");
            if (_ignoreSslPolicyErrors)
            {
                CertificateValidationHelper.OverrideCertificateValidation();
            }
#endif
            _logger.WriteDebug($"Enabling Tls, Tls11, Tls12 and Tls13 security protocols on HttpClientHandler. Timeout[{_timeout}] IgnoreSslPolicyErrors[{_ignoreSslPolicyErrors}]");
            if (_ignoreSslPolicyErrors)
            {
                // ISHRemote 0.x used CertificateValidationHelper.OverrideCertificateValidation which only works on net48 and overwrites the full AppDomain,
                // below solution is cleaner for HttpHandler (so connectionconfiguration.xml and future OpenAPI) and SOAP proxies use factory.Credentials.ServiceCertificate.SslCertificateAuthentication
                // CertificateValidationHelper.OverrideCertificateValidation();
                // overwrite certificate handling for HttpClient requests
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
            handler.SslProtocols = (System.Security.Authentication.SslProtocols)(SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13);
            _httpClient = new HttpClient(handler);
            _httpClient.Timeout = _timeout;
            // webServicesBaseUrl should have trailing slash, otherwise .NET throws unhandy "Reference to undeclared entity 'raquo'." error
            _webServicesBaseUri = (webServicesBaseUrl.EndsWith("/")) ? new Uri(webServicesBaseUrl) : new Uri(webServicesBaseUrl + "/");
            _ishUserName = ishUserName == null ? Environment.UserName : ishUserName;
            _ishSecurePassword = ishSecurePassword;
            _clientId = clientId;
            _clientSecureSecret = clientSecureSecret;

            // Detecting versions
            var ishwsConnectionConfigurationUri = new Uri(_webServicesBaseUri, "connectionconfiguration.xml");
            IshConnectionConfiguration ishwsConnectionConfiguration = LoadConnectionConfiguration(ishwsConnectionConfigurationUri);
            _logger.WriteVerbose($"LoadConnectionConfiguration found InfoShareWSUrl[{ishwsConnectionConfiguration.InfoShareWSUrl}] ApplicationName[{ishwsConnectionConfiguration.ApplicationName}] SoftwareVersion[{ishwsConnectionConfiguration.SoftwareVersion}]");
            if (ishwsConnectionConfiguration.InfoShareWSUrl != _webServicesBaseUri)
            {
                _logger.WriteDebug($"LoadConnectionConfiguration noticed incoming _webServicesBaseUri[{_webServicesBaseUri}] differs from ishwsConnectionConfiguration.InfoShareWSUrl[{ishwsConnectionConfiguration.InfoShareWSUrl}]. Using _webServicesBaseUri.");
            }
            IshConnectionConfiguration owcfConnectionConfiguration = null;
            if (new IshVersion(ishwsConnectionConfiguration.SoftwareVersion).MajorVersion >= 15)
            {
                var wcfConnectionConfigurationUri = new Uri(_webServicesBaseUri, "owcf/connectionconfiguration.xml");
                owcfConnectionConfiguration = LoadConnectionConfiguration(wcfConnectionConfigurationUri);
                _logger.WriteVerbose($"LoadConnectionConfiguration found InfoShareWSUrl[{owcfConnectionConfiguration.InfoShareWSUrl}] ApplicationName[{owcfConnectionConfiguration.ApplicationName}] SoftwareVersion[{owcfConnectionConfiguration.SoftwareVersion}]");
            }

            // Detecting protocols based on versions
            if (_protocol == Enumerations.Protocol.Autodetect && (new IshVersion(ishwsConnectionConfiguration.SoftwareVersion).MajorVersion <= 14))
            {
                _protocol = Enumerations.Protocol.WcfSoapWithWsTrust;
                _logger.WriteDebug($"LoadConnectionConfiguration selecting _protocol[{_protocol}] on SoftwareVersion[{ishwsConnectionConfiguration.SoftwareVersion}]");
            }
            else if (_protocol == Enumerations.Protocol.Autodetect && (new IshVersion(ishwsConnectionConfiguration.SoftwareVersion).MajorVersion == 15))
            {
                _protocol = Enumerations.Protocol.WcfSoapWithOpenIdConnect;
                _logger.WriteDebug($"LoadConnectionConfiguration selecting _protocol[{_protocol}] on SoftwareVersion[{ishwsConnectionConfiguration.SoftwareVersion}]");
            }
            else if (_protocol == Enumerations.Protocol.Autodetect && (new IshVersion(ishwsConnectionConfiguration.SoftwareVersion).MajorVersion >= 16))
            {
                // >= 16, so predicting public OpenApi
                _protocol = Enumerations.Protocol.OpenApiWithOpenIdConnect;
                _logger.WriteDebug($"LoadConnectionConfiguration selecting _protocol[{_protocol}] on SoftwareVersion[{ishwsConnectionConfiguration.SoftwareVersion}]");
            }

            // Initializing based on protocols
            switch (_protocol)
            {
                case Enumerations.Protocol.WcfSoapWithWsTrust:
                    _logger.WriteDebug($"LoadConnectionConfiguration selected protocol[{_protocol}]");
                    _infoShareWcfSoapWithWsTrustConnectionParameters = new InfoShareWcfSoapWithWsTrustConnectionParameters
                    {
                        AuthenticationType = ishwsConnectionConfiguration.AuthenticationType,
                        InfoShareWSUrl = ishwsConnectionConfiguration.InfoShareWSUrl,
                        IssuerUrl = ishwsConnectionConfiguration.IssuerUrl,
                        Credential = _ishSecurePassword == null ? null : new NetworkCredential(_ishUserName, SecureStringConversions.SecureStringToString(_ishSecurePassword)),
                        Timeout = _timeout,
                        IgnoreSslPolicyErrors = _ignoreSslPolicyErrors
                    };
                    CreateInfoShareWcfSoapWithWsTrustConnection();
                    break;
                case Enumerations.Protocol.WcfSoapWithOpenIdConnect:
                    _logger.WriteDebug($"LoadConnectionConfiguration selected protocol[{_protocol}]");
                    _infoShareOpenIdConnectConnectionParameters = new InfoShareOpenIdConnectConnectionParameters
                    {
                        AuthenticationType = owcfConnectionConfiguration.AuthenticationType,
                        InfoShareWSUrl = owcfConnectionConfiguration.InfoShareWSUrl,
                        IssuerUrl = owcfConnectionConfiguration.IssuerUrl,
                        Timeout = _timeout,
                        SystemBrowserTimeout = new TimeSpan(0, 1, 0),
                        ClientAppId = _clientAppId,
                        ClientId = _clientId,
                        ClientSecret = SecureStringConversions.SecureStringToString(_clientSecureSecret)
                    };
                    CreateInfoShareWcfSoapWithOpenIdConnectConnection();
                    break;
                case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                    _logger.WriteDebug($"LoadConnectionConfiguration selected protocol[{_protocol}]");
                    _infoShareOpenIdConnectConnectionParameters = new InfoShareOpenIdConnectConnectionParameters
                    {
                        AuthenticationType = owcfConnectionConfiguration.AuthenticationType,
                        InfoShareWSUrl = owcfConnectionConfiguration.InfoShareWSUrl,
                        IssuerUrl = owcfConnectionConfiguration.IssuerUrl,
                        Timeout = _timeout,
                        SystemBrowserTimeout = new TimeSpan(0, 1, 0),
                        ClientAppId = _clientAppId,
                        ClientId = _clientId,
                        ClientSecret = SecureStringConversions.SecureStringToString(_clientSecureSecret)
                    };
                    CreateOpenApiWithOpenIdConnectConnection();
                    // explictly initializing WcfSoapWithOpenIdConnect as well, as many cmdlets have matching OpenAPI calls (and/or implementation) missing
                    CreateInfoShareWcfSoapWithOpenIdConnectConnection();
                    break;
                default:
                    throw new ArgumentException($"IshSession _protocol[{_protocol}] was unexpected.");
            }
        }

        private IshConnectionConfiguration LoadConnectionConfiguration(Uri connectionConfigurationUri)
        {
            _logger.WriteDebug($"LoadConnectionConfiguration uri[{connectionConfigurationUri}] timeout[{_httpClient.Timeout}]");
            var responseMessage = _httpClient.GetAsync(connectionConfigurationUri).GetAwaiter().GetResult();
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new ArgumentException($"LoadConnectionConfiguration uri[{connectionConfigurationUri}] timeout[{_httpClient.Timeout}] failed with StatusCode[{responseMessage.StatusCode}]");
            }
            string response = responseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            //_logger.WriteDebug($"LoadConnectionConfiguration response[{response}]");
            return new IshConnectionConfiguration(response);
        }

        private void CreateInfoShareWcfSoapWithWsTrustConnection()
        {
            _logger.WriteVerbose($"CreateInfoShareWcfSoapWithWsTrustConnection");
            _infoShareWcfSoapWithWsTrustConnection = new InfoShareWcfSoapWithWsTrustConnection(_logger, _httpClient, _infoShareWcfSoapWithWsTrustConnectionParameters);
            // application proxy to get server version or authentication context init is a must as it also confirms credentials, can take up to 1s
            _logger.WriteDebug("CreateInfoShareWcfSoapWithWsTrustConnection _serverVersion GetApplication25Channel");
            var application25Proxy = _infoShareWcfSoapWithWsTrustConnection.GetApplication25Channel();
            _logger.WriteDebug("CreateInfoShareWcfSoapWithWsTrustConnection _serverVersion GetApplication25Channel.GetVersion");
            _serverVersion = new IshVersion(application25Proxy.GetVersion());
        }

        private void CreateInfoShareWcfSoapWithOpenIdConnectConnection()
        {
            _logger.WriteVerbose($"CreateInfoShareWcfSoapWithOpenIdConnectConnection");
            _infoShareWcfSoapWithOpenIdConnectConnection = new InfoShareWcfSoapWithOpenIdConnectConnection(_logger, _httpClient, _infoShareOpenIdConnectConnectionParameters);
            // application proxy to get server version or authentication context init is a must as it also confirms credentials, can take up to 1s
            _logger.WriteDebug("CreateInfoShareWcfSoapWithOpenIdConnectConnection _serverVersion GetApplication25Channel");
            var application25Proxy = _infoShareWcfSoapWithOpenIdConnectConnection.GetApplication25Channel();
            _logger.WriteDebug("CreateInfoShareWcfSoapWithOpenIdConnectConnection _serverVersion GetApplication25Channel.GetVersion");
            _serverVersion = new IshVersion(application25Proxy.GetVersion());
        }

        private void CreateOpenApiWithOpenIdConnectConnection()
        {
            
            _logger.WriteVerbose($"CreateOpenApiWithOpenIdConnectConnection");
            _infoShareOpenApiWithOpenIdConnectConnection = new InfoShareOpenApiWithOpenIdConnectConnection(_logger, _httpClient, _infoShareOpenIdConnectConnectionParameters);
            _logger.WriteDebug("CreateOpenApiWithOpenIdConnectConnection openApi30Service.GetApplicationVersionAsync");
            _serverVersion = new IshVersion(_infoShareOpenApiWithOpenIdConnectConnection.GetOpenApiISH30ServiceProxy().GetApplicationVersionAsync().GetAwaiter().GetResult());
        }

        internal IshTypeFieldSetup IshTypeFieldSetup
        {
            get
            {
                if (_ishTypeFieldSetup == null)
                {
                    if (_serverVersion.MajorVersion >= 13) 
                    {
                        switch (Protocol)
                        {
                            case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                                // TODO [Must] Add OpenApi implementation
                            case Enumerations.Protocol.WcfSoapWithWsTrust:
                            case Enumerations.Protocol.WcfSoapWithOpenIdConnect:
                                _logger.WriteDebug($"Loading Settings25.RetrieveFieldSetupByIshType...");
                                _ishTypeFieldSetup = new IshTypeFieldSetup(_logger, Settings25.RetrieveFieldSetupByIshType(null), ServerIshVersion);
                                break;
                        }
                        _ishTypeFieldSetup.StrictMetadataPreference = _strictMetadataPreference;
                    }
                    else
                    {
                        _logger.WriteDebug($"Loading TriDKXmlSetupFullExport_12_00_01...");
                        var triDKXmlSetupHelper = new TriDKXmlSetupHelper(_logger, Properties.Resouces.ISHTypeFieldSetup.TriDKXmlSetupFullExport_12_00_01);
                        _ishTypeFieldSetup = new IshTypeFieldSetup(_logger, triDKXmlSetupHelper.IshTypeFieldDefinition);
                        _ishTypeFieldSetup.StrictMetadataPreference = Enumerations.StrictMetadataPreference.Off;    // Otherwise custom metadata fields are always removed as they are unknown for the default TriDKXmlSetup Resource
                    }

                    if (_serverVersion.MajorVersion == 13 || (_serverVersion.MajorVersion == 14 && _serverVersion.RevisionVersion < 4))
                    {
                        // Loading/Merging Settings ISHMetadataBinding for 13/13.0.0 up till 14SP4/14.0.4 setup
                        // Note that IMetadataBinding was introduced in 2016/12.0.0 but there was no dynamic FieldSetup retrieval
                        // Passing IshExtensionConfig object to IshTypeFieldSetup constructor
                        _logger.WriteDebug($"Loading Settings25.GetMetadata for field[" + FieldElements.ExtensionConfiguration + "]...");
                        IshFields metadata = new IshFields();
                        metadata.AddField(new IshRequestedMetadataField(FieldElements.ExtensionConfiguration, Enumerations.Level.None, Enumerations.ValueType.Value));  // do not pass over IshTypeFieldSetup.ToIshRequestedMetadataFields, as we are initializing that object
                        string xmlIshObjects = Settings25.GetMetadata(metadata.ToXml());
                        var ishFields = new IshObjects(xmlIshObjects).Objects[0].IshFields;
                        string xmlSettingsExtensionConfig = ishFields.GetFieldValue(FieldElements.ExtensionConfiguration, Enumerations.Level.None, Enumerations.ValueType.Value);
                        IshSettingsExtensionConfig.MergeIntoIshTypeFieldSetup(_logger, _ishTypeFieldSetup, xmlSettingsExtensionConfig);
                    }
                    
                }
                return _ishTypeFieldSetup;
            }
        }

        internal NameHelper NameHelper
        {
            get
            {
                if (_nameHelper == null)
                {
                    _nameHelper = new NameHelper(this);
                }
                return _nameHelper;
            }
        }

        public string WebServicesBaseUrl
        {
            get { return _webServicesBaseUri.ToString(); }
        }

        public Enumerations.Protocol Protocol
        {
            get
            {
                return _protocol;
            }
            set
            {
                _protocol = value;
            }
        }

        /// <summary>
        /// The user name used to authenticate to the service, is initialized to Environment.UserName in case of Windows Authentication through NetworkCredential()
        /// </summary>
        public string IshUserName
        {
            get { return _ishUserName; }
            set { _ishUserName = value; }
        }

        /// <summary>
        /// The Client ID of the client/secret combination you passed for Credential Flow authentication (most likely over Access Management (ISHAM)).
        /// </summary>
        public string ClientId
        {
            get { return _clientId; }
            set { _clientId = value; }
        }

        internal string Name
        {
            get { return $"[{WebServicesBaseUrl}][{IshUserName}]"; }
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
                    switch (Protocol)
                    {
                        case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                        // TODO [Must] Add OpenApi implementation
                        case Enumerations.Protocol.WcfSoapWithWsTrust:
                        case Enumerations.Protocol.WcfSoapWithOpenIdConnect:
                            string requestedMetadata = "<ishfields><ishfield name='USERNAME' level='none'/></ishfields>";
                            string xmlIshObjects = User25.GetMyMetadata(requestedMetadata);
                            Enumerations.ISHType[] ISHType = { Enumerations.ISHType.ISHUser };
                            IshObjects ishObjects = new IshObjects(ISHType, xmlIshObjects);
                            _userName = ishObjects.Objects[0].IshFields.GetFieldValue("USERNAME", Enumerations.Level.None, Enumerations.ValueType.Value);
                            break;
                    }
                }
                return _userName;
            }
        }

        /// <summary>
        /// The user language as available on the InfoShare User Profile in the CMS under field 'FISHUSERLANGUAGE'
        /// </summary>
        public string UserLanguage
        {
            get
            {
                if (_userLanguage == null)
                {
                    //TODO [Could] IshSession could initialize the current IshUser completely based on all available user metadata and store it on the IshSession
                    switch (Protocol)
                    {
                        case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                            // TODO [Must] Add OpenApi implementation
                        case Enumerations.Protocol.WcfSoapWithWsTrust:
                        case Enumerations.Protocol.WcfSoapWithOpenIdConnect:
                            string requestedMetadata = "<ishfields><ishfield name='FISHUSERLANGUAGE' level='none'/></ishfields>";
                            string xmlIshObjects = User25.GetMyMetadata(requestedMetadata);
                            Enumerations.ISHType[] ISHType = { Enumerations.ISHType.ISHUser };
                            IshObjects ishObjects = new IshObjects(ISHType, xmlIshObjects);
                            _userLanguage = ishObjects.Objects[0].IshFields.GetFieldValue("FISHUSERLANGUAGE", Enumerations.Level.None, Enumerations.ValueType.Value);
                            break;
                    }
                }
                return _userLanguage;
            }
        }

        internal IshVersion ServerIshVersion
        {
            get { return _serverVersion; }
        }

        public string ServerVersion
        {
            get { return _serverVersion.ToString(); }
        }

        /// <summary>
        /// Retrieving assembly file version, actually can take up to 500 ms to get this initialized, so moved code to JIT property
        /// </summary>
        internal IshVersion ClientIshVersion
        {
            get
            {
                if (_clientVersion == null)
                {
                    _clientVersion = new IshVersion(FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVer‌​sion);
                }
                return _clientVersion;
            }
        }
        
        public string ClientVersion
        {
            get { return ClientIshVersion.ToString(); }
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
                VerifyConnectionValidity();
                switch (Protocol)
                {
                    case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                        // TODO [Must] Add OpenApi implementation
                    case Enumerations.Protocol.WcfSoapWithWsTrust:
                    case Enumerations.Protocol.WcfSoapWithOpenIdConnect:
                        return Application25.Authenticate2();
                }
                return ("Not-Available-Over-" + Protocol.ToString());
            }
        }

        /// <summary>
        /// Access Token is also known as Bearer Token
        /// </summary>
        public string AccessToken
        {
            get
            {
                VerifyConnectionValidity();
                switch (Protocol)
                {
                    case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                    case Enumerations.Protocol.WcfSoapWithOpenIdConnect:
                        return _infoShareOpenIdConnectConnectionParameters.Tokens.AccessToken;
                }
                return String.Empty;
            }
        }

        /// <summary>
        /// OpenIdConnect Client Application Id that is typically configured in Access Management (ISHID) to allow a local redirect (http://127.0.0.1:SomePort/)
        /// This option is not typically used but allows validating other applications like Tridion_Docs_Content_Importer
        /// </summary>
        public string ClientAppId
        {
            get { return _clientAppId; }
            // Setter is a bit silly as New-IShSession doesn't set it in time, we'll see...
            set { _clientAppId = value; }
        }


        public string Separator
        {
            get { return _separator; }
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
        /// Web Service Retrieve batch size, if implemented, expressed in number of Ids/Objects for usage in metadata calls
        /// </summary>
        public int MetadataBatchSize
        {
            get { return _metadataBatchSize; }
            set { _metadataBatchSize = (value > 0) ? value : 999; }
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
        /// Allows tuning client-side object enrichment like no wrapping (off) or PSObject-with-PSNoteProperty wrapping.
        /// </summary>
        public Enumerations.PipelineObjectPreference PipelineObjectPreference
        {
            get { return _pipelineObjectPreference; }
            set { _pipelineObjectPreference = value; }
        }

        /// <summary>
        /// Any RequestedMetadata will be preloaded with the Descriptive/Basic/All metadata fields known for the ISHType[] in use by the cmdlet
        /// A potential override/append by the specified -RequestedMetadata is possible.
        /// </summary>
        public Enumerations.RequestedMetadataGroup DefaultRequestedMetadata
        {
            get { return _defaultRequestedMetadata; }
            set { _defaultRequestedMetadata = value; }
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

        #region OpenApi Services
        public OpenApiISH30Service OpenApiISH30Service
        {
            get
            {
                VerifyConnectionValidity();
                return _infoShareOpenApiWithOpenIdConnectConnection.GetOpenApiISH30ServiceProxy();
            }
        }
        #endregion

        #region Soap Api 2.0/2.5 Services

        public Annotation25ServiceReference.Annotation Annotation25
        {
            get
            {
                VerifyConnectionValidity();
                switch (_protocol)
                {
                    case Enumerations.Protocol.WcfSoapWithWsTrust:
                        if ((_annotation25 == null) || (_infoShareWcfSoapWithWsTrustConnection.IsTokenAlmostExpired))
                            _annotation25 = _infoShareWcfSoapWithWsTrustConnection.GetAnnotation25Channel();
                        break;
                    case Enumerations.Protocol.WcfSoapWithOpenIdConnect:
                    case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                        if ((_annotation25 == null) || (_infoShareWcfSoapWithOpenIdConnectConnection.IsTokenAlmostExpired))
                            _annotation25 = _infoShareWcfSoapWithOpenIdConnectConnection.GetAnnotation25Channel();
                        break;
                    default:
                        throw new ArgumentException($"IshSession _protocol[{_protocol}] was unexpected.");
                }
                return _annotation25;
            }
        }

        public Application25ServiceReference.Application Application25
        {
            get
            {
                VerifyConnectionValidity();
                switch (_protocol)
                {
                    case Enumerations.Protocol.WcfSoapWithWsTrust:
                        if ((_application25 == null) || (_infoShareWcfSoapWithWsTrustConnection.IsTokenAlmostExpired))
                            _application25 = _infoShareWcfSoapWithWsTrustConnection.GetApplication25Channel();
                        break;
                    case Enumerations.Protocol.WcfSoapWithOpenIdConnect:
                    case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                        if ((_application25 == null) || (_infoShareWcfSoapWithOpenIdConnectConnection.IsTokenAlmostExpired))
                            _application25 = _infoShareWcfSoapWithOpenIdConnectConnection.GetApplication25Channel();
                        break;
                    default:
                        throw new ArgumentException($"IshSession _protocol[{_protocol}] was unexpected.");
                }
                
                return _application25;
            }
        }

        public User25ServiceReference.User User25
        {
            get
            {
                VerifyConnectionValidity();
                switch (_protocol)
                {
                    case Enumerations.Protocol.WcfSoapWithWsTrust:
                        if ((_user25 == null) || (_infoShareWcfSoapWithWsTrustConnection.IsTokenAlmostExpired))
                            _user25 = _infoShareWcfSoapWithWsTrustConnection.GetUser25Channel();
                        break;
                    case Enumerations.Protocol.WcfSoapWithOpenIdConnect:
                    case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                        if ((_user25 == null) || (_infoShareWcfSoapWithOpenIdConnectConnection.IsTokenAlmostExpired))
                            _user25 = _infoShareWcfSoapWithOpenIdConnectConnection.GetUser25Channel();
                        break;
                    default:
                        throw new ArgumentException($"IshSession _protocol[{_protocol}] was unexpected.");
                }
                return _user25;
            }
        }

        public UserRole25ServiceReference.UserRole UserRole25
        {
            get
            {
                VerifyConnectionValidity();
                switch (_protocol)
                {
                    case Enumerations.Protocol.WcfSoapWithWsTrust:
                        if ((_userRole25 == null) || (_infoShareWcfSoapWithWsTrustConnection.IsTokenAlmostExpired))
                            _userRole25 = _infoShareWcfSoapWithWsTrustConnection.GetUserRole25Channel();
                        break;
                    case Enumerations.Protocol.WcfSoapWithOpenIdConnect:
                    case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                        if ((_userRole25 == null) || (_infoShareWcfSoapWithOpenIdConnectConnection.IsTokenAlmostExpired))
                            _userRole25 = _infoShareWcfSoapWithOpenIdConnectConnection.GetUserRole25Channel();
                        break;
                    default:
                        throw new ArgumentException($"IshSession _protocol[{_protocol}] was unexpected.");
                }
                return _userRole25;
            }
        }

        public UserGroup25ServiceReference.UserGroup UserGroup25
        {
            get
            {
                VerifyConnectionValidity();
                switch (_protocol)
                {
                    case Enumerations.Protocol.WcfSoapWithWsTrust:
                        if ((_userGroup25 == null) || (_infoShareWcfSoapWithWsTrustConnection.IsTokenAlmostExpired))
                            _userGroup25 = _infoShareWcfSoapWithWsTrustConnection.GetUserGroup25Channel();
                        break;
                    case Enumerations.Protocol.WcfSoapWithOpenIdConnect:
                    case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                        if ((_userGroup25 == null) || (_infoShareWcfSoapWithOpenIdConnectConnection.IsTokenAlmostExpired))
                            _userGroup25 = _infoShareWcfSoapWithOpenIdConnectConnection.GetUserGroup25Channel();
                        break;
                    default:
                        throw new ArgumentException($"IshSession _protocol[{_protocol}] was unexpected.");
                }
                return _userGroup25;
            }
        }

        public DocumentObj25ServiceReference.DocumentObj DocumentObj25
        {
            get
            {
                VerifyConnectionValidity();
                switch (_protocol)
                {
                    case Enumerations.Protocol.WcfSoapWithWsTrust:
                        if ((_documentObj25 == null) || (_infoShareWcfSoapWithWsTrustConnection.IsTokenAlmostExpired))
                            _documentObj25 = _infoShareWcfSoapWithWsTrustConnection.GetDocumentObj25Channel();
                        break;
                    case Enumerations.Protocol.WcfSoapWithOpenIdConnect:
                    case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                        if ((_documentObj25 == null) || (_infoShareWcfSoapWithOpenIdConnectConnection.IsTokenAlmostExpired))
                            _documentObj25 = _infoShareWcfSoapWithOpenIdConnectConnection.GetDocumentObj25Channel();
                        break;
                    default:
                        throw new ArgumentException($"IshSession _protocol[{_protocol}] was unexpected.");
                }
                return _documentObj25;
            }
        }

        public PublicationOutput25ServiceReference.PublicationOutput PublicationOutput25
        {
            get
            {
                VerifyConnectionValidity();
                switch (_protocol)
                {
                    case Enumerations.Protocol.WcfSoapWithWsTrust:
                        if ((_publicationOutput25 == null) || (_infoShareWcfSoapWithWsTrustConnection.IsTokenAlmostExpired))
                            _publicationOutput25 = _infoShareWcfSoapWithWsTrustConnection.GetPublicationOutput25Channel();
                        break;
                    case Enumerations.Protocol.WcfSoapWithOpenIdConnect:
                    case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                        if ((_publicationOutput25 == null) || (_infoShareWcfSoapWithOpenIdConnectConnection.IsTokenAlmostExpired))
                            _publicationOutput25 = _infoShareWcfSoapWithOpenIdConnectConnection.GetPublicationOutput25Channel();
                        break;
                    default:
                        throw new ArgumentException($"IshSession _protocol[{_protocol}] was unexpected.");
                }
                return _publicationOutput25;
            }
        }

        public Settings25ServiceReference.Settings Settings25
        {
            get
            {
                VerifyConnectionValidity();
                switch (_protocol)
                {
                    case Enumerations.Protocol.WcfSoapWithWsTrust:
                        if ((_settings25 == null) || (_infoShareWcfSoapWithWsTrustConnection.IsTokenAlmostExpired))
                            _settings25 = _infoShareWcfSoapWithWsTrustConnection.GetSettings25Channel();
                        break;
                    case Enumerations.Protocol.WcfSoapWithOpenIdConnect:
                    case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                        if ((_settings25 == null) || (_infoShareWcfSoapWithOpenIdConnectConnection.IsTokenAlmostExpired))
                            _settings25 = _infoShareWcfSoapWithOpenIdConnectConnection.GetSettings25Channel();
                        break;
                    default:
                        throw new ArgumentException($"IshSession _protocol[{_protocol}] was unexpected.");
                }
                return _settings25;
            }
        }

        public EventMonitor25ServiceReference.EventMonitor EventMonitor25
        {
            get
            {
                VerifyConnectionValidity();
                switch (_protocol)
                {
                    case Enumerations.Protocol.WcfSoapWithWsTrust:
                        if ((_eventMonitor25 == null) || (_infoShareWcfSoapWithWsTrustConnection.IsTokenAlmostExpired))
                            _eventMonitor25 = _infoShareWcfSoapWithWsTrustConnection.GetEventMonitor25Channel();
                        break;
                    case Enumerations.Protocol.WcfSoapWithOpenIdConnect:
                    case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                        if ((_eventMonitor25 == null) || (_infoShareWcfSoapWithOpenIdConnectConnection.IsTokenAlmostExpired))
                            _eventMonitor25 = _infoShareWcfSoapWithOpenIdConnectConnection.GetEventMonitor25Channel();
                        break;
                    default:
                        throw new ArgumentException($"IshSession _protocol[{_protocol}] was unexpected.");
                }
                return _eventMonitor25;
            }
        }

        public Baseline25ServiceReference.Baseline Baseline25
        {
            get
            {
                VerifyConnectionValidity();
                switch (_protocol)
                {
                    case Enumerations.Protocol.WcfSoapWithWsTrust:
                        if ((_baseline25 == null) || (_infoShareWcfSoapWithWsTrustConnection.IsTokenAlmostExpired))
                            _baseline25 = _infoShareWcfSoapWithWsTrustConnection.GetBaseline25Channel();
                        break;
                    case Enumerations.Protocol.WcfSoapWithOpenIdConnect:
                    case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                        if ((_baseline25 == null) || (_infoShareWcfSoapWithOpenIdConnectConnection.IsTokenAlmostExpired))
                            _baseline25 = _infoShareWcfSoapWithOpenIdConnectConnection.GetBaseline25Channel();
                        break;
                    default:
                        throw new ArgumentException($"IshSession _protocol[{_protocol}] was unexpected.");
                }
                return _baseline25;
            }
        }

        public MetadataBinding25ServiceReference.MetadataBinding MetadataBinding25
        {
            get
            {
                VerifyConnectionValidity();
                switch (_protocol)
                {
                    case Enumerations.Protocol.WcfSoapWithWsTrust:
                        if ((_metadataBinding25 == null) || (_infoShareWcfSoapWithWsTrustConnection.IsTokenAlmostExpired))
                            _metadataBinding25 = _infoShareWcfSoapWithWsTrustConnection.GetMetadataBinding25Channel();
                        break;
                    case Enumerations.Protocol.WcfSoapWithOpenIdConnect:
                    case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                        if ((_metadataBinding25 == null) || (_infoShareWcfSoapWithOpenIdConnectConnection.IsTokenAlmostExpired))
                            _metadataBinding25 = _infoShareWcfSoapWithOpenIdConnectConnection.GetMetadataBinding25Channel();
                        break;
                    default:
                        throw new ArgumentException($"IshSession _protocol[{_protocol}] was unexpected.");
                }
                return _metadataBinding25;
            }
        }

        public Folder25ServiceReference.Folder Folder25
        {
            get
            {
                VerifyConnectionValidity();
                switch (_protocol)
                {
                    case Enumerations.Protocol.WcfSoapWithWsTrust:
                        if ((_folder25 == null) || (_infoShareWcfSoapWithWsTrustConnection.IsTokenAlmostExpired))
                            _folder25 = _infoShareWcfSoapWithWsTrustConnection.GetFolder25Channel();
                        break;
                    case Enumerations.Protocol.WcfSoapWithOpenIdConnect:
                    case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                        if ((_folder25 == null) || (_infoShareWcfSoapWithOpenIdConnectConnection.IsTokenAlmostExpired))
                            _folder25 = _infoShareWcfSoapWithOpenIdConnectConnection.GetFolder25Channel();
                        break;
                    default:
                        throw new ArgumentException($"IshSession _protocol[{_protocol}] was unexpected.");
                }
                return _folder25;
            }
        }

        public ListOfValues25ServiceReference.ListOfValues ListOfValues25
        {
            get
            {
                VerifyConnectionValidity();
                switch (_protocol)
                {
                    case Enumerations.Protocol.WcfSoapWithWsTrust:
                        if ((_listOfValues25 == null) || (_infoShareWcfSoapWithWsTrustConnection.IsTokenAlmostExpired))
                            _listOfValues25 = _infoShareWcfSoapWithWsTrustConnection.GetListOfValues25Channel();
                        break;
                    case Enumerations.Protocol.WcfSoapWithOpenIdConnect:
                    case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                        if ((_listOfValues25 == null) || (_infoShareWcfSoapWithOpenIdConnectConnection.IsTokenAlmostExpired))
                            _listOfValues25 = _infoShareWcfSoapWithOpenIdConnectConnection.GetListOfValues25Channel();
                        break;
                    default:
                        throw new ArgumentException($"IshSession _protocol[{_protocol}] was unexpected.");
                }
                return _listOfValues25;
            }
        }

        public OutputFormat25ServiceReference.OutputFormat OutputFormat25
        {
            get
            {
                VerifyConnectionValidity();
                switch (_protocol)
                {
                    case Enumerations.Protocol.WcfSoapWithWsTrust:
                        if ((_outputFormat25 == null) || (_infoShareWcfSoapWithWsTrustConnection.IsTokenAlmostExpired))
                            _outputFormat25 = _infoShareWcfSoapWithWsTrustConnection.GetOutputFormat25Channel();
                        break;
                    case Enumerations.Protocol.WcfSoapWithOpenIdConnect:
                    case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                        if ((_outputFormat25 == null) || (_infoShareWcfSoapWithOpenIdConnectConnection.IsTokenAlmostExpired))
                            _outputFormat25 = _infoShareWcfSoapWithOpenIdConnectConnection.GetOutputFormat25Channel();
                        break;
                    default:
                        throw new ArgumentException($"IshSession _protocol[{_protocol}] was unexpected.");
                }
                return _outputFormat25;
            }
        }

        public EDT25ServiceReference.EDT EDT25
        {
            get
            {
                VerifyConnectionValidity();
                switch (_protocol)
                {
                    case Enumerations.Protocol.WcfSoapWithWsTrust:
                        if ((_EDT25 == null) || (_infoShareWcfSoapWithWsTrustConnection.IsTokenAlmostExpired))
                            _EDT25 = _infoShareWcfSoapWithWsTrustConnection.GetEDT25Channel();
                        break;
                    case Enumerations.Protocol.WcfSoapWithOpenIdConnect:
                    case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                        if ((_EDT25 == null) || (_infoShareWcfSoapWithOpenIdConnectConnection.IsTokenAlmostExpired))
                            _EDT25 = _infoShareWcfSoapWithOpenIdConnectConnection.GetEDT25Channel();
                        break;
                    default:
                        throw new ArgumentException($"IshSession _protocol[{_protocol}] was unexpected.");
                }
                return _EDT25;
            }
        }

        public TranslationJob25ServiceReference.TranslationJob TranslationJob25
        {
            get
            {
                VerifyConnectionValidity();
                switch (_protocol)
                {
                    case Enumerations.Protocol.WcfSoapWithWsTrust:
                        if ((_translationJob25 == null) || (_infoShareWcfSoapWithWsTrustConnection.IsTokenAlmostExpired))
                            _translationJob25 = _infoShareWcfSoapWithWsTrustConnection.GetTranslationJob25Channel();
                        break;
                    case Enumerations.Protocol.WcfSoapWithOpenIdConnect:
                    case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                        if ((_translationJob25 == null) || (_infoShareWcfSoapWithOpenIdConnectConnection.IsTokenAlmostExpired))
                            _translationJob25 = _infoShareWcfSoapWithOpenIdConnectConnection.GetTranslationJob25Channel();
                        break;
                    default:
                        throw new ArgumentException($"IshSession _protocol[{_protocol}] was unexpected.");
                }
                return _translationJob25;
            }
        }

        public TranslationTemplate25ServiceReference.TranslationTemplate TranslationTemplate25
        {
            get
            {
                VerifyConnectionValidity();
                switch (_protocol)
                {
                    case Enumerations.Protocol.WcfSoapWithWsTrust:
                        if ((_translationTemplate25 == null) || (_infoShareWcfSoapWithWsTrustConnection.IsTokenAlmostExpired))
                            _translationTemplate25 = _infoShareWcfSoapWithWsTrustConnection.GetTranslationTemplate25Channel();
                        break;
                    case Enumerations.Protocol.WcfSoapWithOpenIdConnect:
                    case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                        if ((_translationTemplate25 == null) || (_infoShareWcfSoapWithOpenIdConnectConnection.IsTokenAlmostExpired))
                            _translationTemplate25 = _infoShareWcfSoapWithOpenIdConnectConnection.GetTranslationTemplate25Channel();
                        break;
                    default:
                        throw new ArgumentException($"IshSession _protocol[{_protocol}] was unexpected.");
                }
                return _translationTemplate25;
            }
        }

        public Search25ServiceReference.Search Search25
        {
            get
            {
                VerifyConnectionValidity();
                switch (_protocol)
                {
                    case Enumerations.Protocol.WcfSoapWithWsTrust:
                        if ((_search25 == null) || (_infoShareWcfSoapWithWsTrustConnection.IsTokenAlmostExpired))
                            _search25 = _infoShareWcfSoapWithWsTrustConnection.GetSearch25Channel();
                        break;
                    case Enumerations.Protocol.WcfSoapWithOpenIdConnect:
                    case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                        if ((_search25 == null) || (_infoShareWcfSoapWithOpenIdConnectConnection.IsTokenAlmostExpired))
                            _search25 = _infoShareWcfSoapWithOpenIdConnectConnection.GetSearch25Channel();
                        break;
                    default:
                        throw new ArgumentException($"IshSession _protocol[{_protocol}] was unexpected.");
                }
                return _search25;
            }
        }

        public BackgroundTask25ServiceReference.BackgroundTask BackgroundTask25
        {
            get
            {
                VerifyConnectionValidity();
                switch (_protocol)
                {
                    case Enumerations.Protocol.WcfSoapWithWsTrust:
                        if ((_backgroundTask25 == null) || (_infoShareWcfSoapWithWsTrustConnection.IsTokenAlmostExpired))
                            _backgroundTask25 = _infoShareWcfSoapWithWsTrustConnection.GetBackgroundTask25Channel();
                        break;
                    case Enumerations.Protocol.WcfSoapWithOpenIdConnect:
                    case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                        if ((_backgroundTask25 == null) || (_infoShareWcfSoapWithOpenIdConnectConnection.IsTokenAlmostExpired))
                            _backgroundTask25 = _infoShareWcfSoapWithOpenIdConnectConnection.GetBackgroundTask25Channel();
                        break;
                    default:
                        throw new ArgumentException($"IshSession _protocol[{_protocol}] was unexpected.");
                }
                return _backgroundTask25;
            }
        }

#endregion

        private void VerifyConnectionValidity()
        {
            switch (_protocol)
            {
                case Enumerations.Protocol.WcfSoapWithWsTrust:
                    if (!_infoShareWcfSoapWithWsTrustConnection.IsTokenAlmostExpired)
                    {
                        // Not valid...
                        // ...dispose connection
                        _infoShareWcfSoapWithWsTrustConnection.Dispose();
                        // ...discard all channels
                        _annotation25 = null;
                        _application25 = null;
                        _backgroundTask25 = null;
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
                        CreateInfoShareWcfSoapWithWsTrustConnection();
                    }
                    break;
                case Enumerations.Protocol.WcfSoapWithOpenIdConnect:
                    if (_infoShareOpenApiWithOpenIdConnectConnection == null)
                    {
                        // ... discard OpenApiISH30Service
                        // ...and re-create connection
                        CreateOpenApiWithOpenIdConnectConnection();
                    }
                    if (_infoShareWcfSoapWithOpenIdConnectConnection == null)
                    {
                        // Not valid...
                        // ...dispose connection
                        //_infoShareWcfSoapWithOpenIdConnectConnection.Dispose();
                        // ...discard all channels
                        _annotation25 = null;
                        _application25 = null;
                        _backgroundTask25 = null;
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
                        CreateInfoShareWcfSoapWithOpenIdConnectConnection();
                    }
                    break;
                case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                    if (_infoShareOpenApiWithOpenIdConnectConnection == null)
                    {
                        // ... discard OpenApiISH30Service
                        // ...and re-create connection
                        CreateOpenApiWithOpenIdConnectConnection();
                    }
                    if (_infoShareWcfSoapWithOpenIdConnectConnection == null)
                    {
                        // Not valid...
                        // ...dispose connection
                        //_infoShareWcfSoapWithOpenIdConnectConnection.Dispose();
                        // ...discard all channels
                        _annotation25 = null;
                        _application25 = null;
                        _backgroundTask25 = null;
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
                        CreateInfoShareWcfSoapWithOpenIdConnectConnection();
                    }
                    break;
                default:
                    throw new ArgumentException($"IshSession _protocol[{_protocol}] was unexpected.");
            }
        }

        public void Dispose()
        {
            if (_infoShareWcfSoapWithWsTrustConnection != null)
            {
                _infoShareWcfSoapWithWsTrustConnection.Dispose();
            }
            if (_infoShareWcfSoapWithOpenIdConnectConnection != null) 
            {
                _infoShareWcfSoapWithOpenIdConnectConnection.Dispose( );
            }
            if (_infoShareOpenApiWithOpenIdConnectConnection != null) 
            {
                _infoShareOpenApiWithOpenIdConnectConnection.Dispose( );
            }
        }
        public void Close()
        {
            Dispose();
        }
    }
}
