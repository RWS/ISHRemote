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
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.Cmdlets.PublicationOutput
{
    /// <summary>
    /// Abstract class used for the publication output commandlets.
    /// </summary>
    /// <remarks>Inherits from <see cref="TrisoftCmdlet"/>.</remarks>
    public abstract class PublicationOutputCmdlet : TrisoftCmdlet
    {
        /// <summary>
        /// Add the required fields to the requested metadata so when piping the object the necesarry identifiers are provided.
        /// </summary>
        /// <param name="currentFields">The current <see cref="IshFields"/> object to append.</param>
        /// <returns>The updated <see cref="IshFields"/> object.</returns>
        public virtual IshFields AddRequiredFields(IshFields currentFields)
        {
            currentFields.AddOrUpdateField(new IshRequestedMetadataField("FISHPUBLNGCOMBINATION", Enumerations.Level.Lng,Enumerations.ValueType.Value));
            currentFields.AddOrUpdateField(new IshRequestedMetadataField("FISHOUTPUTFORMATREF", Enumerations.Level.Lng, Enumerations.ValueType.Element));
            currentFields.AddOrUpdateField(new IshRequestedMetadataField("FISHOUTPUTFORMATREF", Enumerations.Level.Lng, Enumerations.ValueType.Value));
            currentFields.AddOrUpdateField(new IshRequestedMetadataField("VERSION", Enumerations.Level.Version,Enumerations.ValueType.Value));
            currentFields.AddOrUpdateField(new IshRequestedMetadataField("FTITLE", Enumerations.Level.Logical,Enumerations.ValueType.Value));
            return currentFields;
        }


        /// <summary>
        /// Get the identifier fields that are needed to identify a publication output object.
        /// </summary>
        /// <param name="fieldList">The <see cref="IshFields"/> object to get the identifier fields from.</param>
        /// <returns>A <see cref="IshFields"/> object with the identifier fields.</returns>
        public virtual IshFields GetIdentifierFields(IshFields fieldList)
        {
            IshFields returnFields = new IshFields();
            returnFields.AddField(fieldList.RetrieveFirst("VERSION", Enumerations.Level.Version, Enumerations.ValueType.Value));
            returnFields.AddField(fieldList.RetrieveFirst("FISHOUTPUTFORMATREF", Enumerations.Level.Lng, Enumerations.ValueType.Element));           
            returnFields.AddField(fieldList.RetrieveFirst("FISHPUBLNGCOMBINATION", Enumerations.Level.Lng, Enumerations.ValueType.Value));
            return returnFields;
        }

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

            if (actionMode == Enumerations.ActionMode.Create || actionMode == Enumerations.ActionMode.Update)
            {
                //  These fields can be retrieved BUT cannot be set 

                // Publication
                // Publication Version
                ishFields.RemoveField(FieldElements.PublicationModifiedOn, Enumerations.Level.Version, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.PublicationReleased, Enumerations.Level.Version, Enumerations.ValueType.All);

                // Publication Output
                ishFields.RemoveField(FieldElements.PublishEventId, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.Publisher, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.PublishStartDate, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.PublishEndDate, Enumerations.Level.Lng, Enumerations.ValueType.All);

                // These fields are set via parameters
                ishFields.RemoveField(FieldElements.PublicationLanguageCombination, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.OutputFormatReference, Enumerations.Level.Lng, Enumerations.ValueType.All);

                // General
                ishFields.RemoveField(FieldElements.Name, Enumerations.Level.Logical, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.Name, Enumerations.Level.Version, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.Name, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.Ancestor, Enumerations.Level.Version, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.Ancestor, Enumerations.Level.Lng, Enumerations.ValueType.All);
                // General version fields
                ishFields.RemoveField(FieldElements.Version, Enumerations.Level.Version, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.BranchNumber, Enumerations.Level.Version, Enumerations.ValueType.All);

                // General language fields
                ishFields.RemoveField(FieldElements.MapId, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.StatusType, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.DocumentLanguage, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.RevisionCounter, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.RevisionLog, Enumerations.Level.Lng, Enumerations.ValueType.All);

                // General document fields
                ishFields.RemoveField(FieldElements.ED, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.Revisions, Enumerations.Level.Lng, Enumerations.ValueType.All);
                // General date fields
                ishFields.RemoveField(FieldElements.CreationDate, Enumerations.Level.Logical, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.CreationDate, Enumerations.Level.Version, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.CreationDate, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.ModificationDate, Enumerations.Level.Logical, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.ModificationDate, Enumerations.Level.Version, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.ModificationDate, Enumerations.Level.Lng, Enumerations.ValueType.All);

                // General security fields
                ishFields.RemoveField(FieldElements.ReadAccess, Enumerations.Level.Logical, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.ReadAccess, Enumerations.Level.Version, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.ReadAccess, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.Usergroup, Enumerations.Level.Logical, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.Usergroup, Enumerations.Level.Version, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.Usergroup, Enumerations.Level.Lng, Enumerations.ValueType.All);

            }

            // Version Object
            // The field is set by PublicationContext after the creation of a Card of type CTCONTEXT
            // and you can only retrieve it via PublicationContext
            ishFields.RemoveField(FieldElements.SavedContexts, Enumerations.Level.Version, Enumerations.ValueType.All);

            // Publication Output
            // Retrieve the value using the IWSPublicationOutputView20.GetReport method on PublicationOutput
            ishFields.RemoveField(FieldElements.PublishReport, Enumerations.Level.Lng, Enumerations.ValueType.All);

            // General logical fields
            ishFields.RemoveField(FieldElements.DocVersionMultiLng, Enumerations.Level.Logical, Enumerations.ValueType.All);
            // General version fields
            ishFields.RemoveField(FieldElements.LngVersion, Enumerations.Level.Version, Enumerations.ValueType.All);

            // General document fields
            ishFields.RemoveField(FieldElements.CheckedOut, Enumerations.Level.Lng, Enumerations.ValueType.All);
            ishFields.RemoveField(FieldElements.CheckedOutBy, Enumerations.Level.Lng, Enumerations.ValueType.All);

            // General security fields
            ishFields.RemoveField(FieldElements.ModifyAccess, Enumerations.Level.Logical, Enumerations.ValueType.All);
            ishFields.RemoveField(FieldElements.ModifyAccess, Enumerations.Level.Version, Enumerations.ValueType.All);
            ishFields.RemoveField(FieldElements.ModifyAccess, Enumerations.Level.Lng, Enumerations.ValueType.All);
            ishFields.RemoveField(FieldElements.DeleteAccess, Enumerations.Level.Logical, Enumerations.ValueType.All);
            ishFields.RemoveField(FieldElements.DeleteAccess, Enumerations.Level.Version, Enumerations.ValueType.All);
            ishFields.RemoveField(FieldElements.DeleteAccess, Enumerations.Level.Lng, Enumerations.ValueType.All);
            ishFields.RemoveField(FieldElements.Owner, Enumerations.Level.Logical, Enumerations.ValueType.All);
            ishFields.RemoveField(FieldElements.Owner, Enumerations.Level.Version, Enumerations.ValueType.All);
            ishFields.RemoveField(FieldElements.Owner, Enumerations.Level.Lng, Enumerations.ValueType.All);

            return ishFields;
        }
    }
}
