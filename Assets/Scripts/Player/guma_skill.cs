using UnityEngine;

public class guma_skill : MonoBehaviour
{
    [Header("Skill List")]
    public SkillBase[] skills;

    [Header("Key Mapping")]
    public KeyCode slashKey = KeyCode.X;
    public KeyCode buffKey = KeyCode.C;

    void Update()
    {
        if (Input.GetKeyDown(slashKey) && skills.Length > 0)
            skills[0].Activate();

        if (Input.GetKeyDown(buffKey) && skills.Length > 1)
            skills[1].Activate();
    }
}
