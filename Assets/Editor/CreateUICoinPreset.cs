// Assets/Editor/CreateUICoinPreset.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class CreateUICoinPreset : MonoBehaviour
{
    const string Root = "Assets/UI/Coin";
    const string SpritePath = Root + "/CoinGold.png";
    const string PrefabPath = Root + "/CoinIcon.prefab";

    [MenuItem("Tools/UI/Create Coin Icon Preset")]
    public static void CreateCoin()
    {
        // 1) 폴더 보장
        if (!AssetDatabase.IsValidFolder("Assets/UI")) AssetDatabase.CreateFolder("Assets", "UI");
        if (!AssetDatabase.IsValidFolder(Root)) AssetDatabase.CreateFolder("Assets/UI", "Coin");

        // 2) 256x256 코인 텍스처 생성 (라디얼 그라데이션 + 테두리)
        int size = 256;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        var px = new Color[size * size];

        // 색상 팔레트 (원하면 취향대로 바꿔도 됨)
        Color cCenter = new Color(1.00f, 0.86f, 0.45f, 1f); // 가운데
        Color cEdge   = new Color(0.85f, 0.66f, 0.20f, 1f); // 가장자리
        Color cRimHi  = new Color(1.00f, 0.92f, 0.55f, 1f); // 외곽 하이라이트

        Vector2 mid = new Vector2((size-1)/2f, (size-1)/2f);
        float rMax = mid.x; // 반지름

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int i = x + y * size;
                Vector2 p = new Vector2(x, y);
                float r = Vector2.Distance(p, mid) / rMax; // 0~1
                if (r > 1f) { px[i] = new Color(0,0,0,0); continue; } // 원 밖은 투명

                // 기본 라디얼 그라데이션
                Color col = Color.Lerp(cCenter, cEdge, Mathf.SmoothStep(0f, 1f, r));

                // 외곽 림 밝게
                float rim = Mathf.SmoothStep(0.85f, 1f, r);
                col = Color.Lerp(col, cRimHi, rim * 0.5f);

                // 간단 스펙 하이라이트 (좌상단 쪽 타원형)
                Vector2 h = (p - (mid + new Vector2(-size*0.18f, size*0.18f))) / (size*0.42f);
                float hlen = h.x*h.x + h.y*h.y;
                float highlight = Mathf.Clamp01(1f - hlen);
                col = Color.Lerp(col, Color.white, highlight * 0.18f);

                px[i] = col;
            }
        }
        tex.SetPixels(px);
        tex.Apply(false, false);

        // 3) PNG로 저장
        File.WriteAllBytes(SpritePath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        // 4) 스프라이트 임포트 세팅
        AssetDatabase.ImportAsset(SpritePath, ImportAssetOptions.ForceUpdate);
        var importer = (TextureImporter)AssetImporter.GetAtPath(SpritePath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Bilinear;
        importer.spritePixelsPerUnit = 100;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        AssetDatabase.WriteImportSettingsIfDirty(SpritePath);
        AssetDatabase.ImportAsset(SpritePath, ImportAssetOptions.ForceUpdate);

        // 5) 프리팹 생성 (UI Image가 스프라이트를 사용)
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        var go = new GameObject("CoinIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(64, 64);
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;

        PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
        GameObject.DestroyImmediate(go);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("완료", "코인 아이콘 프리셋 생성됨:\n" + PrefabPath, "OK");
    }
}
#endif
