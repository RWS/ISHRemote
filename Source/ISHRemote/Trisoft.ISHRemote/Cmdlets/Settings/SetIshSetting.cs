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

namespace Trisoft.ISHRemote.Cmdlets.Settings
{
    /// <summary>
    /// <para type="synopsis">This cmdlet can be used to set a configuration setting in the repository. Depending on the parameters you use, the value to set is read from different inputs.
    /// If you provide:
    /// * A metadata parameter with the fields to set, all fields and values will be read from the IshFields and set
    /// * A fieldname and a value, the value will be set for the given field
    /// * A fieldname and a filepath, the value will be read from the file and set for the given field</para>
    /// <para type="description">This cmdlet can be used to set a configuration setting in the repository. Depending on the parameters you use, the value to set is read from different inputs.
    /// If you provide:
    /// * A metadata parameter with the fields to set, all fields and values will be read from the IshFields and set
    /// * A fieldname and a value, the value will be set for the given field
    /// * A fieldname and a filepath, the value will be read from the file and set for the given field</para>
    /// </summary>
    /// <example>
    /// <code>
    /// Param(
    ///     $wsBaseUrl = 'https://example.com/InfoShareWS/',
    ///     $userName = 'admin',
    ///     $password = 'admin',
    ///     $settingsFolderPath = 'D:\temp'
    /// )
    /// $ishSession = New-IshSession -WsBaseUrl $wsBaseUrl -IshUserName $userName -IshPassword $password
    /// Write-Verbose "Submitting Xml Settings from $settingsFolderPath"
    /// $filePath = Join-Path -Path $settingsFolderPath -ChildPath "Admin.XMLInboxConfiguration.xml"
    /// Set-IshSetting -IshSession $ishSession -FieldName "FINBOXCONFIGURATION" -FilePath $filePath
    /// $filePath = Join-Path -Path $settingsFolderPath -ChildPath "Admin.XMLBackgroundTaskConfiguration.xml"
    /// Set-IshSetting -IshSession $ishSession -FieldName "FISHBACKGROUNDTASKCONFIG" -FilePath $filePath
    /// $filePath = Join-Path -Path $settingsFolderPath -ChildPath "Admin.XMLChangeTrackerConfig.xml"
    /// Set-IshSetting -IshSession $ishSession -FieldName "FISHCHANGETRACKERCONFIG" -FilePath $filePath
    /// $filePath = Join-Path -Path $settingsFolderPath -ChildPath "Admin.XMLExtensionConfiguration.xml"
    /// Set-IshSetting -IshSession $ishSession -FieldName "FISHEXTENSIONCONFIG" -FilePath $filePath
    /// $filePath = Join-Path -Path $settingsFolderPath -ChildPath "Admin.XMLPluginConfig.xml"
    /// Set-IshSetting -IshSession $ishSession -FieldName "FISHPLUGINCONFIGXML" -FilePath $filePath
    /// # Version 13.0.0 requires a status to be present before status transitions can be confirmed
    /// # Add-IshLovValue -IshSession $ishSession -LovId DSTATUS -Label "Translation In Review" -Description "Translation In Review"
    /// $filePath = Join-Path -Path $settingsFolderPath -ChildPath "Admin.XMLStatusConfiguration.xml"
    /// Set-IshSetting -IshSession $ishSession -FieldName "FSTATECONFIGURATION" -FilePath $filePath
    /// $filePath = Join-Path -Path $settingsFolderPath -ChildPath "Admin.XMLTranslationConfiguration.xml"
    /// Set-IshSetting -IshSession $ishSession -FieldName "FTRANSLATIONCONFIGURATION" -FilePath $filePath
    /// $filePath = Join-Path -Path $settingsFolderPath -ChildPath "Admin.XMLWriteObjPluginConfig.xml"
    /// Set-IshSetting -IshSession $ishSession -FieldName "FISHWRITEOBJPLUGINCFG" -FilePath $filePath
    /// $filePath = Join-Path -Path $settingsFolderPath -ChildPath "Admin.XMLPublishPluginConfiguration.xml"
    /// Set-IshSetting -IshSession $ishSession -FieldName "FISHPUBLISHPLUGINCONFIG" -FilePath $filePath
    /// Write-Host "Done"
    /// </code>
    /// <para>Submit all Xml Settings configuration entries from your prepared folder (or standard EnterViaUI folder).</para>
    /// </example>
    /// <example>
    /// <code>
    /// New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -PSCredential "username"
    /// Set-IshSetting -FieldName FISHLCURI -Value https://something/deleteThis
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Update CTCONFIGURATION field with the presented value.</para>
    /// </example>
    /// <example>
    /// <code>
    /// New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -PSCredential "username"
    /// Set-IshSetting -FieldName FISHLCURI 
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Update CTCONFIGURATION field with the value empty string ("").</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "IshSetting", SupportsShouldProcess = true)]
    [OutputType(typeof(IshField))]
    public sealed class SetIshSetting : SettingsCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFieldsGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ValueGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FileGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The metadata with the fields and values to set</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFieldsGroup")]
        [ValidateNotNull]
        public IshField[] Metadata { get; set; }

        /// <summary>
        /// <para type="description">The settings field to set</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ValueGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "FileGroup")]
        [ValidateNotNullOrEmpty]
        public string FieldName { get; set; }

        /// <summary>
        /// <para type="description">The value to set</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ValueGroup")]
        [ValidateNotNullOrEmpty]
        public string Value { get; set; }

        /// <summary>
        /// <para type="description">File on the Windows filesystem where to read the setting from</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "FileGroup")]
        [ValidateNotNullOrEmpty]
        public string FilePath { get; set; }

        /// <summary>
        /// <para type="description">The required current metadata of the setting. This parameter can be used to avoid that users override changes done by other users. The cmdlet will check whether the fields provided in this parameter still have the same values in the repository:</para>
        /// <para type="description">If the metadata is still the same, the metadata will be set.</para>
        /// <para type="description">If the metadata is not the same anymore, an error is given and the metadata will be set.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFieldsGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ValueGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FileGroup")]
        [ValidateNotNullOrEmpty]
        public IshField[] RequiredCurrentMetadata { get; set; }

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            try
            {
                IshFields metadata = new IshFields();
                IshFields requiredCurrentMetadata = new IshFields(RequiredCurrentMetadata);

                // 1. Doing the update
                WriteDebug("Updating");
                if (Metadata != null)
                {
                    metadata = new IshFields(Metadata);
                }
                else if (FilePath != null)
                {
                    //Check file exists
                    if (!File.Exists(FilePath))
                    {
                        throw new FileNotFoundException(@"File '" + FilePath + "' does not exist.");
                    }
                    try
                    {
                        // Let's try to see if it is xml first
                        var doc = XDocument.Load(FilePath, LoadOptions.PreserveWhitespace);
                        // ToString does not keep xml declaration <?xml, but that does not really matter as we are passing it in as string anyway (and the API removed the xml desclaration anyway)
                        string value = doc.ToString(SaveOptions.DisableFormatting);
                        metadata.AddField(new IshMetadataField(FieldName, Enumerations.Level.None, Enumerations.ValueType.Value, value));
                    }
                    catch (Exception)
                    {
                        // Read it as a text file
                        string value = String.Join(IshSession.Separator, File.ReadAllLines(FilePath));
                        metadata.AddField(new IshMetadataField(FieldName, Enumerations.Level.None, Enumerations.ValueType.Value, value));
                    }
                }
                else
                {
                    Value = Value ?? "";  // if the value is not offered we presumse empty string ("")
                    metadata.AddField(new IshMetadataField(FieldName, Enumerations.Level.None, Enumerations.ValueType.Value, Value));
                }

                if (ShouldProcess("CTCONFIGURATION"))
                {
                    metadata = IshSession.IshTypeFieldSetup.ToIshMetadataFields(ISHType, metadata, Enumerations.ActionMode.Update);
                    requiredCurrentMetadata = IshSession.IshTypeFieldSetup.ToIshRequiredCurrentMetadataFields(ISHType, requiredCurrentMetadata, Enumerations.ActionMode.Update);
                    IshSession.Settings25.SetMetadata3(metadata.ToXml(), requiredCurrentMetadata.ToXml());
                }
                           
                // 2. Retrieve the updated material from the database and write it out
                WriteDebug("Retrieving");
                var requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, metadata, Enumerations.ActionMode.Read);
                string xmlIshObjects = IshSession.Settings25.GetMetadata(requestedMetadata.ToXml());
                var ishFieldArray = new IshObjects(xmlIshObjects).Objects[0].IshFields.Fields();

                // 3. Write it
                WriteVerbose("returned object count[1]");
                WriteObject(ishFieldArray, true);
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


