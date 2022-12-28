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
using Trisoft.ISHRemote.Annotation25ServiceReference;
using Trisoft.ISHRemote.Application25ServiceReference;
using Trisoft.ISHRemote.BackgroundTask25ServiceReference;
using Trisoft.ISHRemote.Baseline25ServiceReference;
using Trisoft.ISHRemote.DocumentObj25ServiceReference;
using Trisoft.ISHRemote.EDT25ServiceReference;
using Trisoft.ISHRemote.EventMonitor25ServiceReference;
using Trisoft.ISHRemote.Folder25ServiceReference;
using Trisoft.ISHRemote.ListOfValues25ServiceReference;
using Trisoft.ISHRemote.MetadataBinding25ServiceReference;
using Trisoft.ISHRemote.OutputFormat25ServiceReference;
using Trisoft.ISHRemote.PublicationOutput25ServiceReference;
using Trisoft.ISHRemote.Search25ServiceReference;
using Trisoft.ISHRemote.Settings25ServiceReference;
using Trisoft.ISHRemote.TranslationJob25ServiceReference;
using Trisoft.ISHRemote.TranslationTemplate25ServiceReference;
using Trisoft.ISHRemote.User25ServiceReference;
using Trisoft.ISHRemote.UserGroup25ServiceReference;
using Trisoft.ISHRemote.UserRole25ServiceReference;

namespace Trisoft.ISHRemote.Connection
{
    internal interface IInfoShareWcfSoapConnection
    {
        Uri InfoShareWSBaseUri { get; }
        bool IsValid { get; }

        void Close();
        void Dispose();
        Annotation GetAnnotation25Channel();
        Application GetApplication25Channel();
        BackgroundTask GetBackgroundTask25Channel();
        Baseline GetBaseline25Channel();
        DocumentObj GetDocumentObj25Channel();
        EDT GetEDT25Channel();
        EventMonitor GetEventMonitor25Channel();
        Folder GetFolder25Channel();
        ListOfValues GetListOfValues25Channel();
        MetadataBinding GetMetadataBinding25Channel();
        OutputFormat GetOutputFormat25Channel();
        PublicationOutput GetPublicationOutput25Channel();
        Search GetSearch25Channel();
        Settings GetSettings25Channel();
        TranslationJob GetTranslationJob25Channel();
        TranslationTemplate GetTranslationTemplate25Channel();
        User GetUser25Channel();
        UserGroup GetUserGroup25Channel();
        UserRole GetUserRole25Channel();
    }
}