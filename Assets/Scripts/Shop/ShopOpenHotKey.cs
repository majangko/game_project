// Assets/Scripts/Shop/ShopOpenHotkey.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopOpenHotkey : MonoBehaviour
{
    public string shopSceneName = "Shop";
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F9))
        {
            Debug.Log("[ShopOpenHotkey] F9 pressed, loading Shop additively...");
            SceneManager.LoadSceneAsync(shopSceneName, LoadSceneMode.Additive);
        }
    }
}
