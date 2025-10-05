using UnityEngine;

/// <summary>
/// 캐릭터의 스킬 입력 및 실행을 관리하는 스크립트.
/// SkillBase를 상속한 스킬들을 자동으로 관리한다.
/// </summary>
public class guma_skill : MonoBehaviour
{
    [Header("Key Mapping")]
    public KeyCode skill1Key = KeyCode.X; // 첫 번째 스킬
    public KeyCode skill2Key = KeyCode.C; // 두 번째 스킬

    [Header("스킬 리스트 (자동 할당 또는 수동 지정 가능)")]
    public SkillBase[] skills;

    private Animator anim;
    private bool isCasting = false;

    private void Start()
    {
        anim = GetComponent<Animator>();

        // 스킬 목록이 비어 있으면 자동으로 가져오기
        if (skills == null || skills.Length == 0)
            skills = GetComponents<SkillBase>();
    }

    private void Update()
    {
        if (isCasting) return; // 시전 중이면 입력 무시

        if (Input.GetKeyDown(skill1Key))
            TryUseSkill(0);

        if (Input.GetKeyDown(skill2Key))
            TryUseSkill(1);
    }

    /// <summary>
    /// 지정된 인덱스의 스킬을 발동 시도
    /// </summary>
    private void TryUseSkill(int index)
    {
        if (index < 0 || index >= skills.Length || skills[index] == null)
            return;

        SkillBase skill = skills[index];

        // 쿨타임 중이면 무시
        if (IsSkillCooling(skill))
            return;

        // 애니메이션 시전 상태 확인
        if (anim != null && anim.GetCurrentAnimatorStateInfo(0).IsTag("Skill"))
            return; // 스킬 태그 중복 방지

        StartCoroutine(CastSkill(skill));
    }

    /// <summary>
    /// 스킬 시전 루틴
    /// </summary>
    private System.Collections.IEnumerator CastSkill(SkillBase skill)
    {
        isCasting = true;

        // 스킬 발동
        skill.Activate();

        // Animator에 "Skill" 태그가 붙은 애니메이션이 끝날 때까지 대기
        if (anim != null)
        {
            yield return new WaitForSeconds(GetSkillAnimLength(skill.animTrigger));
        }
        else
        {
            yield return new WaitForSeconds(0.5f); // 기본 대기
        }

        isCasting = false;
    }

    /// <summary>
    /// 스킬 쿨타임 확인 (SkillBase 내부 상태로 체크)
    /// </summary>
    private bool IsSkillCooling(SkillBase skill)
    {
        var type = skill.GetType().Name;
        var field = typeof(SkillBase).GetField("isCoolingDown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        bool cooling = (bool)field.GetValue(skill);

        if (cooling)
        {
            Debug.Log($"[{type}] 스킬이 아직 쿨타임 중입니다.");
            return true;
        }

        return false;
    }

    /// <summary>
    /// 애니메이터에서 특정 Trigger 애니메이션의 길이를 반환
    /// </summary>
    private float GetSkillAnimLength(string triggerName)
    {
        if (string.IsNullOrEmpty(triggerName) || anim == null)
            return 0.5f;

        // 현재 재생 중인 클립 길이를 반환 (태그 방식)
        var clips = anim.GetCurrentAnimatorClipInfo(0);
        if (clips.Length > 0)
            return clips[0].clip.length;

        return 0.5f;
    }
}
