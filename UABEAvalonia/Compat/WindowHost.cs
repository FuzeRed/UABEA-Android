#if ANDROID
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace UABEAvalonia
{
    /// <summary>
    /// Single-view container that stacks "windows". The top window is shown full screen
    /// below a small title bar with a back button (also bound to the Android back key).
    /// </summary>
    public class WindowHost : Grid
    {
        public static WindowHost? Current { get; set; }

        private readonly List<Window> stack = new List<Window>();
        private readonly TextBlock titleText;
        private readonly Button backButton;
        private readonly Border body;

        public event Action? ExitRequested;

        public WindowHost()
        {
            Current = this;

            RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            RowDefinitions.Add(new RowDefinition(GridLength.Star));

            backButton = new Button
            {
                Content = "\u2190",
                MinWidth = 44,
                MinHeight = 44,
                FontSize = 20,
                VerticalAlignment = VerticalAlignment.Center
            };
            backButton.Click += (_, _) => HandleBack();

            titleText = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16,
                FontWeight = FontWeight.SemiBold,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Margin = new Thickness(8, 0, 8, 0)
            };

            var bar = new DockPanel { Margin = new Thickness(4, 2, 4, 2) };
            DockPanel.SetDock(backButton, Dock.Left);
            bar.Children.Add(backButton);
            bar.Children.Add(titleText);

            var barBorder = new Border
            {
                Child = bar,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(0, 0, 0, 1)
            };
            Grid.SetRow(barBorder, 0);
            Children.Add(barBorder);

            body = new Border();
            Grid.SetRow(body, 1);
            Children.Add(body);
        }

        public Window? Top => stack.Count > 0 ? stack[stack.Count - 1] : null;

        internal void Push(Window w)
        {
            w.Host = this;
            // windows have fixed Width/Height in the shared XAML (desktop sizes). Remember
            // them, then let the window fill the screen instead.
            w.DesignWidth = w.Width;
            w.DesignHeight = w.Height;
            w.ClearValue(WidthProperty);
            w.ClearValue(HeightProperty);
            w.HorizontalAlignment = HorizontalAlignment.Stretch;
            w.VerticalAlignment = VerticalAlignment.Stretch;
            // very wide windows (asset list etc.) scroll sideways instead of being squashed
            if (!double.IsNaN(w.DesignWidth) && w.DesignWidth > 500)
                w.MinWidth = Math.Min(w.DesignWidth, 900);

            stack.Remove(w);
            stack.Add(w);
            ShowTop();
        }

        internal void BringToFront(Window w)
        {
            if (!stack.Contains(w))
                return;
            stack.Remove(w);
            stack.Add(w);
            ShowTop();
        }

        internal void RefreshTitle(Window w)
        {
            if (w == Top)
                titleText.Text = w.Title ?? "UABEA";
        }

        private void ShowTop()
        {
            Window? top = Top;
            if (top == null)
            {
                body.Child = null;
                return;
            }

            // detach from any previous parent
            if (top.Parent is ScrollViewer oldSv)
                oldSv.Content = null;
            else if (top.Parent is Border oldB)
                oldB.Child = null;

            var sv = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = top
            };
            body.Child = sv;
            titleText.Text = top.Title ?? "UABEA";
            backButton.IsVisible = stack.Count > 1 || true;
        }

        internal void CloseWindow(Window w, object? result)
        {
            if (w.IsClosed || !stack.Contains(w))
                return;

            // window may veto (e.g. "unsaved changes" prompts)
            if (w.RaiseClosing())
                return;

            bool wasTop = w == Top;
            w.IsClosed = true;
            stack.Remove(w);

            if (wasTop)
            {
                if (w.Parent is ScrollViewer sv)
                    sv.Content = null;
                ShowTop();
            }

            w.Host = null;
            w.RaiseClosed(result);

            if (stack.Count == 0)
                ExitRequested?.Invoke();
        }

        /// <summary>Android back key / back arrow. Returns true if handled.</summary>
        public bool HandleBack()
        {
            Window? top = Top;
            if (top == null)
                return false;
            top.Close();
            return true;
        }
    }
}
#endif
