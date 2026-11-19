# UABEA (Android port)

This repo contains the original desktop UABEA plus an **Android port** in `UABEAvalonia.Android/`.
Same code base: the Android app compiles the desktop sources and the four plugins (TextAsset, Font,
AudioClip, Texture) directly into the APK.

## Getting the APK

Push to GitHub. The workflow in `.github/workflows/android.yml` builds the APK; download it from
the run's **Artifacts** (`UABEA-android-apk`) on the Actions tab, then install it on your phone.

Local build (needs .NET 8 SDK + `dotnet workload install android`):

    dotnet publish UABEAvalonia.Android/UABEAvalonia.Android.csproj -c Release -f net8.0-android

## How it differs from desktop

- **Windows**: Android has no windows, so every UABEA window/dialog is stacked full-screen inside one
  view (`Compat/Window.cs`, `Compat/WindowHost.cs`). The back arrow / Android back key closes the top one.
  Landscape is recommended for the asset list.
- **Files**: UABEA works on real paths, so the system document picker (content:// URIs) isn't used.
  An in-app file browser (`Compat/FileBrowserWindow.cs`) is used and the app asks for
  *All files access*. Grant it when prompted (or from the browser's "Grant" button).
- **Plugins** are compiled in rather than loaded from a `plugins` folder.
- `classdata.tpk` and `config.json` live in the app's private storage.
- **Not available**: command line mode, drag & drop, syntax highlighting in the dump editor
  (needs a native lib), and the native texture encoders/decoders used for some formats
  (crunch, PVRTC, ASTC/ETC via ISPC/PVRTexLib; `textoolwrap` has no Android build). Other
  texture formats, text, fonts and audio work.

---

# UABEA

## UABEANext (https://github.com/nesrak1/UABEANext)

[Latest Nightly (Windows)](https://nightly.link/nesrak1/UABEANext/workflows/build-windows/master/uabea-windows.zip) | [Latest Nightly (Linux)](https://nightly.link/nesrak1/UABEANext/workflows/build-ubuntu/master/uabea-ubuntu.zip)

The new version of UABEA with docking and multi-bundle opening. Still a work in progress.

## UABEA original

[Latest Nightly (Windows)](https://nightly.link/nesrak1/UABEA/workflows/dotnet-desktop/master/uabea-windows.zip) | [Latest Nightly (Linux)](https://nightly.link/nesrak1/UABEA/workflows/dotnet-ubuntu/master/uabea-ubuntu.zip) | [Latest Release](https://github.com/nesrak1/UABEA/releases)

[![GitHub issues](https://img.shields.io/github/issues/nesrak1/UABEA?logo=GitHub&style=flat-square)](https://github.com/nesrak1/UABEA/issues) [![discord](https://img.shields.io/discord/862035581491478558?label=discord&logo=discord&logoColor=FFFFFF&style=flat-square)](https://discord.gg/hd9VdswwZs)

Cross-platform Asset Bundle/Serialized File reader and writer. Originally based on (but not a fork of) [UABE](https://github.com/SeriousCache/UABE).

## Extracting assets

I develop UABEA as more of a modding/research tool than an extracting tool. Use [AssetRipper](https://github.com/AssetRipper/AssetRipper) or [AssetStudio](https://github.com/Perfare/AssetStudio/) if you only want to extract assets.

## Addressables

Many games are also now using addressables. You can tell if the bundle you're opening is part of addressables because it has the path `StreamingAssets/aa/XXX/something.bundle`. [If you want to edit these bundles, you will need to clear the CRC checks with the CRC cleaning tool here](https://github.com/nesrak1/AddressablesTools/releases). Use `Example patchcrc catalog.json`, then move or rename the old catalog.json file and rename catalog.json.patched to catalog.json.

## Libraries

- [Avalonia](https://github.com/AvaloniaUI/Avalonia) (MIT license)
  - [Dock.Avalonia](https://github.com/wieslawsoltes/Dock) (MIT license)
  - [AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit) (MIT license)
- [AssetsTools.NET](https://github.com/nesrak1/AssetsTools.NET/tree/upd21-with-inst) (MIT license)
  - [Cpp2IL](https://github.com/SamboyCoding/Cpp2IL) (MIT license)
  - [Mono.Cecil](https://github.com/jbevain/cecil) (MIT license)
  - [AssetRipper.TextureDecoder](https://github.com/AssetRipper/TextureDecoder) (MIT license)
- [ISPC Texture Compressor](https://github.com/GameTechDev/ISPCTextureCompressor) (MIT license)
- [Unity crnlib](https://github.com/Unity-Technologies/crunch/tree/unity) (zlib license)
- [PVRTexLib](https://developer.imaginationtech.com/pvrtextool) (PVRTexTool license)
- [ImageSharp](https://github.com/SixLabors/ImageSharp) (Apache License 2.0)
- [Fsb5Sharp](https://github.com/SamboyCoding/Fmod5Sharp) (MIT license)
- [Font Awesome](https://fontawesome.com) (CC BY 4.0 license)
