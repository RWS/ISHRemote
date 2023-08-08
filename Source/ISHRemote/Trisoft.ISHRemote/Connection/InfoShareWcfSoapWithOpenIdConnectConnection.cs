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

#if NET48
//using System.IdentityModel.Protocols.WSTrust;
using System.Security.Cryptography;
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
    internal sealed class InfoShareWcfSoapWithOpenIdConnectConnection : IDisposable, IInfoShareWcfSoapConnection
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
        private readonly InfoShareWcfSoapWithOpenIdConnectConnectionParameters _connectionParameters;
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

#if NET48
        #region NET48 OpenIdConnect
        /// <summary>
        /// Wrapping the Bearer/Access Token as jwt (Json Web Token) claim into a Saml 2.0 token to push over WCF (Windows Communication Foundation) SOAP
        /// </summary>
        private static GenericXmlSecurityToken WrapJwt(string jwt)
        {
            // https://leastprivilege.com/2015/07/02/give-your-wcf-security-architecture-a-makeover-with-identityserver3/
            // https://github.com/IdentityServer/IdentityServer3/issues/1107
            // https://stackoverflow.com/questions/16312907/delivering-a-jwt-securitytoken-to-a-wcf-client
            // https://github.com/IdentityServer/IdentityServer3.Samples/tree/dev/source/Clients/WcfService

            var subject = new ClaimsIdentity("saml");
            subject.AddClaim(new Claim("jwt", jwt));

            var descriptor = new SecurityTokenDescriptor
            {
                TokenType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0",
                TokenIssuerName = "urn:wrappedjwt",
                Subject = subject,
                SigningCredentials = new X509SigningCredentials(_certficate)
            };

            var handler = new System.IdentityModel.Tokens.Saml2SecurityTokenHandler();
            bool canWRite = handler.CanWriteToken;
            var token = handler.CreateToken(descriptor) as System.IdentityModel.Tokens.Saml2SecurityToken;

            StringBuilder sb = new StringBuilder();
            XmlWriter xmlWriter = XmlWriter.Create(sb);
            handler.WriteToken(xmlWriter, token);

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(sb.ToString());
            var xmlToken = new GenericXmlSecurityToken(
                xmlDocument.DocumentElement,
                null,
                DateTime.Now,
                DateTime.Now.AddHours(1),
                null,
                null,
                null);

            return xmlToken;
        }

        /// <summary>
        /// Generate a client certificate to sign the saml token
        /// </summary>
        /// <param name="certName"></param>
        /// <returns></returns>
        private static X509Certificate2 CreateX509Certificate2(string certName = "Client Tools")
        {
            var ecdsa = ECDsa.Create();
            var rsa = RSA.Create();
            var req = new CertificateRequest($"cn={certName}", rsa, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
            var cert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));

            string password = Guid.NewGuid().ToString();
            return new X509Certificate2(cert.Export(X509ContentType.Pfx, password), password);
        }
        
        /// <summary>
        /// Static certificate to reuse signing the saml token
        /// </summary>
        private static X509Certificate2 _certficate = CreateX509Certificate2();
        #endregion
#endif



        /// <summary>
        /// Gets or sets when access token should be refreshed (relative to its expiration time).
        /// </summary>
        public TimeSpan RefreshBeforeExpiration { get; set; } = TimeSpan.FromMinutes(1);

        #region Constructors
        /// <summary>
        /// Initializes a new instance of <c>InfoShareWcfSoapWithOpenIdConnectConnection</c> class.
        /// </summary>
        /// <param name="logger">Instance of Interfaces.ILogger implementation</param>
        /// <param name="httpClient">Incoming reused, probably Ssl/Tls initialized already.</param>
        /// <param name="infoShareWcfConnectionParameters">Connection parameters.</param>
        public InfoShareWcfSoapWithOpenIdConnectConnection(ILogger logger, HttpClient httpClient, InfoShareWcfSoapWithOpenIdConnectConnectionParameters infoShareWcfConnectionParameters)
        {
            _logger = logger;
            _httpClient = httpClient;
            _connectionParameters = infoShareWcfConnectionParameters;
            // Could to more strict _connectionParameters checks

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
            _logger.WriteDebug($"InfoShareWcfSoapWithOpenIdConnectConnection Access Token received ValidTo[{_connectionParameters.Tokens.AccessTokenExpiration}]");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _connectionParameters.Tokens.AccessToken);
            // using the ISHWS url from connectionconfiguration.xml instead of the potentially wrongly cased incoming one [TS-10630]
            _logger.WriteDebug($"InfoShareWcfSoapWithOpenIdConnectConnection using Normalized infoShareWSBaseUri[{_connectionParameters.InfoShareWSUrl}]");
            this.InfoShareWSBaseUri = _connectionParameters.InfoShareWSUrl;

            _logger.WriteDebug($"Resolving Service Uris");
            ResolveServiceUris();

#if NET48
            //extract from WcfSoapWithWsTrustConnection::ResolveEndpoints function to get to binding
            _logger.WriteDebug("InfoShareWcfSoapWithOpenIdConnectConnection Resolving Binding (NET48)");
            Uri wsdlUriApplication = new Uri(InfoShareWSBaseUri, _serviceUriByServiceName[Application25] + "?wsdl");
            var wsdlImporterApplication = GetWsdlImporter(wsdlUriApplication);
            // Get endpont for http or https depending on the base uri passed
            var applicationServiceEndpoint = wsdlImporterApplication.ImportAllEndpoints().Single(x => x.Address.Uri.Scheme == InfoShareWSBaseUri.Scheme);
            CustomBinding customBinding = (CustomBinding)applicationServiceEndpoint.Binding;
            // Increasing Text Message quotas
            var textMessageEncoding = customBinding.Elements.Find<TextMessageEncodingBindingElement>();
            textMessageEncoding.ReaderQuotas.MaxStringContentLength = Int32.MaxValue;
            textMessageEncoding.ReaderQuotas.MaxNameTableCharCount = Int32.MaxValue;
            textMessageEncoding.ReaderQuotas.MaxArrayLength = Int32.MaxValue;
            textMessageEncoding.ReaderQuotas.MaxBytesPerRead = Int32.MaxValue;
            textMessageEncoding.ReaderQuotas.MaxDepth = 64;
            // Increasing Transport Quotas
            var transport = customBinding.Elements.Find<TransportBindingElement>();
            transport.MaxReceivedMessageSize = Int32.MaxValue;
            transport.MaxBufferPoolSize = Int32.MaxValue;
            // Applying Send/Receive Timeouts
            _commonBinding = applicationServiceEndpoint.Binding;
            _commonBinding.SendTimeout = _connectionParameters.IssueTimeout;
            _commonBinding.ReceiveTimeout = _connectionParameters.IssueTimeout;

            _logger.WriteDebug("InfoShareWcfSoapWithOpenIdConnectConnection Wrapping AccessToken (NET48)");
            // The lazy initialization depends on all the initialization above.
            _issuedToken = WrapJwt(_connectionParameters.Tokens.AccessToken);
#else
            _logger.WriteDebug("InfoShareWcfSoapWithOpenIdConnectConnection Resolving Binding (NET6+)");
            _commonBinding = new WSFederationHttpBinding(new WSTrustTokenParameters
            {
                TokenType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0",
                KeyType = SecurityKeyType.BearerKey
            });
            _commonBinding.Security.Message.EstablishSecurityContext = false;
            // Increasing Text Message quotas
            _commonBinding.ReaderQuotas.MaxStringContentLength = Int32.MaxValue;
            _commonBinding.ReaderQuotas.MaxNameTableCharCount = Int32.MaxValue;
            _commonBinding.ReaderQuotas.MaxArrayLength = Int32.MaxValue;
            _commonBinding.ReaderQuotas.MaxBytesPerRead = Int32.MaxValue;
            _commonBinding.ReaderQuotas.MaxDepth = 64;
            // Increasing Transport Quotas
            _commonBinding.MaxReceivedMessageSize = Int32.MaxValue;
            _commonBinding.MaxBufferPoolSize = Int32.MaxValue;
            // Applying Send/Receive Timeouts
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

        #endregion Properties

        #region Public Methods
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
            return _annotationClient.ChannelFactory.CreateChannelWithIssuedToken(_issuedToken);
#else
            if ((_annotationClient == null) || (_annotationClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _annotationClient = new Annotation25ServiceReference.AnnotationClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Annotation25]));
            }

            _annotationClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_annotationClient.ChannelFactory.Credentials);
            var bearerCredentials = new BearerCredentials(_connectionParameters.Tokens.AccessToken);
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
            if ((_applicationClient == null) || (_applicationClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _applicationClient = new Application25ServiceReference.ApplicationClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Application25]));
            }
            return _applicationClient.ChannelFactory.CreateChannelWithIssuedToken(_issuedToken);
#else
            if ((_applicationClient == null) || (_applicationClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _applicationClient = new Application25ServiceReference.ApplicationClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Application25]));

                _applicationClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_applicationClient.ChannelFactory.Credentials);
                var bearerCredentials = new BearerCredentials(_connectionParameters.Tokens.AccessToken);
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
            if ((_documentObjClient == null) || (_documentObjClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _documentObjClient = new DocumentObj25ServiceReference.DocumentObjClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[DocumentObj25]));
            }
            return _documentObjClient.ChannelFactory.CreateChannelWithIssuedToken(_issuedToken);
#else
            if ((_documentObjClient == null) || (_documentObjClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _documentObjClient = new DocumentObj25ServiceReference.DocumentObjClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[DocumentObj25]));
            
                _documentObjClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_documentObjClient.ChannelFactory.Credentials);
                var bearerCredentials = new BearerCredentials(_connectionParameters.Tokens.AccessToken);
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
            if ((_folderClient == null) || (_folderClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _folderClient = new Folder25ServiceReference.FolderClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Folder25]));
            }
            return _folderClient.ChannelFactory.CreateChannelWithIssuedToken(_issuedToken);
#else
            if ((_folderClient == null) || (_folderClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _folderClient = new Folder25ServiceReference.FolderClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Folder25]));

                _folderClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_folderClient.ChannelFactory.Credentials);
                var bearerCredentials = new BearerCredentials(_connectionParameters.Tokens.AccessToken);
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
            if ((_userClient == null) || (_userClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _userClient = new User25ServiceReference.UserClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[User25]));
            }
            return _userClient.ChannelFactory.CreateChannelWithIssuedToken(_issuedToken);
#else
            if ((_userClient == null) || (_userClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _userClient = new User25ServiceReference.UserClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[User25]));

                _userClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_userClient.ChannelFactory.Credentials);
                var bearerCredentials = new BearerCredentials(_connectionParameters.Tokens.AccessToken);
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
            if ((_userRoleClient == null) || (_userRoleClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _userRoleClient = new UserRole25ServiceReference.UserRoleClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[UserRole25]));
            }
            return _userRoleClient.ChannelFactory.CreateChannelWithIssuedToken(_issuedToken);
#else
            if ((_userRoleClient == null) || (_userRoleClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _userRoleClient = new UserRole25ServiceReference.UserRoleClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[UserRole25]));

                _userRoleClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_userRoleClient.ChannelFactory.Credentials);
                var bearerCredentials = new BearerCredentials(_connectionParameters.Tokens.AccessToken);
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
            if ((_userGroupClient == null) || (_userGroupClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _userGroupClient = new UserGroup25ServiceReference.UserGroupClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[UserGroup25]));
            }
            return _userGroupClient.ChannelFactory.CreateChannelWithIssuedToken(_issuedToken);
#else
            if ((_userGroupClient == null) || (_userGroupClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _userGroupClient = new UserGroup25ServiceReference.UserGroupClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[UserGroup25]));

                _userGroupClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_userGroupClient.ChannelFactory.Credentials);
                var bearerCredentials = new BearerCredentials(_connectionParameters.Tokens.AccessToken);
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
            if ((_listOfValuesClient == null) || (_listOfValuesClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _listOfValuesClient = new ListOfValues25ServiceReference.ListOfValuesClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[ListOfValues25]));
            }
            return _listOfValuesClient.ChannelFactory.CreateChannelWithIssuedToken(_issuedToken);
#else
            if ((_listOfValuesClient == null) || (_listOfValuesClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _listOfValuesClient = new ListOfValues25ServiceReference.ListOfValuesClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[ListOfValues25]));

                _listOfValuesClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_listOfValuesClient.ChannelFactory.Credentials);
                var bearerCredentials = new BearerCredentials(_connectionParameters.Tokens.AccessToken);
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
            if ((_publicationOutputClient == null) || (_publicationOutputClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _publicationOutputClient = new PublicationOutput25ServiceReference.PublicationOutputClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[PublicationOutput25]));
            }
            return _publicationOutputClient.ChannelFactory.CreateChannelWithIssuedToken(_issuedToken);
#else
            if ((_publicationOutputClient == null) || (_publicationOutputClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _publicationOutputClient = new PublicationOutput25ServiceReference.PublicationOutputClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[PublicationOutput25]));

                _publicationOutputClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_publicationOutputClient.ChannelFactory.Credentials);
                var bearerCredentials = new BearerCredentials(_connectionParameters.Tokens.AccessToken);
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
            if ((_outputFormatClient == null) || (_outputFormatClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _outputFormatClient = new OutputFormat25ServiceReference.OutputFormatClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[OutputFormat25]));
            }
            return _outputFormatClient.ChannelFactory.CreateChannelWithIssuedToken(_issuedToken);
#else
            if ((_outputFormatClient == null) || (_outputFormatClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _outputFormatClient = new OutputFormat25ServiceReference.OutputFormatClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[OutputFormat25]));

                _outputFormatClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_outputFormatClient.ChannelFactory.Credentials);
                var bearerCredentials = new BearerCredentials(_connectionParameters.Tokens.AccessToken);
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
            if ((_settingsClient == null) || (_settingsClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _settingsClient = new Settings25ServiceReference.SettingsClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Settings25]));
            }
            return _settingsClient.ChannelFactory.CreateChannelWithIssuedToken(_issuedToken);
#else
            if ((_settingsClient == null) || (_settingsClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _settingsClient = new Settings25ServiceReference.SettingsClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Settings25]));

                _settingsClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_settingsClient.ChannelFactory.Credentials);
                var bearerCredentials = new BearerCredentials(_connectionParameters.Tokens.AccessToken);
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
            if ((_EDTClient == null) || (_EDTClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _EDTClient = new EDT25ServiceReference.EDTClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[EDT25]));
            }
            return _EDTClient.ChannelFactory.CreateChannelWithIssuedToken(_issuedToken);
#else
            if ((_EDTClient == null) || (_EDTClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _EDTClient = new EDT25ServiceReference.EDTClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[EDT25]));

                _EDTClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_EDTClient.ChannelFactory.Credentials);
                var bearerCredentials = new BearerCredentials(_connectionParameters.Tokens.AccessToken);
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
            if ((_eventMonitorClient == null) || (_eventMonitorClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _eventMonitorClient = new EventMonitor25ServiceReference.EventMonitorClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[EventMonitor25]));
            }
            return _eventMonitorClient.ChannelFactory.CreateChannelWithIssuedToken(_issuedToken);
#else
            if ((_eventMonitorClient == null) || (_eventMonitorClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _eventMonitorClient = new EventMonitor25ServiceReference.EventMonitorClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[EventMonitor25]));

                _eventMonitorClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_eventMonitorClient.ChannelFactory.Credentials);
                var bearerCredentials = new BearerCredentials(_connectionParameters.Tokens.AccessToken);
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
            if ((_baselineClient == null) || (_baselineClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _baselineClient = new Baseline25ServiceReference.BaselineClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Baseline25]));
            }
            return _baselineClient.ChannelFactory.CreateChannelWithIssuedToken(_issuedToken);
#else
            if ((_baselineClient == null) || (_baselineClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _baselineClient = new Baseline25ServiceReference.BaselineClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Baseline25]));

                _baselineClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_baselineClient.ChannelFactory.Credentials);
                var bearerCredentials = new BearerCredentials(_connectionParameters.Tokens.AccessToken);
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
            if ((_metadataBindingClient == null) || (_metadataBindingClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _metadataBindingClient = new MetadataBinding25ServiceReference.MetadataBindingClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[MetadataBinding25]));
            }
            return _metadataBindingClient.ChannelFactory.CreateChannelWithIssuedToken(_issuedToken);
#else
            if ((_metadataBindingClient == null) || (_metadataBindingClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _metadataBindingClient = new MetadataBinding25ServiceReference.MetadataBindingClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[MetadataBinding25]));

                _metadataBindingClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_metadataBindingClient.ChannelFactory.Credentials);
                var bearerCredentials = new BearerCredentials(_connectionParameters.Tokens.AccessToken);
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
            if ((_searchClient == null) || (_searchClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _searchClient = new Search25ServiceReference.SearchClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Search25]));
            }
            return _searchClient.ChannelFactory.CreateChannelWithIssuedToken(_issuedToken);
#else
            if ((_searchClient == null) || (_searchClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _searchClient = new Search25ServiceReference.SearchClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Search25]));

                _searchClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_searchClient.ChannelFactory.Credentials);
                var bearerCredentials = new BearerCredentials(_connectionParameters.Tokens.AccessToken);
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
            if ((_translationJobClient == null) || (_translationJobClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _translationJobClient = new TranslationJob25ServiceReference.TranslationJobClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[TranslationJob25]));
            }
            return _translationJobClient.ChannelFactory.CreateChannelWithIssuedToken(_issuedToken);
#else
            if ((_translationJobClient == null) || (_translationJobClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _translationJobClient = new TranslationJob25ServiceReference.TranslationJobClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[TranslationJob25]));

                _translationJobClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_translationJobClient.ChannelFactory.Credentials);
                var bearerCredentials = new BearerCredentials(_connectionParameters.Tokens.AccessToken);
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
            if ((_translationTemplateClient == null) || (_translationTemplateClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _translationTemplateClient = new TranslationTemplate25ServiceReference.TranslationTemplateClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[TranslationTemplate25]));
            }
            return _translationTemplateClient.ChannelFactory.CreateChannelWithIssuedToken(_issuedToken);
#else
            if ((_translationTemplateClient == null) || (_translationTemplateClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _translationTemplateClient = new TranslationTemplate25ServiceReference.TranslationTemplateClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[TranslationTemplate25]));

                _translationTemplateClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_translationTemplateClient.ChannelFactory.Credentials);
                var bearerCredentials = new BearerCredentials(_connectionParameters.Tokens.AccessToken);
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
            if ((_backgroundTaskClient == null) || (_backgroundTaskClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _backgroundTaskClient = new BackgroundTask25ServiceReference.BackgroundTaskClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[BackgroundTask25]));
            }
            return _backgroundTaskClient.ChannelFactory.CreateChannelWithIssuedToken(_issuedToken);
#else
            if ((_backgroundTaskClient == null) || (_backgroundTaskClient.InnerChannel.State == System.ServiceModel.CommunicationState.Faulted))
            {
                _backgroundTaskClient = new BackgroundTask25ServiceReference.BackgroundTaskClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[BackgroundTask25]));

                _backgroundTaskClient.ChannelFactory.Endpoint.EndpointBehaviors.Remove(_backgroundTaskClient.ChannelFactory.Credentials);
                var bearerCredentials = new BearerCredentials(_connectionParameters.Tokens.AccessToken);
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
        /// Find the wsdl importer
        /// </summary>
        /// <param name="wsdlUri">The wsdl uri</param>
        /// <returns>A wsdl importer</returns>
        private WsdlImporter GetWsdlImporter(Uri wsdlUri)
        {
            _logger.WriteDebug($"GetWsdlImporter wsdlUri[{wsdlUri}]");
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
        /// Applies quotas to endpoint
        /// </summary>
        /// <param name="endpoint">The endpoint</param>
        private void ApplyQuotas(ServiceEndpoint endpoint)
        {
            _logger.WriteDebug($"ApplyQuotas on serviceEndpoint[{endpoint.Address.Uri}]");
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

        #region Token Handling
        /*
        /// <summary>
        /// Rough get Bearer/Access token based on class parameters without using OidcClient class library. Could be used for debugging
        /// </summary>
        /// <returns>Bearer Token</returns>
        private string GetTokensOverClientCredentialsRaw()
        {
            var requestUri = new Uri(_connectionParameters.IssuerUrl, "connect/token");
            _logger.WriteDebug($"GetTokensOverClientCredentialsRaw from requestUri[{requestUri}] using ClientId[{_connectionParameters.ClientId}] ClientSecret[{new string('*', _connectionParameters.ClientSecret.Length)}]" );

            FormUrlEncodedContent credentialsForm = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", _connectionParameters.ClientId),
                new KeyValuePair<string, string>("client_secret", _connectionParameters.ClientSecret),
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });
            
            HttpResponseMessage response = _httpClient.PostAsync(requestUri, credentialsForm).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            // response holds something like
            // {"access_token":"eyJhbGciOiJSUzI1NiIsImtpZCI6IjA5RTNGMzY3NDdCMEVCODMzOUNDNERENENGQzdDQ0M1N0FBQjQwRkRSUzI1NiIsIng1dCI6IkNlUHpaMGV3NjRNNXpFM1V6OGZNeFhxclFQMCIsInR5cCI6ImF0K2p3dCJ9.eyJpc3MiOiJodHRwczovL21lY2RldjEycWEwMS5nbG9iYWwuc2RsLmNvcnAvaXNoYW1vcmExOSIsIm5iZiI6MTY3MjM5MDI5NywiaWF0IjoxNjcyMzkwMjk3LCJleHAiOjE2NzIzOTM4OTcsImF1ZCI6WyJUcmlkaW9uX0RvY3NfQ29udGVudF9NYW5hZ2VyX0FwaSIsIlRyaWRpb25fRG9jc19XZWJfRXh0ZW5zaW9uc19BcGkiLCJUcmlkaW9uLkFjY2Vzc01hbmFnZW1lbnQiXSwic2NvcGUiOlsiVHJpZGlvbl9Eb2NzX0NvbnRlbnRfTWFuYWdlcl9BcGkiLCJUcmlkaW9uX0RvY3NfV2ViX0V4dGVuc2lvbnNfQXBpIiwiVHJpZGlvbi5BY2Nlc3NNYW5hZ2VtZW50Il0sImNsaWVudF9pZCI6ImM4MjZlN2UxLWMzNWMtNDNmZS05NzU2LWUxYjYxZjQ0YmI0MCIsInN1YiI6ImM4MjZlN2UxLWMzNWMtNDNmZS05NzU2LWUxYjYxZjQ0YmI0MCIsInVzZXJfaWQiOiIzOTYiLCJyb2xlIjpbIlRyaWRpb24uQWNjZXNzTWFuYWdlbWVudC5BZG1pbmlzdHJhdG9yIiwiVHJpZGlvbi5Eb2NzLkNvbnRlbnRNYW5hZ2VyLkFkbWluaXN0cmF0b3IiLCJUcmlkaW9uLkRvY3MuQ29udGVudE1hbmFnZXIuVXNlciIsIlRyaWRpb24uRG9jcy5XZWIuRXh0ZW5zaW9ucy5Vc2VyIl0sImp0aSI6IkMyMkNGQjhDMzVDQzNDN0VBODI3OUI5QkYyOTU5NkY1In0.oPKgzEkLkgaOqmb25uXVQzK4pNh72TBHRFl2ycnX5rHvoheBzsaGasqTwNVtzlCVbnUJkxjPV_pevUSR4dkB6UpgTqvsEfk_AeXVw-f_Nz250fAwjug0Xongp7un5VCFjSiNFUdCBfpBV0fLadyTAWAjMfr1XaFJhoDGk3lCOiH59WvcWazkr5C8LDQt129bCDEZZs3aWMf-TiAxwOkfVmEAcJz-KFz4BwgfhzqAd5sJI98mIfFx_aXEAFt7JcwWKhgwxLleYKKx2sXbL8sFQ2oe8S0e5HR7AQonNx6ygAw9Q1317_Y-fdGHDmGM7SC6Z7EUAsKH9-r2Uf4AuCBR1w","expires_in":3600,"token_type":"Bearer","scope":"Tridion_Docs_Content_Manager_Api Tridion_Docs_Web_Extensions_Api Tridion.AccessManagement"}
            string tokenResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var tokenObject = JsonConvert.DeserializeAnonymousType(tokenResponse, new { access_Token = "" });
            _logger.WriteDebug($"GetTokensOverClientCredentialsRaw from requestUri[{requestUri}] resulted in BearerToken.Length[{tokenObject.access_Token.Length}]");
            return tokenObject.access_Token;
        }
        */

        /// <summary>
        /// OidcClient-based get Bearer/Access based on class parameters. Will refresh if possible.
        /// </summary>
        /// <param name="cancellationToken">Default</param>
        /// <returns>New Tokens with new or refreshed valeus</returns>
        private async Task<Tokens> GetTokensOverClientCredentialsAsync(CancellationToken cancellationToken = default)
        {
            var requestUri = new Uri(_connectionParameters.IssuerUrl, "connect/token");
            Tokens returnTokens = null;
            _logger.WriteDebug($"GetTokensOverClientCredentialsAsync from requestUri[{requestUri}] using ClientId[{_connectionParameters.ClientId}] ClientSecret.Length[{_connectionParameters.ClientSecret.Length}]");
            var tokenRequest = new ClientCredentialsTokenRequest
            {
                Address = requestUri.ToString(),
                ClientId = _connectionParameters.ClientId,
                ClientSecret = _connectionParameters.ClientSecret
            };
            TokenResponse response = await _httpClient.RequestClientCredentialsTokenAsync(tokenRequest, cancellationToken).ConfigureAwait(false);

            // initial usage response.IsError throws error about System.Runtime.CompilerServices.Unsafe v5 required, but OidcClient needs v6
            if (response.IsError || response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new ApplicationException($"GetTokensOverClientCredentialsAsync Access Error[{response.Error}]");
            }

            returnTokens = new Tokens
            {
                AccessToken = response.AccessToken,
                RefreshToken = response.RefreshToken,
                AccessTokenExpiration = DateTime.Now.AddSeconds(response.ExpiresIn)
            };

            return returnTokens;
        }

        private async Task<Tokens> GetTokensOverSystemBrowserAsync(CancellationToken cancellationToken = default)
        {
            _logger.WriteDebug($"GetTokensOverSystemBrowserAsync from Authority[{_connectionParameters.IssuerUrl.ToString()}] using ClientAppId[{_connectionParameters.ClientAppId}] Scope[{_connectionParameters.Scope}]");

            var browser = new InfoShareOpenIdConnectSystemBrowser(_logger, _connectionParameters.RedirectUri);

            string redirectUri = string.Format($"http://127.0.0.1:{browser.Port}");

            var oidcClientOptions = new OidcClientOptions
            {
                Authority = _connectionParameters.IssuerUrl.ToString(),
                ClientId = _connectionParameters.ClientAppId,
                Scope = _connectionParameters.Scope,
                RedirectUri = redirectUri,
                FilterClaims = false,
                Policy = new Policy()
                {
                    Discovery = new IdentityModel.Client.DiscoveryPolicy
                    {
                        ValidateIssuerName = false,  // Casing matters, otherwise "Error loading discovery document: "PolicyViolation" - "Issuer name does not match authority"
                        ValidateEndpoints = false  // Otherwise "Error loading discovery document: Endpoint belongs to different authority: https://mecdev12qa01.global.sdl.corp/ISHAMORA19/.well-known/openid-configuration/jwks"
                    }
                },
                Browser = browser,
                IdentityTokenValidator = new JwtHandlerIdentityTokenValidator(),
                RefreshTokenInnerHttpHandler = new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (message, certificate, chain, sslPolicyErrors) => true
                }
            };

#if NET48
            // Certificate validation works different on .NET Framework 4.8 versus .NET (Core) 6.0+, below is a catch all
            // bypass for /.well-known/openid-configuration detection. Otherwise you get error 
            // "Error loading discovery document: Error connecting to /.well-known/openid-configuration. Operation is not valid due to the current state of the object..'"
            oidcClientOptions.BackchannelHandler = new HttpClientHandler()
            { 
                ServerCertificateCustomValidationCallback = (message, certificate, chain, sslPolicyErrors) => true 
            };
#endif

            var oidcClient = new OidcClient(oidcClientOptions);
            var loginResult = await oidcClient.LoginAsync(new LoginRequest());

            var result = new Tokens
            {
                AccessToken = loginResult.AccessToken,
                IdentityToken = loginResult.IdentityToken,
                RefreshToken = loginResult.RefreshToken,
                AccessTokenExpiration = loginResult.AccessTokenExpiration.LocalDateTime
            };
            return result;
        }

        private async Task<Tokens> RefreshTokensAsync(CancellationToken cancellationToken = default)
        {
            var requestUri = new Uri(_connectionParameters.IssuerUrl, "connect/token");
            Tokens returnTokens = null;
            _logger.WriteDebug($"RefreshTokensAsync from requestUri[{requestUri}] using ClientAppId[{_connectionParameters.ClientAppId}] RefreshToken.Length[{_connectionParameters.Tokens.RefreshToken.Length}]");
            var refreshTokenRequest = new RefreshTokenRequest
            {
                Address = requestUri.ToString(),
                ClientId = _connectionParameters.ClientAppId,
                RefreshToken = _connectionParameters.Tokens.RefreshToken
            };
            TokenResponse response = await _httpClient.RequestRefreshTokenAsync(refreshTokenRequest, cancellationToken).ConfigureAwait(false);
            // initial usage response.IsError throws error about System.Runtime.CompilerServices.Unsafe v5 required, but OidcClient needs v6
            if (response.IsError || response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new ApplicationException($"RefreshTokensAsync Refresh Error[{response.Error}]");
            }
            returnTokens = new Tokens
            {
                AccessToken = response.AccessToken,
                IdentityToken = response.IdentityToken,
                RefreshToken = response.RefreshToken,
                AccessTokenExpiration = DateTime.Now.AddSeconds(response.ExpiresIn)
            };
            return returnTokens;
        }
         
        /// <summary>
        /// Checks whether the token is issued and still valid
        /// </summary>
        public bool IsValid
        {
            get
            {
                // we have the actual issued token which we can check for expiring
                if (_connectionParameters.Tokens.AccessTokenExpiration.Add(RefreshBeforeExpiration).ToUniversalTime() >= DateTime.UtcNow)
                {
                    //_logger.WriteDebug($"Access Token is valid ({_connectionParameters.Tokens.AccessTokenExpiration.Add(RefreshBeforeExpiration).ToUniversalTime()} >= {DateTime.UtcNow})");
                    return true;
                }
                else if (_connectionParameters.Tokens.AccessTokenExpiration.ToUniversalTime() >= DateTime.UtcNow)
                {
                    //_logger.WriteDebug($"Access Token refresh  ({_connectionParameters.Tokens.AccessTokenExpiration.ToUniversalTime()} >= {DateTime.UtcNow})");
                    _connectionParameters.Tokens = RefreshTokensAsync().GetAwaiter().GetResult();
                    _logger.WriteDebug($"InfoShareWcfSoapWithOpenIdConnectConnection Access Token received ValidTo[{_connectionParameters.Tokens.AccessTokenExpiration}]");
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _connectionParameters.Tokens.AccessToken);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
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
