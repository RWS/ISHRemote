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
using System.Net.Http;
using System.ServiceModel;
using System.Xml.Linq;
using Trisoft.ISHRemote.Interfaces;
using System.ServiceModel.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Xml;
using System.IO;

#if !NET48
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
        /// Binding that is common for every endpoint.
        /// </summary>
        private readonly WS2007FederationHttpBinding _commonBinding;
#else
        /// <summary>
        /// Binding that is common for every endpoint.
        /// </summary>
        private readonly WSFederationHttpBinding _commonBinding;
#endif

        /// <summary>
        /// Proxy for annotation
        /// </summary>
        private Annotation25ServiceReference.AnnotationClient _annotationClient;
        private Annotation25ServiceReference.Annotation _annotationServiceReference;
        /// <summary>
        /// Proxy for application
        /// </summary>
        private Application25ServiceReference.ApplicationClient _applicationClient;
        private Application25ServiceReference.Application _applicationServiceReference;
        /// <summary>
        /// Proxy for document obj
        /// </summary>
        private DocumentObj25ServiceReference.DocumentObjClient _documentObjClient;
        private DocumentObj25ServiceReference.DocumentObj _documentObjServiceReference;
        /// <summary>
        /// Proxy for folder
        /// </summary>
        private Folder25ServiceReference.FolderClient _folderClient;
        private Folder25ServiceReference.Folder _folderServiceReference;
        /// <summary>
        /// Proxy for user
        /// </summary>
        private User25ServiceReference.UserClient _userClient;
        private User25ServiceReference.User _userServiceReference;
        /// <summary>
        /// Proxy for user role
        /// </summary>
        private UserRole25ServiceReference.UserRoleClient _userRoleClient;
        private UserRole25ServiceReference.UserRole _userRoleServiceReference;
        /// <summary>
        /// Proxy for user group
        /// </summary>
        private UserGroup25ServiceReference.UserGroupClient _userGroupClient;
        private UserGroup25ServiceReference.UserGroup _userGroupServiceReference;
        /// <summary>
        /// Proxy for LOV
        /// </summary>
        private ListOfValues25ServiceReference.ListOfValuesClient _listOfValuesClient;
        private ListOfValues25ServiceReference.ListOfValues _listOfValuesServiceReference;
        /// <summary>
        /// Proxy for publication output
        /// </summary>
        private PublicationOutput25ServiceReference.PublicationOutputClient _publicationOutputClient;
        private PublicationOutput25ServiceReference.PublicationOutput _publicationOutputServiceReference;
        /// <summary>
        /// Proxy for output format
        /// </summary>
        private OutputFormat25ServiceReference.OutputFormatClient _outputFormatClient;
        private OutputFormat25ServiceReference.OutputFormat _outputFormatServiceReference;
        /// <summary>
        /// Proxy for settings
        /// </summary>
        private Settings25ServiceReference.SettingsClient _settingsClient;
        private Settings25ServiceReference.Settings _settingsServiceReference;
        /// <summary>
        /// Proxy for EDT
        /// </summary>
        private EDT25ServiceReference.EDTClient _EDTClient;
        private EDT25ServiceReference.EDT _EDTServiceReference;
        /// <summary>
        /// Proxy for event monitor
        /// </summary>
        private EventMonitor25ServiceReference.EventMonitorClient _eventMonitorClient;
        private EventMonitor25ServiceReference.EventMonitor _eventMonitorServiceReference;
        /// <summary>
        /// Proxy for baseline
        /// </summary>
        private Baseline25ServiceReference.BaselineClient _baselineClient;
        private Baseline25ServiceReference.Baseline _baselineServiceReference;
        /// <summary>
        /// Proxy for metadata binding
        /// </summary>
        private MetadataBinding25ServiceReference.MetadataBindingClient _metadataBindingClient;
        private MetadataBinding25ServiceReference.MetadataBinding _metadataBindingServiceReference;
        /// <summary>
        /// Proxy for search
        /// </summary>
        private Search25ServiceReference.SearchClient _searchClient;
        private Search25ServiceReference.Search _searchServiceReference;
        /// <summary>
        /// Proxy for translation job
        /// </summary>
        private TranslationJob25ServiceReference.TranslationJobClient _translationJobClient;
        private TranslationJob25ServiceReference.TranslationJob _translationJobServiceReference;
        /// <summary>
        /// Proxy for translation template
        /// </summary>
        private TranslationTemplate25ServiceReference.TranslationTemplateClient _translationTemplateClient;
        private TranslationTemplate25ServiceReference.TranslationTemplate _translationTemplateServiceReference;
        /// <summary>
        /// Proxy for background task
        /// </summary>
        private BackgroundTask25ServiceReference.BackgroundTaskClient _backgroundTaskClient;
        private BackgroundTask25ServiceReference.BackgroundTask _backgroundTaskServiceReference;
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
                    _logger.WriteDebug("InfoShareWcfSoapWithOpenIdConnectConnection System Browser");
                    _connectionParameters.Tokens = GetTokensOverSystemBrowserAsync().GetAwaiter().GetResult();
                }
                else if ((!string.IsNullOrEmpty(_connectionParameters.ClientId)) && (!string.IsNullOrEmpty(_connectionParameters.ClientSecret)))
                {
                    // Raw method without OidcClient, see GetTokensOverClientCredentialsRaw();
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
            _logger.WriteDebug("InfoShareWcfSoapWithOpenIdConnectConnection Resolving Binding (NET48)");
            _commonBinding = new WS2007FederationHttpBinding(WSFederationHttpSecurityMode.TransportWithMessageCredential);
            _commonBinding.Security.Message.IssuedKeyType = SecurityKeyType.BearerKey;
            _commonBinding.Security.Message.IssuedTokenType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0";
#else
            _logger.WriteDebug("InfoShareWcfSoapWithOpenIdConnectConnection Resolving Binding (NET6+)");
            _commonBinding = new WSFederationHttpBinding(new WSTrustTokenParameters
            {
                TokenType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0",
                KeyType = SecurityKeyType.BearerKey
            });
#endif
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
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Root uri for the Web Services
        /// </summary>
        public Uri InfoShareWSBaseUri { get; private set; }
        #endregion Public Properties

        #region Private Faulted Event Handlers
        /// <summary>
        /// Event handler for when the Annotation client channel enters a Faulted state
        /// </summary>
        private void AnnotationClientChannelFaulted(object sender, EventArgs e)
        {
            _logger.WriteVerbose($"AnnotationClientChannelFaulted State[{((ICommunicationObject)sender)?.State}]");
            ((ICommunicationObject)sender)?.Abort();
            _annotationClient = null;
            _annotationServiceReference = null;
        }

        /// <summary>
        /// Event handler for when the Application client channel enters a Faulted state
        /// </summary>
        private void ApplicationClientChannelFaulted(object sender, EventArgs e)
        {
            _logger.WriteVerbose($"ApplicationClientChannelFaulted State[{((ICommunicationObject)sender)?.State}]");
            ((ICommunicationObject)sender)?.Abort();
            _applicationClient = null;
            _applicationServiceReference = null;
        }

        /// <summary>
        /// Event handler for when the DocumentObj client channel enters a Faulted state
        /// </summary>
        private void DocumentObjClientChannelFaulted(object sender, EventArgs e)
        {
            _logger.WriteVerbose($"DocumentObjClientChannelFaulted State[{((ICommunicationObject)sender)?.State}]");
            ((ICommunicationObject)sender)?.Abort();
            _documentObjClient = null;
            _documentObjServiceReference = null;
        }

        /// <summary>
        /// Event handler for when the Folder client channel enters a Faulted state
        /// </summary>
        private void FolderClientChannelFaulted(object sender, EventArgs e)
        {
            _logger.WriteVerbose($"FolderClientChannelFaulted State[{((ICommunicationObject)sender)?.State}]");
            ((ICommunicationObject)sender)?.Abort();
            _folderClient = null;
            _folderServiceReference = null;
        }

        /// <summary>
        /// Event handler for when the User client channel enters a Faulted state
        /// </summary>
        private void UserClientChannelFaulted(object sender, EventArgs e)
        {
            _logger.WriteVerbose($"UserClientChannelFaulted State[{((ICommunicationObject)sender)?.State}]");
            ((ICommunicationObject)sender)?.Abort();
            _userClient = null;
            _userServiceReference = null;
        }

        /// <summary>
        /// Event handler for when the UserRole client channel enters a Faulted state
        /// </summary>
        private void UserRoleClientChannelFaulted(object sender, EventArgs e)
        {
            _logger.WriteVerbose($"UserRoleClientChannelFaulted State[{((ICommunicationObject)sender)?.State}]");
            ((ICommunicationObject)sender)?.Abort();
            _userRoleClient = null;
            _userRoleServiceReference = null;
        }

        /// <summary>
        /// Event handler for when the UserGroup client channel enters a Faulted state
        /// </summary>
        private void UserGroupClientChannelFaulted(object sender, EventArgs e)
        {
            _logger.WriteVerbose($"UserGroupClientChannelFaulted State[{((ICommunicationObject)sender)?.State}]");
            ((ICommunicationObject)sender)?.Abort();
            _userGroupClient = null;
            _userGroupServiceReference = null;
        }

        /// <summary>
        /// Event handler for when the ListOfValues client channel enters a Faulted state
        /// </summary>
        private void ListOfValuesClientChannelFaulted(object sender, EventArgs e)
        {
            _logger.WriteVerbose($"ListOfValuesClientChannelFaulted State[{((ICommunicationObject)sender)?.State}]");
            ((ICommunicationObject)sender)?.Abort();
            _listOfValuesClient = null;
            _listOfValuesServiceReference = null;
        }

        /// <summary>
        /// Event handler for when the PublicationOutput client channel enters a Faulted state
        /// </summary>
        private void PublicationOutputClientChannelFaulted(object sender, EventArgs e)
        {
            _logger.WriteVerbose($"PublicationOutputClientChannelFaulted State[{((ICommunicationObject)sender)?.State}]");
            ((ICommunicationObject)sender)?.Abort();
            _publicationOutputClient = null;
            _publicationOutputServiceReference = null;
        }

        /// <summary>
        /// Event handler for when the OutputFormat client channel enters a Faulted state
        /// </summary>
        private void OutputFormatClientChannelFaulted(object sender, EventArgs e)
        {
            _logger.WriteVerbose($"OutputFormatClientChannelFaulted State[{((ICommunicationObject)sender)?.State}]");
            ((ICommunicationObject)sender)?.Abort();
            _outputFormatClient = null;
            _outputFormatServiceReference = null;
        }

        /// <summary>
        /// Event handler for when the Settings client channel enters a Faulted state
        /// </summary>
        private void SettingsClientChannelFaulted(object sender, EventArgs e)
        {
            _logger.WriteVerbose($"SettingsClientChannelFaulted State[{((ICommunicationObject)sender)?.State}]");
            ((ICommunicationObject)sender)?.Abort();
            _settingsClient = null;
            _settingsServiceReference = null;
        }

        /// <summary>
        /// Event handler for when the EDT client channel enters a Faulted state
        /// </summary>
        private void EDTClientChannelFaulted(object sender, EventArgs e)
        {
            _logger.WriteVerbose($"EDTClientChannelFaulted State[{((ICommunicationObject)sender)?.State}]");
            ((ICommunicationObject)sender)?.Abort();
            _EDTClient = null;
            _EDTServiceReference = null;
        }

        /// <summary>
        /// Event handler for when the EventMonitor client channel enters a Faulted state
        /// </summary>
        private void EventMonitorClientChannelFaulted(object sender, EventArgs e)
        {
            _logger.WriteVerbose($"EventMonitorClientChannelFaulted State[{((ICommunicationObject)sender)?.State}]");
            ((ICommunicationObject)sender)?.Abort();
            _eventMonitorClient = null;
            _eventMonitorServiceReference = null;
        }

        /// <summary>
        /// Event handler for when the Baseline client channel enters a Faulted state
        /// </summary>
        private void BaselineClientChannelFaulted(object sender, EventArgs e)
        {
            _logger.WriteVerbose($"BaselineClientChannelFaulted State[{((ICommunicationObject)sender)?.State}]");
            ((ICommunicationObject)sender)?.Abort();
            _baselineClient = null;
            _baselineServiceReference = null;
        }

        /// <summary>
        /// Event handler for when the MetadataBinding client channel enters a Faulted state
        /// </summary>
        private void MetadataBindingClientChannelFaulted(object sender, EventArgs e)
        {
            _logger.WriteVerbose($"MetadataBindingClientChannelFaulted State[{((ICommunicationObject)sender)?.State}]");
            ((ICommunicationObject)sender)?.Abort();
            _metadataBindingClient = null;
            _metadataBindingServiceReference = null;
        }

        /// <summary>
        /// Event handler for when the Search client channel enters a Faulted state
        /// </summary>
        private void SearchClientChannelFaulted(object sender, EventArgs e)
        {
            _logger.WriteVerbose($"SearchClientChannelFaulted State[{((ICommunicationObject)sender)?.State}]");
            ((ICommunicationObject)sender)?.Abort();
            _searchClient = null;
            _searchServiceReference = null;
        }

        /// <summary>
        /// Event handler for when the TranslationJob client channel enters a Faulted state
        /// </summary>
        private void TranslationJobClientChannelFaulted(object sender, EventArgs e)
        {
            _logger.WriteVerbose($"TranslationJobClientChannelFaulted State[{((ICommunicationObject)sender)?.State}]");
            ((ICommunicationObject)sender)?.Abort();
            _translationJobClient = null;
            _translationJobServiceReference = null;
        }

        /// <summary>
        /// Event handler for when the TranslationTemplate client channel enters a Faulted state
        /// </summary>
        private void TranslationTemplateClientChannelFaulted(object sender, EventArgs e)
        {
            _logger.WriteVerbose($"TranslationTemplateClientChannelFaulted State[{((ICommunicationObject)sender)?.State}]");
            ((ICommunicationObject)sender)?.Abort();
            _translationTemplateClient = null;
            _translationTemplateServiceReference = null;
        }

        /// <summary>
        /// Event handler for when the BackgroundTask client channel enters a Faulted state
        /// </summary>
        private void BackgroundTaskClientChannelFaulted(object sender, EventArgs e)
        {
            _logger.WriteVerbose($"BackgroundTaskClientChannelFaulted State[{((ICommunicationObject)sender)?.State}]");
            ((ICommunicationObject)sender)?.Abort();
            _backgroundTaskClient = null;
            _backgroundTaskServiceReference = null;
        }
        #endregion Private Faulted Event Handlers

        #region Public Get..Channel Methods
        /// <summary>
        /// Create a /Wcf/API25/Annotation.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public Annotation25ServiceReference.Annotation GetAnnotation25Channel()
        {
            var clientInnerChannelState = _annotationClient?.InnerChannel?.State;
            var serviceReferenceState = (_annotationServiceReference as ICommunicationObject)?.State;
#if NET48
            if ((_annotationClient == null) || (_annotationServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetAnnotation25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_annotationClient)?.Dispose();
                ((IDisposable)_annotationServiceReference)?.Dispose();
                _annotationClient = new Annotation25ServiceReference.AnnotationClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Annotation25]));
                ((ICommunicationObject)_annotationClient).Faulted += new EventHandler(AnnotationClientChannelFaulted);
                _annotationServiceReference = _annotationClient.ChannelFactory.CreateChannelWithIssuedToken(WrapJwt(GetAccessToken().Value));
                ((ICommunicationObject)_annotationServiceReference).Faulted += new EventHandler(AnnotationClientChannelFaulted);
            }
            return _annotationServiceReference;
#else
            if ((_annotationClient == null) || (_annotationServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetAnnotation25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_annotationClient)?.Dispose();
                ((IDisposable)_annotationServiceReference)?.Dispose();
                _annotationClient = new Annotation25ServiceReference.AnnotationClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Annotation25]));

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
                ((ICommunicationObject)_annotationClient).Faulted += new EventHandler(AnnotationClientChannelFaulted);
                _annotationServiceReference = _annotationClient.ChannelFactory.CreateChannel();
                ((ICommunicationObject)_annotationServiceReference).Faulted += new EventHandler(AnnotationClientChannelFaulted);
            }
            return _annotationServiceReference;
#endif
        }
        /// <summary>
        /// Create a /Wcf/API25/Application.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public Application25ServiceReference.Application GetApplication25Channel()
        {
            var clientInnerChannelState = _applicationClient?.InnerChannel?.State;
            var serviceReferenceState = (_applicationServiceReference as ICommunicationObject)?.State;
#if NET48
            if ((_applicationClient == null) || (_applicationServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetApplication25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_applicationClient)?.Dispose();
                ((IDisposable)_applicationServiceReference)?.Dispose();
                _applicationClient = new Application25ServiceReference.ApplicationClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Application25]));
                ((ICommunicationObject)_applicationClient).Faulted += new EventHandler(ApplicationClientChannelFaulted);
                _applicationServiceReference = _applicationClient.ChannelFactory.CreateChannelWithIssuedToken(WrapJwt(GetAccessToken().Value));
                ((ICommunicationObject)_applicationServiceReference).Faulted += new EventHandler(ApplicationClientChannelFaulted);
            }
            return _applicationServiceReference;
#else
            if ((_applicationClient == null) || (_applicationServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetApplication25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_applicationClient)?.Dispose();
                ((IDisposable)_applicationServiceReference)?.Dispose();
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
                ((ICommunicationObject)_applicationClient).Faulted += new EventHandler(ApplicationClientChannelFaulted);
                _applicationServiceReference = _applicationClient.ChannelFactory.CreateChannel();
                ((ICommunicationObject)_applicationServiceReference).Faulted += new EventHandler(ApplicationClientChannelFaulted);
            }
            return _applicationServiceReference;
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/DocumentObj.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public DocumentObj25ServiceReference.DocumentObj GetDocumentObj25Channel()
        {
            var clientInnerChannelState = _documentObjClient?.InnerChannel?.State;
            var serviceReferenceState = (_documentObjServiceReference as ICommunicationObject)?.State;
#if NET48
            if ((_documentObjClient == null) || (_documentObjServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetDocumentObj25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_documentObjClient)?.Dispose();
                ((IDisposable)_documentObjServiceReference)?.Dispose();
                _documentObjClient = new DocumentObj25ServiceReference.DocumentObjClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[DocumentObj25]));
                ((ICommunicationObject)_documentObjClient).Faulted += new EventHandler(DocumentObjClientChannelFaulted);
                _documentObjServiceReference = _documentObjClient.ChannelFactory.CreateChannelWithIssuedToken(WrapJwt(GetAccessToken().Value));
                ((ICommunicationObject)_documentObjServiceReference).Faulted += new EventHandler(DocumentObjClientChannelFaulted);
            }
            return _documentObjServiceReference;
#else
            if ((_documentObjClient == null) || (_documentObjServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetDocumentObj25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_documentObjClient)?.Dispose();
                ((IDisposable)_documentObjServiceReference)?.Dispose();
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
                ((ICommunicationObject)_documentObjClient).Faulted += new EventHandler(DocumentObjClientChannelFaulted);
                _documentObjServiceReference = _documentObjClient.ChannelFactory.CreateChannel();
                ((ICommunicationObject)_documentObjServiceReference).Faulted += new EventHandler(DocumentObjClientChannelFaulted);
            }
            return _documentObjServiceReference;
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/Folder.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public Folder25ServiceReference.Folder GetFolder25Channel()
        {
            var clientInnerChannelState = _folderClient?.InnerChannel?.State;
            var serviceReferenceState = (_folderServiceReference as ICommunicationObject)?.State;
#if NET48
            if ((_folderClient == null) || (_folderServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetFolder25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_folderClient)?.Dispose();
                ((IDisposable)_folderServiceReference)?.Dispose();
                _folderClient = new Folder25ServiceReference.FolderClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Folder25]));
                ((ICommunicationObject)_folderClient).Faulted += new EventHandler(FolderClientChannelFaulted);
                _folderServiceReference = _folderClient.ChannelFactory.CreateChannelWithIssuedToken(WrapJwt(GetAccessToken().Value));
                ((ICommunicationObject)_folderServiceReference).Faulted += new EventHandler(FolderClientChannelFaulted);
            }
            return _folderServiceReference;
#else
            if ((_folderClient == null) || (_folderServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetFolder25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_folderClient)?.Dispose();
                ((IDisposable)_folderServiceReference)?.Dispose();
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
                ((ICommunicationObject)_folderClient).Faulted += new EventHandler(FolderClientChannelFaulted);
                _folderServiceReference = _folderClient.ChannelFactory.CreateChannel();
                ((ICommunicationObject)_folderServiceReference).Faulted += new EventHandler(FolderClientChannelFaulted);
            }
            return _folderServiceReference;
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/User.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public User25ServiceReference.User GetUser25Channel()
        {
            var clientInnerChannelState = _userClient?.InnerChannel?.State;
            var serviceReferenceState = (_userServiceReference as ICommunicationObject)?.State;
#if NET48
            if ((_userClient == null) || (_userServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetUser25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_userClient)?.Dispose();
                ((IDisposable)_userServiceReference)?.Dispose();
                _userClient = new User25ServiceReference.UserClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[User25]));
                ((ICommunicationObject)_userClient).Faulted += new EventHandler(UserClientChannelFaulted);
                _userServiceReference = _userClient.ChannelFactory.CreateChannelWithIssuedToken(WrapJwt(GetAccessToken().Value));
                ((ICommunicationObject)_userServiceReference).Faulted += new EventHandler(UserClientChannelFaulted);
            }
            return _userServiceReference;
#else
            if ((_userClient == null) || (_userServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetUser25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_userClient)?.Dispose();
                ((IDisposable)_userServiceReference)?.Dispose();
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
                ((ICommunicationObject)_userClient).Faulted += new EventHandler(UserClientChannelFaulted);
                _userServiceReference = _userClient.ChannelFactory.CreateChannel();
                ((ICommunicationObject)_userServiceReference).Faulted += new EventHandler(UserClientChannelFaulted);
            }
            return _userServiceReference;
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/UserRole.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public UserRole25ServiceReference.UserRole GetUserRole25Channel()
        {
            var clientInnerChannelState = _userRoleClient?.InnerChannel?.State;
            var serviceReferenceState = (_userRoleServiceReference as ICommunicationObject)?.State;
#if NET48
            if ((_userRoleClient == null) || (_userRoleServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetUserRole25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_userRoleClient)?.Dispose();
                ((IDisposable)_userRoleServiceReference)?.Dispose();
                _userRoleClient = new UserRole25ServiceReference.UserRoleClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[UserRole25]));
                ((ICommunicationObject)_userRoleClient).Faulted += new EventHandler(UserRoleClientChannelFaulted);
                _userRoleServiceReference = _userRoleClient.ChannelFactory.CreateChannelWithIssuedToken(WrapJwt(GetAccessToken().Value));
                ((ICommunicationObject)_userRoleServiceReference).Faulted += new EventHandler(UserRoleClientChannelFaulted);
            }
            return _userRoleServiceReference;
#else
            if ((_userRoleClient == null) || (_userRoleServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetUserRole25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_userRoleClient)?.Dispose();
                ((IDisposable)_userRoleServiceReference)?.Dispose();
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
                ((ICommunicationObject)_userRoleClient).Faulted += new EventHandler(UserRoleClientChannelFaulted);
                _userRoleServiceReference = _userRoleClient.ChannelFactory.CreateChannel();
                ((ICommunicationObject)_userRoleServiceReference).Faulted += new EventHandler(UserRoleClientChannelFaulted);
            }
            return _userRoleServiceReference;
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/UserGroup.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public UserGroup25ServiceReference.UserGroup GetUserGroup25Channel()
        {
            var clientInnerChannelState = _userGroupClient?.InnerChannel?.State;
            var serviceReferenceState = (_userGroupServiceReference as ICommunicationObject)?.State;
#if NET48
            if ((_userGroupClient == null) || (_userGroupServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetUserGroup25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_userGroupClient)?.Dispose();
                ((IDisposable)_userGroupServiceReference)?.Dispose();
                _userGroupClient = new UserGroup25ServiceReference.UserGroupClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[UserGroup25]));
                ((ICommunicationObject)_userGroupClient).Faulted += new EventHandler(UserGroupClientChannelFaulted);
                _userGroupServiceReference = _userGroupClient.ChannelFactory.CreateChannelWithIssuedToken(WrapJwt(GetAccessToken().Value));
                ((ICommunicationObject)_userGroupServiceReference).Faulted += new EventHandler(UserGroupClientChannelFaulted);
            }
            return _userGroupServiceReference;
#else
            if ((_userGroupClient == null) || (_userGroupServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetUserGroup25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_userGroupClient)?.Dispose();
                ((IDisposable)_userGroupServiceReference)?.Dispose();
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
                ((ICommunicationObject)_userGroupClient).Faulted += new EventHandler(UserGroupClientChannelFaulted);
                _userGroupServiceReference = _userGroupClient.ChannelFactory.CreateChannel();
                ((ICommunicationObject)_userGroupServiceReference).Faulted += new EventHandler(UserGroupClientChannelFaulted);
            }
            return _userGroupServiceReference;
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/ListOfValues.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public ListOfValues25ServiceReference.ListOfValues GetListOfValues25Channel()
        {
            var clientInnerChannelState = _listOfValuesClient?.InnerChannel?.State;
            var serviceReferenceState = (_listOfValuesServiceReference as ICommunicationObject)?.State;
#if NET48
            if ((_listOfValuesClient == null) || (_listOfValuesServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetListOfValues25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_listOfValuesClient)?.Dispose();
                ((IDisposable)_listOfValuesServiceReference)?.Dispose();
                _listOfValuesClient = new ListOfValues25ServiceReference.ListOfValuesClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[ListOfValues25]));
                ((ICommunicationObject)_listOfValuesClient).Faulted += new EventHandler(ListOfValuesClientChannelFaulted);
                _listOfValuesServiceReference = _listOfValuesClient.ChannelFactory.CreateChannelWithIssuedToken(WrapJwt(GetAccessToken().Value));
                ((ICommunicationObject)_listOfValuesServiceReference).Faulted += new EventHandler(ListOfValuesClientChannelFaulted);
            }
            return _listOfValuesServiceReference;
#else
            if ((_listOfValuesClient == null) || (_listOfValuesServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetListOfValues25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_listOfValuesClient)?.Dispose();
                ((IDisposable)_listOfValuesServiceReference)?.Dispose();
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
                ((ICommunicationObject)_listOfValuesClient).Faulted += new EventHandler(ListOfValuesClientChannelFaulted);
                _listOfValuesServiceReference = _listOfValuesClient.ChannelFactory.CreateChannel();
                ((ICommunicationObject)_listOfValuesServiceReference).Faulted += new EventHandler(ListOfValuesClientChannelFaulted);
            }
            return _listOfValuesServiceReference;
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/PublicationOutput.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public PublicationOutput25ServiceReference.PublicationOutput GetPublicationOutput25Channel()
        {
            var clientInnerChannelState = _publicationOutputClient?.InnerChannel?.State;
            var serviceReferenceState = (_publicationOutputServiceReference as ICommunicationObject)?.State;
#if NET48
            if ((_publicationOutputClient == null) || (_publicationOutputServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetPublicationOutput25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_publicationOutputClient)?.Dispose();
                ((IDisposable)_publicationOutputServiceReference)?.Dispose();
                _publicationOutputClient = new PublicationOutput25ServiceReference.PublicationOutputClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[PublicationOutput25]));
                ((ICommunicationObject)_publicationOutputClient).Faulted += new EventHandler(PublicationOutputClientChannelFaulted);
                _publicationOutputServiceReference = _publicationOutputClient.ChannelFactory.CreateChannelWithIssuedToken(WrapJwt(GetAccessToken().Value));
                ((ICommunicationObject)_publicationOutputServiceReference).Faulted += new EventHandler(PublicationOutputClientChannelFaulted);
            }
            return _publicationOutputServiceReference;
#else
            if ((_publicationOutputClient == null) || (_publicationOutputServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetPublicationOutput25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_publicationOutputClient)?.Dispose();
                ((IDisposable)_publicationOutputServiceReference)?.Dispose();
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
                ((ICommunicationObject)_publicationOutputClient).Faulted += new EventHandler(PublicationOutputClientChannelFaulted);
                _publicationOutputServiceReference = _publicationOutputClient.ChannelFactory.CreateChannel();
                ((ICommunicationObject)_publicationOutputServiceReference).Faulted += new EventHandler(PublicationOutputClientChannelFaulted);
            }
            return _publicationOutputServiceReference;
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/OutputFormat.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public OutputFormat25ServiceReference.OutputFormat GetOutputFormat25Channel()
        {
            var clientInnerChannelState = _outputFormatClient?.InnerChannel?.State;
            var serviceReferenceState = (_outputFormatServiceReference as ICommunicationObject)?.State;
#if NET48
            if ((_outputFormatClient == null) || (_outputFormatServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetOutputFormat25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_outputFormatClient)?.Dispose();
                ((IDisposable)_outputFormatServiceReference)?.Dispose();
                _outputFormatClient = new OutputFormat25ServiceReference.OutputFormatClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[OutputFormat25]));
                ((ICommunicationObject)_outputFormatClient).Faulted += new EventHandler(OutputFormatClientChannelFaulted);
                _outputFormatServiceReference = _outputFormatClient.ChannelFactory.CreateChannelWithIssuedToken(WrapJwt(GetAccessToken().Value));
                ((ICommunicationObject)_outputFormatServiceReference).Faulted += new EventHandler(OutputFormatClientChannelFaulted);
            }
            return _outputFormatServiceReference;
#else
            if ((_outputFormatClient == null) || (_outputFormatServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetOutputFormat25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_outputFormatClient)?.Dispose();
                ((IDisposable)_outputFormatServiceReference)?.Dispose();
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
                ((ICommunicationObject)_outputFormatClient).Faulted += new EventHandler(OutputFormatClientChannelFaulted);
                _outputFormatServiceReference = _outputFormatClient.ChannelFactory.CreateChannel();
                ((ICommunicationObject)_outputFormatServiceReference).Faulted += new EventHandler(OutputFormatClientChannelFaulted);
            }
            return _outputFormatServiceReference;
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/Settings.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public Settings25ServiceReference.Settings GetSettings25Channel()
        {
            var clientInnerChannelState = _settingsClient?.InnerChannel?.State;
            var serviceReferenceState = (_settingsServiceReference as ICommunicationObject)?.State;
#if NET48
            if ((_settingsClient == null) || (_settingsServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetSettings25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_settingsClient)?.Dispose();
                ((IDisposable)_settingsServiceReference)?.Dispose();
                _settingsClient = new Settings25ServiceReference.SettingsClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Settings25]));
                ((ICommunicationObject)_settingsClient).Faulted += new EventHandler(SettingsClientChannelFaulted);
                _settingsServiceReference = _settingsClient.ChannelFactory.CreateChannelWithIssuedToken(WrapJwt(GetAccessToken().Value));
                ((ICommunicationObject)_settingsServiceReference).Faulted += new EventHandler(SettingsClientChannelFaulted);
            }
            return _settingsServiceReference;
#else
            if ((_settingsClient == null) || (_settingsServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetSettings25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_settingsClient)?.Dispose();
                ((IDisposable)_settingsServiceReference)?.Dispose();
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
                ((ICommunicationObject)_settingsClient).Faulted += new EventHandler(SettingsClientChannelFaulted);
                _settingsServiceReference = _settingsClient.ChannelFactory.CreateChannel();
                ((ICommunicationObject)_settingsServiceReference).Faulted += new EventHandler(SettingsClientChannelFaulted);
            }
            return _settingsServiceReference;
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/Edt.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public EDT25ServiceReference.EDT GetEDT25Channel()
        {
            var clientInnerChannelState = _EDTClient?.InnerChannel?.State;
            var serviceReferenceState = (_EDTServiceReference as ICommunicationObject)?.State;
#if NET48
            if ((_EDTClient == null) || (_EDTServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetEDT25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_EDTClient)?.Dispose();
                ((IDisposable)_EDTServiceReference)?.Dispose();
                _EDTClient = new EDT25ServiceReference.EDTClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[EDT25]));
                ((ICommunicationObject)_EDTClient).Faulted += new EventHandler(EDTClientChannelFaulted);
                _EDTServiceReference = _EDTClient.ChannelFactory.CreateChannelWithIssuedToken(WrapJwt(GetAccessToken().Value));
                ((ICommunicationObject)_EDTServiceReference).Faulted += new EventHandler(EDTClientChannelFaulted);
            }
            return _EDTServiceReference;
#else
            if ((_EDTClient == null) || (_EDTServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetEDT25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_EDTClient)?.Dispose();
                ((IDisposable)_EDTServiceReference)?.Dispose();
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
                ((ICommunicationObject)_EDTClient).Faulted += new EventHandler(EDTClientChannelFaulted);
                _EDTServiceReference = _EDTClient.ChannelFactory.CreateChannel();
                ((ICommunicationObject)_EDTServiceReference).Faulted += new EventHandler(EDTClientChannelFaulted);
            }
            return _EDTServiceReference;
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/EventMonitor.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public EventMonitor25ServiceReference.EventMonitor GetEventMonitor25Channel()
        {
            var clientInnerChannelState = _eventMonitorClient?.InnerChannel?.State;
            var serviceReferenceState = (_eventMonitorServiceReference as ICommunicationObject)?.State;
#if NET48
            if ((_eventMonitorClient == null) || (_eventMonitorServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetEventMonitor25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_eventMonitorClient)?.Dispose();
                ((IDisposable)_eventMonitorServiceReference)?.Dispose();
                _eventMonitorClient = new EventMonitor25ServiceReference.EventMonitorClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[EventMonitor25]));
                ((ICommunicationObject)_eventMonitorClient).Faulted += new EventHandler(EventMonitorClientChannelFaulted);
                _eventMonitorServiceReference = _eventMonitorClient.ChannelFactory.CreateChannelWithIssuedToken(WrapJwt(GetAccessToken().Value));
                ((ICommunicationObject)_eventMonitorServiceReference).Faulted += new EventHandler(EventMonitorClientChannelFaulted);
            }
            return _eventMonitorServiceReference;
#else
            if ((_eventMonitorClient == null) || (_eventMonitorServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetEventMonitor25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_eventMonitorClient)?.Dispose();
                ((IDisposable)_eventMonitorServiceReference)?.Dispose();
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
                ((ICommunicationObject)_eventMonitorClient).Faulted += new EventHandler(EventMonitorClientChannelFaulted);
                _eventMonitorServiceReference = _eventMonitorClient.ChannelFactory.CreateChannel();
                ((ICommunicationObject)_eventMonitorServiceReference).Faulted += new EventHandler(EventMonitorClientChannelFaulted);
            }
            return _eventMonitorServiceReference;
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/Baseline.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public Baseline25ServiceReference.Baseline GetBaseline25Channel()
        {
            var clientInnerChannelState = _baselineClient?.InnerChannel?.State;
            var serviceReferenceState = (_baselineServiceReference as ICommunicationObject)?.State;
#if NET48
            if ((_baselineClient == null) || (_baselineServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetBaseline25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_baselineClient)?.Dispose();
                ((IDisposable)_baselineServiceReference)?.Dispose();
                _baselineClient = new Baseline25ServiceReference.BaselineClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Baseline25]));
                ((ICommunicationObject)_baselineClient).Faulted += new EventHandler(BaselineClientChannelFaulted);
                _baselineServiceReference = _baselineClient.ChannelFactory.CreateChannelWithIssuedToken(WrapJwt(GetAccessToken().Value));
                ((ICommunicationObject)_baselineServiceReference).Faulted += new EventHandler(BaselineClientChannelFaulted);
            }
            return _baselineServiceReference;
#else
            if ((_baselineClient == null) || (_baselineServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetBaseline25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_baselineClient)?.Dispose();
                ((IDisposable)_baselineServiceReference)?.Dispose();
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
                ((ICommunicationObject)_baselineClient).Faulted += new EventHandler(BaselineClientChannelFaulted);
                _baselineServiceReference = _baselineClient.ChannelFactory.CreateChannel();
                ((ICommunicationObject)_baselineServiceReference).Faulted += new EventHandler(BaselineClientChannelFaulted);
            }
            return _baselineServiceReference;
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/MetadataBinding.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public MetadataBinding25ServiceReference.MetadataBinding GetMetadataBinding25Channel()
        {
            var clientInnerChannelState = _metadataBindingClient?.InnerChannel?.State;
            var serviceReferenceState = (_metadataBindingServiceReference as ICommunicationObject)?.State;
#if NET48
            if ((_metadataBindingClient == null) || (_metadataBindingServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetMetadataBinding25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_metadataBindingClient)?.Dispose();
                ((IDisposable)_metadataBindingServiceReference)?.Dispose();
                _metadataBindingClient = new MetadataBinding25ServiceReference.MetadataBindingClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[MetadataBinding25]));
                ((ICommunicationObject)_metadataBindingClient).Faulted += new EventHandler(MetadataBindingClientChannelFaulted);
                _metadataBindingServiceReference = _metadataBindingClient.ChannelFactory.CreateChannelWithIssuedToken(WrapJwt(GetAccessToken().Value));
                ((ICommunicationObject)_metadataBindingServiceReference).Faulted += new EventHandler(MetadataBindingClientChannelFaulted);
            }
            return _metadataBindingServiceReference;
#else
            if ((_metadataBindingClient == null) || (_metadataBindingServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetMetadataBinding25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_metadataBindingClient)?.Dispose();
                ((IDisposable)_metadataBindingServiceReference)?.Dispose();
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
                ((ICommunicationObject)_metadataBindingClient).Faulted += new EventHandler(MetadataBindingClientChannelFaulted);
                _metadataBindingServiceReference = _metadataBindingClient.ChannelFactory.CreateChannel();
                ((ICommunicationObject)_metadataBindingServiceReference).Faulted += new EventHandler(MetadataBindingClientChannelFaulted);
            }
            return _metadataBindingServiceReference;
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/Search.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public Search25ServiceReference.Search GetSearch25Channel()
        {
            var clientInnerChannelState = _searchClient?.InnerChannel?.State;
            var serviceReferenceState = (_searchServiceReference as ICommunicationObject)?.State;
#if NET48
            if ((_searchClient == null) || (_searchServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetSearch25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_searchClient)?.Dispose();
                ((IDisposable)_searchServiceReference)?.Dispose();
                _searchClient = new Search25ServiceReference.SearchClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[Search25]));
                ((ICommunicationObject)_searchClient).Faulted += new EventHandler(SearchClientChannelFaulted);
                _searchServiceReference = _searchClient.ChannelFactory.CreateChannelWithIssuedToken(WrapJwt(GetAccessToken().Value));
                ((ICommunicationObject)_searchServiceReference).Faulted += new EventHandler(SearchClientChannelFaulted);
            }
            return _searchServiceReference;
#else
            if ((_searchClient == null) || (_searchServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetSearch25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_searchClient)?.Dispose();
                ((IDisposable)_searchServiceReference)?.Dispose();
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
                ((ICommunicationObject)_searchClient).Faulted += new EventHandler(SearchClientChannelFaulted);
                _searchServiceReference = _searchClient.ChannelFactory.CreateChannel();
                ((ICommunicationObject)_searchServiceReference).Faulted += new EventHandler(SearchClientChannelFaulted);
            }
            return _searchServiceReference;
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/TranslationJob.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public TranslationJob25ServiceReference.TranslationJob GetTranslationJob25Channel()
        {
            var clientInnerChannelState = _translationJobClient?.InnerChannel?.State;
            var serviceReferenceState = (_translationJobServiceReference as ICommunicationObject)?.State;
#if NET48
            if ((_translationJobClient == null) || (_translationJobServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetTranslationJob25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_translationJobClient)?.Dispose();
                ((IDisposable)_translationJobServiceReference)?.Dispose();
                _translationJobClient = new TranslationJob25ServiceReference.TranslationJobClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[TranslationJob25]));
                ((ICommunicationObject)_translationJobClient).Faulted += new EventHandler(TranslationJobClientChannelFaulted);
                _translationJobServiceReference = _translationJobClient.ChannelFactory.CreateChannelWithIssuedToken(WrapJwt(GetAccessToken().Value));
                ((ICommunicationObject)_translationJobServiceReference).Faulted += new EventHandler(TranslationJobClientChannelFaulted);
            }
            return _translationJobServiceReference;
#else
            if ((_translationJobClient == null) || (_translationJobServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetTranslationJob25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_translationJobClient)?.Dispose();
                ((IDisposable)_translationJobServiceReference)?.Dispose();
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
                ((ICommunicationObject)_translationJobClient).Faulted += new EventHandler(TranslationJobClientChannelFaulted);
                _translationJobServiceReference = _translationJobClient.ChannelFactory.CreateChannel();
                ((ICommunicationObject)_translationJobServiceReference).Faulted += new EventHandler(TranslationJobClientChannelFaulted);
            }
            return _translationJobServiceReference;
#endif
        }

        /// <summary>
        /// Create a /Wcf/API25/TranslationTemplate.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public TranslationTemplate25ServiceReference.TranslationTemplate GetTranslationTemplate25Channel()
        {
            var clientInnerChannelState = _translationTemplateClient?.InnerChannel?.State;
            var serviceReferenceState = (_translationTemplateServiceReference as ICommunicationObject)?.State;
#if NET48
            if ((_translationTemplateClient == null) || (_translationTemplateServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetTranslationTemplate25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_translationTemplateClient)?.Dispose();
                ((IDisposable)_translationTemplateServiceReference)?.Dispose();
                _translationTemplateClient = new TranslationTemplate25ServiceReference.TranslationTemplateClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[TranslationTemplate25]));
                ((ICommunicationObject)_translationTemplateClient).Faulted += new EventHandler(TranslationTemplateClientChannelFaulted);
                _translationTemplateServiceReference = _translationTemplateClient.ChannelFactory.CreateChannelWithIssuedToken(WrapJwt(GetAccessToken().Value));
                ((ICommunicationObject)_translationTemplateServiceReference).Faulted += new EventHandler(TranslationTemplateClientChannelFaulted);
            }
            return _translationTemplateServiceReference;
#else
            if ((_translationTemplateClient == null) || (_translationTemplateServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetTranslationTemplate25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_translationTemplateClient)?.Dispose();
                ((IDisposable)_translationTemplateServiceReference)?.Dispose();
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
                ((ICommunicationObject)_translationTemplateClient).Faulted += new EventHandler(TranslationTemplateClientChannelFaulted);
                _translationTemplateServiceReference = _translationTemplateClient.ChannelFactory.CreateChannel();
                ((ICommunicationObject)_translationTemplateServiceReference).Faulted += new EventHandler(TranslationTemplateClientChannelFaulted);
            }
            return _translationTemplateServiceReference;
#endif
        }


        /// <summary>
        /// Create a /Wcf/API25/BackgroundTask.svc proxy
        /// </summary>
        /// <returns>The proxy</returns>
        public BackgroundTask25ServiceReference.BackgroundTask GetBackgroundTask25Channel()
        {
            var clientInnerChannelState = _backgroundTaskClient?.InnerChannel?.State;
            var serviceReferenceState = (_backgroundTaskServiceReference as ICommunicationObject)?.State;
#if NET48
            if ((_backgroundTaskClient == null) || (_backgroundTaskServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetBackgroundTask25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_backgroundTaskClient)?.Dispose();
                ((IDisposable)_backgroundTaskServiceReference)?.Dispose();
                _backgroundTaskClient = new BackgroundTask25ServiceReference.BackgroundTaskClient(
                    _commonBinding,
                    new EndpointAddress(_serviceUriByServiceName[BackgroundTask25]));
                ((ICommunicationObject)_backgroundTaskClient).Faulted += new EventHandler(BackgroundTaskClientChannelFaulted);
                _backgroundTaskServiceReference = _backgroundTaskClient.ChannelFactory.CreateChannelWithIssuedToken(WrapJwt(GetAccessToken().Value));
                ((ICommunicationObject)_backgroundTaskServiceReference).Faulted += new EventHandler(BackgroundTaskClientChannelFaulted);
            }
            return _backgroundTaskServiceReference;
#else
            if ((_backgroundTaskClient == null) || (_backgroundTaskServiceReference == null) ||
                (clientInnerChannelState == CommunicationState.Faulted) || (serviceReferenceState == CommunicationState.Faulted) ||
                (clientInnerChannelState == CommunicationState.Closing) || (serviceReferenceState == CommunicationState.Closing) ||  
                (clientInnerChannelState == CommunicationState.Closed) || (serviceReferenceState == CommunicationState.Closed) ||
                (GetAccessToken().IsAccessTokenRefreshed))
            {
                _logger.WriteDebug($"GetBackgroundTask25Channel recreating client - ClientInnerChannelState[{clientInnerChannelState}] ServiceReferenceState[{serviceReferenceState}]");
                ((IDisposable)_backgroundTaskClient)?.Dispose();
                ((IDisposable)_backgroundTaskServiceReference)?.Dispose();
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
                ((ICommunicationObject)_backgroundTaskClient).Faulted += new EventHandler(BackgroundTaskClientChannelFaulted);
                _backgroundTaskServiceReference = _backgroundTaskClient.ChannelFactory.CreateChannel();
                ((ICommunicationObject)_backgroundTaskServiceReference).Faulted += new EventHandler(BackgroundTaskClientChannelFaulted);
            }
            return _backgroundTaskServiceReference;
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
#else
        /// <summary>
        /// NET6+ SOAP web services (/ISHWS/OWCF/) with OpenIdConnect authentication need a way to pass the Access/Bearer token.
        /// This method wraps the token up in a SAML token which passes nicely over Windows Communication Foundation as Bearer Token on the Endpoint.
        /// </summary>
        private BearerCredentials GetBearerCredentials()
        {
            BearerCredentials bearerCredentials = new BearerCredentials(GetAccessToken().Value);
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
            ((IDisposable) _annotationClient)?.Dispose();
            ((IDisposable)_annotationServiceReference)?.Dispose();
            ((IDisposable)_annotationClient)?.Dispose();
            ((IDisposable)_annotationServiceReference)?.Dispose();
            ((IDisposable)_applicationClient)?.Dispose();
            ((IDisposable)_applicationServiceReference)?.Dispose();
            ((IDisposable)_applicationClient)?.Dispose();
            ((IDisposable)_applicationServiceReference)?.Dispose();
            ((IDisposable)_documentObjClient)?.Dispose();
            ((IDisposable)_documentObjServiceReference)?.Dispose();
            ((IDisposable)_documentObjClient)?.Dispose();
            ((IDisposable)_documentObjServiceReference)?.Dispose();
            ((IDisposable)_folderClient)?.Dispose();
            ((IDisposable)_folderServiceReference)?.Dispose();
            ((IDisposable)_folderClient)?.Dispose();
            ((IDisposable)_folderServiceReference)?.Dispose();
            ((IDisposable)_userClient)?.Dispose();
            ((IDisposable)_userServiceReference)?.Dispose();
            ((IDisposable)_userClient)?.Dispose();
            ((IDisposable)_userServiceReference)?.Dispose();
            ((IDisposable)_userRoleClient)?.Dispose();
            ((IDisposable)_userRoleServiceReference)?.Dispose();
            ((IDisposable)_userRoleClient)?.Dispose();
            ((IDisposable)_userRoleServiceReference)?.Dispose();
            ((IDisposable)_userGroupClient)?.Dispose();
            ((IDisposable)_userGroupServiceReference)?.Dispose();
            ((IDisposable)_userGroupClient)?.Dispose();
            ((IDisposable)_userGroupServiceReference)?.Dispose();
            ((IDisposable)_listOfValuesClient)?.Dispose();
            ((IDisposable)_listOfValuesServiceReference)?.Dispose();
            ((IDisposable)_listOfValuesClient)?.Dispose();
            ((IDisposable)_listOfValuesServiceReference)?.Dispose();
            ((IDisposable)_publicationOutputClient)?.Dispose();
            ((IDisposable)_publicationOutputServiceReference)?.Dispose();
            ((IDisposable)_publicationOutputClient)?.Dispose();
            ((IDisposable)_publicationOutputServiceReference)?.Dispose();
            ((IDisposable)_outputFormatClient)?.Dispose();
            ((IDisposable)_outputFormatServiceReference)?.Dispose();
            ((IDisposable)_outputFormatClient)?.Dispose();
            ((IDisposable)_outputFormatServiceReference)?.Dispose();
            ((IDisposable)_settingsClient)?.Dispose();
            ((IDisposable)_settingsServiceReference)?.Dispose();
            ((IDisposable)_settingsClient)?.Dispose();
            ((IDisposable)_settingsServiceReference)?.Dispose();
            ((IDisposable)_EDTClient)?.Dispose();
            ((IDisposable)_EDTServiceReference)?.Dispose();
            ((IDisposable)_EDTClient)?.Dispose();
            ((IDisposable)_EDTServiceReference)?.Dispose();
            ((IDisposable)_eventMonitorClient)?.Dispose();
            ((IDisposable)_eventMonitorServiceReference)?.Dispose();
            ((IDisposable)_eventMonitorClient)?.Dispose();
            ((IDisposable)_eventMonitorServiceReference)?.Dispose();
            ((IDisposable)_baselineClient)?.Dispose();
            ((IDisposable)_baselineServiceReference)?.Dispose();
            ((IDisposable)_baselineClient)?.Dispose();
            ((IDisposable)_baselineServiceReference)?.Dispose();
            ((IDisposable)_metadataBindingClient)?.Dispose();
            ((IDisposable)_metadataBindingServiceReference)?.Dispose();
            ((IDisposable)_metadataBindingClient)?.Dispose();
            ((IDisposable)_metadataBindingServiceReference)?.Dispose();
            ((IDisposable)_searchClient)?.Dispose();
            ((IDisposable)_searchServiceReference)?.Dispose();
            ((IDisposable)_searchClient)?.Dispose();
            ((IDisposable)_searchServiceReference)?.Dispose();
            ((IDisposable)_translationJobClient)?.Dispose();
            ((IDisposable)_translationJobServiceReference)?.Dispose();
            ((IDisposable)_translationJobClient)?.Dispose();
            ((IDisposable)_translationJobServiceReference)?.Dispose();
            ((IDisposable)_translationTemplateClient)?.Dispose();
            ((IDisposable)_translationTemplateServiceReference)?.Dispose();
            ((IDisposable)_translationTemplateClient)?.Dispose();
            ((IDisposable)_translationTemplateServiceReference)?.Dispose();
            ((IDisposable)_backgroundTaskClient)?.Dispose();
            ((IDisposable)_backgroundTaskServiceReference)?.Dispose();
            ((IDisposable)_backgroundTaskClient)?.Dispose();
            ((IDisposable)_backgroundTaskServiceReference)?.Dispose();
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
