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
using System.Management.Automation;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.Exceptions;

namespace Trisoft.ISHRemote.Cmdlets.Settings
{
    public abstract class SettingsCmdlet : TrisoftCmdlet
    {
        /// <summary>
        /// Removes the SYSTEM fields from the given IshFields container. Making the fields ready for an update/write operation. 
        /// </summary>
        internal override IshFields RemoveSystemFields(IshFields ishFields, Enumerations.ActionMode actionMode)
        {
            if (actionMode == Enumerations.ActionMode.Read)
            {
                throw new InvalidOperationException(
                    "We will not remove system fields for read operations anymore as part of TS-9581");
            }

            const Enumerations.Level level = Enumerations.Level.None;
            const Enumerations.ValueType valueType = Enumerations.ValueType.All;

            if (actionMode != Enumerations.ActionMode.Read)
            {
                //  These fields can be retrieved BUT cannot be set 
                ishFields.RemoveField(FieldElements.Name, level, valueType);

                // General date fields
                ishFields.RemoveField(FieldElements.CreationDate, level, valueType);
                ishFields.RemoveField(FieldElements.ModificationDate, level, valueType);
            }

            // General security fields
            ishFields.RemoveField(FieldElements.ReadAccess, level, valueType);
            ishFields.RemoveField(FieldElements.ModifyAccess, level, valueType);
            ishFields.RemoveField(FieldElements.DeleteAccess, level, valueType);
            ishFields.RemoveField(FieldElements.Owner, level, valueType);

            return ishFields;
        }
    }
}
