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

namespace Trisoft.ISHRemote.Connection
{
    /// <summary>
    /// Container to hold and refresh your Access/Bearer tokens and more which are eventually pushed in the wire over an HttpClient class
    /// </summary>
    internal sealed class InfoShareOpenIdConnectTokens
    {
        /// <summary>
        /// Access Token is also known as Bearer Token
        /// </summary>
        internal string AccessToken { get; set; }
        internal string IdentityToken { get; set; }
        internal string RefreshToken { get; set; }
        internal DateTime AccessTokenExpiration { get; set; }
    }
}