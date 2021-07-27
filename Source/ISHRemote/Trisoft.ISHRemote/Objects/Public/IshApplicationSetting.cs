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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Trisoft.ISHRemote.Objects.Public
{

    /// <summary>
    /// <para type="description">Holds a single result of the GetTimeZone call containing network timestamps</para>
    /// </summary>
    public class IshApplicationSetting
    {
        #region Constructor
        public IshApplicationSetting(DateTime startCall, DateTime endCall, string xmlSettings)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlSettings);
            XmlNode localDatetime = xmlDocument.SelectSingleNode("ishsettings/ishapplicationsettings/serverconfiguration/cultureinfo/datetimeinfo/localdatetime");
            XmlNode serverConfiguration = xmlDocument.SelectSingleNode("ishsettings/ishapplicationsettings/serverconfiguration");
            XmlNode appServer = xmlDocument.SelectSingleNode("ishsettings/ishapplicationsettings/timestamps/appserver");
            XmlNode dbServer = xmlDocument.SelectSingleNode("ishsettings/ishapplicationsettings/timestamps/dbserver");
            XmlNode timeZoneInfo = xmlDocument.SelectSingleNode("ishsettings/ishapplicationsettings/serverconfiguration/cultureinfo/datetimeinfo/localdatetime");
            Init(startCall, endCall, localDatetime, appServer, dbServer, serverConfiguration, timeZoneInfo);
        }

        private void Init(DateTime startCall, DateTime endCall, XmlNode localDateTime, XmlNode appServer, XmlNode dbServer, XmlNode serverConfiguration, XmlNode timeZoneInfo)
        {
            DateTime.TryParse(localDateTime.InnerText.ToString(), out _localDateTime);
            DateTime.TryParse(appServer.Attributes["start"].Value, out _appServerStartTime);
            DateTime.TryParse(appServer.Attributes["end"].Value, out _appServerEndTime);
            DateTime.TryParse(dbServer.Attributes["start"].Value, out _dbServerStartTime);
            DateTime.TryParse(dbServer.Attributes["end"].Value, out _dbServerEndTime);
            _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneInfo.Attributes["timezoneid"].Value);
            _timeZoneUtcOffset = timeZoneInfo.Attributes["timezoneutcoffset"].Value;
            Boolean.TryParse(timeZoneInfo.Attributes["timezoneisdaylightsavingtime"].Value, out _timeZoneIsdaylightsavingtime);
            _serverName = serverConfiguration.Attributes["servername"].Value;
            _startCallTime = startCall;
            _endCallTime = endCall;
        }

        #endregion

        #region Private properties

        private DateTime _localDateTime;
        private DateTime _appServerStartTime;
        private DateTime _appServerEndTime;
        private DateTime _dbServerStartTime;
        private DateTime _dbServerEndTime;
        private DateTime _startCallTime;
        private DateTime _endCallTime;
        private TimeZoneInfo _timeZoneInfo;
        private string _timeZoneUtcOffset;
        private bool _timeZoneIsdaylightsavingtime;
        private string _serverName;

        #endregion

        #region Public properties
        public double TimeElapsedAppServer
        {
            get
            {
                return _appServerEndTime.Subtract(_appServerStartTime).TotalMilliseconds;
            }
        }
        public double TimeElapsedDbServer
        {
            get
            {
                return _dbServerEndTime.Subtract(_dbServerStartTime).TotalMilliseconds;
            }
        }
        public double TimeElapsedWsCall
        {
            get
            {
                return _endCallTime.Subtract(_startCallTime).TotalMilliseconds;
            }
        }
        public TimeZoneInfo TimeZoneInfo
        {
            get
            {
                return _timeZoneInfo;
            }
        }

        public string TimeZoneDisplayName
        {
            get
            {
                return _timeZoneInfo.DisplayName;
            }
        }

        public string TimeZoneUtcOffset
        {
            get
            {
                return _timeZoneUtcOffset;
            }
        }

        public bool TimeZoneIsdaylightsavingtime
        {
            get
            {
                return _timeZoneIsdaylightsavingtime;
            }
        }

        public string AppServerComputerName
        {
            get
            {
                return _serverName;
            }
        }
        #endregion
    }
}
