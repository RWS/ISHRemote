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
using System.ComponentModel;
using System.Linq;
using System.Text;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.HelperClasses
{
    public static class EnumConverter
    {

        /// <summary>
        /// Convert a statusfilter string to an enumeration value.
        /// </summary>
        /// <param name="statusFilter">The statusfilter string that needs to be converted.</param>
        /// <returns>A eISHStatusgroup enumeration value.</returns>
        public static TResult ToStatusFilter<TResult>(Enumerations.StatusFilter statusFilter)
        {
            if (!typeof(TResult).IsEnum)
            {
                throw new NotSupportedException("TResult must be an Enum");
            }
            return (TResult)Enum.Parse(typeof(TResult), statusFilter.ToString(), true);
        }

        /// <summary>
        /// Convert a progressStatusFilter string to an enumeration value.
        /// </summary>
        /// <param name="progressStatusFilter">The progressStatusFilter string that needs to be converted.</param>
        /// <returns>A eProgressStatusFilter enumeration value.</returns>
        public static TResult ToProgressStatusFilter<TResult>(Enumerations.ProgressStatusFilter progressStatusFilter)
        {
            if (!typeof(TResult).IsEnum)
            {
                throw new NotSupportedException("TResult must be an Enum");
            }
            return (TResult)Enum.Parse(typeof(TResult), progressStatusFilter.ToString(), true);
        }

        /// <summary>
        /// Convert a BackgroundTaskStatusFilter string to an enumeration value.
        /// </summary>
        /// <param name="backgroundTaskStatusFilter">The backgroundTaskStatusFilter string that needs to be converted.</param>
        /// <returns>A eBackgroundTaskStatusFilter enumeration value.</returns>
        public static TResult ToBackgroundTaskStatusFilter<TResult>(Enumerations.BackgroundTaskStatusFilter backgroundTaskStatusFilter)
        {
            if (!typeof(TResult).IsEnum)
            {
                throw new NotSupportedException("TResult must be an Enum");
            }
            return (TResult)Enum.Parse(typeof(TResult), backgroundTaskStatusFilter.ToString(), true);
        }

        /// <summary>
        /// Convert a userFilter string to an enumeration value.
        /// </summary>
        /// <param name="userFilter">The userFilter string that needs to be converted.</param>
        /// <returns>A UserFilter enumeration value.</returns>
        public static TResult ToUserFilter<TResult>(Enumerations.UserFilter userFilter)
        {
            if (!typeof(TResult).IsEnum)
            {
                throw new NotSupportedException("TResult must be an Enum");
            }
            return (TResult)Enum.Parse(typeof(TResult), userFilter.ToString(), true);
        }

        /// <summary>
        /// Convert a eventLevelFilter string to an enumeration value.
        /// </summary>
        /// <param name="eventLevelFilter">The eventLevelFilter string that needs to be converted.</param>
        /// <returns>A eventLevelFilter enumeration value.</returns>
        public static TResult ToEventLevelFilter<TResult>(Enumerations.EventLevel eventLevelFilter)
        {
            if (!typeof(TResult).IsEnum)
            {
                throw new NotSupportedException("TResult must be an Enum");
            }
            return (TResult)Enum.Parse(typeof(TResult), eventLevelFilter.ToString(), true);
        }

        /// <summary>
        /// Convert a activity to an enumeration value
        /// </summary>      
        /// <param name="activityFilter">The activity filter that needs to be converted.</param>
        /// <returns>A eISHActivityFilter enumeration value.</returns>
        public static TResult ToActivityFilter<TResult>(Enumerations.ActivityFilter activityFilter)
        {
            if (!typeof(TResult).IsEnum)
            {
                throw new NotSupportedException("TResult must be an Enum");
            }
            return (TResult)Enum.Parse(typeof(TResult), activityFilter.ToString(), true);
        }
        /// <summary>
        /// Convert a basefolder to an enumeration value
        /// </summary>      
        /// <param name="baseFolder">The base folder that needs to be converted.</param>
        /// <returns>A eISHBaseFolder enumeration value.</returns>
        public static TResult ToBaseFolder<TResult>(Enumerations.BaseFolder baseFolder)
        {
            if (!typeof(TResult).IsEnum)
            {
                throw new NotSupportedException("TResult must be an Enum");
            }
            return (TResult)Enum.Parse(typeof(TResult), baseFolder.ToString(), true);
        }

        /// <summary>
        /// Convert a basefolder to an enumeration value
        /// </summary>      
        /// <param name="baseFolder">The base folder that needs to be converted.</param>
        /// <returns>A eISHBaseFolder enumeration value.</returns>
        public static TResult ToBaseFolder<TResult>(string baseFolder)
        {
            if (!typeof(TResult).IsEnum)
            {
                throw new NotSupportedException("TResult must be an Enum");
            }
            return (TResult)Enum.Parse(typeof(TResult), baseFolder, true);
        }

        /// <summary>
        /// Convert a folderType to an enumeration value
        /// </summary>      
        /// <param name="folderType">The folder type that needs to be converted.</param>
        /// <returns>eISHFolderType enumeration value.</returns>
        public static TResult ToFolderType<TResult>(Enumerations.IshFolderType folderType)
        {
            if (!typeof(TResult).IsEnum)
            {
                throw new NotSupportedException("TResult must be an Enum");
            }
            return (TResult)Enum.Parse(typeof(TResult), folderType.ToString(), true);
        }


        /// <summary>
        /// Convert a ishType to an enumeration value
        /// </summary>      
        /// <param name="ishType">The folder type that needs to be converted.</param>
        /// <returns>eISHFolderType enumeration value.</returns>
        public static TResult ToIshType<TResult>(Enumerations.ISHType ishType)
        {
            if (!typeof(TResult).IsEnum)
            {
                throw new NotSupportedException("TResult must be an Enum");
            }
            return (TResult)Enum.Parse(typeof(TResult), ishType.ToString(), true);
        }

    }
}
