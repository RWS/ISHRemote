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

namespace Trisoft.ISHRemote.Objects.Public
{
    /// <summary>
    /// <para type="description">Stores a version number. It knows how to parse InfoShare version numbers regarding compatibility and build information</para>
    /// </summary>
    public class IshVersion
    {
        const char Separator = '.';
        int _majorVersion = 0;
        int _minorVersion = 0;
        int _buildVersion = 0;
        int _revisionVersion = 0;


        public IshVersion(string version)
        {
            string[] versionParts = version.Split(Separator);
            _majorVersion = (versionParts.Length >= 1) ? Convert.ToInt32(versionParts[0]) : 0;
            _minorVersion = (versionParts.Length >= 2) ? Convert.ToInt32(versionParts[1]) : 0;
            _buildVersion = (versionParts.Length >= 3) ? Convert.ToInt32(versionParts[2]) : 0;
            _revisionVersion = (versionParts.Length >= 4) ? Convert.ToInt32(versionParts[3]) : 0;
        }

        public override string ToString()
        {
            return _majorVersion.ToString() + Separator + _minorVersion.ToString() + Separator + _buildVersion.ToString() + Separator + _revisionVersion.ToString();
        }

        public int MajorVersion
        {
            get { return _majorVersion; }
        }

        public int MinorVersion
        {
            get { return _minorVersion; }
        }

        public int BuildVersion
        {
            get { return _buildVersion; }
        }

        public int RevisionVersion
        {
            get { return _revisionVersion; }
        }
    }
}
