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

namespace Trisoft.ISHRemote.Cmdlets.Session
{
    /// <summary>
    /// <para type="synopsis">The New-IshSession cmdlet creates a new IshSession object using the parameters that are provided.</para>
    /// <para type="description">The New-IshSession cmdlet creates a new IshSession object using the parameters that are provided.</para>
    /// <para type="description">The object contains the service endpoint proxies, and api contract information like multi-value seperator, date format, etc</para>
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
    /// $DebugPreference   = "Continue"
    /// $VerbosePreference = "Continue" 
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
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -WsTrustIssuerUrl "https://example.com/ISHSTS/issue/wstrust/mixed/username" -WsTrustIssuerMexUrl "https://example.com/ISHSTS/issue/wstrust/mex" -PSCredential "Admin"
    /// </code>
    /// <para>Create a new session with explicit issuer. In this example, the issuer is the ISHSTS next to the ISHWS.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://localhost/ISHWS/" -WsTrustIssuerUrl "https://localhost/ISHSTS/issue/wstrust/mixed/username" -WsTrustIssuerMexUrl "https://localhost/ISHSTS/issue/wstrust/mex" -PSCredential "Admin" -IgnoreSslPolicyErrors
    /// </code>
    /// <para>Create a new session with explicit issuer while using only local endpoints.</para>
    /// </example>
    [Cmdlet(VerbsCommon.New, "IshSession", SupportsShouldProcess = false)]
    [OutputType(typeof(IshSession))]
    public sealed class NewIshSession : SessionCmdlet
    {
        /// <summary>
        /// <para type="description">SDL Tridion Docs Content Manager web services main URL. Note that the URL is case-sensitive and should end with an ending slash! For example: "https://example.com/ISHWS/"</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ActiveDirectory")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "UserNamePassword")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "PSCredential")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ActiveDirectory-ExplicitIssuer")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "UserNamePassword-ExplicitIssuer")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "PSCredential-ExplicitIssuer")]
        [ValidateNotNullOrEmpty]
        public string WsBaseUrl { get; set; }

        /// <summary>
        /// <para type="description">The Security Token Service WS-Trust issuer url! For example: "https://example.com/ISHSTS/issue/wstrust/mixed/username"</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ActiveDirectory-ExplicitIssuer")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "UserNamePassword-ExplicitIssuer")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "PSCredential-ExplicitIssuer")]
        [ValidateNotNullOrEmpty]
        public string WsTrustIssuerUrl { get; set; }

        /// <summary>
        /// <para type="description">The Security Token Service Metata Exchange Endpoint url! For example: "https://example.com/ISHSTS/issue/wstrust/mex"</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ActiveDirectory-ExplicitIssuer")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "UserNamePassword-ExplicitIssuer")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "PSCredential-ExplicitIssuer")]
        [ValidateNotNullOrEmpty]
        public string WsTrustIssuerMexUrl { get; set; }

        /// <summary>
        /// <para type="description">Standard PowerShell Credential class</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "PSCredential")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "PSCredential-ExplicitIssuer")]
        [ValidateNotNullOrEmpty]
        [Credential]
        public PSCredential PSCredential { get; set; }

        /// <summary>
        /// <para type="description">Username to login into SDL Tridion Docs Content Manager</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "UserNamePassword")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "UserNamePassword-ExplicitIssuer")]
        [ValidateNotNullOrEmpty]
        public string IshUserName
        {
            get { return _ishUserName; }
            set { _ishUserName = value; }
        }

        /// <summary>
        /// <para type="description">Password to login into SDL Tridion Docs Content Manager</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "UserNamePassword")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "UserNamePassword-ExplicitIssuer")]
        [ValidateNotNullOrEmpty]
        public string IshPassword
        {
            get { return _ishPassword; }
            set { _ishPassword = value; }
        }

        /// <summary>
        /// <para type="description">Timeout value expressed as TimeSpan, that controls Send/Receive timeouts of HttpClient when downloading content like connectionconfiguration.xml Defaults to 20 seconds.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        public TimeSpan Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        /// <summary>
        /// <para type="description">Timeout value expressed as TimeSpan, that controls Send/Receive timeouts of WCF when issuing a token. Defaults to maximum timespan.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        public TimeSpan TimeoutIssue
        {
            get { return _timeoutIssue; }
            set { _timeoutIssue = value; }
        }

        /// <summary>
        /// <para type="description">Timeout value expressed as TimeSpan, that controls Send/Receive timeouts of WCF for ISHWS proxies. Defaults to maximum timespan.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        public TimeSpan TimeoutService
        {
            get { return _timeoutService; }
            set { _timeoutService = value; }
        }

        /// <summary>
        /// <para type="description">IgnoreSslPolicyErrors presence indicates that a custom callback will be assigned to ServicePointManager.ServerCertificateValidationCallback. Defaults false of course, as this is creates security holes! But very handy for Fiddler usage though.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        public SwitchParameter IgnoreSslPolicyErrors
        {
            get { return _ignoreSslPolicyErrors; }
            set { _ignoreSslPolicyErrors = value; }
        }

        #region Private fields 
        private string _ishUserName = null;
        private string _ishPassword = null;
        private SecureString _ishSecurePassword = null;
        private TimeSpan _timeout = new TimeSpan(0, 0, 20);  // up to 15s for a DNS lookup according to https://msdn.microsoft.com/en-us/library/system.net.http.httpclient.timeout%28v=vs.110%29.aspx
        private TimeSpan _timeoutIssue = TimeSpan.MaxValue;
        private TimeSpan _timeoutService = TimeSpan.MaxValue;
        private bool _ignoreSslPolicyErrors = false;

        #endregion
        protected override void ProcessRecord()
        {
            try
            {
                int ishPasswordLength = _ishPassword == null ? 0 : _ishPassword.Length;
                if (PSCredential != null)
                {
                    _ishUserName = PSCredential.UserName;
                    _ishSecurePassword = PSCredential.Password;
                }
                else if (!String.IsNullOrWhiteSpace(_ishPassword))
                {
                    _ishSecurePassword = SecureStringConversions.StringToSecureString(_ishPassword);
                }
                else
                {
                    _ishSecurePassword = null;
                }

                WriteVerbose($"Connecting to WsBaseUrl[{WsBaseUrl}] IshUserName[{_ishUserName}] IshPassword[" + new string('*', ishPasswordLength) + "]");
                WriteDebug($"Connecting to WsBaseUrl[{WsBaseUrl}] IshUserName[{_ishUserName}] IshPassword[" + new string('*', ishPasswordLength) + $"] Timeout[{_timeout}] TimeoutIssue[{_timeoutIssue}] TimeoutService[{_timeoutService}] IgnoreSslPolicyErrors[{_ignoreSslPolicyErrors}]");
                IshSession ishSession = null;
                if (this.ParameterSetName.EndsWith("-ExplicitIssuer"))
                {
                    WriteDebug($"Connecting to WsBaseUrl[{WsBaseUrl}] WsTrustIssuerUrl[{WsTrustIssuerUrl}] WsTrustIssuerMexUrl[{WsTrustIssuerMexUrl}]");
                    ishSession = new IshSession(Logger, WsBaseUrl, WsTrustIssuerUrl, WsTrustIssuerMexUrl, _ishUserName, _ishSecurePassword, _timeout, _timeoutIssue, _timeoutService, _ignoreSslPolicyErrors);
                }
                else
                {
                    ishSession = new IshSession(Logger, WsBaseUrl, _ishUserName, _ishSecurePassword, _timeout, _timeoutIssue, _timeoutService, _ignoreSslPolicyErrors);
                }

                // Do early load of IshTypeFieldSetup (either <13-TriDKXmlSetup-based or >=13-RetrieveFieldSetupByIshType-API-based) for
                // usage by ToIshMetadataFields/.../ToIshRequestedMetadataFields and Expand-ISHParameter.ps1 parameter autocompletion
                var ishTypeFieldSetup = ishSession.IshTypeFieldSetup;

                WriteObject(ishSession);
            }
            catch (NotSupportedException notSupportedException)
            {
                WriteError(new ErrorRecord(notSupportedException, "-1", ErrorCategory.InvalidOperation, null));
            }
            catch (TrisoftAutomationException trisoftAutomationException)
            {
                ThrowTerminatingError(new ErrorRecord(trisoftAutomationException, base.GetType().Name, ErrorCategory.InvalidOperation, null));
            }
            catch (AggregateException aggregateException)
            {
                var flattenedAggregateException = aggregateException.Flatten();
                WriteVerbose(flattenedAggregateException.ToString());
                ThrowTerminatingError(new ErrorRecord(flattenedAggregateException, base.GetType().Name, ErrorCategory.NotSpecified, null));
            }
            catch (Exception exception)
            {
                ThrowTerminatingError(new ErrorRecord(exception, base.GetType().Name, ErrorCategory.NotSpecified, null));
            }
        }
    }
}
