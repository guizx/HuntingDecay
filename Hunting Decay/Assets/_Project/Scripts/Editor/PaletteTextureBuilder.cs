using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HuntingDecay.Editor
{
    /// <summary>
    /// Builds a palette strip (1 pixel per unique color, sorted by luminance) from a limited-color image,
    /// so the dither shader uses exactly the artist's colors.
    /// </summary>
    public static class PaletteTextureBuilder
    {
        const string OutputFolder = "Assets/_Project/Art/Palettes";
        const int MaxColors = 64; // Must match MAX_PALETTE_SIZE in PaletteDither.shader.

        [MenuItem("Assets/Hunting Decay/Build Palette From Image")]
        static void BuildFromSelection() => Build(AssetDatabase.GetAssetPath(Selection.activeObject));

        [MenuItem("Assets/Hunting Decay/Build Palette From Image", true)]
        static bool CanBuildFromSelection() => Selection.activeObject is Texture2D;

        public static string Build(string sourcePath)
        {
            // Read the file itself, so import settings (compression, max size) can't alter the colors.
            var source = new Texture2D(2, 2);
            source.LoadImage(File.ReadAllBytes(sourcePath));
            Color32[] colors = source.GetPixels32()
                .Select(c => new Color32(c.r, c.g, c.b, 255))
                .Distinct()
                .OrderBy(c => 0.299f * c.r + 0.587f * c.g + 0.114f * c.b)
                .ToArray();
            Object.DestroyImmediate(source);

            if (colors.Length > MaxColors)
            {
                Debug.LogError($"{sourcePath} has {colors.Length} unique colors. The dither shader supports up to {MaxColors}.");
                return null;
            }

            var palette = new Texture2D(colors.Length, 1, TextureFormat.RGBA32, false);
            palette.SetPixels32(colors);
            Directory.CreateDirectory(OutputFolder);
            string outputPath = $"{OutputFolder}/{Path.GetFileNameWithoutExtension(sourcePath)}Palette.png";
            File.WriteAllBytes(outputPath, palette.EncodeToPNG());
            Object.DestroyImmediate(palette);

            AssetDatabase.ImportAsset(outputPath);
            var importer = (TextureImporter)AssetImporter.GetAtPath(outputPath);
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = false; // The shader compares raw sRGB values.
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();

            Debug.Log($"Built {outputPath} with {colors.Length} colors from {sourcePath}.");
            return outputPath;
        }
    }
}
