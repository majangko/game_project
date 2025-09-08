// FILE: TeamMemberSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "TeamMember_", menuName = "Game/Team Member")]
public class TeamMemberSO : ScriptableObject
{
    [Header("기본")]
    public string id;               // 고유 키(중복금지)
    public string displayName;      // 표시 이름(숫자 목업도 여기 사용)
    public Rarity rarity = Rarity.Common;

    [Header("이미지")]
    public Sprite portrait;         // 카드 앞면 전체 이미지
    public Sprite skillIconA;       // 뒷면 스킬 아이콘 1
    public Sprite skillIconB;       // 뒷면 스킬 아이콘 2

    [Header("설명")]
    [TextArea(3, 8)]
    public string description;
}
