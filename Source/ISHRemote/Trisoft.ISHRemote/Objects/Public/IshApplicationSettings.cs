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
    /// <para type="description">Container holding multiple results of GetTimeZone calls containing network timestamps allowing basic network statistics verification and calculation</para>
    /// </summary>
    public class IshApplicationSettings
    {
        private List<IshApplicationSetting> _applicationSettings;

        #region Constructors
        public IshApplicationSettings()
        {
            _applicationSettings = new List<IshApplicationSetting>();
        }
        #endregion

        #region Public methods
        public void Add(DateTime startCall, DateTime endCall, string xmlSettings)
        {
            _applicationSettings.Add(new IshApplicationSetting(startCall, endCall, xmlSettings));
        }
        #endregion

        #region Public properties
        // Web Service call length is the total IshSession.Settings25.GetTimeZone call length measured on the client side
        public string MinClientElapsed
        {
            get
            {
                return _applicationSettings.Select(x => x.TimeElapsedWsCall).Min() + " ms";
            }
        }
        public string AvgClientElapsed
        {
            get
            {
                return _applicationSettings.Select(x => x.TimeElapsedWsCall).Average() + " ms";
            }
        }
        public string MaxClientElapsed
        {
            get
            {
                return _applicationSettings.Select(x => x.TimeElapsedWsCall).Max() + " ms";
            }
        }

        // Application Server call length is the total time spend on the web/app server while executing IshSession.Settings25.GetTimeZone
        public string MinAppServerElapsed
        {
            get
            {
                return _applicationSettings.Select(x => x.TimeElapsedAppServer).Min() + " ms";
            }
        }
        public string AvgAppServerElapsed
        {
            get
            {
                return _applicationSettings.Select(x => x.TimeElapsedAppServer).Average() + " ms";
            }
        }
        public string MaxAppServerElapsed
        {
            get
            {
                return _applicationSettings.Select(x => x.TimeElapsedAppServer).Max() + " ms";
            }
        }

        // Database Server call length is the total time spend on the database server while executing IshSession.Settings25.GetTimeZone
        public string MinDbServerElapsed
        {
            get 
            {
                return _applicationSettings.Select(x => x.TimeElapsedDbServer).Min() + " ms"; 
            }
        }
        public string AvgDbServerElapsed
        {
            get
            {
                return _applicationSettings.Select(x => x.TimeElapsedDbServer).Average() + " ms";
            }
        }
        public string MaxDbServerElapsed
        {
            get
            {
                return _applicationSettings.Select(x => x.TimeElapsedDbServer).Max() + " ms";
            }
        }

        public string TimeZoneId
        {
            get
            {
                return _applicationSettings.First().TimeZoneInfo.Id;
            }
        }
        #endregion
    }
}
