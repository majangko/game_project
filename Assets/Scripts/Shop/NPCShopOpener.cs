// Assets/Scripts/Interaction/NPCShopOpener.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class NPCShopOpener : MonoBehaviour
{
    [Tooltip("Additive로 열 Shop 씬 이름 (Build Settings에 등록 필요)")]
    public string shopSceneName = "Shop";

    [Header("UI")]
    public TMP_Text promptText; // "E: 상점 열기" 안내문 (선택)

    private bool _playerIn;

    private void Start()
    {
        if (promptText) promptText.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerIn = true;
        if (promptText) promptText.gameObject.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerIn = false;
        if (promptText) promptText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_playerIn && Input.GetKeyDown(KeyCode.E))
        {
            SceneManager.LoadSceneAsync(shopSceneName, LoadSceneMode.Additive);
            if (promptText) promptText.gameObject.SetActive(false);
        }
    }
}
