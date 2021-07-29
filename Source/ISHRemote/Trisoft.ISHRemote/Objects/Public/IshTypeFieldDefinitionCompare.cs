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

namespace Trisoft.ISHRemote.Objects.Public
{
    /// <summary>
    /// <para type="description">Object holding the denormalized ISHType (like ISHMasterDoc, ISHEvent,...) with field information (like FTITLE, PROGRESS,..) including a compare result.</para>
    /// </summary>
    public class IshTypeFieldDefinitionCompare : IshTypeFieldDefinition
    {
        public enum Compare
        {
            /// <summary>
            /// <para type="description">Compare result indicates the entry exists in both lists with the same descriptive properties</para>
            /// </summary>
            Identical,
            /// <summary>
            /// <para type="description">Compare result indicates the entry exists in both lists with differences on the descriptive properties, showing left entry</para>
            /// </summary>
            LeftDifferent,
            /// <summary>
            /// <para type="description">Compare result indicates the entry exists in both lists with differences on the descriptive properties, showing right entry</para>
            /// </summary>
            RightDifferent,
            /// <summary>
            /// <para type="description">Compare result indicates the entry only exist in the left list</para>
            /// </summary>
            LeftOnly,
            /// <summary>
            /// <para type="description">Compare result indicates the entry only exist in the right list</para>
            /// </summary>
            RightOnly
        }

        private readonly Compare _compareResult;

        internal IshTypeFieldDefinitionCompare(IshTypeFieldDefinition ishTypeFieldDefinition, Compare compare)
            : base(ishTypeFieldDefinition)
        {
            _compareResult = compare;
        }

        public string CompareResult
        {
            get
            {
                switch (_compareResult)
                {
                    case Compare.Identical:
                        return "===";
                    case Compare.LeftDifferent:
                        return "<<>";
                    case Compare.RightDifferent:
                        return "<>>";
                    case Compare.LeftOnly:
                        return "<<!";
                    case Compare.RightOnly:
                        return "!>>";
                }
                return "??";
            }
        }
    }
}
