// Scripts/UI/HUD/GoldUI.cs
using TMPro;
using UnityEngine;

public class GoldUI : MonoBehaviour
{
    public TMP_Text goldText;
    void OnEnable(){ Refresh(); }
    public void Refresh(){
        if (goldText) goldText.text = GameState.I.player.gold.ToString();
    }
    // 예: 골드가 바뀌는 곳에서 FindObjectOfType<GoldUI>()?.Refresh();
}
