using UnityEngine;

public class StageClearTrigger : MonoBehaviour
{
    public void OnBossDefeated()
    {
        int stage = GameManager.Instance.CurrentStage;

        if (stage == 5)
        {
            SceneLoader.Instance.LoadGameClear();
            return;
        }

        SceneLoader.Instance.LoadRewardEvent(stage);
    }
}
