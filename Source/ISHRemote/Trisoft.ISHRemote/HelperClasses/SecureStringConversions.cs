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
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Trisoft.ISHRemote.HelperClasses
{
    public static class SecureStringConversions
    {
        public static SecureString StringToSecureString(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            char[] chars = value.ToCharArray();
            var ss = new SecureString();
            for (int i = 0; i < chars.Length; i++)
            {
                ss.AppendChar(chars[i]);
            }
            return ss;
        }

        public static String SecureStringToString(SecureString value)
        {
            if (value == null) return null;
            IntPtr bstr = Marshal.SecureStringToBSTR(value);
            try
            {
                return Marshal.PtrToStringBSTR(bstr);
            }
            finally
            {
                Marshal.FreeBSTR(bstr);
            }
        }
    }
}
