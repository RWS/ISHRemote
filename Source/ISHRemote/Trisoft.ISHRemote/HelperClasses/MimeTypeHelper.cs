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
using System.IO;
using Microsoft.Win32;

namespace Trisoft.ISHRemote.HelperClasses
{
    /// <summary>
    /// Helper class that contains methods concerning mimetype's.
    /// </summary>
    internal static class MimeTypeHelper
    {   
        /// <summary>
        /// Function that returns the mimetype related to an extension.
        /// </summary>
        /// <param name="extension">The extension to look up.</param>
        /// <returns>A string with the mimetype defenition.</returns>
        internal static string GetMimeType(string extension)
        {
            string mimeType = "application/unknown";

            RegistryKey regKey = Registry.ClassesRoot.OpenSubKey(
                extension.ToLower()
                );

            if(regKey != null)
            {
                object contentType = regKey.GetValue("Content Type");

                if(contentType != null)
                    mimeType = contentType.ToString();
            }

            return mimeType;
        }
    }
}
