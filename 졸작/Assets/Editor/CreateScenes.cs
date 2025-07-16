using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class CreateScenes : MonoBehaviour
{
    class SceneInfo
    {
        public string folder;
        public string name;

        public SceneInfo(string folder, string name)
        {
            this.folder = folder;
            this.name = name;
        }
    }

    [MenuItem("Tools/Auto Create Scenes")]
    static void CreateGameScenes()
    {
        string[] folders = {
            "Assets/Scenes/Core",
            "Assets/Scenes/Island",
            "Assets/Scenes/Stages",
            "Assets/Scenes/GameFlow"
        };

        // 폴더 생성
        foreach (var folder in folders)
        {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
        }

        // 씬 목록
        List<SceneInfo> scenes = new List<SceneInfo>
        {
            new SceneInfo("Core", "Main"),
            new SceneInfo("Core", "StoryIntro"),
            new SceneInfo("Island", "StartIsland"),
            new SceneInfo("Stages", "Stage01"),
            new SceneInfo("Stages", "Stage02"),
            new SceneInfo("Stages", "Stage03"),
            new SceneInfo("Stages", "Stage04"),
            new SceneInfo("Stages", "Stage05"),
            new SceneInfo("Stages", "Boss01"),
            new SceneInfo("Stages", "Boss02"),
            new SceneInfo("Stages", "Boss03"),
            new SceneInfo("Stages", "Boss04"),
            new SceneInfo("Stages", "LastBoss"),
            new SceneInfo("GameFlow", "GameOverResult"),
            new SceneInfo("GameFlow", "GameOver"),
            new SceneInfo("GameFlow", "GameClear")
        };

        // 씬 생성
        foreach (var sceneInfo in scenes)
        {
            string fullPath = $"Assets/Scenes/{sceneInfo.folder}/{sceneInfo.name}.unity";
            if (!File.Exists(fullPath))
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, fullPath);
                Debug.Log($"[✓] Created scene: {sceneInfo.name}");
            }
            else
            {
                Debug.LogWarning($"[!] Scene already exists: {sceneInfo.name}");
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("완료!", "모든 씬이 자동 생성되었습니다!", "확인");
    }
}
