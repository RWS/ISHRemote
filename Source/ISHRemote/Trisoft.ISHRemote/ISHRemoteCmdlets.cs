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
using System.ComponentModel;
using System.Management.Automation;


namespace Trisoft.ISHRemote
{
    [RunInstaller(true)]
    public sealed class ISHRemoteCmdlets : PSSnapIn
    {
        public override string Name
        {
            get { return "ISHRemote"; }
        }

        /// <summary>Gets vendor of the snap-in.</summary>
        public override string Vendor
        {
            get { return "SDL Belgium NV"; }
        }

        /// <summary>Gets description of the snap-in. </summary>
        public override string Description
        {
            get { return "SDL ISHRemote Automation Cmdlets"; }
        }

        public override string[] Formats
        {
            get { return new string[] { "ISHRemote.Format.ps1xml" }; }
        }
    }
}
