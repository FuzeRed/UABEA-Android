using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Avalonia;
using Avalonia.Android;
using System;
using System.IO;
using UABEAvalonia;

namespace UABEAHead
{
    [Activity(
        Label = "UABEA",
        Theme = "@style/MyTheme.NoActionBar",
        Icon = "@drawable/icon",
        MainLauncher = true,
        WindowSoftInputMode = Android.Views.SoftInput.AdjustResize,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : AvaloniaMainActivity<App>
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            // must happen before Avalonia creates MainWindow (it reads classdata.tpk / config.json)
            string dataDir = FilesDir!.AbsolutePath;
            AppPaths.DataDir = dataDir;
            AppPaths.HasStorageAccess = HasAllFilesAccess;
            AppPaths.RequestStorageAccess = RequestAllFilesAccess;
            AppPaths.ExitApp = () => Finish();
            CopyAssetIfMissing("classdata.tpk", Path.Combine(dataDir, "classdata.tpk"));

            string? ext = Android.OS.Environment.ExternalStorageDirectory?.AbsolutePath;
            if (!string.IsNullOrEmpty(ext))
                AppPaths.StartDirectory = ext;

            base.OnCreate(savedInstanceState);

            if (!HasAllFilesAccess())
                RequestAllFilesAccess();
        }

        public override void OnBackPressed()
        {
            if (WindowHost.Current != null && WindowHost.Current.HandleBack())
                return;
            // no windows left: leave the app
            Finish();
        }


        private void CopyAssetIfMissing(string assetName, string destPath)
        {
            try
            {
                // always refresh when the size differs so app updates can ship a new tpk
                using Stream src = Assets!.Open(assetName);
                if (File.Exists(destPath) && new FileInfo(destPath).Length == src.Length)
                    return;
                using FileStream dst = File.Create(destPath);
                src.CopyTo(dst);
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(FilesDir!.AbsolutePath, "uabeacrash.log"), ex.ToString());
            }
        }

        private bool HasAllFilesAccess()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                return Android.OS.Environment.IsExternalStorageManager;

            return CheckSelfPermission(global::Android.Manifest.Permission.ReadExternalStorage) == Permission.Granted
                && CheckSelfPermission(global::Android.Manifest.Permission.WriteExternalStorage) == Permission.Granted;
        }

        private void RequestAllFilesAccess()
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                {
                    var intent = new Intent(Settings.ActionManageAppAllFilesAccessPermission,
                        global::Android.Net.Uri.Parse("package:" + PackageName));
                    StartActivity(intent);
                }
                else
                {
                    RequestPermissions(new[]
                    {
                        global::Android.Manifest.Permission.ReadExternalStorage,
                        global::Android.Manifest.Permission.WriteExternalStorage
                    }, 1);
                }
            }
            catch
            {
                // some ROMs lack the per-app screen; fall back to the generic one
                StartActivity(new Intent(Settings.ActionManageAllFilesAccessPermission));
            }
        }
    }
}
