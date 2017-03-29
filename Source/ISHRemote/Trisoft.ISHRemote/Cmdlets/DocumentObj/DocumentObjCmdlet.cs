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


namespace Trisoft.ISHRemote.Cmdlets.DocumentObj
{
    public abstract class DocumentObjCmdlet : TrisoftCmdlet
    {
        public Enumerations.ISHType[] ISHType
        {
            get { return new Enumerations.ISHType[] { Enumerations.ISHType.ISHIllustration, Enumerations.ISHType.ISHLibrary, Enumerations.ISHType.ISHMasterDoc, Enumerations.ISHType.ISHModule, Enumerations.ISHType.ISHTemplate }; }
        }

        /// <summary>
        /// Add the required fields to the requested metadata so when piping the object the necesarry identifiers are provided.
        /// </summary>
        /// <param name="currentFields">The current <see cref="IshFields"/> object to append.</param>
        /// <returns>The updated <see cref="IshFields"/> object.</returns>
        public virtual IshFields AddRequiredFields(IshFields currentFields)
        {
            throw new NotSupportedException("Replaced by IshSession.IshTypeFieldSetup");

            currentFields.AddOrUpdateField(new IshRequestedMetadataField("FTITLE", Enumerations.Level.Logical, Enumerations.ValueType.Value));
            currentFields.AddOrUpdateField(new IshRequestedMetadataField("VERSION", Enumerations.Level.Version, Enumerations.ValueType.Value));
            currentFields.AddOrUpdateField(new IshRequestedMetadataField("DOC-LANGUAGE", Enumerations.Level.Lng, Enumerations.ValueType.Value));
            currentFields.AddOrUpdateField(new IshRequestedMetadataField("FRESOLUTION", Enumerations.Level.Lng, Enumerations.ValueType.Value));            
            return currentFields;
        }

        /// <summary>
        /// Removes the SYSTEM fields from the given IshFields container. Making the fields ready for an update/write operation. 
        /// </summary>
        internal override IshFields RemoveSystemFields(IshFields ishFields, Enumerations.ActionMode actionMode)
        {
            throw new NotSupportedException("Replaced by IshSession.IshTypeFieldSetup");

            if (actionMode == Enumerations.ActionMode.Read)
            {
                throw new InvalidOperationException(
                    "We will not remove system fields for read operations anymore as part of TS-9581");
            }

            if (actionMode == Enumerations.ActionMode.Create || actionMode == Enumerations.ActionMode.Update)
            {
                //  These fields can be retrieved BUT cannot be set 

                // Logical object
                ishFields.RemoveField(FieldElements.InheritedLanguages, Enumerations.Level.Logical, Enumerations.ValueType.All);

                // Version object
                ishFields.RemoveField(FieldElements.ReferencedFrozenBaselines, Enumerations.Level.Version, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.ReusedInVersion, Enumerations.Level.Version, Enumerations.ValueType.All);

                // Language Object
                // ishFields.RemoveField(FieldElements.DocumentLanguage); // To support Language Applicability the Language must be updatable...

                ishFields.RemoveField(FieldElements.Resolution, Enumerations.Level.Lng, Enumerations.ValueType.All);
                // TS-8890 - TrisoftRepositoryImport - Issues with importing translated modules: SourceLanguage removed from system fields 
                // ishFields.RemoveField(FieldElements.SourceLanguage);
                ishFields.RemoveField(FieldElements.LastModifiedBy, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.LastModifiedOn, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.VariablesInUse, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.VariableAssignments, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.FragmentLinks, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.Targets, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.Links, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.HyperLinks, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.ImageLinks, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.ReusableObjects, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.Conditions, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.SuccessfulPlugins, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.ReusableObjectSystemLock, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.WordCount, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.TranslationLoad, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.ToBeTranslatedWordCount, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.TranslationManagementStartDate, Enumerations.Level.Lng, Enumerations.ValueType.All);

                // General
                ishFields.RemoveField(FieldElements.Name, Enumerations.Level.Logical, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.Name, Enumerations.Level.Version, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.Name, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.Ancestor, Enumerations.Level.Logical, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.Ancestor, Enumerations.Level.Version, Enumerations.ValueType.All);

                // General version fields
                ishFields.RemoveField(FieldElements.Version, Enumerations.Level.Version, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.BranchNumber, Enumerations.Level.Version, Enumerations.ValueType.All);

                // General language fields
                ishFields.RemoveField(FieldElements.MapId, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.StatusType, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.RevisionCounter, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.RevisionLog, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.RevisionHistoryLogging, Enumerations.Level.Lng, Enumerations.ValueType.All);

                // General document fields
                ishFields.RemoveField(FieldElements.CheckedOut, Enumerations.Level.Lng, Enumerations.ValueType.All);
                ishFields.RemoveField(FieldElements.CheckedOutBy, Enumerations.Level.Lng, Enumerations.ValueType.All);
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

            // Logical Object
            // The field is set by PublicationContext after the creation of a Card of type CTCONTEXT
            ishFields.RemoveField(FieldElements.SavedContexts, Enumerations.Level.Logical, Enumerations.ValueType.All);

            // General logical fields
            ishFields.RemoveField(FieldElements.DocVersionMultiLng, Enumerations.Level.Logical, Enumerations.ValueType.All);
            // General version fields
            ishFields.RemoveField(FieldElements.LngVersion, Enumerations.Level.Version, Enumerations.ValueType.All);

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

        /// <summary>
        /// Returns IshFields container containing VERSION, DOC-LANGUAGE, RESOLUTION fields retrieved from the provided IshFields container
        /// Returned fields are ready to be used as a filter in the Retrieve call in the Set/Add commandlet.
        /// </summary>
        protected IshFields RemoveNonIdentifierFields(IshFields ishFields)
        {
            throw new NotSupportedException("Replaced by IshSession.IshTypeFieldSetup. Never used even.");

            IshFields returnIshFields = new IshFields();

            // Version
            IshField ishFieldVersion = ishFields.RetrieveFirst("VERSION", Enumerations.Level.Version, Enumerations.ValueType.Value);
            if (ishFieldVersion != null)
            {
                returnIshFields.AddField(ishFieldVersion);
            }

            // Language
            IshField ishFieldLanguage;
            
            // Case of the Illustration object type where having (comma space separated) multi languages is allowed.
            string languages = ishFields.GetFieldValue("DOC-LANGUAGE", Enumerations.Level.Lng, Enumerations.ValueType.Value);
            if (languages.Contains(","))
            {
                foreach (string language in languages.Split(", ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries) )
                {
                    ishFieldLanguage = new IshMetadataFilterField("DOC-LANGUAGE", Enumerations.Level.Lng, Enumerations.FilterOperator.Equal, language, Enumerations.ValueType.Value);
                    returnIshFields.AddField(ishFieldLanguage);
                }
            }
            else
            {
                ishFieldLanguage = ishFields.RetrieveFirst("DOC-LANGUAGE", Enumerations.Level.Lng, Enumerations.ValueType.Value);
                if (ishFieldLanguage != null)
                {
                    returnIshFields.AddField(ishFieldLanguage);
                }
            }
           
            // Resolution
            IshField ishFieldResolution = ishFields.RetrieveFirst("FRESOLUTION", Enumerations.Level.Lng, Enumerations.ValueType.Value);
            if (ishFieldResolution != null)
            {
                returnIshFields.AddField(ishFieldResolution);
            }
            
            return returnIshFields;
        }
    
    }
}
