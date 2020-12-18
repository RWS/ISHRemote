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
using System.IO;
using System.Xml;

namespace Trisoft.ISHRemote.Objects.Public
{
    /// <summary>
    /// <para type="description">The IshEventData object is used to hold Event binary information.</para>
    /// </summary>
    public class IshEventData
    {
        private long _size = 0;
        private byte[] _byteArray = null;

        /// <summary>
        /// Creates a new instance of the <see cref="IshEventData"/> class.
        /// </summary>
        /// <param name="content">The content for the event data</param>
        public IshEventData(string content)
        {
            if (content != null)
            {
                _size = content.Length;
                // initialize the byte array
                _byteArray = new byte[_size * sizeof(char)];
                System.Buffer.BlockCopy(content.ToCharArray(), 0, _byteArray, 0, _byteArray.Length);
            }
        }

        /// <summary>
        /// Initializing by something that looks like  <ishdata size="70500"><![CDATA[RQB4AGkAdABDAG8AZABlADoAIAAtADIAMAAwADAAMwANAAoATQBpAGMAcgBvAHMAbwBmAHQAIAAoAFIAKQAgAF ...	]]></ishdata> for IshEvent
        /// </summary>
        public IshEventData(XmlElement ishEventData)
        {
            if (ishEventData != null)
            {
                _size =Convert.ToInt64( ishEventData.Attributes["size"].Value);
                _byteArray = System.Convert.FromBase64String(ishEventData.InnerText);
            }
        }

        /// <summary>
        /// Gets the Size.
        /// </summary>
        public long Size
        {
            get { return _size; }
        }

        /// <summary>
        /// Gets the blob.
        /// </summary>
        public byte[] ByteArray
        {
            get { return _byteArray; }
        }
    }
}
