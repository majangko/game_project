using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.UI;
using System.IO;

public class CreateTorchLightPreset : MonoBehaviour
{
    private const string RootFolder = "Assets/UI/TorchLight";
    private const string SpritePath = RootFolder + "/GradientLight.png";
    private const string AnimPath = RootFolder + "/TorchLightFlicker.anim";
    private const string ControllerPath = RootFolder + "/TorchLight.controller";
    private const string PrefabPath = RootFolder + "/TorchLightOverlay.prefab";

    [MenuItem("Tools/Create TorchLight Overlay Preset")]
    public static void CreatePreset()
    {
        if (!AssetDatabase.IsValidFolder("Assets/UI"))
            AssetDatabase.CreateFolder("Assets", "UI");
        if (!AssetDatabase.IsValidFolder(RootFolder))
            AssetDatabase.CreateFolder("Assets/UI", "TorchLight");

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        if (sprite == null)
        {
            EditorUtility.DisplayDialog(
                "GradientLight.png 필요",
                "Assets/UI/TorchLight/GradientLight.png 를 먼저 임포트해 주세요.\n(텍스처 타입: Sprite(2D and UI))",
                "확인");
            return;
        }

        var clip = new AnimationClip { name = "TorchLightFlicker", frameRate = 60 };

        var alphaCurve = new AnimationCurve();
        alphaCurve.AddKey(new Keyframe(0.0f, 0.40f));
        alphaCurve.AddKey(new Keyframe(0.5f, 0.80f));
        alphaCurve.AddKey(new Keyframe(1.0f, 0.40f));

        var scaleX = new AnimationCurve();
        scaleX.AddKey(new Keyframe(0.0f, 1.00f));
        scaleX.AddKey(new Keyframe(0.5f, 1.08f));
        scaleX.AddKey(new Keyframe(1.0f, 1.00f));

        var scaleY = new AnimationCurve();
        scaleY.AddKey(new Keyframe(0.0f, 1.00f));
        scaleY.AddKey(new Keyframe(0.5f, 0.94f));
        scaleY.AddKey(new Keyframe(1.0f, 1.00f));

        var colorCurveBinding = EditorCurveBinding.FloatCurve("", typeof(Graphic), "m_Color.a");
        var scaleXBinding = EditorCurveBinding.FloatCurve("", typeof(RectTransform), "m_LocalScale.x");
        var scaleYBinding = EditorCurveBinding.FloatCurve("", typeof(RectTransform), "m_LocalScale.y");

        AnimationUtility.SetEditorCurve(clip, colorCurveBinding, alphaCurve);
        AnimationUtility.SetEditorCurve(clip, scaleXBinding, scaleX);
        AnimationUtility.SetEditorCurve(clip, scaleYBinding, scaleY);

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, AnimPath);

        var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        var stateMachine = controller.layers[0].stateMachine;
        var state = stateMachine.AddState("Flicker");
        state.motion = clip;
        stateMachine.defaultState = state;

        var go = new GameObject("TorchLightOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Animator));
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(560, 560);

        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.raycastTarget = false;
        img.color = new Color(1f, 0.78f, 0.59f, 0.45f);

        var anim = go.GetComponent<Animator>();
        anim.runtimeAnimatorController = controller;

        PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
        GameObject.DestroyImmediate(go);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("완료", "TorchLightOverlay 프리셋이 생성되었습니다.\n\n경로:\n" + PrefabPath, "좋아요");
    }
}
