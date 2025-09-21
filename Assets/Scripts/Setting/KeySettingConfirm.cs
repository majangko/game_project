using UnityEngine;
using UnityEngine.UI;

public class KeySettingConfirm : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject keySettingPanel;   // ��ü ���� �г� (���� �� ����)
    [SerializeField] private GameObject confirmPanel;      // Ȯ��â �г� (���)

    [Header("Buttons")]
    [SerializeField] private Button saveButton;            // ��� Save ��ư
    [SerializeField] private Button yesButton;             // Ȯ��â Yes
    [SerializeField] private Button noButton;              // Ȯ��â No

    private void Awake()
    {
        // ���� �� Ȯ��â�� ����
        if (confirmPanel != null) confirmPanel.SetActive(false);

        // ��ư �̺�Ʈ ����
        if (saveButton != null) saveButton.onClick.AddListener(OpenConfirm);
        if (yesButton != null) yesButton.onClick.AddListener(ConfirmYes);
        if (noButton != null) noButton.onClick.AddListener(ConfirmNo);
    }

    // Save Ŭ�� �� Ȯ��â ����
    private void OpenConfirm()
    {
        if (confirmPanel != null) confirmPanel.SetActive(true);
    }

    // ��(Yes) �� ���� ���� ���� �� Ȯ��â �ݰ� ����â�� �ݱ�
    private void ConfirmYes()
    {
        // TODO: ���� Ű���ε� ���� ������ ���⼭ ȣ��
        // ����) PlayerPrefs.Save();  �Ǵ� KeyBindingManager.Instance.Save();
        Debug.Log("[KeySetting] ���� �Ϸ�");

        if (confirmPanel != null) confirmPanel.SetActive(false);

        // ���� UI���� �ݰ� ���� ȭ������ ����
        if (keySettingPanel != null) keySettingPanel.SetActive(false);
        // �ʿ��ϸ� �ٸ� Canvas/���� HUD�� SetActive(true) �ص� ��
    }

    // �ƴϿ�(No) �� Ȯ��â�� �ݱ�
    private void ConfirmNo()
    {
        if (confirmPanel != null) confirmPanel.SetActive(false);
    }
}
