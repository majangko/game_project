#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;

public static class TMPBulkAssign
{
    [MenuItem("Tools/TextMeshPro/Assign KR_Main_SDF To All TMP In Scene")]
    public static void AssignKRToAll()
    {
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/KR_Main_SDF.asset");
        if (!font) { Debug.LogError("KR_Main_SDF.asset 경로/이름 확인!"); return; }

        int count = 0;
        foreach (var tmp in GameObject.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (tmp.font != font) { tmp.font = font; count++; EditorUtility.SetDirty(tmp); }
        }
        Debug.Log($"[TMPBulkAssign] 교체 완료: {count}개");
    }
}
#endif
