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
using System.Threading.Tasks;
using System.Xml;
using Trisoft.ISHRemote.HelperClasses;
using Trisoft.ISHRemote.Interfaces;
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.Objects
{
    /// <summary>
    /// <para type="description">Provides functionality on IshTypeFieldDefinitions</para>
    /// </summary>
    internal class IshTypeFieldSetup
    {
        private readonly ILogger _logger;
        /// <summary>
        /// Lookup dictionary based on field identifier (like ISHType, Level, Name concatenation)
        /// </summary>
        private readonly SortedDictionary<string, IshTypeFieldDefinition> _ishTypeFieldDefinitions;
        /// <summary>
        /// Client side filtering of nonexisting or unallowed metadata can be done silently, with warning or not at all.
        /// </summary>
        private Enumerations.StrictMetadataPreference _strictMetadataPreference = Enumerations.StrictMetadataPreference.Continue;

        /// <summary>
        /// Creates a management object to work with the ISHType and FieldDefinitions.
        /// </summary>
        public IshTypeFieldSetup(ILogger logger, string xmlIshFieldSetup)
        {
            _logger = logger;
            _ishTypeFieldDefinitions = new SortedDictionary<string, IshTypeFieldDefinition>();
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlIshFieldSetup);
            foreach (XmlNode xmlIshTypeDefinition in xmlDocument.SelectNodes("ishfieldsetup/ishtypedefinition"))
            {
                string name = xmlIshTypeDefinition.Attributes.GetNamedItem("name").Value;

                Enumerations.ISHType ishType;
                if (Enum.TryParse(name, true, out ishType))
                {
                    _logger.WriteDebug($"IshTypeFieldSetup ishType[{ishType}]");
                    foreach (XmlNode xmlIshFieldDefinition in xmlIshTypeDefinition.SelectNodes("ishfielddefinition"))
                    {
                        IshTypeFieldDefinition ishTypeFieldDefinition =
                            new IshTypeFieldDefinition(_logger, ishType, (XmlElement) xmlIshFieldDefinition);
                        _ishTypeFieldDefinitions.Add(ishTypeFieldDefinition.Key, ishTypeFieldDefinition);
                    }
                }
                else
                {
                    _logger.WriteWarning($"IshType '{name}' is not supported");
                }
            }

            AddIshBackgroundTaskTableFieldSetup();
            AddIshEventTableFieldSetup();
            AddIshTranslationJobItemTableFieldSetup();
        }

        /// <summary>
        /// Returns a management object with sorting on the field definition key for comparison and nicer rendering
        /// </summary>
        public IshTypeFieldSetup(ILogger logger, List<IshTypeFieldDefinition> ishTypeFieldDefinitions)
        {
            _logger = logger;
            _ishTypeFieldDefinitions = new SortedDictionary<string, IshTypeFieldDefinition>();
            foreach (IshTypeFieldDefinition ishTypeFieldDefinition in ishTypeFieldDefinitions)
            {
                //Make sure the type, level (logical before version before lng), fieldname sorting is there
                _ishTypeFieldDefinitions.Add(ishTypeFieldDefinition.Key, ishTypeFieldDefinition);
            }
        }

        internal List<IshTypeFieldDefinition> IshTypeFieldDefinition
        {
            get
            {
                return _ishTypeFieldDefinitions.Values.ToList<IshTypeFieldDefinition>();
            }
        }

        public IshTypeFieldDefinition GetValue(string key)
        {
            IshTypeFieldDefinition ishTypeFieldDefinition;
            if (_ishTypeFieldDefinitions.TryGetValue(key, out ishTypeFieldDefinition))
            {
                return ishTypeFieldDefinition;
            }
            return null;
        }

        /// <summary>
        /// Client side filtering of nonexisting or unallowed metadata can be done silently, with warning or not at all. 
        /// </summary>
        public Enumerations.StrictMetadataPreference StrictMetadataPreference
        {
            get { return _strictMetadataPreference; }
            set { _strictMetadataPreference = value; }
        }

        #region Assist functions on Table (compared to Card) field setup
        private void AddIshBackgroundTaskTableFieldSetup()
        {
            _logger.WriteDebug($"IshTypeFieldSetup ishType[ISHBackgroundTask]");
            List<IshTypeFieldDefinition> ishTypeFieldDefinitions = new List<Public.IshTypeFieldDefinition>();
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, true, false, true, false, false, false, true, true, true, "TASKID", Enumerations.DataType.Number, "", "The column 'TASKID' contains the unique identifier of the background task. This value can only be used for filtering!"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, true, false, true, false, false, false, true, true, false, "USERID", Enumerations.DataType.ISHLov, "USERNAME", "The user that started the background task Note: For this field you can request the value (Admin) or the element name (VUSERADMIN)"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, true, false, true, false, false, false, true, true, false, "STATUS", Enumerations.DataType.ISHLov, "DBACKGROUNDTASKSTATUS", "The status of the background task Note: For this field you can request the value (Pending) or the element name (VBACKGROUNDTASKSTATUSPENDING)"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, false, false, true, false, false, false, true, false, false, "HASHID", Enumerations.DataType.String, "", "String containing the 'hash' representation for this background task. This will be used to skip older background task for the same action. For instance, synchronizing the same language object only once to SDL LiveContent."));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, true, false, true, false, false, false, true, true, false, "EVENTTYPE", Enumerations.DataType.ISHLov, "DEVENTTYPE", "The type of the event (e.g PUBLISH)"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, true, false, true, false, false, false, true, false, false, "PROGRESSID", Enumerations.DataType.Number, "", "The unique identifier of the event log linked with this background task"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, true, false, true, false, false, false, true, false, false, "TRACKINGID", Enumerations.DataType.Number, "", "Currently the trackingId is the same as the progressId"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, false, false, true, false, false, false, true, true, false, "CREATIONDATE", Enumerations.DataType.DateTime, "", "The date time that the background task was created"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, false, false, true, false, false, false, true, true, false, "MODIFICATIONDATE", Enumerations.DataType.DateTime, "", "The date time that the background task was last modified"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, false, false, true, false, false, false, true, true, false, "EXECUTEAFTERDATE", Enumerations.DataType.DateTime, "", "The background task should not be executed before this date time."));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, false, false, true, false, false, false, true, true, false, "LEASEDON", Enumerations.DataType.DateTime, "", "The date and time when background task was leased"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, false, false, true, false, false, false, true, true, false, "LEASEDBY", Enumerations.DataType.String, "", "The id of the process/thread that leased the background task"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, false, false, true, false, false, false, true, true, false, "CURRENTATTEMPT", Enumerations.DataType.Number, "", "Number containing the current attempt"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, false, false, true, false, false, false, true, false, false, "INPUTDATAID", Enumerations.DataType.Number, "", "Data reference linking to the inputdata of the background task"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, false, false, true, false, false, false, true, false, false, "OUTPUTDATAID", Enumerations.DataType.Number, "", "Data reference linking to the outputdata of the last execution of the background task"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.History, true, false, true, false, false, false, true, true, true, "HISTORYID", Enumerations.DataType.Number, "", "The column 'HISTORYID' contains the unique identifier of one of the history record of the background task. This value can only be used for filtering!"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.History, false, false, true, false, false, false, true, true, false, "EXITCODE", Enumerations.DataType.Number, "", "The exit code for this background task execution"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.History, false, false, true, false, false, false, true, true, false, "ERRORNUMBER", Enumerations.DataType.Number, "", "The error number thrown by this background task execution"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.History, false, false, true, false, false, false, true, false, false, "OUTPUT", Enumerations.DataType.Number, "", "Data reference linking to the outputdata of this background task execution"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.History, false, false, true, false, false, false, true, false, false, "ERROR", Enumerations.DataType.Number, "", "Data reference linking to the detailed error of this background task execution"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.History, false, false, true, false, false, false, true, true, false, "STARTDATE", Enumerations.DataType.DateTime, "", "The date time that this execution of the background task was started"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.History, false, false, true, false, false, false, true, true, false, "ENDDATE", Enumerations.DataType.DateTime, "", "The date time that this execution of the background task was finished"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.History, false, false, true, false, false, false, true, true, false, "HOSTNAME", Enumerations.DataType.String, "", "The host name from which the background task was created"));
            foreach (var ishTypeFieldDefinition in ishTypeFieldDefinitions)
            { 
                _ishTypeFieldDefinitions.Add(ishTypeFieldDefinition.Key, ishTypeFieldDefinition);
            }
        }

        private void AddIshEventTableFieldSetup()
        {
            _logger.WriteDebug($"IshTypeFieldSetup ishType[ISHEvent]");
            List<IshTypeFieldDefinition> ishTypeFieldDefinitions = new List<Public.IshTypeFieldDefinition>();
            //internal IshTypeFieldDefinition(ILogger logger, Enumerations.ISHType ishType, Enumerations.Level level,       bool isMandatory, bool isMultiValue, bool allowOnRead, bool allowOnCreate, bool allowOnUpdate, bool allowOnSearch, bool isSystem, bool isBasic, bool isDescriptive, string name, Enumerations.DataType dataType, string referenceLov, string description)
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHEvent, Enumerations.Level.Progress, false, false, true, false, false, false, true, true, true, "PROGRESSID", Enumerations.DataType.Number, "", "The column 'PROGRESSID' contains the unique identifier of the event. This value can only be used for filtering!"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHEvent, Enumerations.Level.Progress, false, false, true, false, false, false, true, true, true, "EVENTID", Enumerations.DataType.String, "", "The unique readable identifier of the event (e.g. CREATETRANSLATIONFROMLIST HOSTNAME01 20190321164905585 2115660297)"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHEvent, Enumerations.Level.Progress, false, false, true, false, false, false, true, true, false, "CREATIONDATE", Enumerations.DataType.DateTime, "", "The date time that the event was created"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHEvent, Enumerations.Level.Progress, false, false, true, false, false, false, true, true, false, "MODIFICATIONDATE", Enumerations.DataType.DateTime, "", "The date time that the event was last modified"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHEvent, Enumerations.Level.Progress, true, false, true, false, false, false, true, true, true, "EVENTTYPE", Enumerations.DataType.String, "", "The event type, typically directly linked to one of the background task message handlers. (e.g. CREATETRANSLATIONFROMLIST)"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHEvent, Enumerations.Level.Progress, true, false, true, false, false, false, true, true, false, "DESCRIPTION", Enumerations.DataType.String, "", "The free text description of the event. (e.g. Create translation for lngCardId[379780])"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHEvent, Enumerations.Level.Progress, false, false, true, false, true, false, true, true, false, "STATUS", Enumerations.DataType.String, "", "The actual or aggregated/calculated status of event. Values range from Busy, Success, Warning, Failed to Calculate."));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHEvent, Enumerations.Level.Progress, false, false, true, false, false, false, true, true, false, "USERID", Enumerations.DataType.ISHLov, "USERNAME", "The user that started the event"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHEvent, Enumerations.Level.Progress, false, false, true, false, false, false, true, false, false, "PARENTPROGRESSID", Enumerations.DataType.Number, "", "The column 'PARENTPROGRESSID' contains the unique identifier of the parent event. The system allows one-level hierarchical aggregation."));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHEvent, Enumerations.Level.Progress, false, false, true, false, true, false, true, true, false, "MAXIMUMPROGRESS", Enumerations.DataType.Number, "", "Number containing the expected maximum attempt. Could be a percentage indicated by 100, but also some total number."));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHEvent, Enumerations.Level.Progress, false, false, true, false, true, false, true, true, false, "CURRENTPROGRESS", Enumerations.DataType.Number, "", "Number containing the current attempt."));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHEvent, Enumerations.Level.Detail, true, false, true, false, false, false, true, true, true, "DETAILID", Enumerations.DataType.Number, "", "The column 'DETAILID' contains the unique identifier of one of the detail record of the event. This value can only be used for filtering!"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHEvent, Enumerations.Level.Detail, true, false, true, false, false, false, true, true, false, "PROGRESSID", Enumerations.DataType.Number, "", "This detail record belongs to this progress identifier."));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHEvent, Enumerations.Level.Detail, false, false, true, false, false, false, true, true, false, "CREATIONDATE", Enumerations.DataType.DateTime, "", "The date time that the event detail was created"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHEvent, Enumerations.Level.Detail, false, false, true, false, false, false, true, true, false, "HOSTNAME", Enumerations.DataType.String, "", "The host name from which the event detail was created"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHEvent, Enumerations.Level.Detail, true, false, true, false, false, false, true, true, false, "ACTION", Enumerations.DataType.String, "", "The short action name that is currently being executed"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHEvent, Enumerations.Level.Detail, true, false, true, false, false, false, true, true, false, "DESCRIPTION", Enumerations.DataType.String, "", "The longer description of the action that is currently being executed"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHEvent, Enumerations.Level.Detail, true, false, true, false, false, false, true, true, false, "STATUS", Enumerations.DataType.String, "", "The actual status of the event detail that is later aggregated/calculated on the progress level. Values range from Success, Warning to Failed."));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHEvent, Enumerations.Level.Detail, true, false, true, false, false, false, true, true, false, "EVENTLEVEL", Enumerations.DataType.Number, "", "The log level of the event detail, allows 'Verbose' filtering. Values range from Exception(10), Warning(20), Configuration(30), Information(40), Verbose(50) to Debug(60)"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHEvent, Enumerations.Level.Detail, false, false, true, false, false, false, true, false, false, "PROCESSID", Enumerations.DataType.Number, "", "The operating system process id on the system identified by hostname that submitted this event detail."));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHEvent, Enumerations.Level.Detail, false, false, true, false, false, false, true, false, false, "THREADID", Enumerations.DataType.Number, "", "The thread id within the operating system process id on the system identified by hostname that submitted this event detail."));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHEvent, Enumerations.Level.Detail, true, false, true, false, false, false, true, true, false, "EVENTDATATYPE", Enumerations.DataType.Number, "", "The event data type that indicates the content data type of the referenced blob under the same detailid. Values range from None(0), String(1), List(2), Xml(3), SendEventData(10), LogObject, StatusReport(21), CommandOutput(30) to Other(99)"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHEvent, Enumerations.Level.Detail, false, false, true, false, false, false, true, true, false, "EVENTDATASIZE", Enumerations.DataType.Number, "", "The event data size that indicates the content data size of the referenced blob under the same detailid."));
            foreach (var ishTypeFieldDefinition in ishTypeFieldDefinitions)
            {
                _ishTypeFieldDefinitions.Add(ishTypeFieldDefinition.Key, ishTypeFieldDefinition);
            }
        }

        private void AddIshTranslationJobItemTableFieldSetup()
        {
            _logger.WriteDebug($"TODO IshTypeFieldSetup ishType[ISHTranslationJobItem]");
            /*
            List<IshTypeFieldDefinition> ishTypeFieldDefinitions = new List<Public.IshTypeFieldDefinition>();
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, true, false, true, false, false, false, true, true, true, "TASKID", Enumerations.DataType.Number, "", "The column 'TASKID' contains the unique identifier of the background task. This value can only be used for filtering!"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, true, false, true, false, false, false, true, true, false, "USERID", Enumerations.DataType.ISHLov, "USERNAME", "The user that started the background task Note: For this field you can request the value ( Admin) or the element name ( VUSERADMIN)"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, true, false, true, false, false, false, true, true, false, "STATUS", Enumerations.DataType.ISHLov, "DBACKGROUNDTASKSTATUS", "The status of the background task Note: For this field you can request the value ( Pending) or the element name ( VBACKGROUNDTASKSTATUSPENDING)"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, false, false, true, false, false, false, true, false, false, "HASHID", Enumerations.DataType.String, "", "String containing the 'hash' representation for this background task. This will be used to skip older background task for the same action. For instance, synchronizing the same language object only once to SDL LiveContent."));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, true, false, true, false, false, false, true, true, false, "EVENTTYPE", Enumerations.DataType.ISHLov, "DEVENTTYPE", "The type of the event (e.g PUBLISH)"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, true, false, true, false, false, false, true, false, false, "PROGRESSID", Enumerations.DataType.Number, "", "The unique identifier of the event log linked with this background task"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, true, false, true, false, false, false, true, false, false, "TRACKINGID", Enumerations.DataType.Number, "", "Currently the trackingId is the same as the progressId"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, false, false, true, false, false, false, true, true, false, "CREATIONDATE", Enumerations.DataType.DateTime, "", "The date time that the background task was created"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, false, false, true, false, false, false, true, true, false, "MODIFICATIONDATE", Enumerations.DataType.DateTime, "", "The date time that the background task was last modified"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, false, false, true, false, false, false, true, true, false, "EXECUTEAFTERDATE", Enumerations.DataType.DateTime, "", "The background task should not be executed before this date time."));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, false, false, true, false, false, false, true, true, false, "LEASEDON", Enumerations.DataType.DateTime, "", "The date and time when background task was leased"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, false, false, true, false, false, false, true, true, false, "LEASEDBY", Enumerations.DataType.String, "", "The id of the process/thread that leased the background task"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, false, false, true, false, false, false, true, true, false, "CURRENTATTEMPT", Enumerations.DataType.Number, "", "Number containing the current attempt"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, false, false, true, false, false, false, true, false, false, "INPUTDATAID", Enumerations.DataType.Number, "", "Data reference linking to the inputdata of the background task"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.Task, false, false, true, false, false, false, true, false, false, "OUTPUTDATAID", Enumerations.DataType.Number, "", "Data reference linking to the outputdata of the last execution of the background task"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.History, true, false, true, false, false, false, true, true, true, "HISTORYID", Enumerations.DataType.Number, "", "The column 'HISTORYID' contains the unique identifier of one of the history record of the background task. This value can only be used for filtering!"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.History, false, false, true, false, false, false, true, true, false, "EXITCODE", Enumerations.DataType.Number, "", "The exit code for this background task execution"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.History, false, false, true, false, false, false, true, true, false, "ERRORNUMBER", Enumerations.DataType.Number, "", "The error number thrown by this background task execution"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.History, false, false, true, false, false, false, true, false, false, "OUTPUT", Enumerations.DataType.Number, "", "Data reference linking to the outputdata of this background task execution"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.History, false, false, true, false, false, false, true, false, false, "ERROR", Enumerations.DataType.Number, "", "Data reference linking to the detailed error of this background task execution"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.History, false, false, true, false, false, false, true, true, false, "STARTDATE", Enumerations.DataType.DateTime, "", "The date time that this execution of the background task was started"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.History, false, false, true, false, false, false, true, true, false, "ENDDATE", Enumerations.DataType.DateTime, "", "The date time that this execution of the background task was finished"));
            ishTypeFieldDefinitions.Add(new IshTypeFieldDefinition(_logger, Enumerations.ISHType.ISHBackgroundTask, Enumerations.Level.History, false, false, true, false, false, false, true, true, false, "HOSTNAME", Enumerations.DataType.String, "", "The host name from which the background task was created"));
            foreach (var ishTypeFieldDefinition in ishTypeFieldDefinitions)
            {
                _ishTypeFieldDefinitions.Add(ishTypeFieldDefinition.Key, ishTypeFieldDefinition);
            }
            */
        }
        #endregion


        #region Assist functions on allowed field usage based on IshFieldDefinition[]

        /// <summary>
        /// Remove IshField entries that by preferring ishvaluetype id over element over value
        /// </summary>
        private IshFields RemoveDuplicateFields(IshFields ishFields)
        {
            if (ishFields == null || (ishFields.Count() == 0))
            {
                return new IshFields();
            }
            // Initialize using the Id fields
            IshFields returnIshFields = new IshFields(ishFields.Fields().Where(f => f.ValueType == Enumerations.ValueType.Id).ToArray());
            // Add the fields having ishvaluetype Element, if not already specified as Id
            foreach (var ishField in ishFields.Fields().Where(f => f.ValueType == Enumerations.ValueType.Element))
            {
                if (! returnIshFields.Fields().Any(f => f.Name == ishField.Name && f.Level == ishField.Level))
                {
                    returnIshFields.AddField(ishField);
                }
            }
            // Add the fields having ishvaluetype Value, if not already specified as Id or Element
            foreach (var ishField in ishFields.Fields().Where(f => f.ValueType == Enumerations.ValueType.Value))
            {
                if (! returnIshFields.Fields().Any(f => f.Name == ishField.Name && f.Level == ishField.Level))
                {
                    returnIshFields.AddField(ishField);
                }
            }
            return returnIshFields;
        }

        /// <summary>
        /// Reverse lookup for all fields that are marked descriptive for the incoming ishTypes; then add them to ishFields.
        /// Descriptive fields are fields who are database-object-wise consired primary key, so to uniquely identify an object.
        /// </summary>
        private IshFields AddDescriptiveFields(Enumerations.ISHType[] ishTypes, IshFields ishFields)
        {
            foreach (Enumerations.ISHType ishType in ishTypes)
            {
                foreach (var ishTypeFieldDefinition in _ishTypeFieldDefinitions.Values.Where(d => d.ISHType == ishType && d.IsDescriptive == true && d.AllowOnRead == true))
                {
                    //TODO [Could] IshTypeFieldSetup adding descriptive fields potentially has an issue with removing too many ValueType entries
                    ishFields.AddOrUpdateField(new IshRequestedMetadataField(ishTypeFieldDefinition.Name, ishTypeFieldDefinition.Level, Enumerations.ValueType.Value), Enumerations.ActionMode.Read);
                    if (ishTypeFieldDefinition.DataType == Enumerations.DataType.ISHLov || ishTypeFieldDefinition.DataType == Enumerations.DataType.ISHType)
                    {
                        ishFields.AddOrUpdateField(new IshRequestedMetadataField(ishTypeFieldDefinition.Name, ishTypeFieldDefinition.Level, Enumerations.ValueType.Element), Enumerations.ActionMode.Read);
                    }
                }
            }
            return ishFields;
        }

        /// <summary>
        /// Reverse lookup for all fields that are marked basic for the incoming ishTypes; then add them to ishFields.
        /// Basic fields are fields who offer user-friendly business logic.
        /// </summary>
        private IshFields AddBasicFields(Enumerations.ISHType[] ishTypes, IshFields ishFields)
        {
            foreach (Enumerations.ISHType ishType in ishTypes)
            {
                foreach (var ishTypeFieldDefinition in _ishTypeFieldDefinitions.Values.Where(d => d.ISHType == ishType && d.IsBasic == true && d.AllowOnRead == true))
                {
                    //TODO [Could] IshTypeFieldSetup adding basic fields potentially has an issue with removing too many ValueType entries
                    ishFields.AddOrUpdateField(new IshRequestedMetadataField(ishTypeFieldDefinition.Name, ishTypeFieldDefinition.Level, Enumerations.ValueType.Value), Enumerations.ActionMode.Read);
                    if (ishTypeFieldDefinition.DataType == Enumerations.DataType.ISHLov || ishTypeFieldDefinition.DataType == Enumerations.DataType.ISHType)
                    {
                        ishFields.AddOrUpdateField(new IshRequestedMetadataField(ishTypeFieldDefinition.Name, ishTypeFieldDefinition.Level, Enumerations.ValueType.Element), Enumerations.ActionMode.Read);
                    }
                }
            }
            return ishFields;
        }

        /// <summary>
        /// Reverse lookup for all fields that are marked basic for the incoming ishTypes; then add them to ishFields.
        /// All fields of the specified object ISHType, so all descriptive, basic, system fields.
        /// </summary>
        private IshFields AddAllFields(Enumerations.ISHType[] ishTypes, IshFields ishFields)
        {
            foreach (Enumerations.ISHType ishType in ishTypes)
            {
                foreach (var ishTypeFieldDefinition in _ishTypeFieldDefinitions.Values.Where(d => d.ISHType == ishType && d.AllowOnRead == true))
                {
                    //TODO [Could] IshTypeFieldSetup adding basic fields potentially has an issue with removing too many ValueType entries
                    ishFields.AddOrUpdateField(new IshRequestedMetadataField(ishTypeFieldDefinition.Name, ishTypeFieldDefinition.Level, Enumerations.ValueType.Value), Enumerations.ActionMode.Read);
                    if (ishTypeFieldDefinition.DataType == Enumerations.DataType.ISHLov || ishTypeFieldDefinition.DataType == Enumerations.DataType.ISHType)
                    {
                        ishFields.AddOrUpdateField(new IshRequestedMetadataField(ishTypeFieldDefinition.Name, ishTypeFieldDefinition.Level, Enumerations.ValueType.Element), Enumerations.ActionMode.Read);
                    }
                }
            }
            return ishFields;
        }

        /// <summary>
        /// Returns the DataType of the specified field linked to the mentioned card type
        /// </summary>
        /// <param name="ishType">The card type</param>
        /// <param name="ishField">Field object indicating level and name</param>
        public Enumerations.DataType GetDataType(Enumerations.ISHType ishType, IshField ishField)
        {
            //_logger.WriteDebug($"GetDataType ISHType[{ishType}] Level[{ishField.Level}] Name[{ishField.Name}]");  //Remove slow logging
            IshTypeFieldDefinition ishTypeFieldDefinition = _ishTypeFieldDefinitions.Values.FirstOrDefault(d => d.ISHType == ishType && d.Level == ishField.Level && d.Name == ishField.Name);
            if (ishTypeFieldDefinition == null)
            {
                return Enumerations.DataType.String;
            }
            return ishTypeFieldDefinition.DataType;
        }

        /// <summary>
        /// Requested metadata fields will be duplicated and enriched to cater our Public Objects initilization.
        /// The minimal fields (IsDescriptive) will be added to allow initialization of the various IshObject types for all ValueTypes. 
        /// Unallowed fields for read operations (IshFieldDefinition.AllowOnRead) will be stripped with a Debug log message.
        /// </summary>
        /// <remarks>Logs debug entry for unknown combinations of ishTypes and ishField (name, level) entries - will not throw.</remarks>
        /// <param name="requestedMetadataGroup">Initialize return field set given the default/initial requested metadata, this way interactive mode becomes a lot less chatty</param>
        /// <param name="ishTypes">Given ISHTypes (like ISHMasterDoc, ISHLibrary, ISHConfiguration,...) to verify/alter the IshFields for</param>
        /// <param name="ishFields">Incoming IshFields entries will be transformed to matching and allowed IshRequestedMetadataField entries</param>
        /// <param name="actionMode">RequestedMetadataFields only has value for read operations like Read, Find, Search,...</param>
        public IshFields ToIshRequestedMetadataFields(Enumerations.RequestedMetadataGroup requestedMetadataGroup, Enumerations.ISHType[] ishTypes, IshFields ishFields, Enumerations.ActionMode actionMode)
        {
            IshFields requestedMetadataFields = new IshFields();

            // Preload RequestedMetadata by retrieving the selection specified by requestedMetadataGroup and ishTypes
            switch (requestedMetadataGroup)
            {
                case Enumerations.RequestedMetadataGroup.All:
                        requestedMetadataFields = AddAllFields(ishTypes, requestedMetadataFields);
                        break;
                case Enumerations.RequestedMetadataGroup.Basic:
                        requestedMetadataFields = AddBasicFields(ishTypes, requestedMetadataFields);
                        break;
                case Enumerations.RequestedMetadataGroup.Descriptive:
                    // always required, AddDescriptiveFields is called at the end of this function
                    break;
            }
            _logger.WriteDebug($"ToIshRequestedMetadataFields loaded RequestedMetadataGroup[{requestedMetadataGroup}] resulting in requestedMetadataFields.Count[{requestedMetadataFields.Count()}]");

            // AddOrUpdate with the specified RequestedMetadata
            foreach (Enumerations.ISHType ishType in ishTypes)
            {
                // Check incoming IshField with IshTypeFieldDefinitions
                foreach (IshField ishField in ishFields.Fields())
                {
                    var key = Enumerations.Key(ishType, ishField.Level, ishField.Name);
                    if (!_ishTypeFieldDefinitions.ContainsKey(key))
                    {
                        switch (_strictMetadataPreference)
                        {
                            case Enumerations.StrictMetadataPreference.SilentlyContinue:
                                _logger.WriteDebug($"ToIshRequestedMetadataFields skipping unknown ishType[{ishType}] level[{ishField.Level}] name[{ishField.Name}] valueType[{ishField.ValueType}]");
                                break;
                            case Enumerations.StrictMetadataPreference.Continue:
                                _logger.WriteVerbose($"ToIshRequestedMetadataFields skipping unknown ishType[{ishType}] level[{ishField.Level}] name[{ishField.Name}] valueType[{ishField.ValueType}]");
                                break;
                            case Enumerations.StrictMetadataPreference.Off:
                                requestedMetadataFields.AddField(ishField.ToRequestedMetadataField());
                                break;
                        }
                        continue; // move to next ishField
                    }
                    switch (actionMode)
                    {
                        case Enumerations.ActionMode.Read:
                        case Enumerations.ActionMode.Find:
                            if (!_ishTypeFieldDefinitions[key].AllowOnRead)
                            {
                                switch (_strictMetadataPreference)
                                {
                                    case Enumerations.StrictMetadataPreference.Continue:
                                        _logger.WriteVerbose($"ToIshRequestedMetadataFields AllowOnRead removed ishType[{ishType}] level[{ishField.Level}] name[{ishField.Name}] valueType[{ishField.ValueType}]");
                                        break;
                                    case Enumerations.StrictMetadataPreference.Off:
                                        requestedMetadataFields.AddField(ishField.ToRequestedMetadataField());
                                        break;
                                }
                            }
                            else
                            {
                                requestedMetadataFields.AddOrUpdateField(ishField.ToRequestedMetadataField(), actionMode);
                            }
                            break;
                        case Enumerations.ActionMode.Search:
                            if (!_ishTypeFieldDefinitions[key].AllowOnSearch)
                            {
                                switch (_strictMetadataPreference)
                                {
                                    case Enumerations.StrictMetadataPreference.Continue:
                                        _logger.WriteVerbose($"ToIshRequestedMetadataFields AllowOnSearch removed ishType[{ishType}] level[{ishField.Level}] name[{ishField.Name}] valueType[{ishField.ValueType}]");
                                        break;
                                    case Enumerations.StrictMetadataPreference.Off:
                                        requestedMetadataFields.AddField(ishField.ToRequestedMetadataField());
                                        break;
                                }
                            }
                            else
                            {
                                requestedMetadataFields.AddOrUpdateField(ishField.ToRequestedMetadataField(), actionMode);
                            }
                            break;
                        default:
                            _logger.WriteDebug($"ToIshRequestedMetadataFields called for actionMode[{actionMode}], skipping");
                            break;
                    }
                }
            }
            //Add IsDescriptive fields for the incoming IshType to allow basic descriptive/minimal object initialization
            requestedMetadataFields = AddDescriptiveFields(ishTypes, requestedMetadataFields);
            //TODO [Should] Merges in IsDescriptive for all ValueTypes (for LOV/Card)... we cannot do IMetadataBinding fields yet. Server-side they are retrieved anyway, so the only penalty is xml transfer size.
            return requestedMetadataFields;
        }

        /// <summary>
        /// Metadata write operations will be duplicated and cleared client side.
        /// Unallowed fields for write operations (IshFieldDefinition.AllowOnCreate/AllowOnUpdate) will be stripped with a Debug log message.
        /// </summary>
        /// <remarks>Logs debug entry for unknown combinations of ishTypes, ishField (name, level), mandatory, multi-value,... entries - will not throw.</remarks>
        /// <param name="ishTypes">Given ISHTypes (like ISHMasterDoc, ISHLibrary, ISHConfiguration,...) to verify/alter the IshFields for</param>
        /// <param name="ishFields">Incoming IshFields entries will be transformed to matching and allowed IshMetadataField entries</param>
        /// <param name="actionMode">MetadataFields only has value for write operations like Create, Update,...</param>
        public IshFields ToIshMetadataFields(Enumerations.ISHType[] ishTypes, IshFields ishFields, Enumerations.ActionMode actionMode)
        {
            IshFields metadataFields = new IshFields();
            foreach (Enumerations.ISHType ishType in ishTypes)
            {
                foreach (IshField ishField in ishFields.Fields())
                {
                    var key = Enumerations.Key(ishType, ishField.Level, ishField.Name);
                    // Any unknown field will be skipped, unless strict is Off
                    if (!_ishTypeFieldDefinitions.ContainsKey(key))
                    {
                        switch (_strictMetadataPreference)
                        {
                            case Enumerations.StrictMetadataPreference.SilentlyContinue:
                                _logger.WriteDebug($"ToIshMetadataFields skipping unknown ishType[{ishType}] level[{ishField.Level}] name[{ishField.Name}] valueType[{ishField.ValueType}]");
                                break;
                            case Enumerations.StrictMetadataPreference.Continue:
                                _logger.WriteVerbose($"ToIshMetadataFields skipping unknown ishType[{ishType}] level[{ishField.Level}] name[{ishField.Name}] valueType[{ishField.ValueType}]");
                                break;
                            case Enumerations.StrictMetadataPreference.Off:
                                metadataFields.AddOrUpdateField(ishField.ToMetadataField(), actionMode);
                                break;
                        }
                        continue; // move to next ishField
                    }
                    // Known field, however could be not allowed for current action
                    switch (actionMode)
                    {
                        case Enumerations.ActionMode.Create:
                            if (!_ishTypeFieldDefinitions[key].AllowOnCreate)
                            {
                                switch (_strictMetadataPreference)
                                {
                                    case Enumerations.StrictMetadataPreference.Continue:
                                        _logger.WriteVerbose($"ToIshMetadataFields AllowOnCreate removed ishType[{ishType}] level[{ishField.Level}] name[{ishField.Name}] valueType[{ishField.ValueType}]");
                                        break;
                                }
                            }
                            else
                            {
                                switch (_strictMetadataPreference)
                                {
                                    case Enumerations.StrictMetadataPreference.SilentlyContinue:
                                    case Enumerations.StrictMetadataPreference.Continue:
                                        // Doing an Add, as a later RemoveDuplicateFields can be smarter to keep the 'best' metadata field
                                        metadataFields.AddField(ishField.ToMetadataField());
                                        break;
                                    case Enumerations.StrictMetadataPreference.Off:
                                        // Doing an AddOrUpdate (Replace), so only one ValueType is left to avoid ambiguity on a Create/Update API call
                                        metadataFields.AddOrUpdateField(ishField.ToMetadataField(), actionMode);
                                        break;
                                }
                            }
                            break;
                        case Enumerations.ActionMode.Update:
                            if (!_ishTypeFieldDefinitions[key].AllowOnUpdate)
                            {
                                switch (_strictMetadataPreference)
                                {
                                    case Enumerations.StrictMetadataPreference.Continue:
                                        _logger.WriteVerbose($"ToIshMetadataFields AllowOnUpdate removed ishType[{ishType}] level[{ishField.Level}] name[{ishField.Name}]");
                                        break;
                                }
                            }
                            else
                            {
                                switch (_strictMetadataPreference)
                                {
                                    case Enumerations.StrictMetadataPreference.SilentlyContinue:
                                    case Enumerations.StrictMetadataPreference.Continue:
                                        // Doing an Add, as a later RemoveDuplicateFields can be smarter to keep the 'best' metadata field
                                        metadataFields.AddField(ishField.ToMetadataField());
                                        break;
                                    case Enumerations.StrictMetadataPreference.Off:
                                        // Doing an AddOrUpdate (Replace), so only one ValueType is left to avoid ambiguity on a Create/Update API call
                                        metadataFields.AddOrUpdateField(ishField.ToMetadataField(), actionMode);
                                        break;
                                }
                                //TODO [Should] IshTypeFieldSetup - Potential conflict if ishField having multiple ishvaluetype have conflicting entries for id/element/value
                                // 1. For IshBaseline the name field is a controlled field, so FISHDOCUMENTRELEASE linked to DDOCUMENTRELEASE.
                                //    Specifying the label FISHDOCUMENTRELEASE overrules the element name, because you are renaming
                                // 2. For IshDocumentObj the author field is a controlled field, so FAUTHOR linked to USER
                                //    Specifying the label "Admin" is less accurate than the element name "VUSERADMIN"
                                // Two cases to illustrate that is not easy to fix. Workaround is to do Set-* cmdlets by -Id and -Metadata instead of -IshObject holding the new values
                                // ==> For now solved by passing ActionMode to IshFields.AddOrUpdateField where for Create/Update all ValueTypes are removed, last one wins
                            }
                            break;
                        default:
                            _logger.WriteDebug($"ToIshMetadataFields called for actionMode[{actionMode}], skipping");
                            break;
                    }
                }
            }

            switch (_strictMetadataPreference)
            {
                case Enumerations.StrictMetadataPreference.SilentlyContinue:
                    _logger.WriteDebug($"Removing duplicate IshField entries");
                    metadataFields = RemoveDuplicateFields(metadataFields);
                    break;
                case Enumerations.StrictMetadataPreference.Continue:
                    _logger.WriteVerbose($"Removing duplicate IshField entries");
                    metadataFields = RemoveDuplicateFields(metadataFields);
                    break;
                case Enumerations.StrictMetadataPreference.Off:
                    // not removing duplicate entries potentially causing errors like: [-105007] The field "FISHUSERLANGUAGE" can only be specified once on level "None". [105007;FieldAlreadySpecified]
                    break;
            }
            return metadataFields;
        }

        /// <summary>
        /// Correctly named overload of ToIshMetadataFields(...). Allows tuning in the future.
        /// </summary>
        public IshFields ToIshRequiredCurrentMetadataFields(Enumerations.ISHType[] ishTypes, IshFields ishFields, Enumerations.ActionMode actionMode)
        {
            return ToIshMetadataFields(ishTypes, ishFields, actionMode);
        }
        #endregion
    }
}
