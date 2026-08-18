using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace NinjaBattle.UI.Editor
{
    /// <summary>
    /// Packs the small, reusable Figma UI assets. Large full-screen illustrations
    /// deliberately stay out of this atlas so they don't inflate its memory page.
    /// </summary>
    public static class FigmaItemAtlasBuilder
    {
        private const string ItemsFolder = "Assets/Art/FigmaItemAtlas";
        private const string AtlasPath = ItemsFolder + "/FigmaUIItems.spriteatlas";

        private static readonly string[] AtlasItems =
        {
            "figma_item_01.png", "figma_item_02.png", "figma_item_04.png",
            "figma_item_05.png", "figma_item_06.png", "figma_item_08.png",
            "figma_item_10.png", "figma_item_13.png", "figma_item_15.png",
            "figma_item_16.png", "figma_item_17.png", "figma_item_18.png",
            "figma_item_19.png", "figma_item_20.png"
        };

        [MenuItem("Tools/NinjaBattle/UI/Build Figma Item Atlas")]
        public static void Build()
        {
            var packables = new List<Object>();
            foreach (string fileName in AtlasItems)
            {
                string path = ItemsFolder + "/" + fileName;
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null || importer.textureType != TextureImporterType.Sprite)
                {
                    Debug.LogWarning($"Figma item atlas skipped non-sprite asset: {path}");
                    continue;
                }

                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture != null)
                    packables.Add(texture);
            }

            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AtlasPath);
            if (atlas == null)
            {
                atlas = new SpriteAtlas();
                AssetDatabase.CreateAsset(atlas, AtlasPath);
            }

            var existing = atlas.GetPackables();
            if (existing != null && existing.Length > 0)
                SpriteAtlasExtensions.Remove(atlas, existing);
            SpriteAtlasExtensions.Add(atlas, packables.ToArray());
            atlas.SetPackingSettings(new SpriteAtlasPackingSettings
            {
                enableRotation = false,
                enableTightPacking = false,
                padding = 4
            });
            atlas.SetTextureSettings(new SpriteAtlasTextureSettings
            {
                generateMipMaps = false,
                readable = false,
                sRGB = true,
                filterMode = FilterMode.Bilinear
            });
            atlas.SetPlatformSettings(new TextureImporterPlatformSettings
            {
                name = "Android",
                overridden = true,
                maxTextureSize = 2048,
                format = TextureImporterFormat.ASTC_6x6,
                compressionQuality = 50
            });
            atlas.SetIncludeInBuild(true);
            EditorUtility.SetDirty(atlas);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Figma UI atlas built with {packables.Count} reusable item sprites.");
        }

    }
}
