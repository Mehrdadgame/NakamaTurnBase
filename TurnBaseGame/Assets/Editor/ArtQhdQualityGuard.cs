using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

internal static class ArtQhdQualityGuard
{
    private static readonly HashSet<string> ImageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".tga", ".psd", ".psb", ".tif", ".tiff", ".bmp"
    };

    public static void Run()
    {
        var changed = new List<string>();
        string[] paths = AssetDatabase.GetAllAssetPaths();

        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (string path in paths)
            {
                if (!path.StartsWith("Assets/", StringComparison.Ordinal) ||
                    !ImageExtensions.Contains(Path.GetExtension(path)))
                    continue;

                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null || importer.textureType != TextureImporterType.Sprite)
                    continue;

                importer.GetSourceTextureWidthAndHeight(out int width, out int height);
                int shortSide = Mathf.Min(width, height);
                int longSide = Mathf.Max(width, height);
                if (shortSide < 900 || longSide < 1800)
                    continue;

                // Preserve near-source resolution for portrait/landscape screen art on QHD phones.
                importer.maxTextureSize = 4096;
                TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
                android.name = "Android";
                android.overridden = true;
                android.maxTextureSize = 4096;
                android.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
                android.format = TextureImporterFormat.ASTC_5x5;
                android.textureCompression = TextureImporterCompression.CompressedHQ;
                android.compressionQuality = 100;
                android.crunchedCompression = false;
                android.allowsAlphaSplitting = false;
                importer.SetPlatformTextureSettings(android);

                if (AssetDatabase.WriteImportSettingsIfDirty(path))
                    changed.Add(path);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        foreach (string path in changed)
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        AssetDatabase.SaveAssets();
        Debug.Log($"QHD quality guard applied to {changed.Count} full-screen sprites.");
        EditorApplication.Exit(0);
    }
}
