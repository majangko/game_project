#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public static class CreateRoundedButtonSprite
{
    [MenuItem("Tools/Art/Create Rounded Button Sprites")]
    public static void Create()
    {
        int  width  = 640;
        int  height = 160;
        int  radius = 42;
        int  border = 32;
        Color baseCol   = FromHex("#C68B4B");
        Color lightCol  = FromHex("#E2AE6F");
        Color shadowCol = FromHex("#7E532C");
        float aa = 1.5f;

        string folder = "Assets/UI/Sprites/Buttons";
        Directory.CreateDirectory(folder);

        var texNormal  = MakeRoundedButton(width, height, radius, baseCol, lightCol, shadowCol, aa, 1.0f);
        SaveSprite(texNormal, Path.Combine(folder, "Button_Rounded_Normal.png"), border);

        var texPressed = MakeRoundedButton(width, height, radius, baseCol * 0.85f, lightCol * 0.9f, shadowCol * 1.1f, aa, 0.95f);
        SaveSprite(texPressed, Path.Combine(folder, "Button_Rounded_Pressed.png"), border);

        AssetDatabase.Refresh();
        Debug.Log("[RoundedButton] Created sprites in " + folder);
    }

    static Texture2D MakeRoundedButton(int w, int h, int r, Color baseCol, Color topCol, Color bottomShadow, float aa, float globalMul)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.name = "RoundedBtn";

        float rr = Mathf.Max(2, r);
        float aaWidth = Mathf.Max(0.5f, aa);

        for (int y = 0; y < h; y++)
        {
            float gy = y + 0.5f;
            for (int x = 0; x < w; x++)
            {
                float gx = x + 0.5f;

                float dx = Mathf.Min(gx, w - gx);
                float dy = Mathf.Min(gy, h - gy);
                float dist;

                if (dx >= rr || dy >= rr)
                {
                    dist = Mathf.Min(dx - rr, dy - rr);
                }
                else
                {
                    float cx = rr - dx;
                    float cy = rr - dy;
                    dist = rr - Mathf.Sqrt(cx * cx + cy * cy);
                }

                float alpha = Mathf.Clamp01((dist + aaWidth) / (aaWidth));
                if (alpha <= 0f) { tex.SetPixel(x, y, Color.clear); continue; }

                float t = Mathf.InverseLerp(0, h - 1, y);
                Color grad = Color.Lerp(topCol, baseCol, Mathf.SmoothStep(0f, 1f, t));

                float sh = Mathf.SmoothStep(0.6f, 1.0f, t) * 0.15f;
                grad = Color.Lerp(grad, bottomShadow, sh);

                float edgeHi = Mathf.Clamp01((dist) / (rr * 0.6f));
                grad = Color.Lerp(Color.white, grad, 0.85f + 0.15f * edgeHi);

                grad.a = alpha;
                grad *= globalMul;

                tex.SetPixel(x, y, grad);
            }
        }
        tex.Apply(false, false);
        return tex;
    }

    static void SaveSprite(Texture2D tex, string path, int border)
    {
        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;

        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);

        importer.spriteBorder = new Vector4(border, border, border, border);
        importer.SaveAndReimport();
    }

    static Color FromHex(string hex)
    {
        Color c; if (ColorUtility.TryParseHtmlString(hex, out c)) return c;
        return Color.white;
    }
}
#endif
