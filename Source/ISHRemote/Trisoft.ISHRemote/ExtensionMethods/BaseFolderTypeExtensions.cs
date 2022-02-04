using Trisoft.ISHRemote.Objects;

namespace Trisoft.ISHRemote.ExtensionMethods
{
    internal static class BaseFolderTypeExtensions
    {
        internal static OpenApi.BaseFolder ToIshBaseFolder(this Enumerations.BaseFolder baseFolder)
        {
            OpenApi.BaseFolder openApiBaseFolder = OpenApi.BaseFolder.None;

            switch (baseFolder)
            {
                case Enumerations.BaseFolder.Data:
                    openApiBaseFolder = OpenApi.BaseFolder.Data;
                    break;

                case Enumerations.BaseFolder.Favorites:
                    openApiBaseFolder = OpenApi.BaseFolder.Favorites;
                    break;

                case Enumerations.BaseFolder.System:
                    openApiBaseFolder = OpenApi.BaseFolder.System;
                    break;

                case Enumerations.BaseFolder.EditorTemplate:
                    openApiBaseFolder = OpenApi.BaseFolder.EditorTemplate;
                    break;
            }

            return openApiBaseFolder;
        }
    }
}
