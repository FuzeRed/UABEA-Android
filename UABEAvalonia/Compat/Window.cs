// Window abstraction.
//  - Desktop: a thin subclass of Avalonia's Window (nothing changes).
//  - Android: Avalonia has no Window on mobile (single view only), so this is a
//    UserControl that is stacked inside a WindowHost and behaves like a window
//    (Show / ShowDialog / Close(result) / Closing / Closed ...).
#if !ANDROID
namespace UABEAvalonia
{
    public class Window : Avalonia.Controls.Window
    {
    }
}
#else
using Avalonia.Controls;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace UABEAvalonia
{
    public class Window : UserControl
    {
        private TaskCompletionSource<object?>? tcs;
        internal bool IsClosed;
        internal WindowHost? Host;
        internal double DesignWidth = double.NaN;
        internal double DesignHeight = double.NaN;

        private string? title;
        public string? Title
        {
            get => title;
            set { title = value; Host?.RefreshTitle(this); }
        }

        // accepted so the shared XAML ("Icon", "CanResize", "WindowStartupLocation") still loads
        public string? Icon { get; set; }
        public bool CanResize { get; set; } = true;
        public WindowStartupLocation WindowStartupLocation { get; set; }
        public Window? Owner { get; internal set; }

        private PathStorageProvider? storageProvider;
        public PathStorageProvider StorageProvider => storageProvider ??= new PathStorageProvider(this);

        public event EventHandler<CancelEventArgs>? Closing;
        public event EventHandler? Closed;
        public event EventHandler? Opened;

        public void Show()
        {
            Display(null);
        }

        public void Show(Window owner)
        {
            Display(owner);
        }

        public Task ShowDialog(Window owner)
        {
            return Display(owner);
        }

        public async Task<T> ShowDialog<T>(Window owner)
        {
            object? result = await Display(owner);
            return result is T t ? t : default!;
        }

        /// <summary>Takes the window off screen without closing it (it can still be Close()d later).</summary>
        public void Hide()
        {
            Host?.HideWindow(this);
        }

        public void Activate()
        {
            Host?.BringToFront(this);
        }

        public void Close()
        {
            Close(null);
        }

        public void Close(object? dialogResult)
        {
            if (Host == null)
            {
                IsClosed = true;
                tcs?.TrySetResult(dialogResult);
                return;
            }
            Host.CloseWindow(this, dialogResult);
        }

        private Task<object?> Display(Window? owner)
        {
            WindowHost? host = owner?.Host ?? Host ?? WindowHost.Current;
            if (host == null)
                throw new InvalidOperationException("No WindowHost available to show window.");

            Owner = owner;
            if (Host == null || IsClosed)
            {
                IsClosed = false;
                tcs = new TaskCompletionSource<object?>();
                host.Push(this);
                Opened?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                host.BringToFront(this);
            }
            return tcs!.Task;
        }

        internal bool RaiseClosing()
        {
            var args = new CancelEventArgs();
            Closing?.Invoke(this, args);
            return args.Cancel;
        }

        internal void RaiseClosed(object? result)
        {
            Closed?.Invoke(this, EventArgs.Empty);
            tcs?.TrySetResult(result);
        }
    }
}
#endif
