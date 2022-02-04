using System;
using System.Collections.Generic;
using System.Text;
using Trisoft.ISHRemote.Objects;

namespace Trisoft.ISHRemote.ExtensionMethods
{
    internal static class FolderTypeExtensions
    {
        internal static Enumerations.IshFolderType ToIshFolderType(this OpenApi.FolderType folderType)
        {
            Enumerations.IshFolderType ishFolderType = Enumerations.IshFolderType.ISHNone;
            switch (folderType)
            {
                case OpenApi.FolderType.Folder:
                    ishFolderType = Enumerations.IshFolderType.ISHNone;
                    break;

                case OpenApi.FolderType.IllustrationFolder:
                    ishFolderType = Enumerations.IshFolderType.ISHIllustration;
                    break;

                case OpenApi.FolderType.LibraryFolder:
                    ishFolderType = Enumerations.IshFolderType.ISHLibrary;
                    break;

                case OpenApi.FolderType.MapFolder:
                    ishFolderType = Enumerations.IshFolderType.ISHMasterDoc;
                    break;

                case OpenApi.FolderType.OtherFolder:
                    ishFolderType = Enumerations.IshFolderType.ISHNone;
                    break;

                case OpenApi.FolderType.PublicationFolder:
                    ishFolderType = Enumerations.IshFolderType.ISHPublication;
                    break;

                case OpenApi.FolderType.ReferenceFolder:
                    ishFolderType = Enumerations.IshFolderType.ISHReference;
                    break;

                case OpenApi.FolderType.TopicFolder:
                    ishFolderType = Enumerations.IshFolderType.ISHModule;
                    break;

                default:
                    ishFolderType = Enumerations.IshFolderType.ISHNone;
                    break;
            }

            return ishFolderType;
        }
    }
}
