using AssetsTools.NET;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using System.IO;
using System.Text;
#if !ANDROID
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;
using UABEAvalonia.TextHighlighting;
#endif

namespace UABEAvalonia
{
    public partial class EditDataWindow : Window
    {
        private AssetImportExport impexp;

        public EditDataWindow()
        {
            InitializeComponent();
#if DEBUG && !ANDROID
            this.AttachDevTools();
#endif
            //generated events
            btnOk.Click += BtnOk_Click;
            btnCancel.Click += BtnCancel_Click;
        }

        public EditDataWindow(AssetTypeValueField baseField) : this()
        {
            using MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);
            
            impexp = new AssetImportExport();
            impexp.DumpTextAsset(sw, baseField);

            sw.Flush();
            ms.Position = 0;

            string str = Encoding.UTF8.GetString(ms.ToArray());
#if !ANDROID
            var registryOptions = new UABEDumpRegistryOptions(ThemeHandler.UseDarkTheme ? ThemeName.DarkPlus : ThemeName.LightPlus);
            var textMateInstallation = textEditor.InstallTextMate(registryOptions);
            textMateInstallation.SetGrammar("source.utxt");
#endif
            textEditor.Load(ms);
        }

        private async void BtnOk_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            string text = textEditor.Text;
            using MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(text));
            StreamReader sr = new StreamReader(ms);
            byte[]? data = impexp.ImportTextAsset(sr, out string? exceptionMessage);
            if (data == null)
            {
                await MessageBoxUtil.ShowDialog(this, "Compile Error", "Problem with import:\n" + exceptionMessage);
                return;
            }

            Close(data);
        }

        private void BtnCancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close(null);
        }
    }
}
