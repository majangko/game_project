using UnityEngine;
using TMPro;

public class SkillTooltipUI : MonoBehaviour
{
    public static SkillTooltipUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private TextMeshProUGUI extraText;

    private RectTransform rect;

    void Awake()
    {
        Instance = this;
        rect = GetComponent<RectTransform>();
        Hide();
    }



    public void Show(string name, string desc, string extra = "")
    {
        nameText.text = name;
        descText.text = desc;
        extraText.text = extra;

        canvasGroup.alpha = 1;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        canvasGroup.alpha = 0;
        gameObject.SetActive(false);
    }
}
