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

using Trisoft.ISHRemote.Interfaces;
using System;
using System.Management.Automation;

namespace Trisoft.ISHRemote.Cmdlets
{
    /// <summary>
    /// Singleton proxy for single instance of <see cref="TrisoftCmdlet"/> class that provides logging functionality.
    /// </summary>
    /// <seealso cref="ILogger" />
    public sealed class TrisoftCmdletLogger : ILogger
    {
        /// <summary>
        /// The static instance of the <see cref="TrisoftCmdletLogger"/> class.
        /// </summary>
        private static readonly TrisoftCmdletLogger _instance = new TrisoftCmdletLogger();

        /// <summary>
        /// The instance of the <see cref="TrisoftCmdlet"/> class.
        /// </summary>
        private static TrisoftCmdlet _cmdlet;

        /// <summary>
        /// The progress activity identifier.
        /// </summary>
        private const int ProgressActivityId = 3333;

        /// <summary>
        /// The parent progress activity identifier.
        /// </summary>
        private const int ParentProgressActivityId = 4444;

        /// <summary>
        /// The progress record.
        /// </summary>
        private ProgressRecord _progressRecord;

        /// <summary>
        /// The parent progress record.
        /// </summary>
        private ProgressRecord _parentProgressRecord;

        /// <summary>
        /// Gets the instance of the <see cref="ILogger"/> instance.
        /// </summary>
        /// <returns>Instance of the <see cref="ILogger"/> interface.</returns>
        public static ILogger Instance()
        {
            return _instance;
        }

        /// <summary>
        /// Initializes the the instance of the <see cref="TrisoftCmdletLogger"/> with instance of <see cref="BaseCmdlet"/> class.
        /// </summary>
        /// <param name="cmdlet">The instance of the <see cref="TrisoftCmdlet"/> class.</param>
        public static void Initialize(TrisoftCmdlet cmdlet)
        {
            _cmdlet = cmdlet;
        }

        /// <summary>
        /// Writes message as Write-Host wrapper.
        /// </summary>
        /// <param name="message">Verbose message.</param>
        //public void WriteHostEmulation(string message)
        //{
        //    // !!!Warning, please use carefully.
        //    _cmdlet.SessionState.InvokeCommand.InvokeScript("Write-Host \"" + message + "\"");
        //}

        /// <summary>
        /// Reports progress.
        /// </summary>
        /// <param name="activity">Activity that takes place.</param>
        /// <param name="statusDescription">Current status description.</param>
        /// <param name="percentComplete">Complete progress in percent equivalent.</param>
        public void WriteProgress(string activity, string statusDescription, int percentComplete = -1)
        {
            if (_progressRecord == null)
            {
                _progressRecord = new ProgressRecord(ProgressActivityId, activity, statusDescription);
            }

            _progressRecord.ParentActivityId = (_parentProgressRecord != null) ? ParentProgressActivityId : -1;
            _progressRecord.Activity = activity;
            _progressRecord.StatusDescription = statusDescription;
            _progressRecord.PercentComplete = percentComplete;

            try
            {
                _cmdlet.WriteProgress(_progressRecord);
            }
            catch (PSInvalidOperationException exception)
            {
                //TODO [Should] Make logging work everywhere avoiding PSInvalidOperationException: The WriteObject and WriteError methods cannot be called from outside the overrides of the BeginProcessing, ProcessRecord, and EndProcessing
            }
        }

        /// <summary>
        /// Reports progress for parent progress bar.
        /// </summary>
        /// <param name="activity">Activity that takes place.</param>
        /// <param name="statusDescription">Status description.</param>
        /// <param name="percentComplete">Complete progress in percent equivalent.</param>
        public void WriteParentProgress(string activity, string statusDescription, int percentComplete)
        {
            if (_parentProgressRecord == null)
            {
                _parentProgressRecord = new ProgressRecord(ParentProgressActivityId, activity, statusDescription);
            }

            _parentProgressRecord.Activity = activity;
            _parentProgressRecord.StatusDescription = statusDescription;
            _parentProgressRecord.PercentComplete = percentComplete;

            try
            {
                _cmdlet.WriteProgress(_parentProgressRecord);
            }
            catch (PSInvalidOperationException exception)
            {
                //TODO [Should] Make logging work everywhere avoiding PSInvalidOperationException: The WriteObject and WriteError methods cannot be called from outside the overrides of the BeginProcessing, ProcessRecord, and EndProcessing
            }
        }


        /// <summary>
        /// Writes verbose message.
        /// </summary>
        /// <param name="message">Verbose message.</param>
        public void WriteVerbose(string message)
        {
            try
            {
                _cmdlet.WriteVerbose(message);
            }
            catch (PSInvalidOperationException exception)
            {
                //TODO [Should] Make logging work everywhere avoiding PSInvalidOperationException: The WriteObject and WriteError methods cannot be called from outside the overrides of the BeginProcessing, ProcessRecord, and EndProcessing
            }
        }
        
        /// <summary>
        /// Writes debug-useful information.
        /// </summary>
        /// <param name="message">Debug message.</param>
        public void WriteDebug(string message)
        {
            try
            {
                _cmdlet.WriteDebug(message);
            }
            catch (PSInvalidOperationException exception)
            {
                //TODO [Should] Make logging work everywhere avoiding PSInvalidOperationException: The WriteObject and WriteError methods cannot be called from outside the overrides of the BeginProcessing, ProcessRecord, and EndProcessing
            }
        }

        /// <summary>
        /// Writes warning message.
        /// </summary>
        /// <param name="message">Warning message.</param>
        public void WriteWarning(string message)
        {
            try
            {
                _cmdlet.WriteWarning(message);
            }
            catch (PSInvalidOperationException exception)
            {
                //TODO [Should] Make logging work everywhere avoiding PSInvalidOperationException: The WriteObject and WriteError methods cannot be called from outside the overrides of the BeginProcessing, ProcessRecord, and EndProcessing
            }
        }

        /// <summary>
        /// Writes non-terminating error.
        /// </summary>
        /// <param name="ex">Exception as a result of the error.</param>
        /// <param name="errorObject">Object that caused error.</param>
        public void WriteError(Exception ex, object errorObject = null)
        {
            try
            {
                _cmdlet.WriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.NotSpecified, errorObject));
            }
            catch (PSInvalidOperationException exception)
            {
                //TODO [Should] Make logging work everywhere avoiding PSInvalidOperationException: The WriteObject and WriteError methods cannot be called from outside the overrides of the BeginProcessing, ProcessRecord, and EndProcessing
            }
        }
    }
}
