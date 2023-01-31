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
using System.Management.Automation;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.Exceptions;
using System.Reflection;
using Trisoft.ISHRemote.HelperClasses;
using System.Security;
using System.IO;

namespace Trisoft.ISHRemote.Cmdlets.Session
{
    /// <summary>
    /// <para type="synopsis">The New-IshSession cmdlet creates a new IshSession object using the parameters that are provided.</para>
    /// <para type="description">The New-IshSession cmdlet creates a new IshSession object using the parameters that are provided.</para>
    /// <para type="description">The object contains the service endpoint proxies, and api contract information like multi-value separator, date format, etc</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -IshUserName "admin" -IshPassword "admin"
    /// </code>
    /// <para>Building a session for the chosen service based on username/password authentication. The username/password will be used to build a NetworkCredential object to pass for authentication to the service.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/"
    /// </code>
    /// <para>Building a session for the chosen service based on Active Directory authentication. An implicit NetworkCredential object will be passed for authentication to the service.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -IshUserName "" -IshPassword "admin"
    /// </code>
    /// <para>Building a session for the chosen service based on Active Directory authentication. By providing an empty username (and ignoring the password), an implicit NetworkCredential object will be passed for authentication to the service. This makes it possible to write generic scripts for UserNameMixed/ActiveDirectory authentication.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// </code>
    /// <para>Iteratively the New-IshSession line with PSCredential parameter holding a string representation will prompt you for a password.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -IshUserName "admin" -IshPassword "admin" -Timeout (New-TimeSpan -Seconds 30)
    /// </code>
    /// <para>Building a session for the chosen service based on username/password authentication. The Timeout parameter, expressed as TimeSpan object, controls Send/Receive timeouts of HttpClient when downloading content like connectionconfiguration.xml.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -IshUserName "admin" -IshPassword "admin" -TimeoutIssue (New-TimeSpan -Seconds 120) -TimeoutService (New-TimeSpan -Seconds 600)
    /// </code>
    /// <para>Building a session for the chosen service based on username/password authentication. The Timeout parameters, expressed as TimeSpan objects, control Send/Receive timeouts of WCF when issuing a token or working with proxies.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $securePassword = ConvertTo-SecureString "MYPASSWORD" -AsPlainText -Force
    /// $mycredentials = New-Object System.Management.Automation.PSCredential("MYISHUSERNAME", $securePassword)
    /// New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential $mycredentials
    /// </code>
    /// <para>Extensive automation example based on the PSCredential parameter. Responsibility of the plain text password is yours.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/Internal/" -IshUserName "admin" -IshPassword "admin"
    /// </code>
    /// <para>When ISHDeploy Enable-ISHIntegrationSTSInternalAuthentication was executed on the server, the web services are directed to a secondary Secure Token Service (STS). This happens through the '/Internal/' postfix which in essence points to a different connectionconfiguration.xml for initialization.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/Internal/" -IshUserName "admin" -IshPassword "admin" -IgnoreSslPolicyErrors
    /// </code>
    /// <para>IgnoreSslPolicyErrors presence indicates that a custom callback will be assigned to ServicePointManager.ServerCertificateValidationCallback. Defaults false of course, as this is creates security holes! But very handy for Fiddler usage though.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -IshUserName "admin" -IshPassword "admin" -Protocol WcfSoapWithWsTrust
    /// </code>
    /// <para>Building a session for the chosen service based on username/password authentication. Parameter Protocol indicates the preferred communication/authentication route.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -IshUserName "admin" -IshPassword "admin" -Protocol OpenApiWithOpenIdConnect -ClientId "c826e7e1-c35c-43fe-9756-e1b61f44bb40" -ClientSecret "ziKiGbx6N0G3m69/vWMZUTs2paVO1Mzqt6Y6TX7mnpPJyFVODsI1Vw=="
    /// </code>
    /// <para>Building a session for the chosen service based on username/password authentication. Parameter Protocol indicates the preferred communication/authentication route.</para>
    /// </example>
    [Cmdlet(VerbsCommon.New, "IshSession", SupportsShouldProcess = false)]
    [OutputType(typeof(IshSession))]
    public sealed class NewIshSession : SessionCmdlet
    {
        /// <summary>
        /// <para type="description">Tridion Docs Content Manager web services main URL. Note that the URL is case-sensitive and should end with an ending slash! For example: "https://example.com/ISHWS/"</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ActiveDirectory")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "UserNamePassword")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "PSCredential")]
        [ValidateNotNullOrEmpty]
        public string WsBaseUrl { get; set; }

        /// <summary>
        /// <para type="description">Standard PowerShell Credential class</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "PSCredential")]
        [ValidateNotNullOrEmpty]
        [Credential]
        public PSCredential PSCredential { get; set; }

        /// <summary>
        /// <para type="description">Username to login into Tridion Docs Content Manager. When left empty, fall back to ActiveDirectory.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "UserNamePassword")]
        [AllowEmptyString]
        public string IshUserName
        {
            get { return _ishUserName; }
            set { _ishUserName = value; }
        }

        /// <summary>
        /// <para type="description">Password to login into Tridion Docs Content Manager</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "UserNamePassword")]
        [AllowEmptyString]
        public string IshPassword
        {
            get { return _ishPassword; }
            set { _ishPassword = value; }
        }

        /// <summary>
        /// <para type="description">Client ID when Protocol OpenApiWithOpenIdConnect is used to trigger OAuth2/OpenIDConnect Client Credential Flow for usage to Issuer's /connect/token endpoint.</para>
        /// </summary>
        // TODO [Must] ClientId/ClientSecret should become mandatory on seperate ParameterSetName = "CredentialFlow". When all OpenApi function are there, can be removed from ParameterSetName UserNamePassword/PSCredential as no fall back is required anymore.
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "UserNamePassword")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "PSCredential")]
        [AllowEmptyString]
        public string ClientId
        {
            get { return _clientId; }
            set { _clientId = value; }
        }

        /// <summary>
        /// <para type="description">Client Secret when Protocol OpenApiWithOpenIdConnect is used to trigger OAuth2/OpenIDConnect Client Credential Flow for usage to Issuer's /connect/token endpoint.</para>
        /// </summary>
        // TODO [Must] ClientId/ClientSecret should become mandatory on seperate ParameterSetName = "CredentialFlow". When all OpenApi function are there, can be removed from ParameterSetName UserNamePassword/PSCredential as no fall back is required anymore.
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "UserNamePassword")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "PSCredential")]
        [AllowEmptyString]
        public string ClientSecret
        {
            get { return _clientSecret; }
            set { _clientSecret = value; }
        }

        /// <summary>
        /// <para type="description">Timeout value expressed as TimeSpan, that controls Send/Receive timeouts of HttpClient when downloading content like connectionconfiguration.xml Defaults to 30 minutes.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ActiveDirectory")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "UserNamePassword")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "PSCredential")]
        public TimeSpan Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        /// <summary>
        /// <para type="description">IgnoreSslPolicyErrors presence indicates that a custom callback will be assigned to ServicePointManager.ServerCertificateValidationCallback. Defaults false of course, as this is creates security holes! But very handy for Fiddler usage though.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ActiveDirectory")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "UserNamePassword")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "PSCredential")]
        public SwitchParameter IgnoreSslPolicyErrors
        {
            get { return _ignoreSslPolicyErrors; }
            set { _ignoreSslPolicyErrors = value; }
        }

        /// <summary>
        /// <para type="description">IshSession Protocol tries to connect the communication protocol like ASMX (Soap11), WCF (Soap12), OpenAPI (rest) with the authentication protocol like WS-Trust (WCF-only), AuthenticationContext (Asxm-only), etc. Offering shorthand for working combinations. See also <see cref="Protocol"/>.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "UserNamePassword")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "PSCredential")]
        [ValidateNotNullOrEmpty]
        public Enumerations.Protocol Protocol
        {
            get { return _protocol; }
            set { _protocol = value; }
        }

        #region Private fields 
        private string _ishUserName = null;
        private string _ishPassword = null;
        private SecureString _ishSecurePassword = null;
        private string _clientId = null;
        private string _clientSecret = null;
        private SecureString _clientSecureSecret = null;
        private TimeSpan _timeout = new TimeSpan(0, 30, 0);  // up to 15s for a DNS lookup according to https://msdn.microsoft.com/en-us/library/system.net.http.httpclient.timeout%28v=vs.110%29.aspx
        private bool _ignoreSslPolicyErrors = false;
        private Enumerations.Protocol _protocol = Enumerations.Protocol.Autodetect;
        #endregion

        protected override void ProcessRecord()
        {
            try
            {
                if ((this.ParameterSetName.StartsWith("UserNamePassword")) && string.IsNullOrEmpty(_ishUserName))
                {
                    // We came in with an empty username but with -IshUserName/-IshPassword; 
                    // so fallback to NetworkCredential/ActiveDirectory
                    WriteWarning("Empty -IshUserName so fall back to NetworkCredential/ActiveDirectory, ignoring -IshPassword.");
                    _ishUserName = null;
                    _ishPassword = null;
                }
                int ishPasswordLength = _ishPassword == null ? 0 : _ishPassword.Length;
                int clientSecretLength = _clientSecret == null ? 0 : _clientSecret.Length;

                if (PSCredential != null)
                {
                    _ishUserName = PSCredential.UserName;
                    _ishSecurePassword = PSCredential.Password;
                }
                else if (!String.IsNullOrWhiteSpace(_ishPassword))
                {
                    _ishSecurePassword = SecureStringConversions.StringToSecureString(_ishPassword);
                    _clientSecureSecret = _clientSecret == null ? null : SecureStringConversions.StringToSecureString(_clientSecret);
                }
                else
                {
                    _ishSecurePassword = null;
                    _clientSecureSecret = null;
                }

                WriteVerbose($"Connecting to WsBaseUrl[{WsBaseUrl}] IshUserName[{_ishUserName}] IshPassword[" + new string('*', ishPasswordLength) + $"] ClientId[{_clientId}] ClientSecret[" + new string('*', clientSecretLength) + "]");
                WriteDebug($"Connecting to WsBaseUrl[{WsBaseUrl}] IshUserName[{_ishUserName}] IshPassword[" + new string('*', ishPasswordLength) + $"] ClientId[{_clientId}] ClientSecret[" + new string('*', clientSecretLength) + $"] Timeout[{_timeout}] IgnoreSslPolicyErrors[{_ignoreSslPolicyErrors}] Protocol[{_protocol}]");
                IshSession ishSession = new IshSession(Logger, WsBaseUrl, _ishUserName, _ishSecurePassword, _clientId, _clientSecureSecret, _timeout, _ignoreSslPolicyErrors, _protocol);

                // Do early load of IshTypeFieldSetup (either <13-TriDKXmlSetup-based or >=13-RetrieveFieldSetupByIshType-API-based) for
                // usage by ToIshMetadataFields/.../ToIshRequestedMetadataFields and Expand-ISHParameter.ps1 parameter autocompletion
                var ishTypeFieldSetup = ishSession.IshTypeFieldSetup;

                // Submit the PSVariable so you don't have to specify '-IshSession $ishSession' all the time, can be retrieved in every cmdlet, preferably in BeginProcessing()
                WriteVerbose($"Storing IshSession[{ishSession.Name}] under SessionState.{ISHRemoteSessionStateIshSession}");
                this.SessionState.PSVariable.Set(ISHRemoteSessionStateIshSession, ishSession);

                WriteObject(ishSession);
            }
            catch (TrisoftAutomationException trisoftAutomationException)
            {
                ThrowTerminatingError(new ErrorRecord(trisoftAutomationException, base.GetType().Name, ErrorCategory.InvalidOperation, null));
            }
            catch (AggregateException aggregateException)
            {
                var flattenedAggregateException = aggregateException.Flatten();
                WriteWarning(flattenedAggregateException.ToString());
                ThrowTerminatingError(new ErrorRecord(flattenedAggregateException, base.GetType().Name, ErrorCategory.NotSpecified, null));
            }
            catch (Exception exception)
            {
                if (exception.InnerException != null)
                {
                    WriteWarning(exception.InnerException.ToString());
                }
                ThrowTerminatingError(new ErrorRecord(exception, base.GetType().Name, ErrorCategory.NotSpecified, null));
            }
        }
    }
}
