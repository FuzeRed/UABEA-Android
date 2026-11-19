#if ANDROID
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UABEAvalonia
{
    /// <summary>A picked file or folder. Always a real filesystem path (UABEA works on paths).</summary>
    public class PathItem
    {
        public string Path { get; }
        public PathItem(string path) { Path = path; }
        public string Name => System.IO.Path.GetFileName(Path.TrimEnd('/'));
    }

    /// <summary>
    /// Replacement for Window.StorageProvider on Android. The system document picker hands out
    /// content:// URIs, which UABEA can't use (it needs sibling files, dependencies, in-place
    /// saves ...), so this opens an in-app file browser that returns real paths. Needs the
    /// "All files access" permission, which MainActivity asks for.
    /// Method names match Avalonia's IStorageProvider so the shared call sites are unchanged.
    /// </summary>
    public class PathStorageProvider
    {
        private readonly Window owner;

        public PathStorageProvider(Window owner)
        {
            this.owner = owner;
        }

        public bool CanOpen => true;
        public bool CanSave => true;
        public bool CanPickFolder => true;

        public async Task<IReadOnlyList<PathItem>> OpenFilePickerAsync(FilePickerOpenOptions options)
        {
            var patterns = options.FileTypeFilter?.FirstOrDefault()?.Patterns;
            var win = new FileBrowserWindow(FileBrowserMode.OpenFiles, options.Title ?? "Open file",
                options.AllowMultiple, patterns, null);
            string[]? res = await win.ShowDialog<string[]?>(owner);
            if (res == null)
                return new List<PathItem>();
            return res.Select(p => new PathItem(p)).ToList();
        }

        public async Task<PathItem?> SaveFilePickerAsync(FilePickerSaveOptions options)
        {
            var win = new FileBrowserWindow(FileBrowserMode.Save, options.Title ?? "Save file",
                false, null, options.SuggestedFileName);
            string[]? res = await win.ShowDialog<string[]?>(owner);
            if (res == null || res.Length == 0)
                return null;
            return new PathItem(res[0]);
        }

        public async Task<IReadOnlyList<PathItem>> OpenFolderPickerAsync(FolderPickerOpenOptions options)
        {
            var win = new FileBrowserWindow(FileBrowserMode.Folder, options.Title ?? "Select folder",
                false, null, null);
            string[]? res = await win.ShowDialog<string[]?>(owner);
            if (res == null)
                return new List<PathItem>();
            return res.Select(p => new PathItem(p)).ToList();
        }

        // Only used for "suggested start location"; the browser remembers its last folder itself.
        public Task<IStorageFolder?> TryGetFolderFromPathAsync(string path)
        {
            return Task.FromResult<IStorageFolder?>(null);
        }
    }
}
#endif
