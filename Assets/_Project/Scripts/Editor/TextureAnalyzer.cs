#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace OtsimoColoring.EditorTools
{
    /// <summary>
    /// Quick brightness histogram printer for line-art textures.
    /// Right-click a Texture2D in the Project window and choose
    /// "OtsimoColoring/Analyze Brightness Histogram".
    /// </summary>
    public static class TextureAnalyzer
    {
        [MenuItem("Assets/OtsimoColoring/Analyze Brightness Histogram", true)]
        private static bool ValidateAnalyze()
        {
            return Selection.activeObject is Texture2D;
        }

        [MenuItem("Assets/OtsimoColoring/Analyze Brightness Histogram")]
        private static void Analyze()
        {
            var tex = Selection.activeObject as Texture2D;
            if (tex == null)
            {
                Debug.LogError("Select a Texture2D first.");
                return;
            }

            // Make sure the texture is readable.
            var path = AssetDatabase.GetAssetPath(tex);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && !importer.isReadable)
            {
                Debug.LogError($"{tex.name}: enable Read/Write on the texture import settings first.");
                return;
            }

            var pixels = tex.GetPixels32();
            int total = pixels.Length;

            // 10 buckets: 0.0-0.1, 0.1-0.2, ... 0.9-1.0
            int[] buckets = new int[10];
            int totalAlpha = 0;
            int zeroAlphaCount = 0;

            for (int i = 0; i < total; i++)
            {
                var p = pixels[i];
                totalAlpha += p.a;
                if (p.a == 0) zeroAlphaCount++;

                float brightness = (p.r + p.g + p.b) / (3f * 255f);
                int bucket = Mathf.Clamp(Mathf.FloorToInt(brightness * 10f), 0, 9);
                buckets[bucket]++;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"=== {tex.name} ({tex.width}x{tex.height}, {total} pixels) ===");
            sb.AppendLine($"Average alpha: {(float)totalAlpha / total:F1} / 255");
            sb.AppendLine($"Pixels with alpha=0: {zeroAlphaCount} ({(float)zeroAlphaCount / total * 100f:F1}%)");
            sb.AppendLine("Brightness histogram (0.0 = black, 1.0 = white):");
            for (int i = 0; i < 10; i++)
            {
                float pct = (float)buckets[i] / total * 100f;
                int barLen = Mathf.Min(50, Mathf.RoundToInt(pct / 2f));
                string bar = new string('#', barLen);
                sb.AppendLine($"  {i / 10f:F1}-{(i + 1) / 10f:F1} : {buckets[i],8} ({pct,5:F2}%) {bar}");
            }

            Debug.Log(sb.ToString());
        }
    }
}
#endif