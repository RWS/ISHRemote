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
using System.Management.Automation;
using System.ServiceModel;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.Cmdlets.Annotation
{
    /// <summary>
    /// <para type="synopsis">The Remove-IshAnnotation cmdlet removes annotation and all of its replies</para>
    /// <para type="description">The Remove-IshAnnotation cmdlet removes annotation and all of its replies</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// Remove-IshAnnotation -IshSession $ishsession -AnnotationId "MYANNOTATIONREF"
    /// </code>
    /// <para>Remove annotation providing AnnotationId. For the $ishAnnotation variable holding IshAnnotation object, AnnotationId is stored as property $ishAnnotation.IshRef</para>
    /// </example>
    /// <example>
    /// <code>
    /// New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -PSCredential username
    /// $ishAnnotations | Remove-IshAnnotation
    /// </code>
    /// <para>Remove annotations passing IshAnnotation array(or a single object) through the pipeline</para>
    /// </example>

    [Cmdlet(VerbsCommon.Remove, "IshAnnotation", SupportsShouldProcess = true)]
    public sealed class RemoveIshAnnotation : AnnotationCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParametersGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshAnnotationGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">AnnotationId</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParametersGroup")]
        [ValidateNotNullOrEmpty]
        public string AnnotationId { get; set; }
       
        /// <summary>
        /// <para type="description">The IshAnnotation array that needs to be deleted. Pipeline</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshAnnotationGroup")]
        public IshAnnotation[] IshAnnotation { get; set; }
        
        #region Private fields
        private readonly List<IshAnnotation> _retrievedIshAnnotations = new List<IshAnnotation>();
        #endregion

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateGlobalIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession} or in turn SessionState.{ISHRemoteSessionStateGlobalIshSession}");

            if ((IshSession.ServerIshVersion.MajorVersion < 14) || ((IshSession.ServerIshVersion.MajorVersion == 14) && (IshSession.ServerIshVersion.RevisionVersion < 1)))
            {
                throw new PlatformNotSupportedException($"Remove-IshAnnotation requires server-side Annotation API which is only available starting from 14.0.1 and up. ServerIshVersion[{IshSession.ServerVersion}]");
            }
            base.BeginProcessing();
        }

        /// <summary>
        /// Process the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                if (IshAnnotation != null)
                {
                    foreach (IshAnnotation ishAnnotation in IshAnnotation)
                    {
                        _retrievedIshAnnotations.Add(ishAnnotation);
                    }
                }
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

        /// <summary>
        /// Process the Remove-IshAnnotation commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        protected override void EndProcessing()
        {
            try
            {
                WriteDebug("Deleting");
                List<string> annotationIdsToDelete = new List<string>();

                //1. Prepare list of annotations to delete depending on the chosen ParameterSet
                switch(ParameterSetName)
                {
                    case "ParametersGroup":
                        annotationIdsToDelete.Add(AnnotationId);
                        break;
                    case "IshAnnotationGroup":
                        annotationIdsToDelete = _retrievedIshAnnotations.Select(ishAnnotation => Convert.ToString(ishAnnotation.IshRef)).ToList();
                        break;
                }

                //2. Delete required annotations
                //2.1 Delete replies first
                if (annotationIdsToDelete.Count > 0)
                {
                    // remove duplicates of AnnotationIds (IshRef of IshAnnotation object)
                    annotationIdsToDelete = annotationIdsToDelete.Distinct().ToList();

                    // find replies for the required annotation(s)
                    IshFields requestedMetadata = new IshFields();
                    List<IshObject> ishObjectsList = new List<IshObject>();
                    IshFields ishMetadataFilterField = new IshFields();
                    ishMetadataFilterField.AddField(new IshMetadataFilterField(FieldElements.AnnotationReplies, Enumerations.Level.Annotation, Enumerations.FilterOperator.NotEmpty, "", Enumerations.ValueType.Value));
                    requestedMetadata.AddField(new IshRequestedMetadataField(FieldElements.AnnotationText, Enumerations.Level.Reply, Enumerations.ValueType.Value));
                    string xmlIshAnnotationReplies = IshSession.Annotation25.RetrieveMetadata(annotationIdsToDelete.ToArray(), ishMetadataFilterField.ToXml(), requestedMetadata.ToXml());
                    ishObjectsList.AddRange(new IshObjects(ISHType, xmlIshAnnotationReplies).Objects);
                    foreach (var ishAnnotationReply in ishObjectsList.ConvertAll(x => (IshAnnotation)x))
                    {
                        string replyText = ishAnnotationReply.IshFields.GetFieldValue(FieldElements.AnnotationText, Enumerations.Level.Reply, Enumerations.ValueType.Value);
                        if (ShouldProcess(ishAnnotationReply.ReplyRef + " " + replyText))
                        {
                            IshSession.Annotation25.DeleteReply(Convert.ToInt64(ishAnnotationReply.ReplyRef));
                        }
                    }
                }
                
                //2.2 Delete all annotations
                int current = 0;
                foreach (string annotationId in annotationIdsToDelete)
                {
                    WriteDebug($"Id[{annotationId}] {++current}/{annotationIdsToDelete.Count}");
                    if (ShouldProcess(annotationId))
                    {
                        IshSession.Annotation25.Delete(annotationId);
                    }
                }

                WriteVerbose("returned object count[0]");
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
            finally
            {
                base.EndProcessing();
            }
        }
    }
}
