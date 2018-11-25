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
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.Exceptions;
using System.Threading;
using System.Xml;
using Trisoft.ISHRemote.Interfaces;
using Trisoft.ISHRemote.Folder25ServiceReference;

namespace Trisoft.ISHRemote.Cmdlets
{
    /// <summary>
    /// Which verb to use: check http://msdn.microsoft.com/en-us/library/ms714428(v=vs.85).aspx
    /// How to trace the pipelin: http://www.eggheadcafe.com/software/aspnet/31521302/valuefrompipelinebypropertyname.aspx
    /// External MAML help: http://cmille19.wordpress.com/2009/09/24/external-maml-help-files/
    ///                and: http://technet.microsoft.com/en-us/library/dd819489.aspx
    /// Progress Record: http://community.bartdesmet.net/blogs/bart/archive/2006/11/26/PowerShell-_2D00_-A-cmdlet-that-reports-progress-_2D00_-A-simple-file-downloader-cmdlet.aspx
    /// </summary>
    public abstract class TrisoftCmdlet : PSCmdlet
    {
        /// <summary>
        /// Sleep constant used to slow down the progress bars to check the messages.
        /// </summary>
        protected const int SleepTime = 0;

        public readonly ILogger Logger;

        private string _levelNameValueTypeSeparator = "--";     // consider removing and get from IshSession.NameHelper
        private int _tickCountStart;
        
        private ProgressRecord _parentProgressRecord;
        protected int _parentCurrent;
        protected int _parentTotal;
        private ProgressRecord _childProgressRecord;

        /// <summary>
        /// Name of the PSVariable so you don't have to specify '-IshSession $ishSession' anymore, should be set by New-IshSession
        /// </summary>
        internal const string ISHRemoteSessionStateIshSession = "ISHRemoteSessionStateIshSession";
        /// <summary>
        /// Error message you get when you didn't pass an explicit -IshSession on the cmdlet, or New-IshSession didn't set the SessionState variable
        /// </summary>
        internal const string ISHRemoteSessionStateIshSessionException = "IshSession is null. Please create a session first using New-IshSession.Or explicitly pass parameter -IshSession to your cmdlet.";

        /// <summary>
        /// Returns the PSObject NoteProperty separator to generate additional auxiliary properties
        /// </summary>
        public string LevelNameValueTypeSeparator
        {
            get { return _levelNameValueTypeSeparator; }
        }

        public TrisoftCmdlet()
        {
            Logger = TrisoftCmdletLogger.Instance();
            TrisoftCmdletLogger.Initialize(this);
            const int parentActivityId = 1111; //whatever number
            const int childActivityId = 2222;
            _parentProgressRecord = new ProgressRecord(parentActivityId, base.GetType().Name, "Processing...");
            _parentProgressRecord.SecondsRemaining = -1;
            _childProgressRecord = new ProgressRecord(childActivityId, base.GetType().Name + " subtask ", "Subprocessing...");
            _childProgressRecord.ParentActivityId = parentActivityId;
            _childProgressRecord.SecondsRemaining = -1;
        }


        protected override void BeginProcessing()
        {
            WriteDebug("BeginProcessing");
            _tickCountStart = Environment.TickCount;
            base.BeginProcessing();
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
            WriteDebug($"EndProcessing  elapsed:{(Environment.TickCount - _tickCountStart)}ms");
        }

        /// <summary>
        /// Write progress over the complete cmdlet, so over updates and retrieves
        /// </summary>
        /// <param name="message">What are you processing</param>
        /// <param name="current">Current step number</param>
        /// <param name="total">Total number of steps</param>
        public void WriteParentProgress(string message, int current, int total)
        {
            try
            {
                _parentTotal = total;
                if (current <= total)
                {
                    _parentCurrent = current;
                }
                else
                {
                    WriteDebug($"WriteParentProgress Corrected to 100% progress for incoming message[{message}] current[{current}] total[{total}]");
                    _parentCurrent = total;
                }
                _parentProgressRecord.PercentComplete = _parentCurrent * 100 / _parentTotal;
                _parentProgressRecord.StatusDescription = $"{message}  ({_parentCurrent}/{_parentTotal})";
                base.WriteProgress(_parentProgressRecord);
#if DEBUG
                Thread.Sleep(SleepTime);
#endif
            }
            catch (Exception exception)
            {
                WriteWarning(exception.Message);
            }
        }

        /// <summary>
        /// Write progress over the specific (webservice) calls required to process a list of ids/ishobjects
        /// </summary>
        /// <param name="message">What are you processing</param>
        /// <param name="current">Current step number</param>
        /// <param name="total">Total number of steps</param>
        public void WriteChildProgress(string message, int current, int total)
        {
            try
            {
                // Updating progress bar of the child
                _childProgressRecord.PercentComplete = current * 100 / total;
                _childProgressRecord.StatusDescription = message;
                WriteVerbose(message);
                // Skipping child write progress when current is '1', so only a progress when there is more to do
                // Alternative could be if current==total to avoid a lot progress records to scroll by
                if (current > 1)
                {
                    base.WriteProgress(_childProgressRecord);
                }
                // Updating progress bar of the parent
                _parentProgressRecord.PercentComplete = (int)((_parentCurrent * 100 / _parentTotal) + ((current * 100 / total) * (1.0 / _parentTotal)));
                base.WriteProgress(_parentProgressRecord);
#if DEBUG
                Thread.Sleep(SleepTime);
#endif
            }
            catch (Exception exception)
            {
                WriteWarning(exception.Message);
            }
        }

        /// <summary>
        /// Intentional override to include cmdlet origin
        /// </summary>
        public new void WriteVerbose(string message)
        {
            base.WriteVerbose(base.GetType().Name + "  " + message);
            WriteDebug(message);
        }

        /// <summary>
        /// Intentional override to include cmdlet origin and timestamp
        /// </summary>
        public new void WriteDebug(string message)
        {
            base.WriteDebug($"{base.GetType().Name} {DateTime.Now.ToString("yyyyMMdd.HHmmss.fff")} {message}");
        }

        /// <summary>
        /// Intentional override to include cmdlet origin 
        /// </summary>
        public new void WriteWarning(string message)
        {
            base.WriteWarning(base.GetType().Name + "  " + message);
        }

        /// <summary>
        /// Convert base folder label into a BaseFolder enumeration value
        /// </summary>
        /// <param name="ishSession">The <see cref="IshSession"/>.</param>
        /// <param name="baseFolderLabel">Label of the base folder</param>
        /// <returns>The BaseFolder enumeration value</returns>
        internal virtual Folder25ServiceReference.BaseFolder BaseFolderLabelToEnum(IshSession ishSession, string baseFolderLabel)
        {
            foreach (Folder25ServiceReference.BaseFolder baseFolder in System.Enum.GetValues(typeof(Folder25ServiceReference.BaseFolder)))
            {
                var folderLabel = BaseFolderEnumToLabel(ishSession, baseFolder);
                if (String.CompareOrdinal(folderLabel, baseFolderLabel) == 0)
                {
                    return baseFolder;
                }
            }

            // The baseFolder is wrong
            // EL: DIRTY WORKAROUND BELOW TO THROW AN EXCEPTION WITH ERROR CODE 102001
            string xmlIshFolder = ishSession.Folder25.GetMetadata(
                BaseFolder.System,
                new string[] { "'" + baseFolderLabel + "'" },  // Use faulty folder path with quotes added, so we can throw the expected exception with errorcode=102001
                "");

            return BaseFolder.Data;
        }

        /// <summary>
        /// Convert a BaseFolder enumeration value into a base folder label
        /// </summary>
        /// <param name="ishSession">Client session object to the InfoShare server instance. Keeps track of your security tokens and provide you clients to the various API end points. Holds matching contract parameters like seperators, batch and chunk sizes.</param>
        /// <param name="baseFolder">BaseFolder enumeration value</param>
        /// <returns>base folder label</returns>
        internal virtual string BaseFolderEnumToLabel(IshSession ishSession, Folder25ServiceReference.BaseFolder baseFolder)
        {
            IshFields requestedMetadata = new IshFields();
            requestedMetadata.AddField(new IshRequestedMetadataField("FNAME", Enumerations.Level.None, Enumerations.ValueType.All));
            string xmlIshFolder = ishSession.Folder25.GetMetadata(
                baseFolder,
                new string[] { },  // Use empty folder path so we can just get the basefolder name
                requestedMetadata.ToXml());
            XmlDocument result = new XmlDocument();
            result.LoadXml(xmlIshFolder);
            XmlElement xmlIshFolderElement = (XmlElement)result.SelectSingleNode("ishfolder");
            IshFolder ishFolder = new IshFolder(xmlIshFolderElement);
            return ((IshMetadataField)ishFolder.IshFields.RetrieveFirst("FNAME", Enumerations.Level.None, Enumerations.ValueType.Value)).Value;
        }


        /// <summary>
        /// Devides one list to multiple lists by batchsize
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="list">List to devide</param>
        /// <param name="batchSize"></param>
        /// <returns>Multiple lists, all having maximally batchsize elements</returns>
        internal List<List<T>> DevideListInBatches<T>(List<T> list, int batchSize)
        {
            var outList = new List<List<T>>();
            if (list != null)
            {
                for (int i = 0; i < list.Count; i += batchSize)
                {
                    outList.Add(list.Skip(i).Take(batchSize).ToList<T>());
                }
            }
            return outList;
        }

    }
}
