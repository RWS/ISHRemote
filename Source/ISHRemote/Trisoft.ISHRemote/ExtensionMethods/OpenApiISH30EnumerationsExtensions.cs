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
using static Trisoft.ISHRemote.Objects.Enumerations;

namespace Trisoft.ISHRemote.ExtensionMethods
{
    internal static class OpenApiISH30EnumerationsExtensions
    {
        /// <summary>
        /// OpenApi (dd 20211228) implements FieldGroup as derived from ISHRemote RequestedMetadataGroup, should be 1-1 conversion to server-side decide which fields to retrieve
        /// </summary>
        internal static OpenApiISH30.FieldGroup ToOpenApiISH30FieldGroup(this RequestedMetadataGroup requestedMetadataGroup)
        {
            switch (requestedMetadataGroup)
            {
                case RequestedMetadataGroup.All:
                    return OpenApiISH30.FieldGroup.All;
                case RequestedMetadataGroup.Basic:
                    return OpenApiISH30.FieldGroup.Basic;
                case RequestedMetadataGroup.Descriptive:
                default:
                    return OpenApiISH30.FieldGroup.Descriptive;
            }

        }

        /// <summary>
        /// OpenApi (dd 20211228) implements Field Level, needs mapping to ISHRemote levels
        /// </summary>
        internal static Level ToISHFieldLevel(this OpenApiISH30.Level oLevel)
        {
            switch (oLevel)
            {
                case OpenApiISH30.Level.None:
                    return Level.None;
                case OpenApiISH30.Level.Logical:
                    return Level.Logical;
                case OpenApiISH30.Level.Version:
                    return Level.Version;
                case OpenApiISH30.Level.Language:
                    return Level.Lng;
                case OpenApiISH30.Level.Data:
                    return Level.Data;
                case OpenApiISH30.Level.Annotation:
                    return Level.Annotation;
                case OpenApiISH30.Level.Reply:
                    return Level.Reply;
                case OpenApiISH30.Level.Progress:
                    return Level.Progress;
                case OpenApiISH30.Level.Detail:
                    return Level.Detail;
                case OpenApiISH30.Level.Object:
                case OpenApiISH30.Level.Compute:
                default:
                    throw new ArgumentException($"Enumerations.ToFieldLevel OpenApiISH30.Level[{oLevel}] was unexpected.");
            }
        }
    }
}
