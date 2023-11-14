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
using System.Net.Http.Headers;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using System.Threading.Tasks;
using System.Threading;
using Trisoft.ISHRemote.OpenApiISH30;
using System.Security.Claims;
using System.Text;
using System.Xml;
using System.IO;

#if NET48
using System.Security.Cryptography;
using System.ServiceModel.Channels;
using System.ServiceModel.Security.Tokens;
#else
using System.ServiceModel.Federation;
#endif

namespace Trisoft.ISHRemote.Connection
{
    /// <summary>
    /// Dynamic proxy (so without app.config) generation of Service References towards the InfoShare Web Services writen in Windows Communication Foundation (WCF) protected by OpenIdConnect security protocol.
    /// On ISHRemote v1 and earlier, so in turn before InfoShare 15 and earlier, this class was your starting point for dynamic proxy (so without app.config) generation of Service References. The inital class was written in .NET Framework style. Inspired by https://devblogs.microsoft.com/dotnet/wsfederationhttpbinding-in-net-standard-wcf/ this class has pragmas to illustrate .NET Framework and .NET 6.0+ style side-by-side.
    /// </summary>
    internal sealed class InfoShareWcfSoapWithOpenIdConnectConnection : InfoShareOpenIdConnectConnectionBase, IDisposable
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
        /// Service URIs by service.
        /// </summary>
        private readonly Dictionary<string, Uri> _serviceUriByServiceName = new Dictionary<string, Uri>();
#if NET48
        /// <summary>
		/// The token that is used to access the services.
		/// </summary>
		private GenericXmlSecurityToken _issuedToken = null;
        /// <summary>
        /// Binding that is common for every endpoint.
        /// </summary>
        private Binding _commonBinding = null;
#else
        /// <summary>
        /// Binding that is common for every endpoint.
        /// </summary>
        private WSFederationHttpBinding _commonBinding = null;
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
        /// Initializes a new instance of <c>InfoShareWcfSoapWithOpenIdConnectConnection</c> class.
        /// </summary>
        /// <param name="logger">Instance of Interfaces.ILogger implementation</param>
        /// <param name="httpClient">Incoming reused, probably Ssl/Tls initialized already.</param>
        /// <param name="infoShareOpenIdConnectConnectionParameters">OpenIdConnect connection parameters to be shared with WcfSoapWithOpenIdConnect and OpenApiWithOpenIdConnect</param>
        public InfoShareWcfSoapWithOpenIdConnectConnection(ILogger logger, HttpClient httpClient, InfoShareOpenIdConnectConnectionParameters infoShareOpenIdConnectConnectionParameters)
            : base(logger, httpClient, infoShareOpenIdConnectConnectionParameters)
        {
            _logger.WriteDebug($"InfoShareWcfSoapWithOpenIdConnectConnection InfoShareWSUrl[{_connectionParameters.InfoShareWSUrl}]");
            if (_connectionParameters.Tokens == null)
            {
                if ((string.IsNullOrEmpty(_connectionParameters.ClientId)) && (string.IsNullOrEmpty(_connectionParameters.ClientSecret)))
                {
                    // attempt System Browser retrieval of Access/Bearer Token
                    _logger.WriteDebug($"InfoShareWcfSoapWithOpenIdConnectConnection System Browser");
                    _connectionParameters.Tokens = GetTokensOverSystemBrowserAsync().GetAwaiter().GetResult();
                }
                else if ((!string.IsNullOrEmpty(_connectionParameters.ClientId)) && (!string.IsNullOrEmpty(_connectionParameters.ClientSecret)))
                {
                    // Raw method without OidcClient works
                    //_connectionParameters.BearerToken = GetTokensOverClientCredentialsRaw();
                    _logger.WriteDebug($"InfoShareWcfSoapWithOpenIdConnectConnection ClientId[{_connectionParameters.ClientId}] ClientSecret[{new string('*', _connectionParameters.ClientSecret.Length)}]");
                    _connectionParameters.Tokens = GetTokensOverClientCredentialsAsync().GetAwaiter().GetResult();
                }
                else
                {
                    throw new ArgumentException("Expected ClientId and ClientSecret to be not null or empty.");
                }
            }
            else
            {
                // Don't think this will happen
                _logger.WriteDebug($"InfoShareWcfSoapWithOpenIdConnectConnection reusing AccessToken[{_connectionParameters.Tokens.AccessToken}] AccessTokenExpiration[{_connectionParameters.Tokens.AccessTokenExpiration}]");
            }
            _logger.WriteDebug($"InfoShareWcfSoapWithOpenIdConnectConnection Access Token received ValidTo[{_connectionParameters.Tokens.AccessTokenExpiration.ToString("yyyyMMdd.HHmmss.fff")}]");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _connectionParameters.Tokens.AccessToken);
            // using the ISHWS url from connectionconfiguration.xml instead of the potentially wrongly cased incoming one [TS-10630]
            _logger.WriteDebug($"InfoShareWcfSoapWithOpenIdConnectConnection using Normalized infoShareWSBaseUri[{_connectionParameters.InfoShareWSUrl}]");
            this.InfoShareWSBaseUri = _connectionParameters.InfoShareWSUrl;

            _logger.WriteDebug($"InfoShareWcfSoapWithOpenIdConnectConnection Resolving Service Uris");
            ResolveServiceUris();

#if NET48
            //extract from WcfSoapWithWsTrustConnection::ResolveEndpoints function to get to binding
            _logger.WriteDebug("InfoShareWcfSoapWithOpenIdConnectConnection Resolving Binding (NET48)");
            Uri wsdlUriApplication = new Uri(InfoShareWSBaseUri, _serviceUriByServiceName[Application25] + "?wsdl");
            var wsdlImporterApplication = GetWsdlImporter(wsdlUriApplication);
            // Get endpont for http or https depending on the base uri passed
            var applicationServiceEndpoint = wsdlImporterApplication.ImportAllEndpoints().Single(x => x.Address.Uri.Scheme == InfoShareWSBaseUri.Scheme);

            _logger.WriteDebug("InfoShareWcfSoapWithOpenIdConnectConnection Binding Text ReaderQuotas");
            XmlDictionaryReaderQuotas readerQuotas = new XmlDictionaryReaderQuotas();
            readerQuotas.MaxStringContentLength = Int32.MaxValue;
            readerQuotas.MaxNameTableCharCount = Int32.MaxValue;
            readerQuotas.MaxArrayLength = Int32.MaxValue;
            readerQuotas.MaxBytesPerRead = Int32.MaxValue;
            readerQuotas.MaxDepth = 64;
            applicationServiceEndpoint.Binding.GetType().GetProperty("ReaderQuotas").SetValue(applicationServiceEndpoint.Binding, readerQuotas, null);

            _logger.WriteDebug("InfoShareWcfSoapWithOpenIdConnectConnection Binding Transport Quotas");
            CustomBinding customBinding = new CustomBinding(applicationServiceEndpoint.Binding.CreateBindingElements());
            var transport = customBinding.Elements.Find<TransportBindingElement>();
            transport.MaxReceivedMessageSize = Int32.MaxValue;
            transport.MaxBufferPoolSize = Int32.MaxValue;

            _logger.WriteDebug("InfoShareWcfSoapWithOpenIdConnectConnection Binding Send/Receive Timeouts");
            _commonBinding = customBinding;
            _commonBinding.SendTimeout = _connectionParameters.IssueTimeout;
            _commonBinding.ReceiveTimeout = _connectionParameters.IssueTimeout;

            _logger.WriteDebug("InfoShareWcfSoapWithOpenIdConnectConnection Wrapping AccessToken");
            _issuedToken = WrapJwt(_connectionParameters.Tokens.AccessToken);
#else
            _logger.WriteDebug("InfoShareWcfSoapWithOpenIdConnectConnection Resolving Binding (NET6+)");
            _commonBinding = new WSFederationHttpBinding(new WSTrustTokenParameters
            {
                TokenType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0",
                KeyType = SecurityKeyType.BearerKey
            });
            _commonBinding.Security.Message.EstablishSecurityContext = false;
            _logger.WriteDebug("InfoShareWcfSoapWithOpenIdConnectConnection Binding Text ReaderQuotas");
            _commonBinding.ReaderQuotas.MaxStringContentLength = Int32.MaxValue;
            _commonBinding.ReaderQuotas.MaxNameTableCharCount = Int32.MaxValue;
            _commonBinding.ReaderQuotas.MaxArrayLength = Int32.MaxValue;
            _commonBinding.ReaderQuotas.MaxBytesPerRead = Int32.MaxValue;
            _commonBinding.ReaderQuotas.MaxDepth = 64;
            _logger.WriteDebug("InfoShareWcfSoapWithOpenIdConnectConnection Binding Transport Quotas");
            _commonBinding.MaxReceivedMessageSize = Int32.MaxValue;
            _commonBinding.MaxBufferPoolSize = Int32.MaxValue;
            _logger.WriteDebug("InfoShareWcfSoapWithOpenIdConnectConnection Binding Send/Receive Timeouts");
            _commonBinding.SendTimeout = _connectionParameters.IssueTimeout;
            _commonBinding.ReceiveTimeout = _connectionParameters.IssueTimeout;
#endif
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Root uri for the Web Services
        /// </summary>
        public Uri InfoShareWSBaseUri { get; private set; }
        #endregion Public Properties

        #region Public Get..Channel Methods
        /// <summary>
        /// Create a /Wcf/API25/Annotation.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public Annotation25ServiceReference.Annotation GetAnnotation25Channel()
        {
#if NET48
            if ((_annotationClient == null) || (_annotationClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _annotationClient = new Annotation25ServiceReference.AnnotationClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Annotation25]));
            }
            return _annotationClient.ChannelFactory.CreateChannelWithIssuedToken(GetIssuedToken());
#else
            if ((_annotationClient == null) || (_annotationClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _annotationClient = new Annotation25ServiceReference.AnnotationClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Annotation25]));
            }

            _annotationClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_annotationClient.ChannelFactory.Credentials);
            var bearerCredentials = GetBearerCredentials();
            _annotationClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(bearerCredentials);

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
            if ((_applicationClient == null) || (_applicationClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _applicationClient = new Application25ServiceReference.ApplicationClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Application25]));
            }
            return _applicationClient.ChannelFactory.CreateChannelWithIssuedToken(GetIssuedToken());
#else
            if ((_applicationClient == null) || (_applicationClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _applicationClient = new Application25ServiceReference.ApplicationClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Application25]));

                _applicationClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_applicationClient.ChannelFactory.Credentials);
                var bearerCredentials = GetBearerCredentials();
                _applicationClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(bearerCredentials);

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
            if ((_documentObjClient == null) || (_documentObjClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _documentObjClient = new DocumentObj25ServiceReference.DocumentObjClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[DocumentObj25]));
            }
            return _documentObjClient.ChannelFactory.CreateChannelWithIssuedToken(GetIssuedToken());
#else
            if ((_documentObjClient == null) || (_documentObjClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _documentObjClient = new DocumentObj25ServiceReference.DocumentObjClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[DocumentObj25]));

                _documentObjClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_documentObjClient.ChannelFactory.Credentials);
                var bearerCredentials = GetBearerCredentials();
                _documentObjClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(bearerCredentials);

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
            if ((_folderClient == null) || (_folderClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _folderClient = new Folder25ServiceReference.FolderClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Folder25]));
            }
            return _folderClient.ChannelFactory.CreateChannelWithIssuedToken(GetIssuedToken());
#else
            if ((_folderClient == null) || (_folderClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _folderClient = new Folder25ServiceReference.FolderClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Folder25]));

                _folderClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_folderClient.ChannelFactory.Credentials);
                var bearerCredentials = GetBearerCredentials();
                _folderClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(bearerCredentials);

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
            if ((_userClient == null) || (_userClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _userClient = new User25ServiceReference.UserClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[User25]));
            }
            return _userClient.ChannelFactory.CreateChannelWithIssuedToken(GetIssuedToken());
#else
            if ((_userClient == null) || (_userClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _userClient = new User25ServiceReference.UserClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[User25]));

                _userClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_userClient.ChannelFactory.Credentials);
                var bearerCredentials = GetBearerCredentials();
                _userClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(bearerCredentials);

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
            if ((_userRoleClient == null) || (_userRoleClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _userRoleClient = new UserRole25ServiceReference.UserRoleClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[UserRole25]));
            }
            return _userRoleClient.ChannelFactory.CreateChannelWithIssuedToken(GetIssuedToken());
#else
            if ((_userRoleClient == null) || (_userRoleClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _userRoleClient = new UserRole25ServiceReference.UserRoleClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[UserRole25]));

                _userRoleClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_userRoleClient.ChannelFactory.Credentials);
                var bearerCredentials = GetBearerCredentials();
                _userRoleClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(bearerCredentials);

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
            if ((_userGroupClient == null) || (_userGroupClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _userGroupClient = new UserGroup25ServiceReference.UserGroupClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[UserGroup25]));
            }
            return _userGroupClient.ChannelFactory.CreateChannelWithIssuedToken(GetIssuedToken());
#else
            if ((_userGroupClient == null) || (_userGroupClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _userGroupClient = new UserGroup25ServiceReference.UserGroupClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[UserGroup25]));

                _userGroupClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_userGroupClient.ChannelFactory.Credentials);
                var bearerCredentials = GetBearerCredentials();
                _userGroupClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(bearerCredentials);

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
            if ((_listOfValuesClient == null) || (_listOfValuesClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _listOfValuesClient = new ListOfValues25ServiceReference.ListOfValuesClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[ListOfValues25]));
            }
            return _listOfValuesClient.ChannelFactory.CreateChannelWithIssuedToken(GetIssuedToken());
#else
            if ((_listOfValuesClient == null) || (_listOfValuesClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _listOfValuesClient = new ListOfValues25ServiceReference.ListOfValuesClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[ListOfValues25]));

                _listOfValuesClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_listOfValuesClient.ChannelFactory.Credentials);
                var bearerCredentials = GetBearerCredentials();
                _listOfValuesClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(bearerCredentials);

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
            if ((_publicationOutputClient == null) || (_publicationOutputClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _publicationOutputClient = new PublicationOutput25ServiceReference.PublicationOutputClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[PublicationOutput25]));
            }
            return _publicationOutputClient.ChannelFactory.CreateChannelWithIssuedToken(GetIssuedToken());
#else
            if ((_publicationOutputClient == null) || (_publicationOutputClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _publicationOutputClient = new PublicationOutput25ServiceReference.PublicationOutputClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[PublicationOutput25]));

                _publicationOutputClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_publicationOutputClient.ChannelFactory.Credentials);
                var bearerCredentials = GetBearerCredentials();
                _publicationOutputClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(bearerCredentials);

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
            if ((_outputFormatClient == null) || (_outputFormatClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _outputFormatClient = new OutputFormat25ServiceReference.OutputFormatClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[OutputFormat25]));
            }
            return _outputFormatClient.ChannelFactory.CreateChannelWithIssuedToken(GetIssuedToken());
#else
            if ((_outputFormatClient == null) || (_outputFormatClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _outputFormatClient = new OutputFormat25ServiceReference.OutputFormatClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[OutputFormat25]));

                _outputFormatClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_outputFormatClient.ChannelFactory.Credentials);
                var bearerCredentials = GetBearerCredentials();
                _outputFormatClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(bearerCredentials);

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
            if ((_settingsClient == null) || (_settingsClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _settingsClient = new Settings25ServiceReference.SettingsClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Settings25]));
            }
            return _settingsClient.ChannelFactory.CreateChannelWithIssuedToken(GetIssuedToken());
#else
            if ((_settingsClient == null) || (_settingsClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _settingsClient = new Settings25ServiceReference.SettingsClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Settings25]));

                _settingsClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_settingsClient.ChannelFactory.Credentials);
                var bearerCredentials = GetBearerCredentials();
                _settingsClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(bearerCredentials);

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
            if ((_EDTClient == null) || (_EDTClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _EDTClient = new EDT25ServiceReference.EDTClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[EDT25]));
            }
            return _EDTClient.ChannelFactory.CreateChannelWithIssuedToken(GetIssuedToken());
#else
            if ((_EDTClient == null) || (_EDTClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _EDTClient = new EDT25ServiceReference.EDTClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[EDT25]));

                _EDTClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_EDTClient.ChannelFactory.Credentials);
                var bearerCredentials = GetBearerCredentials();
                _EDTClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(bearerCredentials);

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
            if ((_eventMonitorClient == null) || (_eventMonitorClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _eventMonitorClient = new EventMonitor25ServiceReference.EventMonitorClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[EventMonitor25]));
            }
            return _eventMonitorClient.ChannelFactory.CreateChannelWithIssuedToken(GetIssuedToken());
#else
            if ((_eventMonitorClient == null) || (_eventMonitorClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _eventMonitorClient = new EventMonitor25ServiceReference.EventMonitorClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[EventMonitor25]));

                _eventMonitorClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_eventMonitorClient.ChannelFactory.Credentials);
                var bearerCredentials = GetBearerCredentials();
                _eventMonitorClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(bearerCredentials);

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
            if ((_baselineClient == null) || (_baselineClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _baselineClient = new Baseline25ServiceReference.BaselineClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Baseline25]));
            }
            return _baselineClient.ChannelFactory.CreateChannelWithIssuedToken(GetIssuedToken());
#else
            if ((_baselineClient == null) || (_baselineClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _baselineClient = new Baseline25ServiceReference.BaselineClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Baseline25]));

                _baselineClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_baselineClient.ChannelFactory.Credentials);
                var bearerCredentials = GetBearerCredentials();
                _baselineClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(bearerCredentials);

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
            if ((_metadataBindingClient == null) || (_metadataBindingClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _metadataBindingClient = new MetadataBinding25ServiceReference.MetadataBindingClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[MetadataBinding25]));
            }
            return _metadataBindingClient.ChannelFactory.CreateChannelWithIssuedToken(GetIssuedToken());
#else
            if ((_metadataBindingClient == null) || (_metadataBindingClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _metadataBindingClient = new MetadataBinding25ServiceReference.MetadataBindingClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[MetadataBinding25]));

                _metadataBindingClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_metadataBindingClient.ChannelFactory.Credentials);
                var bearerCredentials = GetBearerCredentials();
                _metadataBindingClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(bearerCredentials);

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
            if ((_searchClient == null) || (_searchClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _searchClient = new Search25ServiceReference.SearchClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Search25]));
            }
            return _searchClient.ChannelFactory.CreateChannelWithIssuedToken(GetIssuedToken());
#else
            if ((_searchClient == null) || (_searchClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _searchClient = new Search25ServiceReference.SearchClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Search25]));

                _searchClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_searchClient.ChannelFactory.Credentials);
                var bearerCredentials = GetBearerCredentials();
                _searchClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(bearerCredentials);

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
            if ((_translationJobClient == null) || (_translationJobClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _translationJobClient = new TranslationJob25ServiceReference.TranslationJobClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[TranslationJob25]));
            }
            return _translationJobClient.ChannelFactory.CreateChannelWithIssuedToken(GetIssuedToken());
#else
            if ((_translationJobClient == null) || (_translationJobClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _translationJobClient = new TranslationJob25ServiceReference.TranslationJobClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[TranslationJob25]));

                _translationJobClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_translationJobClient.ChannelFactory.Credentials);
                var bearerCredentials = GetBearerCredentials();
                _translationJobClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(bearerCredentials);

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
            if ((_translationTemplateClient == null) || (_translationTemplateClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _translationTemplateClient = new TranslationTemplate25ServiceReference.TranslationTemplateClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[TranslationTemplate25]));
            }
            return _translationTemplateClient.ChannelFactory.CreateChannelWithIssuedToken(GetIssuedToken());
#else
            if ((_translationTemplateClient == null) || (_translationTemplateClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _translationTemplateClient = new TranslationTemplate25ServiceReference.TranslationTemplateClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[TranslationTemplate25]));

                _translationTemplateClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_translationTemplateClient.ChannelFactory.Credentials);
                var bearerCredentials = GetBearerCredentials();
                _translationTemplateClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(bearerCredentials);

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
            if ((_backgroundTaskClient == null) || (_backgroundTaskClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _backgroundTaskClient = new BackgroundTask25ServiceReference.BackgroundTaskClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[BackgroundTask25]));
            }
            return _backgroundTaskClient.ChannelFactory.CreateChannelWithIssuedToken(GetIssuedToken());
#else
            if ((_backgroundTaskClient == null) || (_backgroundTaskClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted) || (IsTokenAlmostExpired))
            {
                _backgroundTaskClient = new BackgroundTask25ServiceReference.BackgroundTaskClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[BackgroundTask25]));

                _backgroundTaskClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_backgroundTaskClient.ChannelFactory.Credentials);
                var bearerCredentials = GetBearerCredentials();
                _backgroundTaskClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(bearerCredentials);

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

#if NET48
        /// <summary>
        /// Wrapping the Bearer/Access Token as jwt (Json Web Token) claim into a Saml 2.0 token to push over WCF (Windows Communication Foundation) SOAP
        /// Inspired by, hence hat tip to,
        /// https://leastprivilege.com/2015/07/02/give-your-wcf-security-architecture-a-makeover-with-identityserver3/
        /// https://github.com/IdentityServer/IdentityServer3/issues/1107
        /// https://stackoverflow.com/questions/16312907/delivering-a-jwt-securitytoken-to-a-wcf-client
        /// https://github.com/IdentityServer/IdentityServer3.Samples/tree/dev/source/Clients/WcfService
        /// </summary>
        private static GenericXmlSecurityToken WrapJwt(string jwt)
        {
            var subject = new ClaimsIdentity("saml");
            subject.AddClaim(new Claim("jwt", jwt));

            var descriptor = new SecurityTokenDescriptor
            {
                TokenType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0",
                TokenIssuerName = "urn:wrappedjwt",
                Subject = subject,
            };

            var handler = new System.IdentityModel.Tokens.Saml2SecurityTokenHandler();

            var token = handler.CreateToken(descriptor);
            var sb = new StringBuilder(128);
            using (var xmlTextWriter = new XmlTextWriter(new StringWriter(sb)))
            {
                handler.WriteToken(xmlTextWriter, token);
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(sb.ToString());
                var xmlToken = new GenericXmlSecurityToken(
                    xmlDocument.DocumentElement,
                    null,
                    DateTime.UtcNow,
                    DateTime.UtcNow.AddHours(1),
                    null,
                    null,
                    null);
                return xmlToken;
            }
        }

        /// <summary>
        /// Find the wsdl importer
        /// </summary>
        /// <param name="wsdlUri">The wsdl uri</param>
        /// <returns>A wsdl importer</returns>
        private WsdlImporter GetWsdlImporter(Uri wsdlUri)
        {
            _logger.WriteDebug($"InfoShareWcfSoapWithOpenIdConnectConnection GetWsdlImporter wsdlUri[{wsdlUri}]");
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
        /// NET48 SOAP web services (/ISHWS/OWCF/) with OpenIdConnect authentication need a way to pass the Access/Bearer token.
        /// This method wraps the token up in a Json Web Token (Jwt) which passes nicely over Windows Communication Foundation as Issued Token.
        /// </summary>
        private GenericXmlSecurityToken GetIssuedToken()
        {
            return WrapJwt(GetAccessToken());
        }
#else
        /// <summary>
        /// NET6+ SOAP web services (/ISHWS/OWCF/) with OpenIdConnect authentication need a way to pass the Access/Bearer token.
        /// This method wraps the token up in a SAML token which passes nicely over Windows Communication Foundation as Bearer Token on the Endpoint.
        /// </summary>
        private BearerCredentials GetBearerCredentials()
        {
            BearerCredentials bearerCredentials = new BearerCredentials(GetAccessToken());
            return bearerCredentials;
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
