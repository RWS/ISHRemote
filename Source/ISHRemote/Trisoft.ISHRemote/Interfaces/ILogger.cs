/*
* Copyright © 2014 All Rights Reserved by the RWS Group for and on behalf of its affiliates and subsidiaries.
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

namespace Trisoft.ISHRemote.Interfaces
{
    /// <summary>
    /// Represents logging functionality.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Writes verbose message.
        /// </summary>
        /// <param name="message">Verbose message.</param>
        void WriteVerbose(string message);

        /// <summary>
        /// Reports progress.
        /// </summary>
        /// <param name="activity">Activity that takes place.</param>
        /// <param name="statusDescription">Activity description.</param>
        /// <param name="percentComplete">Complete progress in percent equivalent.</param>
        void WriteProgress(string activity, string statusDescription, int percentComplete = -1);

        /// <summary>
        /// Reports parent progress.
        /// </summary>
        /// <param name="activity">Activity that takes place.</param>
        /// <param name="statusDescription">Activity description.</param>
        /// <param name="percentComplete">Complete progress in percent equivalent.</param>
        void WriteParentProgress(string activity, string statusDescription, int percentComplete);

        /// <summary>
        /// Writes debug-useful information.
        /// </summary>
        /// <param name="message">Debug message.</param>
        void WriteDebug(string message);

        /// <summary>
        /// Writes warning message.
        /// </summary>
        /// <param name="message">Warning message.</param>
        void WriteWarning(string message);

        /// <summary>
        /// Writes non-terminating error.
        /// </summary>
        /// <param name="ex">Exception as a result of the error.</param>
        /// <param name="errorObject">Object that caused error.</param>
        void WriteError(Exception ex, object errorObject = null);
    }
}
