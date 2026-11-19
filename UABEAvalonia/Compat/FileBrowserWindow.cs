#if ANDROID
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UABEAvalonia
{
    public enum FileBrowserMode
    {
        OpenFiles,
        Folder,
        Save
    }

    /// <summary>
    /// Touch friendly file/folder browser built in code. Result (via Close(result)) is a string[]
    /// of full paths, or null if cancelled.
    /// </summary>
    public class FileBrowserWindow : Window
    {
        private static string? lastDirectory;

        private readonly FileBrowserMode mode;
        private readonly bool allowMultiple;
        private readonly List<string> patterns;
        private readonly TextBox pathBox;
        private readonly TextBox nameBox;
        private readonly StackPanel listPanel;
        private readonly Button okButton;
        private readonly TextBlock statusText;
        private readonly HashSet<string> selected = new HashSet<string>();
        private string currentDir;

        public FileBrowserWindow(FileBrowserMode mode, string title, bool allowMultiple,
            IReadOnlyList<string>? patterns, string? suggestedName)
        {
            this.mode = mode;
            this.allowMultiple = allowMultiple;
            this.patterns = (patterns ?? new List<string>()).Where(p => p != "*" && p != "*.*").ToList();
            Title = title;

            currentDir = lastDirectory ?? AppPaths.StartDirectory;
            if (!Directory.Exists(currentDir))
                currentDir = AppPaths.DataDir;

            var root = new DockPanel { Margin = new Thickness(8) };

            // ---- top: up button + path box
            var topBar = new DockPanel { Margin = new Thickness(0, 0, 0, 6) };
            var upBtn = new Button { Content = "\u2191 Up", MinHeight = 44, Margin = new Thickness(0, 0, 6, 0) };
            upBtn.Click += (_, _) => GoUp();
            DockPanel.SetDock(upBtn, Dock.Left);
            var goBtn = new Button { Content = "Go", MinHeight = 44, Margin = new Thickness(6, 0, 0, 0) };
            goBtn.Click += (_, _) => NavigateTo(pathBox!.Text ?? "");
            DockPanel.SetDock(goBtn, Dock.Right);
            pathBox = new TextBox { MinHeight = 44, VerticalContentAlignment = VerticalAlignment.Center };
            topBar.Children.Add(upBtn);
            topBar.Children.Add(goBtn);
            topBar.Children.Add(pathBox);
            DockPanel.SetDock(topBar, Dock.Top);
            root.Children.Add(topBar);

            // ---- bottom: name box (save / new folder) + ok / cancel
            var bottom = new StackPanel { Spacing = 6, Margin = new Thickness(0, 6, 0, 0) };
            statusText = new TextBlock { TextWrapping = TextWrapping.Wrap, FontSize = 12 };
            bottom.Children.Add(statusText);

            nameBox = new TextBox { MinHeight = 44, IsVisible = mode != FileBrowserMode.OpenFiles };
            if (mode == FileBrowserMode.Save)
            {
                nameBox.Text = suggestedName ?? "";
                nameBox.Watermark = "File name";
            }
            else
            {
                nameBox.Watermark = "New folder name";
            }
            var nameRow = new DockPanel { IsVisible = mode != FileBrowserMode.OpenFiles };
            if (mode != FileBrowserMode.OpenFiles)
            {
                var newFolderBtn = new Button { Content = "New folder", MinHeight = 44, Margin = new Thickness(6, 0, 0, 0) };
                newFolderBtn.Click += (_, _) => CreateFolder();
                DockPanel.SetDock(newFolderBtn, Dock.Right);
                nameRow.Children.Add(newFolderBtn);
            }
            nameRow.Children.Add(nameBox);
            bottom.Children.Add(nameRow);

            var buttons = new UniformGrid { Rows = 1, Columns = 2 };
            okButton = new Button
            {
                MinHeight = 48,
                Margin = new Thickness(0, 0, 4, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            okButton.Click += (_, _) => Confirm();
            var cancelBtn = new Button
            {
                Content = "Cancel",
                MinHeight = 48,
                Margin = new Thickness(4, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            cancelBtn.Click += (_, _) => Close(null);
            buttons.Children.Add(okButton);
            buttons.Children.Add(cancelBtn);
            bottom.Children.Add(buttons);
            DockPanel.SetDock(bottom, Dock.Bottom);
            root.Children.Add(bottom);

            // ---- center: listing
            listPanel = new StackPanel { Spacing = 2 };
            var scroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = listPanel
            };
            root.Children.Add(scroll);

            Content = root;
            Refresh();
        }

        private void UpdateOkButton()
        {
            switch (mode)
            {
                case FileBrowserMode.OpenFiles:
                    okButton.Content = selected.Count > 0 ? $"Open ({selected.Count})" : "Open";
                    okButton.IsEnabled = selected.Count > 0;
                    break;
                case FileBrowserMode.Folder:
                    okButton.Content = "Select this folder";
                    break;
                case FileBrowserMode.Save:
                    okButton.Content = "Save here";
                    break;
            }
        }

        private void GoUp()
        {
            string? parent = Path.GetDirectoryName(currentDir.TrimEnd('/'));
            if (!string.IsNullOrEmpty(parent))
                NavigateTo(parent);
        }

        private void NavigateTo(string dir)
        {
            if (!Directory.Exists(dir))
            {
                statusText.Text = "Folder does not exist: " + dir;
                return;
            }
            currentDir = dir;
            lastDirectory = dir;
            selected.Clear();
            Refresh();
        }

        private bool Matches(string fileName)
        {
            if (patterns.Count == 0)
                return true;
            foreach (string p in patterns)
            {
                if (p.StartsWith("*.") && fileName.EndsWith(p.Substring(1), StringComparison.OrdinalIgnoreCase))
                    return true;
                if (string.Equals(p, fileName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private void Refresh()
        {
            pathBox.Text = currentDir;
            listPanel.Children.Clear();
            statusText.Text = "";
            UpdateOkButton();

            List<string> dirs;
            List<string> files = new List<string>();
            try
            {
                dirs = Directory.EnumerateDirectories(currentDir).OrderBy(d => d, StringComparer.OrdinalIgnoreCase).ToList();
                if (mode != FileBrowserMode.Folder)
                    files = Directory.EnumerateFiles(currentDir).Where(f => Matches(Path.GetFileName(f)))
                        .OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToList();
            }
            catch (Exception ex)
            {
                dirs = new List<string>();
                statusText.Text = "Can't read this folder: " + ex.Message;
                AddPermissionHint();
                return;
            }

            if (dirs.Count == 0 && files.Count == 0)
            {
                statusText.Text = "(empty)";
                if (AppPaths.HasStorageAccess != null && !AppPaths.HasStorageAccess())
                    AddPermissionHint();
            }

            foreach (string d in dirs)
            {
                string dir = d;
                var btn = MakeRow("\U0001F4C1  " + Path.GetFileName(dir));
                btn.Click += (_, _) => NavigateTo(dir);
                listPanel.Children.Add(btn);
            }

            foreach (string f in files)
            {
                string file = f;
                string label = Path.GetFileName(file) + "   (" + FormatSize(file) + ")";
                if (mode == FileBrowserMode.OpenFiles && allowMultiple)
                {
                    var tb = new ToggleButton
                    {
                        Content = label,
                        MinHeight = 44,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        HorizontalContentAlignment = HorizontalAlignment.Left,
                        IsChecked = selected.Contains(file)
                    };
                    tb.Click += (_, _) =>
                    {
                        if (tb.IsChecked == true) selected.Add(file); else selected.Remove(file);
                        UpdateOkButton();
                    };
                    listPanel.Children.Add(tb);
                }
                else
                {
                    var btn = MakeRow("\U0001F4C4  " + label);
                    btn.Click += (_, _) =>
                    {
                        if (mode == FileBrowserMode.Save)
                        {
                            nameBox.Text = Path.GetFileName(file);
                        }
                        else
                        {
                            Close(new string[] { file });
                        }
                    };
                    listPanel.Children.Add(btn);
                }
            }
        }

        private static Button MakeRow(string text)
        {
            return new Button
            {
                Content = text,
                MinHeight = 44,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Background = Brushes.Transparent
            };
        }

        private void AddPermissionHint()
        {
            if (AppPaths.RequestStorageAccess == null)
                return;
            var grant = new Button
            {
                Content = "Grant \"All files access\"",
                MinHeight = 48,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            grant.Click += (_, _) => AppPaths.RequestStorageAccess!();
            listPanel.Children.Add(grant);
        }

        private static string FormatSize(string path)
        {
            try
            {
                long len = new FileInfo(path).Length;
                if (len < 1024) return len + " B";
                if (len < 1024 * 1024) return (len / 1024.0).ToString("0.#") + " KB";
                if (len < 1024L * 1024 * 1024) return (len / (1024.0 * 1024)).ToString("0.#") + " MB";
                return (len / (1024.0 * 1024 * 1024)).ToString("0.##") + " GB";
            }
            catch
            {
                return "?";
            }
        }

        private void CreateFolder()
        {
            string name = (nameBox.Text ?? "").Trim();
            if (name.Length == 0 || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                statusText.Text = "Type a valid folder name first.";
                return;
            }
            try
            {
                Directory.CreateDirectory(Path.Combine(currentDir, name));
                if (mode == FileBrowserMode.Folder)
                    nameBox.Text = "";
                Refresh();
            }
            catch (Exception ex)
            {
                statusText.Text = "Couldn't create folder: " + ex.Message;
            }
        }

        private void Confirm()
        {
            switch (mode)
            {
                case FileBrowserMode.OpenFiles:
                    if (selected.Count == 0)
                        return;
                    Close(selected.OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToArray());
                    break;
                case FileBrowserMode.Folder:
                    Close(new string[] { currentDir });
                    break;
                case FileBrowserMode.Save:
                    string name = (nameBox.Text ?? "").Trim();
                    if (name.Length == 0 || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                    {
                        statusText.Text = "Type a valid file name.";
                        return;
                    }
                    Close(new string[] { Path.Combine(currentDir, name) });
                    break;
            }
        }
    }
}
#endif
