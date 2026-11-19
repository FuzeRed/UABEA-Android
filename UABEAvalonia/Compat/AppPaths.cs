using System;

namespace UABEAvalonia
{
    /// <summary>
    /// Where UABEA keeps its own files (classdata.tpk, config.json, plugins).
    /// Desktop: next to the exe. Android: the app's private files dir (set by MainActivity).
    /// </summary>
    public static class AppPaths
    {
        public static string DataDir { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

        // Android only: hooks filled in by MainActivity.
        public static Func<bool>? HasStorageAccess { get; set; }
        public static Action? RequestStorageAccess { get; set; }
        public static Action? ExitApp { get; set; }
        public static string StartDirectory { get; set; } = "/storage/emulated/0";
    }
}
