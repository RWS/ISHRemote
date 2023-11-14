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
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Xml.Linq;
using System.Xml.XPath;
using Trisoft.ISHRemote.Interfaces;
using System.ServiceModel.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.IdentityModel.Selectors;

#if NET48
using System.IdentityModel.Protocols.WSTrust;
using System.ServiceModel.Channels;
using System.ServiceModel.Security.Tokens;
#else
using System.ServiceModel.Federation;
#endif

namespace Trisoft.ISHRemote.Connection
{
    /// <summary>
    /// Dynamic proxy (so without app.config) generation of Service References towards the InfoShare Web Services writen in Windows Communication Foundation (WCF) protected by WS-Trust (aka WS-Federation active) SOAP protocol.
    /// On ISHRemote v1 and earlier, so in turn before InfoShare 15 and earlier, this class was your starting point for dynamic proxy (so without app.config) generation of Service References. The inital class was written in .NET Framework style. Inspired by https://devblogs.microsoft.com/dotnet/wsfederationhttpbinding-in-net-standard-wcf/ this class has pragmas to illustrate .NET Framework and .NET 6.0+ style side-by-side.
    /// </summary>
    internal sealed class InfoShareWcfSoapWithWsTrustConnection : IDisposable
    {
        #region Constants
        /// <summary>
        /// Annotation25
        /// </summary>
        private const string Annotation25 = "Annotation25";
        /// <summary>
        /// Application25
        /// </summary>
        private const string Application25 = "Application25";
        /// <summary>
        /// DocumentObj25
        /// </summary>
        private const string DocumentObj25 = "DocumentObj25";
        /// <summary>
        /// Folder25
        /// </summary>
        private const string Folder25 = "Folder25";
        /// <summary>
        /// User25
        /// </summary>
        private const string User25 = "User25";
        /// <summary>
        /// UserRole25
        /// </summary>
        private const string UserRole25 = "UserRole25";
        /// <summary>
        /// UserGroup25
        /// </summary>
        private const string UserGroup25 = "UserGroup25";
        /// <summary>
        /// ListOfValues25
        /// </summary>
        private const string ListOfValues25 = "ListOfValues25";
        /// <summary>
        /// PublicationOutput25
        /// </summary>
        private const string PublicationOutput25 = "PublicationOutput25";
        /// <summary>
        /// OutputFormat25
        /// </summary>
        private const string OutputFormat25 = "OutputFormat25";
        /// <summary>
        /// Settings25
        /// </summary>
        private const string Settings25 = "Settings25";
        /// <summary>
        /// EDT25
        /// </summary>
        private const string EDT25 = "EDT25";
        /// <summary>
        /// EventMonitor25
        /// </summary>
        private const string EventMonitor25 = "EventMonitor25";
        /// <summary>
        /// Baseline25
        /// </summary>
        private const string Baseline25 = "Baseline25";
        /// <summary>
        /// MetadataBinding25
        /// </summary>
        private const string MetadataBinding25 = "MetadataBinding25";
        /// <summary>
        /// Search25
        /// </summary>
        private const string Search25 = "Search25";
        /// <summary>
        /// TranslationJob25
        /// </summary>
        private const string TranslationJob25 = "TranslationJob25";
        /// <summary>
        /// TranslationTemplate25
        /// </summary>
        private const string TranslationTemplate25 = "TranslationTemplate25";
        /// <summary>
        /// BackgroundTask25
        /// </summary>
        private const string BackgroundTask25 = "BackgroundTask25";
        #endregion

        #region Private Members
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger _logger;
        /// <summary>
        /// HttpClient. Incoming reused, probably Ssl/Tls initialized already.
        /// </summary>
        private HttpClient _httpClient;
        /// <summary>
        /// Parameters that configure the connection behavior.
        /// </summary>
        private readonly InfoShareWcfSoapWithWsTrustConnectionParameters _connectionParameters;
        /// <summary>
        /// The binding type that is required by the end point of the WS-Trust issuer.
        /// </summary>
        private readonly string _issuerAuthenticationType;
        /// <summary>
        /// The WS-Trust endpoint for the Security Token Service that provides the functionality to issue tokens as specified by the issuerwstrustbindingtype.
        /// </summary>
        private readonly Uri _issuerWSTrustEndpointUri;
        /// <summary>
        /// WS STS Realm to issue tokens for
        /// </summary>
        private readonly Uri _infoShareWSAppliesTo;
        /// <summary>
        /// Service URIs by service.
        /// </summary>
        private readonly Dictionary<string, Uri> _serviceUriByServiceName = new Dictionary<string, Uri>();
        /// <summary>
		/// The token that is used to access the services.
		/// </summary>
		private readonly Lazy<GenericXmlSecurityToken> _issuedToken;
#if NET48
        /// <summary>
        /// Binding that is common for every endpoint.
        /// </summary>
        private Binding _commonBinding;
#else
        /// <summary>
        /// Binding that is common for every endpoint.
        /// </summary>
        private WSFederationHttpBinding _commonBinding;
#endif

        /// <summary>
        /// Proxy for annotation
        /// </summary>
        private Annotation25ServiceReference.AnnotationClient _annotationClient;
        /// <summary>
        /// Proxy for application
        /// </summary>
        private Application25ServiceReference.ApplicationClient _applicationClient;
        /// <summary>
        /// Proxy for document obj
        /// </summary>
        private DocumentObj25ServiceReference.DocumentObjClient _documentObjClient;
        /// <summary>
        /// Proxy for folder
        /// </summary>
        private Folder25ServiceReference.FolderClient _folderClient;
        /// <summary>
        /// Proxy for user
        /// </summary>
        private User25ServiceReference.UserClient _userClient;
        /// <summary>
        /// Proxy for user role
        /// </summary>
        private UserRole25ServiceReference.UserRoleClient _userRoleClient;
        /// <summary>
        /// Proxy for user group
        /// </summary>
        private UserGroup25ServiceReference.UserGroupClient _userGroupClient;
        /// <summary>
        /// Proxy for LOV
        /// </summary>
        private ListOfValues25ServiceReference.ListOfValuesClient _listOfValuesClient;
        /// <summary>
        /// Proxy for publication output
        /// </summary>
        private PublicationOutput25ServiceReference.PublicationOutputClient _publicationOutputClient;
        /// <summary>
        /// Proxy for output format
        /// </summary>
        private OutputFormat25ServiceReference.OutputFormatClient _outputFormatClient;
        /// <summary>
        /// Proxy for settings
        /// </summary>
        private Settings25ServiceReference.SettingsClient _settingsClient;
        /// <summary>
        /// Proxy for EDT
        /// </summary>
        private EDT25ServiceReference.EDTClient _EDTClient;
        /// <summary>
        /// Proxy for event monitor
        /// </summary>
        private EventMonitor25ServiceReference.EventMonitorClient _eventMonitorClient;
        /// <summary>
        /// Proxy for baseline
        /// </summary>
        private Baseline25ServiceReference.BaselineClient _baselineClient;
        /// <summary>
        /// Proxy for metadata binding
        /// </summary>
        private MetadataBinding25ServiceReference.MetadataBindingClient _metadataBindingClient;
        /// <summary>
        /// Proxy for search
        /// </summary>
        private Search25ServiceReference.SearchClient _searchClient;
        /// <summary>
        /// Proxy for translation job
        /// </summary>
        private TranslationJob25ServiceReference.TranslationJobClient _translationJobClient;
        /// <summary>
        /// Proxy for translation template
        /// </summary>
        private TranslationTemplate25ServiceReference.TranslationTemplateClient _translationTemplateClient;
        /// <summary>
        /// Proxy for background task
        /// </summary>
        private BackgroundTask25ServiceReference.BackgroundTaskClient _backgroundTaskClient;
        #endregion Private Members

        #region Constructors
        /// <summary>
        /// Initializes a new instance of <c>InfoShareWcfSoapWithWsTrustConnection</c> class.
        /// </summary>
        /// <param name="logger">Instance of Interfaces.ILogger implementation</param>
        /// <param name="httpClient">Incoming reused, probably Ssl/Tls initialized already.</param>
        /// <param name="infoShareWcfSoapWithWsTrustConnectionParameters">Connection parameters.</param>
        public InfoShareWcfSoapWithWsTrustConnection(ILogger logger, HttpClient httpClient, InfoShareWcfSoapWithWsTrustConnectionParameters infoShareWcfSoapWithWsTrustConnectionParameters)
        {
            _logger = logger;
            _httpClient = httpClient;
            _connectionParameters = infoShareWcfSoapWithWsTrustConnectionParameters;
            // Could to more strict _connectionParameters checks

            _logger.WriteDebug($"InfoShareWcfSoapWithWsTrustConnection InfoShareWSUrl[{_connectionParameters.InfoShareWSUrl}] IssuerUrl[{_connectionParameters.IssuerUrl}] AuthenticationType[{_connectionParameters.AuthenticationType}]");

              // using the ISHWS url from connectionconfiguration.xml instead of the potentially wrongly cased incoming one [TS-10630]
            _logger.WriteDebug($"InfoShareWcfSoapWithWsTrustConnection using Normalized infoShareWSBaseUri[{_connectionParameters.InfoShareWSUrl}]");
            this.InfoShareWSBaseUri = _connectionParameters.InfoShareWSUrl;
            _issuerAuthenticationType = _connectionParameters.AuthenticationType;
            _infoShareWSAppliesTo = _connectionParameters.InfoShareWSUrl;
            _issuerWSTrustEndpointUri = _connectionParameters.IssuerUrl;
#if NET6_0_OR_GREATER
            if (_issuerWSTrustEndpointUri.ToString().EndsWith("windowsmixed"))
            {
                throw new PlatformNotSupportedException($"PowerShell7+/NET6+ only supports /wstrust/mixed/username. Windows PowerShell 5.1/NET4.8 supports /wstrust/mixed/username and windowsmixed (aka Windows Authentication). IssuerUrl[{_issuerWSTrustEndpointUri}] PlatformVersion[{Environment.Version}]");
            }
#endif
            _logger.WriteDebug($"InfoShareWcfSoapWithWsTrustConnection Resolving Service Uris");
            ResolveServiceUris();

            // The lazy initialization depends on all the initialization above.
            _issuedToken = new Lazy<GenericXmlSecurityToken>(IssueToken);

#if NET48
            // Set the endpoints
            ResolveEndpoints(_connectionParameters.AutoAuthenticate);
#else
            WS2007HttpBinding issuerBinding = new WS2007HttpBinding(SecurityMode.TransportWithMessageCredential);
            issuerBinding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
            issuerBinding.Security.Message.EstablishSecurityContext = false;
            issuerBinding.SendTimeout = _connectionParameters.IssueTimeout;
            issuerBinding.ReceiveTimeout = _connectionParameters.IssueTimeout;

            EndpointAddress issuerAddress = new EndpointAddress(IssuerWSTrustEndpointUri);

            WSTrustTokenParameters tokenParameters = WSTrustTokenParameters.CreateWS2007FederationTokenParameters(issuerBinding, issuerAddress); // WS-Trust 1.3 is 2007
            tokenParameters.KeyType = SecurityKeyType.SymmetricKey;
            // CacheIssuedTokens, MaxIssuedCachingTime, and IssuedTokenRenewalThresholdPercentage These properties indicate whether tokens should be
            // cached and for how long.In many cases, these properties don�t need to be set as the defaults(tokens are cached for 60 % of their lifetime) are sufficient.
            tokenParameters.CacheIssuedTokens = true;
            tokenParameters.MessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10;

            // AppliesTo is currently implicitly set to full case-sensitive .../ISHWS/....svc urls, and not .../ISHWS/ using ISHSTS historic fallback mechanism
            // https://github.com/dotnet/wcf/issues/4542 or https://dotnetfiddle.net/h3C2ZC samples go over CreateSecurityTokenProvider and allows setting AppliesTo but forces a completely different code base

            _commonBinding = new System.ServiceModel.Federation.WSFederationHttpBinding(tokenParameters);
            _commonBinding.SendTimeout = _connectionParameters.IssueTimeout;
            _commonBinding.ReceiveTimeout = _connectionParameters.IssueTimeout;
            _commonBinding.MaxReceivedMessageSize = Int32.MaxValue;
            _commonBinding.MaxBufferPoolSize = Int32.MaxValue;
            _commonBinding.ReaderQuotas.MaxStringContentLength = Int32.MaxValue;
            _commonBinding.ReaderQuotas.MaxNameTableCharCount = Int32.MaxValue;
            _commonBinding.ReaderQuotas.MaxArrayLength = Int32.MaxValue;
            _commonBinding.ReaderQuotas.MaxBytesPerRead = Int32.MaxValue;
            _commonBinding.ReaderQuotas.MaxDepth = 64;
#endif
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Root uri for the Web Services
        /// </summary>
        public Uri InfoShareWSBaseUri { get; private set; }
        /// <summary>
        /// Checks whether the token is issued and still valid
        /// </summary>
        public bool IsTokenAlmostExpired
        {
            get
            {
                // we have the actual issued token which we can check for expiring
                bool result = IssuedToken.ValidTo.ToUniversalTime() >= DateTime.UtcNow;
                //_logger.WriteDebug($"Token still valid? {result} ({IssuedToken.ValidTo.ToUniversalTime()} >= {DateTime.UtcNow})");
                return result;
            }
        }
        #endregion Properties

        #region Public Get..Channel Methods
        /// <summary>
        /// Create a /Wcf/API25/Annotation.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public Annotation25ServiceReference.Annotation GetAnnotation25Channel()
        {
#if NET48
            if ((_annotationClient == null) || (_annotationClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _annotationClient = new Annotation25ServiceReference.AnnotationClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Annotation25]));
            }
            return _annotationClient.ChannelFactory.CreateChannelWithIssuedToken(IssuedToken);
#else
            if ((_annotationClient == null) || (_annotationClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _annotationClient = new Annotation25ServiceReference.AnnotationClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Annotation25]));
            }
            _annotationClient.ClientCredentials.UserName.UserName = _connectionParameters.Credential.UserName;
            _annotationClient.ClientCredentials.UserName.Password = _connectionParameters.Credential.Password;
            _annotationClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
            if (_connectionParameters.IgnoreSslPolicyErrors)
            {
                _annotationClient.ChannelFactory.Credentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication()
                {
                    CertificateValidationMode = X509CertificateValidationMode.None,
                    RevocationMode = X509RevocationMode.NoCheck
                };
            }
            return _annotationClient.ChannelFactory.CreateChannel();
#endif
        }
        /// <summary>
        /// Create a /Wcf/API25/Application.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public Application25ServiceReference.Application GetApplication25Channel()
        {
#if NET48
            if ((_applicationClient == null) || (_applicationClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _applicationClient = new Application25ServiceReference.ApplicationClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Application25]));
            }
            return _applicationClient.ChannelFactory.CreateChannelWithIssuedToken(IssuedToken);
#else
            if ((_applicationClient == null) || (_applicationClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _applicationClient = new Application25ServiceReference.ApplicationClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Application25]));
                _applicationClient.ClientCredentials.UserName.UserName = _connectionParameters.Credential.UserName;
                _applicationClient.ClientCredentials.UserName.Password = _connectionParameters.Credential.Password;
                _applicationClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
                if (_connectionParameters.IgnoreSslPolicyErrors)
                {
                    _applicationClient.ChannelFactory.Credentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication()
                    {
                        CertificateValidationMode = X509CertificateValidationMode.None,
                        RevocationMode = X509RevocationMode.NoCheck
                    };
                }
            }
            return _applicationClient.ChannelFactory.CreateChannel();
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/DocumentObj.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public DocumentObj25ServiceReference.DocumentObj GetDocumentObj25Channel()
        {
#if NET48
            if ((_documentObjClient == null) || (_documentObjClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _documentObjClient = new DocumentObj25ServiceReference.DocumentObjClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[DocumentObj25]));
            }
            return _documentObjClient.ChannelFactory.CreateChannelWithIssuedToken(IssuedToken);
#else
            if ((_documentObjClient == null) || (_documentObjClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _documentObjClient = new DocumentObj25ServiceReference.DocumentObjClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[DocumentObj25]));
                _documentObjClient.ClientCredentials.UserName.UserName = _connectionParameters.Credential.UserName;
                _documentObjClient.ClientCredentials.UserName.Password = _connectionParameters.Credential.Password;
                _documentObjClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
                if (_connectionParameters.IgnoreSslPolicyErrors)
                {
                    _documentObjClient.ChannelFactory.Credentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication()
                    {
                        CertificateValidationMode = X509CertificateValidationMode.None,
                        RevocationMode = X509RevocationMode.NoCheck
                    };
                }
            }
            return _documentObjClient.ChannelFactory.CreateChannel();
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/Folder.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public Folder25ServiceReference.Folder GetFolder25Channel()
        {
#if NET48
            if ((_folderClient == null) || (_folderClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _folderClient = new Folder25ServiceReference.FolderClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Folder25]));
            }
            return _folderClient.ChannelFactory.CreateChannelWithIssuedToken(IssuedToken);
#else
            if ((_folderClient == null) || (_folderClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _folderClient = new Folder25ServiceReference.FolderClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Folder25]));
                _folderClient.ClientCredentials.UserName.UserName = _connectionParameters.Credential.UserName;
                _folderClient.ClientCredentials.UserName.Password = _connectionParameters.Credential.Password;
                _folderClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
                if (_connectionParameters.IgnoreSslPolicyErrors)
                {
                    _folderClient.ChannelFactory.Credentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication()
                    {
                        CertificateValidationMode = X509CertificateValidationMode.None,
                        RevocationMode = X509RevocationMode.NoCheck
                    };
                }
            }
            return _folderClient.ChannelFactory.CreateChannel();
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/User.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public User25ServiceReference.User GetUser25Channel()
        {
#if NET48
            if ((_userClient == null) || (_userClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _userClient = new User25ServiceReference.UserClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[User25]));
            }
            return _userClient.ChannelFactory.CreateChannelWithIssuedToken(IssuedToken);
#else
            if ((_userClient == null) || (_userClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _userClient = new User25ServiceReference.UserClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[User25]));
                _userClient.ClientCredentials.UserName.UserName = _connectionParameters.Credential.UserName;
                _userClient.ClientCredentials.UserName.Password = _connectionParameters.Credential.Password;
                _userClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
                if (_connectionParameters.IgnoreSslPolicyErrors)
                {
                    _userClient.ChannelFactory.Credentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication()
                    {
                        CertificateValidationMode = X509CertificateValidationMode.None,
                        RevocationMode = X509RevocationMode.NoCheck
                    };
                }
            }
            return _userClient.ChannelFactory.CreateChannel();
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/UserRole.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public UserRole25ServiceReference.UserRole GetUserRole25Channel()
        {
#if NET48
            if ((_userRoleClient == null) || (_userRoleClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _userRoleClient = new UserRole25ServiceReference.UserRoleClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[UserRole25]));
            }
            return _userRoleClient.ChannelFactory.CreateChannelWithIssuedToken(IssuedToken);
#else
            if ((_userRoleClient == null) || (_userRoleClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _userRoleClient = new UserRole25ServiceReference.UserRoleClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[UserRole25]));
                _userRoleClient.ClientCredentials.UserName.UserName = _connectionParameters.Credential.UserName;
                _userRoleClient.ClientCredentials.UserName.Password = _connectionParameters.Credential.Password;
                _userRoleClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
                if (_connectionParameters.IgnoreSslPolicyErrors)
                {
                    _userRoleClient.ChannelFactory.Credentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication()
                    {
                        CertificateValidationMode = X509CertificateValidationMode.None,
                        RevocationMode = X509RevocationMode.NoCheck
                    };
                }
            }
            return _userRoleClient.ChannelFactory.CreateChannel();
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/UserGroup.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public UserGroup25ServiceReference.UserGroup GetUserGroup25Channel()
        {
#if NET48
            if ((_userGroupClient == null) || (_userGroupClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _userGroupClient = new UserGroup25ServiceReference.UserGroupClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[UserGroup25]));
            }
            return _userGroupClient.ChannelFactory.CreateChannelWithIssuedToken(IssuedToken);
#else
            if ((_userGroupClient == null) || (_userGroupClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _userGroupClient = new UserGroup25ServiceReference.UserGroupClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[UserGroup25]));
                _userGroupClient.ClientCredentials.UserName.UserName = _connectionParameters.Credential.UserName;
                _userGroupClient.ClientCredentials.UserName.Password = _connectionParameters.Credential.Password;
                _userGroupClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
                if (_connectionParameters.IgnoreSslPolicyErrors)
                {
                    _userGroupClient.ChannelFactory.Credentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication()
                    {
                        CertificateValidationMode = X509CertificateValidationMode.None,
                        RevocationMode = X509RevocationMode.NoCheck
                    };
                }
            }
            return _userGroupClient.ChannelFactory.CreateChannel();
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/ListOfValues.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public ListOfValues25ServiceReference.ListOfValues GetListOfValues25Channel()
        {
#if NET48
            if ((_listOfValuesClient == null) || (_listOfValuesClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _listOfValuesClient = new ListOfValues25ServiceReference.ListOfValuesClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[ListOfValues25]));
            }
            return _listOfValuesClient.ChannelFactory.CreateChannelWithIssuedToken(IssuedToken);
#else
            if ((_listOfValuesClient == null) || (_listOfValuesClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _listOfValuesClient = new ListOfValues25ServiceReference.ListOfValuesClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[ListOfValues25]));
                _listOfValuesClient.ClientCredentials.UserName.UserName = _connectionParameters.Credential.UserName;
                _listOfValuesClient.ClientCredentials.UserName.Password = _connectionParameters.Credential.Password;
                _listOfValuesClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
                if (_connectionParameters.IgnoreSslPolicyErrors)
                {
                    _listOfValuesClient.ChannelFactory.Credentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication()
                    {
                        CertificateValidationMode = X509CertificateValidationMode.None,
                        RevocationMode = X509RevocationMode.NoCheck
                    };
                }
            }
            return _listOfValuesClient.ChannelFactory.CreateChannel();
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/PublicationOutput.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public PublicationOutput25ServiceReference.PublicationOutput GetPublicationOutput25Channel()
        {
#if NET48
            if ((_publicationOutputClient == null) || (_publicationOutputClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _publicationOutputClient = new PublicationOutput25ServiceReference.PublicationOutputClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[PublicationOutput25]));
            }
            return _publicationOutputClient.ChannelFactory.CreateChannelWithIssuedToken(IssuedToken);
#else
            if ((_publicationOutputClient == null) || (_publicationOutputClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _publicationOutputClient = new PublicationOutput25ServiceReference.PublicationOutputClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[PublicationOutput25]));
                _publicationOutputClient.ClientCredentials.UserName.UserName = _connectionParameters.Credential.UserName;
                _publicationOutputClient.ClientCredentials.UserName.Password = _connectionParameters.Credential.Password;
                _publicationOutputClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
                if (_connectionParameters.IgnoreSslPolicyErrors)
                {
                    _publicationOutputClient.ChannelFactory.Credentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication()
                    {
                        CertificateValidationMode = X509CertificateValidationMode.None,
                        RevocationMode = X509RevocationMode.NoCheck
                    };
                }
            }
            return _publicationOutputClient.ChannelFactory.CreateChannel();
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/OutputFormat.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public OutputFormat25ServiceReference.OutputFormat GetOutputFormat25Channel()
        {
#if NET48
            if ((_outputFormatClient == null) || (_outputFormatClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _outputFormatClient = new OutputFormat25ServiceReference.OutputFormatClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[OutputFormat25]));
            }
            return _outputFormatClient.ChannelFactory.CreateChannelWithIssuedToken(IssuedToken);
#else
            if ((_outputFormatClient == null) || (_outputFormatClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _outputFormatClient = new OutputFormat25ServiceReference.OutputFormatClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[OutputFormat25]));
                _outputFormatClient.ClientCredentials.UserName.UserName = _connectionParameters.Credential.UserName;
                _outputFormatClient.ClientCredentials.UserName.Password = _connectionParameters.Credential.Password;
                _outputFormatClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
                if (_connectionParameters.IgnoreSslPolicyErrors)
                {
                    _outputFormatClient.ChannelFactory.Credentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication()
                    {
                        CertificateValidationMode = X509CertificateValidationMode.None,
                        RevocationMode = X509RevocationMode.NoCheck
                    };
                }
            }
            return _outputFormatClient.ChannelFactory.CreateChannel();
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/Settings.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public Settings25ServiceReference.Settings GetSettings25Channel()
        {
#if NET48
            if ((_settingsClient == null) || (_settingsClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _settingsClient = new Settings25ServiceReference.SettingsClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Settings25]));
            }
            return _settingsClient.ChannelFactory.CreateChannelWithIssuedToken(IssuedToken);
#else
            if ((_settingsClient == null) || (_settingsClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _settingsClient = new Settings25ServiceReference.SettingsClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Settings25]));
                _settingsClient.ClientCredentials.UserName.UserName = _connectionParameters.Credential.UserName;
                _settingsClient.ClientCredentials.UserName.Password = _connectionParameters.Credential.Password;
                _settingsClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
                if (_connectionParameters.IgnoreSslPolicyErrors)
                {
                    _settingsClient.ChannelFactory.Credentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication()
                    {
                        CertificateValidationMode = X509CertificateValidationMode.None,
                        RevocationMode = X509RevocationMode.NoCheck
                    };
                }
            }
            return _settingsClient.ChannelFactory.CreateChannel();
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/Edt.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public EDT25ServiceReference.EDT GetEDT25Channel()
        {
#if NET48
            if ((_EDTClient == null) || (_EDTClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _EDTClient = new EDT25ServiceReference.EDTClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[EDT25]));
            }
            return _EDTClient.ChannelFactory.CreateChannelWithIssuedToken(IssuedToken);
#else
            if ((_EDTClient == null) || (_EDTClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _EDTClient = new EDT25ServiceReference.EDTClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[EDT25]));
                _EDTClient.ClientCredentials.UserName.UserName = _connectionParameters.Credential.UserName;
                _EDTClient.ClientCredentials.UserName.Password = _connectionParameters.Credential.Password;
                _EDTClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
                if (_connectionParameters.IgnoreSslPolicyErrors)
                {
                    _EDTClient.ChannelFactory.Credentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication()
                    {
                        CertificateValidationMode = X509CertificateValidationMode.None,
                        RevocationMode = X509RevocationMode.NoCheck
                    };
                }
            }
            return _EDTClient.ChannelFactory.CreateChannel();
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/EventMonitor.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public EventMonitor25ServiceReference.EventMonitor GetEventMonitor25Channel()
        {
#if NET48
            if ((_eventMonitorClient == null) || (_eventMonitorClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _eventMonitorClient = new EventMonitor25ServiceReference.EventMonitorClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[EventMonitor25]));
            }
            return _eventMonitorClient.ChannelFactory.CreateChannelWithIssuedToken(IssuedToken);
#else
            if ((_eventMonitorClient == null) || (_eventMonitorClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _eventMonitorClient = new EventMonitor25ServiceReference.EventMonitorClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[EventMonitor25]));
                _eventMonitorClient.ClientCredentials.UserName.UserName = _connectionParameters.Credential.UserName;
                _eventMonitorClient.ClientCredentials.UserName.Password = _connectionParameters.Credential.Password;
                _eventMonitorClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
                if (_connectionParameters.IgnoreSslPolicyErrors)
                {
                    _eventMonitorClient.ChannelFactory.Credentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication()
                    {
                        CertificateValidationMode = X509CertificateValidationMode.None,
                        RevocationMode = X509RevocationMode.NoCheck
                    };
                }
            }
            return _eventMonitorClient.ChannelFactory.CreateChannel();
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/Baseline.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public Baseline25ServiceReference.Baseline GetBaseline25Channel()
        {
#if NET48
            if ((_baselineClient == null) || (_baselineClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _baselineClient = new Baseline25ServiceReference.BaselineClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Baseline25]));
            }
            return _baselineClient.ChannelFactory.CreateChannelWithIssuedToken(IssuedToken);
#else
            if ((_baselineClient == null) || (_baselineClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _baselineClient = new Baseline25ServiceReference.BaselineClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Baseline25]));
                _baselineClient.ClientCredentials.UserName.UserName = _connectionParameters.Credential.UserName;
                _baselineClient.ClientCredentials.UserName.Password = _connectionParameters.Credential.Password;
                _baselineClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
                if (_connectionParameters.IgnoreSslPolicyErrors)
                {
                    _baselineClient.ChannelFactory.Credentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication()
                    {
                        CertificateValidationMode = X509CertificateValidationMode.None,
                        RevocationMode = X509RevocationMode.NoCheck
                    };
                }
            }
            return _baselineClient.ChannelFactory.CreateChannel();
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/MetadataBinding.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public MetadataBinding25ServiceReference.MetadataBinding GetMetadataBinding25Channel()
        {
#if NET48
            if ((_metadataBindingClient == null) || (_metadataBindingClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _metadataBindingClient = new MetadataBinding25ServiceReference.MetadataBindingClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[MetadataBinding25]));
            }
            return _metadataBindingClient.ChannelFactory.CreateChannelWithIssuedToken(IssuedToken);
#else
            if ((_metadataBindingClient == null) || (_metadataBindingClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _metadataBindingClient = new MetadataBinding25ServiceReference.MetadataBindingClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[MetadataBinding25]));
                _metadataBindingClient.ClientCredentials.UserName.UserName = _connectionParameters.Credential.UserName;
                _metadataBindingClient.ClientCredentials.UserName.Password = _connectionParameters.Credential.Password;
                _metadataBindingClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
                if (_connectionParameters.IgnoreSslPolicyErrors)
                {
                    _metadataBindingClient.ChannelFactory.Credentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication()
                    {
                        CertificateValidationMode = X509CertificateValidationMode.None,
                        RevocationMode = X509RevocationMode.NoCheck
                    };
                }
            }
            return _metadataBindingClient.ChannelFactory.CreateChannel();
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/Search.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public Search25ServiceReference.Search GetSearch25Channel()
        {
#if NET48
            if ((_searchClient == null) || (_searchClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _searchClient = new Search25ServiceReference.SearchClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Search25]));
            }
            return _searchClient.ChannelFactory.CreateChannelWithIssuedToken(IssuedToken);
#else
            if ((_searchClient == null) || (_searchClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _searchClient = new Search25ServiceReference.SearchClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Search25]));
                _searchClient.ClientCredentials.UserName.UserName = _connectionParameters.Credential.UserName;
                _searchClient.ClientCredentials.UserName.Password = _connectionParameters.Credential.Password;
                _searchClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
                if (_connectionParameters.IgnoreSslPolicyErrors)
                {
                    _searchClient.ChannelFactory.Credentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication()
                    {
                        CertificateValidationMode = X509CertificateValidationMode.None,
                        RevocationMode = X509RevocationMode.NoCheck
                    };
                }
            }
            return _searchClient.ChannelFactory.CreateChannel();
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/TranslationJob.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public TranslationJob25ServiceReference.TranslationJob GetTranslationJob25Channel()
        {
#if NET48
            if ((_translationJobClient == null) || (_translationJobClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _translationJobClient = new TranslationJob25ServiceReference.TranslationJobClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[TranslationJob25]));
            }
            return _translationJobClient.ChannelFactory.CreateChannelWithIssuedToken(IssuedToken);
#else
            if ((_translationJobClient == null) || (_translationJobClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _translationJobClient = new TranslationJob25ServiceReference.TranslationJobClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[TranslationJob25]));
                _translationJobClient.ClientCredentials.UserName.UserName = _connectionParameters.Credential.UserName;
                _translationJobClient.ClientCredentials.UserName.Password = _connectionParameters.Credential.Password;
                _translationJobClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
                if (_connectionParameters.IgnoreSslPolicyErrors)
                {
                    _translationJobClient.ChannelFactory.Credentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication()
                    {
                        CertificateValidationMode = X509CertificateValidationMode.None,
                        RevocationMode = X509RevocationMode.NoCheck
                    };
                }
            }
            return _translationJobClient.ChannelFactory.CreateChannel();
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/TranslationTemplate.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public TranslationTemplate25ServiceReference.TranslationTemplate GetTranslationTemplate25Channel()
        {
#if NET48
            if ((_translationTemplateClient == null) || (_translationTemplateClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _translationTemplateClient = new TranslationTemplate25ServiceReference.TranslationTemplateClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[TranslationTemplate25]));
            }
            return _translationTemplateClient.ChannelFactory.CreateChannelWithIssuedToken(IssuedToken);
#else
            if ((_translationTemplateClient == null) || (_translationTemplateClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _translationTemplateClient = new TranslationTemplate25ServiceReference.TranslationTemplateClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[TranslationTemplate25]));
                _translationTemplateClient.ClientCredentials.UserName.UserName = _connectionParameters.Credential.UserName;
                _translationTemplateClient.ClientCredentials.UserName.Password = _connectionParameters.Credential.Password;
                _translationTemplateClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
                if (_connectionParameters.IgnoreSslPolicyErrors)
                {
                    _translationTemplateClient.ChannelFactory.Credentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication()
                    {
                        CertificateValidationMode = X509CertificateValidationMode.None,
                        RevocationMode = X509RevocationMode.NoCheck
                    };
                }
            }
            return _translationTemplateClient.ChannelFactory.CreateChannel();
#endif
        }


        /// <summary>
        /// Create a /Wcf/API25/BackgroundTask.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public BackgroundTask25ServiceReference.BackgroundTask GetBackgroundTask25Channel()
        {
#if NET48
            if ((_backgroundTaskClient == null) || (_backgroundTaskClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _backgroundTaskClient = new BackgroundTask25ServiceReference.BackgroundTaskClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[BackgroundTask25]));
            }
            return _backgroundTaskClient.ChannelFactory.CreateChannelWithIssuedToken(IssuedToken);
#else
            if ((_backgroundTaskClient == null) || (_backgroundTaskClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _backgroundTaskClient = new BackgroundTask25ServiceReference.BackgroundTaskClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[BackgroundTask25]));
                _backgroundTaskClient.ClientCredentials.UserName.UserName = _connectionParameters.Credential.UserName;
                _backgroundTaskClient.ClientCredentials.UserName.Password = _connectionParameters.Credential.Password;
                _backgroundTaskClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
                if (_connectionParameters.IgnoreSslPolicyErrors)
                {
                    _backgroundTaskClient.ChannelFactory.Credentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication()
                    {
                        CertificateValidationMode = X509CertificateValidationMode.None,
                        RevocationMode = X509RevocationMode.NoCheck
                    };
                }
            }
            return _backgroundTaskClient.ChannelFactory.CreateChannel();
#endif
        }
        #endregion

        #region Private Properties
        /// <summary>
        /// Gets the binding type that is required by the end point of the WS-Trust issuer.
        /// </summary>
        private string IssuerAuthenticationType
        {
            get
            {
                return _issuerAuthenticationType;
            }
        }

        /// <summary>
        /// Gets the WS-Trust endpoint for the Security Token Service that provides the functionality to issue tokens as specified by the issuerwstrustbindingtype.
        /// </summary>
        private Uri IssuerWSTrustEndpointUri
        {
            get
            {
                return _issuerWSTrustEndpointUri;
            }
        }

        /// <summary>
        /// Gets the token that is used to access the services.
        /// </summary>
        private GenericXmlSecurityToken IssuedToken
        {
            get
            {
                return _issuedToken.Value;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// One location to bind relative urls to service names
        /// </summary>
        private void ResolveServiceUris()
        {
            _serviceUriByServiceName.Add(Annotation25, new Uri(InfoShareWSBaseUri, "Wcf/API25/Annotation.svc"));
            _serviceUriByServiceName.Add(Application25, new Uri(InfoShareWSBaseUri, "Wcf/API25/Application.svc"));
            _serviceUriByServiceName.Add(DocumentObj25, new Uri(InfoShareWSBaseUri, "Wcf/API25/DocumentObj.svc"));
            _serviceUriByServiceName.Add(Folder25, new Uri(InfoShareWSBaseUri, "Wcf/API25/Folder.svc"));
            _serviceUriByServiceName.Add(User25, new Uri(InfoShareWSBaseUri, "Wcf/API25/User.svc"));
            _serviceUriByServiceName.Add(UserRole25, new Uri(InfoShareWSBaseUri, "Wcf/API25/UserRole.svc"));
            _serviceUriByServiceName.Add(UserGroup25, new Uri(InfoShareWSBaseUri, "Wcf/API25/UserGroup.svc"));
            _serviceUriByServiceName.Add(ListOfValues25, new Uri(InfoShareWSBaseUri, "Wcf/API25/ListOfValues.svc"));
            _serviceUriByServiceName.Add(PublicationOutput25, new Uri(InfoShareWSBaseUri, "Wcf/API25/PublicationOutput.svc"));
            _serviceUriByServiceName.Add(OutputFormat25, new Uri(InfoShareWSBaseUri, "Wcf/API25/OutputFormat.svc"));
            _serviceUriByServiceName.Add(Settings25, new Uri(InfoShareWSBaseUri, "Wcf/API25/Settings.svc"));
            _serviceUriByServiceName.Add(EDT25, new Uri(InfoShareWSBaseUri, "Wcf/API25/EDT.svc"));
            _serviceUriByServiceName.Add(EventMonitor25, new Uri(InfoShareWSBaseUri, "Wcf/API25/EventMonitor.svc"));
            _serviceUriByServiceName.Add(Baseline25, new Uri(InfoShareWSBaseUri, "Wcf/API25/Baseline.svc"));
            _serviceUriByServiceName.Add(MetadataBinding25, new Uri(InfoShareWSBaseUri, "Wcf/API25/MetadataBinding.svc"));
            _serviceUriByServiceName.Add(Search25, new Uri(InfoShareWSBaseUri, "Wcf/API25/Search.svc"));
            _serviceUriByServiceName.Add(TranslationJob25, new Uri(InfoShareWSBaseUri, "Wcf/API25/TranslationJob.svc"));
            _serviceUriByServiceName.Add(TranslationTemplate25, new Uri(InfoShareWSBaseUri, "Wcf/API25/TranslationTemplate.svc"));
            _serviceUriByServiceName.Add(BackgroundTask25, new Uri(InfoShareWSBaseUri, "Wcf/API25/BackgroundTask.svc"));
        }

        /// <summary>
        /// Returns the connection configuration (loaded from base [InfoShareWSBaseUri]/connectionconfiguration.xml)
        /// </summary>
        /// <returns>The connection configuration.</returns>
        private XDocument LoadConnectionConfiguration()
        {
            HttpClientHandler handler = new HttpClientHandler();
            if (_connectionParameters.IgnoreSslPolicyErrors)
            {
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }
            handler.SslProtocols = (System.Security.Authentication.SslProtocols)(SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13);
            var httpClient = new HttpClient(handler);
            httpClient.Timeout = _connectionParameters.Timeout;
            var connectionConfigurationUri = new Uri(InfoShareWSBaseUri, "connectionconfiguration.xml");
            _logger.WriteDebug($"LoadConnectionConfiguration uri[{connectionConfigurationUri}] timeout[{httpClient.Timeout}]");
            var responseMessage = httpClient.GetAsync(connectionConfigurationUri).Result;
            string response = responseMessage.Content.ReadAsStringAsync().Result;

            var connectionConfiguration = XDocument.Parse(response);
            return connectionConfiguration;
        }

        /// <summary>
        /// Issues the token
        /// Mostly copied from Service References
        /// </summary>
        private GenericXmlSecurityToken IssueToken()
        {
#if NET48
            _logger.WriteDebug("InfoShareWcfSoapWithWsTrustConnection Issue Token (NET48)");
            var issuerEndpoint = FindIssuerEndpoint();

            var requestSecurityToken = new RequestSecurityToken
            {
                RequestType = RequestTypes.Issue,
                AppliesTo = new EndpointReference(_infoShareWSAppliesTo.AbsoluteUri),
                KeyType = System.IdentityModel.Protocols.WSTrust.KeyTypes.Symmetric
            };
            using (var factory = new WSTrustChannelFactory((WS2007HttpBinding)issuerEndpoint.Binding, issuerEndpoint.Address))
            {
                ApplyCredentials(factory.Credentials);
                ApplyTimeout(factory.Endpoint, _connectionParameters.IssueTimeout);

                factory.TrustVersion = TrustVersion.WSTrust13;
                factory.Credentials.SupportInteractive = false;

                WSTrustChannel channel = null;
                try
                {
                    _logger.WriteDebug($"InfoShareWcfSoapWithWsTrustConnection Issue Token for AppliesTo[{requestSecurityToken.AppliesTo.Uri}]");
                    channel = (WSTrustChannel)factory.CreateChannel();
                    RequestSecurityTokenResponse requestSecurityTokenResponse;
                    var genericXmlToken = channel.Issue(requestSecurityToken, out requestSecurityTokenResponse) as GenericXmlSecurityToken;
                    _logger.WriteDebug($"InfoShareWcfSoapWithWsTrustConnection Issue Token received ValidFrom[{genericXmlToken.ValidFrom}] ValidTo[{genericXmlToken.ValidTo}]");
                    return genericXmlToken;
                }
                catch
                {
                    // Fallback to 10.0.X and 11.0.X configuration using relying party per url like /InfoShareWS/API25/Application.svc
                    requestSecurityToken.AppliesTo = new EndpointReference(_serviceUriByServiceName[Application25].AbsoluteUri);
                    _logger.WriteDebug($"InfoShareWcfSoapWithWsTrustConnection Issue Token for AppliesTo[{requestSecurityToken.AppliesTo.Uri}] as fallback on 10.0.x/11.0.x");
                    RequestSecurityTokenResponse requestSecurityTokenResponse;
                    var genericXmlToken = channel.Issue(requestSecurityToken, out requestSecurityTokenResponse) as GenericXmlSecurityToken;
                    _logger.WriteDebug($"InfoShareWcfSoapWithWsTrustConnection Issue Token received ValidFrom[{genericXmlToken.ValidFrom}] ValidTo[{genericXmlToken.ValidTo}]");
                    return genericXmlToken;
                }
                finally
                {
                    if (channel != null)
                    {
                        channel.Abort();
                    }
                    factory.Abort();
                }
            }
#else
            _logger.WriteDebug("InfoShareWcfSoapWithWsTrustConnection Issue Token (NET6+)");
            SecurityTokenProvider tokenProvider = null;
            try
            {
                _logger.WriteDebug($"InfoShareWcfSoapWithWsTrustConnection Issue Token for AppliesTo[{_infoShareWSAppliesTo.AbsoluteUri}]");
                var issuerWS2007HttpBinding = new WS2007HttpBinding();
                // Originals of UserName Endpoint STS · Issue #4542 · dotnet/wcf https://github.com/dotnet/wcf/issues/4542
                issuerWS2007HttpBinding.Security.Mode = SecurityMode.TransportWithMessageCredential;
                issuerWS2007HttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                issuerWS2007HttpBinding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
                issuerWS2007HttpBinding.Security.Message.EstablishSecurityContext = false;
                issuerWS2007HttpBinding.Security.Message.AlgorithmSuite = SecurityAlgorithmSuite.Default;

                var tokenParams = WSTrustTokenParameters.CreateWS2007FederationTokenParameters(issuerWS2007HttpBinding, new EndpointAddress(IssuerWSTrustEndpointUri));
                tokenParams.KeyType = SecurityKeyType.BearerKey; // "http://schemas.microsoft.com/idfx/keytype/bearer";

                var clientCredentials = new ClientCredentials();
                clientCredentials.UserName.UserName = _connectionParameters.Credential.UserName;
                clientCredentials.UserName.Password = _connectionParameters.Credential.Password;
                var trustCredentials = new WSTrustChannelClientCredentials(clientCredentials);
                var tokenManager = trustCredentials.CreateSecurityTokenManager();

                var tokenRequirement = new SecurityTokenRequirement();
                EndpointAddress appliesTo = new EndpointAddress(_infoShareWSAppliesTo.AbsoluteUri); //Realm WITHOUT Application.svc!!
                tokenRequirement.Properties["http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/IssuedSecurityTokenParameters"] = tokenParams;
                tokenRequirement.Properties["http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/TargetAddress"] = appliesTo;

                tokenProvider = tokenManager.CreateSecurityTokenProvider(tokenRequirement);
                ((ICommunicationObject)tokenProvider).Open();
                // tokenProvider.SupportsTokenRenewal = false currently??
                var genericXmlToken = (GenericXmlSecurityToken)tokenProvider.GetToken(_connectionParameters.IssueTimeout);
                _logger.WriteDebug($"InfoShareWcfSoapWithWsTrustConnection Issue Token received ValidFrom[{genericXmlToken.ValidFrom}] ValidTo[{genericXmlToken.ValidTo}]");
                return genericXmlToken;
            }
            finally
            {
                if (tokenProvider != null)
                {
                    ((ICommunicationObject)tokenProvider).Close();
                }
            }
#endif
        }

#if NET48
        /// <summary>
        /// Resolve endpoints
        /// 1. Binding enpoints for the InfoShareWS endpoints
        /// 2. Look into the issuer elements to extract the issuer binding and endpoint
        /// </summary>
        private void ResolveEndpoints(bool autoAuthenticate)
        {
            _logger.WriteDebug("InfoShareWcfSoapWithWsTrustConnection Resolving endpoints");
            Uri wsdlUriApplication = new Uri(InfoShareWSBaseUri, _serviceUriByServiceName[Application25] + "?wsdl");
            var wsdlImporterApplication = GetWsdlImporter(wsdlUriApplication);
            // Get endpont for http or https depending on the base uri passed
            var applicationServiceEndpoint = wsdlImporterApplication.ImportAllEndpoints().Single(x => x.Address.Uri.Scheme == InfoShareWSBaseUri.Scheme);
            ApplyTimeout(applicationServiceEndpoint, _connectionParameters.ServiceTimeout);
            ApplyQuotas(applicationServiceEndpoint);
            _commonBinding = applicationServiceEndpoint.Binding;

            if (autoAuthenticate)
            {
                // Resolve the token
                var token = IssuedToken;
            }
            _logger.WriteDebug("InfoShareWcfSoapWithWsTrustConnection Resolved endpoints");
        }

        /// <summary>
        /// Extract the Issuer endpoint and configure the appropriate one
        /// </summary>
        private ServiceEndpoint FindIssuerEndpoint()
        {
            _logger.WriteDebug("InfoShareWcfSoapWithWsTrustConnection FindIssuerEndpoint");
            EndpointAddress issuerMetadataAddress = null;
            EndpointAddress issuerAddress = null;
            IssuedSecurityTokenParameters protectionTokenParameters = null;


            //Based on the scheme dynamically extract the protection token parameters from a Property path string using reflection.
            //Writing the code requires to much casting. The paths are taken from the powershell scripts
            if (InfoShareWSBaseUri.Scheme == Uri.UriSchemeHttp)
            {
                dynamic binding = _commonBinding;
                protectionTokenParameters = (IssuedSecurityTokenParameters)binding.Elements[0].ProtectionTokenParameters.BootstrapSecurityBindingElement.ProtectionTokenParameters;
            }
            else
            {
                dynamic binding = _commonBinding;
                protectionTokenParameters = (IssuedSecurityTokenParameters)binding.Elements[0].EndpointSupportingTokenParameters.Endorsing[0].BootstrapSecurityBindingElement.EndpointSupportingTokenParameters.Endorsing[0];
            }
            issuerMetadataAddress = protectionTokenParameters.IssuerMetadataAddress;
            issuerAddress = protectionTokenParameters.IssuerAddress;
            _logger.WriteDebug($"InfoShareWcfSoapWithWsTrustConnection FindIssuerEndpoint issuerMetadataAddress[{issuerMetadataAddress}] issuerAddress[{issuerAddress}]");


            ServiceEndpointCollection serviceEndpointCollection;
            try
            {
                // Start with the mex endpoint
                var wsdlImporter = GetWsdlImporter(issuerMetadataAddress.Uri);
                serviceEndpointCollection = wsdlImporter.ImportAllEndpoints();
            }
            catch
            {
                // Re-try with the wsdl endpoint
                var wsdlImporter = GetWsdlImporter(new Uri(issuerMetadataAddress.Uri.AbsoluteUri.Replace("/mex", "?wsdl")));
                serviceEndpointCollection = wsdlImporter.ImportAllEndpoints();
            }

            var issuerWSTrustEndpointAbsolutePath = IssuerWSTrustEndpointUri.AbsolutePath;
            _logger.WriteDebug($"InfoShareWcfSoapWithWsTrustConnection FindIssuerEndpoint issuerWSTrustEndpointAbsolutePath[{issuerWSTrustEndpointAbsolutePath}]");
            ServiceEndpoint issuerServiceEndpoint = serviceEndpointCollection.FirstOrDefault(
                x => x.Address.Uri.AbsolutePath.Equals(issuerWSTrustEndpointAbsolutePath, StringComparison.OrdinalIgnoreCase));

            if (issuerServiceEndpoint == null)
            {
                throw new InvalidOperationException(String.Format("WSTrust endpoint not configured: '{0}'.", issuerWSTrustEndpointAbsolutePath));
            }
            
            protectionTokenParameters.IssuerBinding = issuerServiceEndpoint.Binding;
            protectionTokenParameters.IssuerAddress = issuerServiceEndpoint.Address;
            return issuerServiceEndpoint;
        }

        /// <summary>
        /// Find the wsdl importer
        /// </summary>
        /// <param name="wsdlUri">The wsdl uri</param>
        /// <returns>A wsdl importer</returns>
        private WsdlImporter GetWsdlImporter(Uri wsdlUri)
        {
            _logger.WriteDebug($"InfoShareWcfSoapWithWsTrustConnection GetWsdlImporter wsdlUri[{wsdlUri}]");
            WSHttpBinding mexBinding = null;
            if (wsdlUri.Scheme == Uri.UriSchemeHttp)
            {
                mexBinding = (WSHttpBinding)MetadataExchangeBindings.CreateMexHttpBinding();
            }
            else
            {
                mexBinding = (WSHttpBinding)MetadataExchangeBindings.CreateMexHttpsBinding();
            }
            mexBinding.MaxReceivedMessageSize = Int32.MaxValue;
            mexBinding.MaxBufferPoolSize = Int32.MaxValue;
            mexBinding.ReaderQuotas.MaxStringContentLength = Int32.MaxValue;
            mexBinding.ReaderQuotas.MaxNameTableCharCount = Int32.MaxValue;
            mexBinding.ReaderQuotas.MaxArrayLength = Int32.MaxValue;
            mexBinding.ReaderQuotas.MaxBytesPerRead = Int32.MaxValue;
            mexBinding.ReaderQuotas.MaxDepth = 64;

            var mexClient = new MetadataExchangeClient(mexBinding);
            mexClient.MaximumResolvedReferences = int.MaxValue;

            var metadataSet = mexClient.GetMetadata(wsdlUri, MetadataExchangeClientMode.HttpGet);
            return new WsdlImporter(metadataSet);
        }

        /// <summary>
        /// Initializes client credentials 
        /// </summary>
        /// <param name="clientCredentials">Client credentials to initialize</param>
        private void ApplyCredentials(ClientCredentials clientCredentials)
        {
            if (IssuerAuthenticationType == "UserNameMixed")
            {
                if (_connectionParameters.Credential == null)
                {
                    throw new InvalidOperationException($"Authentication endpoint {_issuerWSTrustEndpointUri} requires credentials");
                }
                clientCredentials.UserName.UserName = _connectionParameters.Credential.UserName;
                clientCredentials.UserName.Password = _connectionParameters.Credential.Password;
            }
        }

        /// <summary>
        /// Apply timeouts to the endpoint
        /// </summary>
        /// <param name="endpoint">The endpoint</param>
        /// <param name="timeout">The timeout</param>
        private void ApplyTimeout(ServiceEndpoint endpoint, TimeSpan? timeout)
        {
            if (timeout != null)
            {
                endpoint.Binding.ReceiveTimeout = timeout.Value;
                endpoint.Binding.SendTimeout = timeout.Value;
            }
        }

        /// <summary>
        /// Applies quotas to endpoint
        /// </summary>
        /// <param name="endpoint">The endpoint</param>
        private void ApplyQuotas(ServiceEndpoint endpoint)
        {
            _logger.WriteDebug($"InfoShareWcfSoapWithWsTrustConnection ApplyQuotas on serviceEndpoint[{endpoint.Address.Uri}]");
            CustomBinding customBinding = (CustomBinding)endpoint.Binding;
            var textMessageEncoding = customBinding.Elements.Find<TextMessageEncodingBindingElement>();
            textMessageEncoding.ReaderQuotas.MaxStringContentLength = Int32.MaxValue;
            textMessageEncoding.ReaderQuotas.MaxNameTableCharCount = Int32.MaxValue;
            textMessageEncoding.ReaderQuotas.MaxArrayLength = Int32.MaxValue;
            textMessageEncoding.ReaderQuotas.MaxBytesPerRead = Int32.MaxValue;
            textMessageEncoding.ReaderQuotas.MaxDepth = 64;

            var transport = customBinding.Elements.Find<TransportBindingElement>();
            transport.MaxReceivedMessageSize = Int32.MaxValue;
            transport.MaxBufferPoolSize = Int32.MaxValue;
        }
#endif

        #endregion

        #region IDisposable Methods
        /// <summary>
        /// Disposes the object
        /// </summary>
        public void Dispose()
        {
            if (_annotationClient != null)
            {
                ((IDisposable)_annotationClient).Dispose();
            }
            if (_applicationClient != null)
            {
                ((IDisposable)_applicationClient).Dispose();
            }
            if (_backgroundTaskClient != null)
            {
                ((IDisposable)_backgroundTaskClient).Dispose();
            }
            if (_baselineClient != null)
            {
                ((IDisposable)_baselineClient).Dispose();
            }
            if (_documentObjClient != null)
            {
                ((IDisposable)_documentObjClient).Dispose();
            }
            if (_EDTClient != null)
            {
                ((IDisposable)_EDTClient).Dispose();
            }
            if (_eventMonitorClient != null)
            {
                ((IDisposable)_eventMonitorClient).Dispose();
            }
            if (_folderClient != null)
            {
                ((IDisposable)_folderClient).Dispose();
            }
            if (_listOfValuesClient != null)
            {
                ((IDisposable)_listOfValuesClient).Dispose();
            }
            if (_metadataBindingClient != null)
            {
                ((IDisposable)_metadataBindingClient).Dispose();
            }
            if (_outputFormatClient != null)
            {
                ((IDisposable)_outputFormatClient).Dispose();
            }
            if (_publicationOutputClient != null)
            {
                ((IDisposable)_publicationOutputClient).Dispose();
            }
            if (_searchClient != null)
            {
                ((IDisposable)_searchClient).Dispose();
            }
            if (_settingsClient != null)
            {
                ((IDisposable)_settingsClient).Dispose();
            }
            if (_translationJobClient != null)
            {
                ((IDisposable)_translationJobClient).Dispose();
            }
            if (_translationTemplateClient != null)
            {
                ((IDisposable)_translationTemplateClient).Dispose();
            }
            if (_userClient != null)
            {
                ((IDisposable)_userClient).Dispose();
            }
            if (_userRoleClient != null)
            {
                ((IDisposable)_userRoleClient).Dispose();
            }
            if (_userGroupClient != null)
            {
                ((IDisposable)_userGroupClient).Dispose();
            }
        }
        /// <summary>
        /// Disposes the object
        /// </summary>
        public void Close()
        {
            Dispose();
        }
        #endregion
    }
}
