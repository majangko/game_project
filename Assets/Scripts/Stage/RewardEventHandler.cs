using UnityEngine;

public class RewardEventHandler : MonoBehaviour
{
    [SerializeField] float autoProceedDelay = 3f;

    void Start()
    {
        Invoke(nameof(NextStage), Mathf.Max(0.1f, autoProceedDelay));
    }

    void NextStage()
    {
        SceneLoader.Instance.LoadNextStage();
    }
}
