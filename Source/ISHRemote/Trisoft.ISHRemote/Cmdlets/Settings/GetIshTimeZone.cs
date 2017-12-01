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
using System.Management.Automation;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.Exceptions;

namespace Trisoft.ISHRemote.Cmdlets.Settings
{
    /// <summary>
    /// <para type="synopsis">This cmdlet returns the time zone information of the web/app server. This cmdlet can also be used to measure the latency between the client and the server.</para>
    /// <para type="description">This cmdlet returns the time zone information of the web/app server. This cmdlet can also be used to measure the latency between the client and the server. ClientElapsed - either Min, Average or Max - indicates the time taken to execute the web service call measured from the client. AppServerElapsed - either Min, Average or Max - indicates the time taken to execute the database query measured on the web/app server. DbServerElapsed - either Min, Average or Max - indicates the time taken to execute the database query measured on the database server. By performing multiple calls using the Count parameter you will get a better idea on variation on the calls. Client and app server network latency is calculated through: ClientElapsed - AppServerElapsed. App server and db server network latency is calculated through: AppServerElapsed - DbServerElapsed.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -PSCredential Admin
    /// Get-IshTimeZone -IshSession $ishSession 
    /// </code>
    /// <para>Creates an IshApplicationSetting structure some timings but especially holds the TimeZoneInfo of the targeted application server.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -PSCredential Admin
    /// Get-IshTimeZone -IshSession $ishSession -Count 2
    /// </code>
    /// <para>Creates an IshApplicationSettings structure holds the TimeZoneInfo of the targeted application server. But especially holds time difference between client requests, application-server requests and database-server requests.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "IshTimeZone", SupportsShouldProcess = false)]
    [OutputType(typeof(IshApplicationSetting), typeof(IshApplicationSettings))]
    public sealed class GetIshTimeZone : SettingsCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false), ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The amount off get timezone webservice call you wish to perform.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false), ValidateNotNullOrEmpty]
        public int Count { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                if (Count >= 1)
                {
                    IshApplicationSettings applicationSettings = new IshApplicationSettings();
                    for (int i = 0; i < Count; i++)
                    {
                        WriteProgress(new ProgressRecord(0, "Performing get time zone webservice call", string.Format("{0}/{1}", i, Count)));
                        string outXmlSettings = string.Empty;
                        DateTime startCall = DateTime.UtcNow;
                        outXmlSettings = IshSession.Settings25.GetTimeZone();
                        DateTime endCall = DateTime.UtcNow;
                        applicationSettings.Add(startCall, endCall, outXmlSettings);
                    }
                    WriteObject(applicationSettings);
                }
                else
                {
                    string outXmlSettings = string.Empty;
                    DateTime startCall = DateTime.UtcNow;
                    outXmlSettings = IshSession.Settings25.GetTimeZone();
                    DateTime endCall = DateTime.UtcNow;
                    WriteObject(new IshApplicationSetting(startCall, endCall, outXmlSettings));
                }
            }
            catch (TrisoftAutomationException trisoftAutomationException)
            {
                ThrowTerminatingError(new ErrorRecord(trisoftAutomationException, base.GetType().Name, ErrorCategory.InvalidOperation, null));
            }
            catch (Exception exception)
            {
                ThrowTerminatingError(new ErrorRecord(exception, base.GetType().Name, ErrorCategory.NotSpecified, null));
            }
        }
    }
}
