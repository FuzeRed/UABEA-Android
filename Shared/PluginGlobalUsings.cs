// UABEA's plugin API uses UABEAvalonia.Window (a thin wrapper on desktop, a stacked
// "window" on Android) instead of Avalonia.Controls.Window. This makes plugin code that says
// "Window" resolve to it.
global using Window = UABEAvalonia.Window;
