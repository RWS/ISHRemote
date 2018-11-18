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
using System.Collections.Generic;

namespace Trisoft.ISHRemote.Cmdlets.BackgroundTask
{
    public abstract class BackgroundTaskCmdlet : TrisoftCmdlet
    {
        public Enumerations.ISHType[] ISHType
        {
            get { return new Enumerations.ISHType[] { Enumerations.ISHType.ISHBackgroundTask }; }
        }

        /// <summary>
        /// Add the required fields to the requested metadata so when piping the object the necesarry identifiers are provided.
        /// </summary>
        /// <param name="currentFields">The current <see cref="IshFields"/> object to append.</param>
        /// <returns>The updated <see cref="IshFields"/> object.</returns>
        public virtual IshFields AddRequiredFields(IshFields currentFields)
        {
			throw;
            currentFields.AddOrUpdateField(new IshRequestedMetadataField("EVENTID", Enumerations.Level.Progress, Enumerations.ValueType.Value), Enumerations.ActionMode.Read);
            currentFields.AddOrUpdateField(new IshRequestedMetadataField("EVENTTYPE", Enumerations.Level.Progress, Enumerations.ValueType.Value), Enumerations.ActionMode.Read);
            return currentFields;
        }

        /// <summary>
        /// Wrap incoming objects as PSObjects and extend with PSNoteProperties for every IshField value entry
        /// </summary>
        /// <param name="ishEvents">Object to wrap and return as PSObject</param>
        /// <returns>Wrapped PSObjects</returns>
        internal List<PSObject> WrapAsPSObjectAndAddNoteProperties(List<IshEvent> ishEvents)
        {
			throw;
            List<PSObject> psObjects = new List<PSObject>();
            foreach(IshEvent ishEvent in ishEvents)
            {
                PSObject psObject = PSObject.AsPSObject(ishEvent);
                foreach(IshField ishField in ishEvent.IshFields.Fields())
                {
                    string name = ishField.Level + LevelNameValueTypeSeparator + ishField.Name + LevelNameValueTypeSeparator + ishField.ValueType;
                    psObject.Properties.Add(new PSNoteProperty(name, ishEvent.IshFields.GetFieldValue(ishField.Name,ishField.Level,ishField.ValueType)));
                }
                psObjects.Add(psObject);
            }
            return psObjects;
        }
    
    }
}

