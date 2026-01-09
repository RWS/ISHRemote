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
using System.Management.Automation;
using System.ServiceModel;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.Cmdlets.Annotation
{
    /// <summary>
    /// <para type="synopsis">The Find-IshAnnotation cmdlet finds annotations using MetadataFilter</para>
    /// <para type="description">The Find-IshAnnotation cmdlet finds annotations using MetadataFilter</para>
    /// </summary>
    /// <example>
    /// <code>
    /// New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -PSCredential username
    /// $ishAnnotations = Find-IshAnnotation
    /// </code>
    /// <para>Find all annotations, beware that large results could be requested here</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// $requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHANNOTATIONREPLIES" -Level Annotation
    /// $metadataFilter = Set-IshMetadataFilterField -IshSession $ishSession -Name FISHANNOTATIONTEXT -Level Annotation -FilterOperator Like -Value "Test%"
    /// $ishAnnotations = Find-IshAnnotation -IshSession $ishsession `
    ///                                    -RequestedMetadata $requestedMetadata `
    ///                                    -MetadataFilter $metadataFilter
    /// </code>
    /// <para>Find annotations providing RequestedMetadata and MetadataFilter</para>
    /// </example>

    [Cmdlet(VerbsCommon.Find, "IshAnnotation", SupportsShouldProcess = false)]
    [OutputType(typeof(IshAnnotation))]
    public sealed class FindIshAnnotation : AnnotationCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }


        /// <summary>
        /// <para type="description">The metadata filter with the filter fields to limit the amount of annotations returned. Default is no filtering.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public IshField[] MetadataFilter { get; set; }

        /// <summary>
        /// <para type="description">The requested metadata fields to retrieve</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public IshField[] RequestedMetadata { get; set; }

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateGlobalIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession} or in turn SessionState.{ISHRemoteSessionStateGlobalIshSession}");

            if ((IshSession.ServerIshVersion.MajorVersion < 14) || ((IshSession.ServerIshVersion.MajorVersion == 14) && (IshSession.ServerIshVersion.RevisionVersion < 1)))
            {
                throw new PlatformNotSupportedException($"Find-IshAnnotation requires server-side Annotation API which is only available starting from 14.0.1 and up. ServerIshVersion[{IshSession.ServerVersion}]");
            }
            base.BeginProcessing();
        }

        /// <summary>
        /// Process the Find-IshAnnotation commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <remarks>Writes <see cref="IshAnnotation"/> array to the pipeline</remarks>
        protected override void ProcessRecord()
        {
            try
            {
                //1. Initialize
                List<IshObject> returnedObjects = new List<IshObject>();
                IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, new IshFields(RequestedMetadata), Enumerations.ActionMode.Read);
                IshFields metadataFilter = new IshFields(MetadataFilter);

                //2. Call Find
                WriteDebug($"Finding MetadataFilter.length[{metadataFilter.ToXml().Length}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}]");
                string xmlIshObjects = IshSession.Annotation25.Find(metadataFilter.ToXml(), requestedMetadata.ToXml());
                IshObjects ishObjectsFound = new IshObjects(ISHType, xmlIshObjects);
                returnedObjects.AddRange(ishObjectsFound.Objects);
              
                //3. Write it
                WriteVerbose("returned object count[" + returnedObjects.Count + "]");
                WriteObject(IshSession, ISHType, returnedObjects.ConvertAll(x => (IshBaseObject)x), true);
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
            catch (TimeoutException timeoutException)
            {
                WriteVerbose("TimeoutException Message[" + timeoutException.Message + "] StackTrace[" + timeoutException.StackTrace + "]");
                ThrowTerminatingError(new ErrorRecord(timeoutException, base.GetType().Name, ErrorCategory.OperationTimeout, null));
            }
            catch (CommunicationException communicationException)
            {
                WriteVerbose("CommunicationException Message[" + communicationException.Message + "] StackTrace[" + communicationException.StackTrace + "]");
                ThrowTerminatingError(new ErrorRecord(communicationException, base.GetType().Name, ErrorCategory.OperationStopped, null));
            }
            catch (Exception exception)
            {
                ThrowTerminatingError(new ErrorRecord(exception, base.GetType().Name, ErrorCategory.NotSpecified, null));
            }
        }
    }
}
