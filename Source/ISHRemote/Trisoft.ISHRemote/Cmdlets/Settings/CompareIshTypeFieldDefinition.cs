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
    /// <para type="synopsis">This cmdlet compares two IshTypeFieldDefinition sets.</para>
    /// <para type="description">This cmdlet compares two IshTypeFieldDefinition sets allowing system compares (or even a compare with the deprecated TriDKXmlSetup format). The result is an IshTypeFieldDefinition with a Compare property indicating equal, different, left only or right only.
    /// Note that fields FDESCRIPTION and FCHANGES can have either type String or LongText, you can consider this false positives - this will only affect field filter operators.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSessionA = New-IshSession -WsBaseUrl "https://example.com/InfoShareWSPRODUCATION/" -PSCredential "Admin"
    /// $ishSessionB = New-IshSession -WsBaseUrl "https://example.com/InfoShareWSTEST/" -PSCredential "Admin"
    /// Compare-IshTypeFieldDefinition -LeftIshSession $ishSessionA -RightIshSession $ishSessionB
    /// </code>
    /// <para>Compares incoming IshSession entries that are not equal, so indicating differences, left only and right only.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSessionA = New-IshSession -WsBaseUrl "https://example.com/InfoShareWSPRODUCATION/" -PSCredential "Admin"
    /// $ishSessionB = New-IshSession -WsBaseUrl "https://example.com/InfoShareWSTEST/" -PSCredential "Admin"
    /// Compare-IshTypeFieldDefinition -LeftIshSession $ishSessionA -RightIshSession $ishSessionB -ExcludeLeftUnique
    /// </code>
    /// <para>Compares incoming IshSession entries that are not equal, so indicating differences and right only changes compared to the $ishSessionA reference.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWSPRODUCATION/" -PSCredential "Admin"
    /// $ishTypeFieldDefinitions = Get-IshTypeFieldDefinition -TriDKXmlSetupFilePath $tempFilePath
    /// Compare-IshTypeFieldDefinition -LeftIshTypeFieldDefinition $ishTypeFieldDefinitions -RightIshSession $ishSession -ExcludeLeftUnique
    /// </code>
    /// <para>Compares provided reference TriDKXmlSetup export file with incoming IshSession and that lists differences and right only changes.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWSPRODUCATION/" -PSCredential "Admin"
    /// $ishTypeFieldDefinitions = Get-IshTypeFieldDefinition
    /// Compare-IshTypeFieldDefinition -LeftIshSession $ishSession -RightIshTypeFieldDefinition $ishTypeFieldDefinitions -IncludeIdentical -ExcludeDifferent
    /// </code>
    /// <para>Compares incoming IshSession and IshTypeFieldDefinitions (TriDKXmlSetup export file made available through Resource entry). The IncludeIdentical flag will also return matching rows, while the ExcludeDifferent flag will not return rows with differences.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWSPRODUCATION/" -PSCredential "Admin"
    /// $ishTypeFieldDefinitions = Get-IshTypeFieldDefinition
    /// Compare-IshTypeFieldDefinition -LeftIshTypeFieldDefinition $ishTypeFieldDefinitions -RightIshSession $ishSession -IncludeIdentical |
    /// Where-Object -Property Name -NotLike "FTEST*" |
    /// Out-GridView
    /// </code>
    /// <para>Compares reference IshTypeFieldDefinitions (TriDKXmlSetup export file made available through Resource entry) with incoming IshSession.
    /// The IncludeIdentical flag will also return matching rows, while the Where-Object clause filters out fields with a certain name. The PowerShell Out-GridView does a nice visual rendering in PowerShell ISE.</para>
    /// </example>
    [Cmdlet(VerbsData.Compare, "IshTypeFieldDefinition", SupportsShouldProcess = false)]
    [OutputType(typeof(IshTypeFieldDefinitionCompare))]
    public sealed class CompareIshTypeFieldDefinition : SettingsCmdlet
    {

        /// <summary>
        /// <para type="description">The reference object of type IshSession</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public IshSession LeftIshSession { get; set; }

        /// <summary>
        /// <para type="description">The reference object of type IshTypeFieldDefinition array</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public IshTypeFieldDefinition[] LeftIshTypeFieldDefinition { get; set; }

        /// <summary>
        /// <para type="description">The difference object of type IshSession</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public IshSession RightIshSession { get; set; }

        /// <summary>
        /// <para type="description">The difference object of type IshTypeFieldDefinition array</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public IshTypeFieldDefinition[] RightIshTypeFieldDefinition { get; set; }

        /// <summary>
        /// <para type="description">Display characteristics of compared objects that are equal. By default, only characteristics that differ between the left and right objects are displayed.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        public SwitchParameter IncludeIdentical { get; set; }

        /// <summary>
        /// <para type="description">Stop displaying characteristics of compared objects that are different.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        public SwitchParameter ExcludeDifferent { get; set; }

        /// <summary>
        /// <para type="description">Stop displaying characteristics of compared objects that are unique in the left reference object.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        public SwitchParameter ExcludeLeftUnique { get; set; }

        /// <summary>
        /// <para type="description">Stop displaying characteristics of compared objects that are unique in the right difference object.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        public SwitchParameter ExcludeRightUnique { get; set; }

        private IshTypeFieldSetup _leftIshTypeFieldSetup;
        private IshTypeFieldSetup _rightIshTypeFieldSetup;
        private SortedDictionary<string, IshTypeFieldDefinition> _sortedSuperList;

        protected override void ProcessRecord()
        {
            try
            {
                if (LeftIshSession != null)
                {
                    _leftIshTypeFieldSetup = new IshTypeFieldSetup(Logger, LeftIshSession.IshTypeFieldDefinition);
                }
                else if (LeftIshTypeFieldDefinition != null)
                {
                    _leftIshTypeFieldSetup = new IshTypeFieldSetup(Logger, LeftIshTypeFieldDefinition.ToList<IshTypeFieldDefinition>());
                }
                else
                {
                    throw new ArgumentException($"Missing incoming Left... parameter.");
                }
                WriteDebug($"Comparing _leftIshTypeFieldSetup[{_leftIshTypeFieldSetup.IshTypeFieldDefinition.Count}]");

                if (RightIshSession != null)
                {
                    _rightIshTypeFieldSetup = new IshTypeFieldSetup(Logger, RightIshSession.IshTypeFieldDefinition);
                }
                else if (RightIshTypeFieldDefinition != null)
                {
                    _rightIshTypeFieldSetup = new IshTypeFieldSetup(Logger, RightIshTypeFieldDefinition.ToList<IshTypeFieldDefinition>());
                }
                else
                {
                    throw new ArgumentException($"Missing incoming Right... parameter.");
                }
                WriteDebug($"Comparing _rightIshTypeFieldSetup[{_rightIshTypeFieldSetup.IshTypeFieldDefinition.Count}]");

                Compare();
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

        private void Compare()
        {
            // created on sorted superlist of ISHType/Level/FieldName so that compares are happening in a predictable order
            CreateSuperSortedList();
            foreach (string key in _sortedSuperList.Keys)
            {
                IshTypeFieldDefinition left = _leftIshTypeFieldSetup.GetValue(key);
                IshTypeFieldDefinition right = _rightIshTypeFieldSetup.GetValue(key);
                if ((left != null) && (right != null))
                {
                    int compareResult = left.CompareTo(right);
                    if ((compareResult == 0) && IncludeIdentical)
                    {
                        WriteObject(new IshTypeFieldDefinitionCompare(left, IshTypeFieldDefinitionCompare.Compare.Identical));
                    }
                    if ((compareResult != 0) && !ExcludeDifferent)
                    {
                        WriteObject(new IshTypeFieldDefinitionCompare(left, IshTypeFieldDefinitionCompare.Compare.LeftDifferent));
                        WriteObject(new IshTypeFieldDefinitionCompare(right, IshTypeFieldDefinitionCompare.Compare.RightDifferent));
                    }
                }
                else if ((left != null) && !ExcludeLeftUnique)
                {
                    WriteObject(new IshTypeFieldDefinitionCompare(left, IshTypeFieldDefinitionCompare.Compare.LeftOnly));
                }
                else if ((right != null) && !ExcludeRightUnique)
                {
                    WriteObject(new IshTypeFieldDefinitionCompare(right, IshTypeFieldDefinitionCompare.Compare.RightOnly));
                }
            }
            // for each entry in the superlist
            //   lookupLeft and lookupRight, and promote each to IshTypeFieldDefinitionCompare (with left or right enum)
            //   if (lookupLeft != null && lookupRight != null)
            //     CompareIshTypeFieldDefinition()
            //     if CompareIshTypeFieldDefinition() == true && IncludeIdentical
            //       WriteObject with equal enum
            //     if CompareIshTypeFieldDefinition() == false && !ExcludeDifferent
            //       WriteObject with diff enum
            //   else if (lookupLeft != null && !ExcludeLeftUnique)
            //       WriteObject with left enum
            //   else if (lookupRight != null && !ExcludeRightUnique)
            //       WriteObject with right enum
            //   else
            //      WriteDebug($"Comparing , in the sorted list but lookups fails.")
        }

        private void CreateSuperSortedList()
        {
            _sortedSuperList = new SortedDictionary<string, IshTypeFieldDefinition>();
            foreach (var entry in _leftIshTypeFieldSetup.IshTypeFieldDefinition)
            {
                _sortedSuperList.Add(entry.Key, entry);
            }
            foreach (var entry in _rightIshTypeFieldSetup.IshTypeFieldDefinition)
            {
                if (!_sortedSuperList.ContainsKey(entry.Key))
                {
                    _sortedSuperList.Add(entry.Key, entry);
                }
            }
            WriteDebug($"Comparing _sortedSuperList[{_sortedSuperList.Keys.Count}]");
        }
    }
}
