using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UABEAvalonia
{
    public static class FileDialogUtils
    {
#if ANDROID
        // Android: the in-app browser (PathStorageProvider) already returns real paths.
        public static string[] GetOpenFileDialogFiles(IReadOnlyList<PathItem> files)
        {
            return files.Select(sf => sf.Path).ToArray();
        }

        public static string[] GetOpenFolderDialogFiles(IReadOnlyList<PathItem> folders)
        {
            return folders.Select(sf => sf.Path).ToArray();
        }

        public static string? GetSaveFileDialogFile(PathItem? file)
        {
            return file?.Path;
        }
#else
        public static string[] GetOpenFileDialogFiles(IReadOnlyList<IStorageFile> files)
        {
            return files.Select(sf => sf.TryGetLocalPath()).Where(p => p != null).ToArray()!;
        }

        public static string[] GetOpenFolderDialogFiles(IReadOnlyList<IStorageFolder> folders)
        {
            return folders.Select(sf => sf.TryGetLocalPath()).Where(p => p != null).ToArray()!;
        }

        public static string? GetSaveFileDialogFile(IStorageFile? file)
        {
            return file?.TryGetLocalPath();
        }
#endif
    }
}
