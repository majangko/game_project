using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RebindButton : MonoBehaviour
{
    public GameAction action;                     // 이 버튼이 담당할 액션
    public TextMeshProUGUI label;                 // 버튼 안의 TMP 텍스트

    Button _btn;
    bool _waiting;

    void Awake()
    {
        _btn = GetComponent<Button>();
        _btn.onClick.AddListener(BeginRebind);
        RefreshLabel();
    }

    void OnEnable() => RefreshLabel();

    void BeginRebind()
    {
        if (_waiting) return;
        StartCoroutine(WaitForKey());
    }

    IEnumerator WaitForKey()
    {
        _waiting = true;
        var old = label.text;
        label.text = "...";

        // 키가 올라가 있는 상태에서 시작
        yield return null;

        KeyCode captured = KeyCode.None;
        while (captured == KeyCode.None)
        {
            // 모든 KeyCode 순회 체크 (마우스 제외하고 싶으면 필터링)
            foreach (KeyCode k in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (k == KeyCode.None) continue;
                if (Input.GetKeyDown(k))
                {
                    captured = k;
                    break;
                }
            }
            yield return null;
        }

        // 중복 방지(원하면 막고, 아니면 제거해도 됨)
        if (KeyBindingManager.Instance.IsUsing(captured))
        {
            // 이미 쓰는 키면 이전 텍스트로 복구
            label.text = old;
        }
        else
        {
            KeyBindingManager.Instance.SetKey(action, captured);
            label.text = KeyToString(captured);
        }
        _waiting = false;
    }

    void RefreshLabel()
    {
        if (label)
            label.text = KeyToString(KeyBindingManager.Instance.GetKey(action));
    }

    string KeyToString(KeyCode k) => k.ToString().ToUpper();
}
