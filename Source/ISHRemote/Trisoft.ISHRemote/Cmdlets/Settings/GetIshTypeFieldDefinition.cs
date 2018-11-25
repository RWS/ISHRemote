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
using System.IO;
using System.Linq;
using System.Text;
using System.Management.Automation;
using System.Xml;
using System.Xml.Linq;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.HelperClasses;

namespace Trisoft.ISHRemote.Cmdlets.Settings
{
    /// <summary>
    /// <para type="synopsis">This cmdlet retuns the system Type and Field definitions as IshTypeFieldDefinition objects.</para>
    /// <para type="description">This cmdlet will use Settings25.RetrieveFieldSetupByIshType when available. It can be used to load the 
    /// deprecated TriDKXmlSetup format into IshTypeFieldDefinition objects.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// Get-IshTypeFieldDefinition | Where-Object -Property ISHType -eq "ISHUser"
    /// </code>
    /// <para>Without IshSession the latest internal resource string will be loaded (based on Full-Export). The Where-Object allows filtering on chosen properties.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// Get-IshTypeFieldDefinition -IshSession $ishSession
    /// </code>
    /// <para>When ServerVersion -ge 13.0.0 the definition of your targeted IshSession is retrieved, otherwise the best matching internal resource string will be loaded (based on Full-Export).</para>
    /// </example>
    /// <example>
    /// <code>
    /// Get-IshTypeFieldDefinition -IshSession $ishSession |
    /// Where-Object -Property ISHType -eq ISHUser |
    /// Where-Object -Property AllowOnCreate -eq $true |
    /// Where-Object -Property IsMandatory -eq $true
    /// </code>
    /// <para>What are the fields I should pass when creating a new user through Add-IshUser.</para>
    /// </example>
    /// <example>
    /// <code>
    /// Get-IshTypeFieldDefinition -IshSession $ishSession | Out-GridView
    /// </code>
    /// <para>When using PowerShell ISE, you can list the result in a User Interface that even allows selection (through -OutputMode Multiple). The standard PowerShell grid view as extra filters and column sorting functionality.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "IshTypeFieldDefinition", SupportsShouldProcess = false)]
    [OutputType(typeof(IshTypeFieldDefinition))]
    public sealed class GetIshTypeFieldDefinition : SettingsCmdlet
    {

        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">File on the Windows filesystem where to load the TriDKXmlSetup full export from</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public string TriDKXmlSetupFilePath { get; set; }

        /// <summary>
        /// <para type="description">File on the Windows filesystem where to load the DBUT full export from. Where DBUT is the successor of TriDKXmlSetup.</para>
        /// </summary>
        // [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        // [ValidateNotNullOrEmpty]
        // public string DbutFilePath { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                WriteDebug("Importing");
                if (IshSession != null)
                {
                    WriteDebug($"Importing IshSession.ServerVersion[{IshSession.ServerVersion}]");
                    if (13 <= IshSession.ServerIshVersion.MajorVersion)
                    {
                        // when IshSession.ServerVersion >= 13.0.0 use Settings25.RetrieveFieldSetupByIshType
                        WriteVerbose($"Importing Settings25.RetrieveFieldSetupByIshType in IshSession.ServerVersion[{IshSession.ServerVersion}]");
                        IshSession.IshTypeFieldDefinition = new IshTypeFieldSetup(Logger, IshSession.Settings25.RetrieveFieldSetupByIshType(null)).IshTypeFieldDefinition;
                        WriteObject(IshSession.IshTypeFieldDefinition, true);
                        WriteVerbose($"returned object count[{IshSession.IshTypeFieldDefinition.Count()}]");
                    }
                    else
                    {
                        if (TriDKXmlSetupFilePath != null)
                        {
                            // only accept when IshSession.ServerVersion < 13.0.0
                            WriteVerbose($"Importing TriDKXmlSetupFilePath[{TriDKXmlSetupFilePath}] in IshSession.ServerVersion[{IshSession.ServerVersion}]");
                            var triDKXmlSetupHelper = new TriDKXmlSetupHelper(Logger, File.ReadAllText(TriDKXmlSetupFilePath));
                            IshSession.IshTypeFieldDefinition = new IshTypeFieldSetup(Logger, triDKXmlSetupHelper.IshTypeFieldDefinition).IshTypeFieldDefinition;
                            WriteObject(IshSession.IshTypeFieldDefinition, true);
                            WriteVerbose($"returned object count[{IshSession.IshTypeFieldDefinition.Count()}]");
                        }
                        else
                        {
                            // when IshSession.ServerVersion < 13.0.0 use the most appropriate Resources entry
                            WriteWarning($"Importing best match local resource entry for IshSession.ServerVersion[{IshSession.ServerVersion}]. Note that custom metadata fields will be missing, either use option -TriDKXmlSetupFilePath or upgrade to 13.0.0 where your setup can dynamically be retrieved using Settings25.RetrieveFieldSetupByIshType API call.");
                            var triDKXmlSetupHelper = new TriDKXmlSetupHelper(Logger, Properties.Resouces.ISHTypeFieldSetup.TriDKXmlSetupFullExport_12_00_01);
                            IshSession.IshTypeFieldDefinition = new IshTypeFieldSetup(Logger, triDKXmlSetupHelper.IshTypeFieldDefinition).IshTypeFieldDefinition;
                            WriteObject(IshSession.IshTypeFieldDefinition, true);
                            WriteVerbose($"returned object count[{IshSession.IshTypeFieldDefinition.Count()}]");
                        }
                    }
                }
                else
                {
                    if (TriDKXmlSetupFilePath != null)
                    {
                        // always allow? only when IshSession.ServerVersion < 13.0.0
                        WriteVerbose($"Importing TriDKXmlSetupFilePath[{TriDKXmlSetupFilePath}] without IshSession");
                        var triDKXmlSetupHelper = new TriDKXmlSetupHelper(Logger, File.ReadAllText(TriDKXmlSetupFilePath));
                        WriteObject(triDKXmlSetupHelper.IshTypeFieldDefinition, true);
                        WriteVerbose($"returned object count[{triDKXmlSetupHelper.IshTypeFieldDefinition.Count()}]");
                    }
                    else
                    {
                        WriteWarning($"Importing best match local resource entry without IshSession. Note that custom metadata fields will be missing, either use option -TriDKXmlSetupFilePath or upgrade to 13.0.0 where your setup can dynamically be retrieved using Settings25.RetrieveFieldSetupByIshType API call.");
                        var triDKXmlSetupHelper = new TriDKXmlSetupHelper(Logger, Properties.Resouces.ISHTypeFieldSetup.TriDKXmlSetupFullExport_12_00_01);
                        WriteObject(triDKXmlSetupHelper.IshTypeFieldDefinition, true);
                        WriteVerbose($"returned object count[{triDKXmlSetupHelper.IshTypeFieldDefinition.Count()}]");
                    }
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

