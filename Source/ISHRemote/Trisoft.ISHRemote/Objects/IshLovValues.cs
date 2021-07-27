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
using Trisoft.ISHRemote.HelperClasses;
using System.Xml;
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.Objects
{
    /// <summary>
    /// <para type="description">Containter object grouping multiple IshLoveValue entries</para>
    /// </summary>
    internal class IshLovValues
    {
        private Dictionary<string, IshLovValue> _lovValues;
        private const string keySeparator = "===";

        /// <summary>
        /// Creates a new instance of the <see cref="IshLovValues"/> class.
        /// </summary>
        /// <param name="xmlIshLovValues">The xml containing the LovValues.</param>
        public IshLovValues(string xmlIshLovValues)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlIshLovValues);
            _lovValues = new Dictionary<string, IshLovValue>();
            foreach (XmlNode ishLov in xmlDocument.SelectNodes("ishlovs/ishlov"))
            {
                string lovId = ishLov.Attributes["ishref"].Value;
                foreach (XmlNode xmlIshLovValue in ishLov.SelectNodes("./ishlovvalues/ishlovvalue"))
                {
                    var ishLovValue = new IshLovValue(lovId, (XmlElement)xmlIshLovValue);
                    _lovValues.Add((lovId + keySeparator + ishLovValue.IshRef), ishLovValue);
                }
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="IshLovValues"/> class.
        /// </summary>
        /// <param name="ishLovValues">An <see cref="IshLovValue"/> array.</param>
        public IshLovValues(IshLovValue[] ishLovValues)
        {
            _lovValues = new Dictionary<string, IshLovValue>();
            foreach (IshLovValue ishLovValue in ishLovValues)
            {
                _lovValues.Add((ishLovValue.LovId + keySeparator + ishLovValue.IshRef), ishLovValue);
            }
            //TODO [COULD] Earlier List<> implementation initialized by:  _lovValues = new List<IshLovValue>(ishLovValues ?? new IshLovValue[0]);
        }

        /// <summary>
        /// Returns the IshLovValue identified by LovId and LovValueId
        /// </summary>
        /// <param name="lovId">List of Value identifier like 'DRESOLUTION'</param>
        /// <param name="lovValueId">List of Value Value identifier, aka IshRef, like 'VRESHIGH'</param>
        /// <param name="ishLovValue">The complete initialized object including label and active flag</param>
        /// <returns></returns>
        public bool TryGetIshLovValue(string lovId, string lovValueId, out IshLovValue ishLovValue)
        {
            if (_lovValues.TryGetValue(lovId + keySeparator + lovValueId, out ishLovValue))
            {
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Gets the current IshLovValues.
        /// </summary>
        public IshLovValue[] LovValues
        {
            get { return _lovValues.Values.ToArray(); }
        }

        /// <summary>
        /// Return all LovValueId identifiers (ishref)
        /// </summary>
        public string[] ValueIds
        {
            get
            {
                List<string> valueIds = new List<string>();
                foreach (IshLovValue ishLovValue in LovValues)
                {
                    valueIds.Add(ishLovValue.IshRef);
                }
                return valueIds.ToArray();
            }
        }

        /// <summary>
        /// Return all Id identifiers (ishlovvalueref)
        /// </summary>
        public long[] Ids
        {
            get
            {
                List<long> Ids = new List<long>();
                foreach (IshLovValue ishLovValue in LovValues)
                {
                    Ids.Add(ishLovValue.IshLovValueRef);
                }
                return Ids.ToArray();
            }
        }
    }
}
