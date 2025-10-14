// FILE: TeamMemberSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "TeamMember", menuName = "Game/Team Member", order = 1)]
public class TeamMemberSO : ScriptableObject
{
    [Header("기본 정보")]
    [Tooltip("캐릭터의 고유 ID (중복 금지)")]
    public string id;

    [Tooltip("게임 내 표시될 이름")]
    public string displayName;

    [TextArea(2, 4)]
    [Tooltip("캐릭터 설명 텍스트 (카드 뒷면 표시용)")]
    public string description;

    [Tooltip("캐릭터의 희귀도 (Common, Epic, Legendary 등)")]
    public Rarity rarity = Rarity.Common;

    [Header("이미지 / 아이콘")]
    [Tooltip("카드/태그/HUD에 표시될 초상화 이미지")]
    public Sprite portrait;

    [Tooltip("스킬 A 아이콘 (뒷면 표시용)")]
    public Sprite skillIconA;

    [Tooltip("스킬 B 아이콘 (뒷면 표시용)")]
    public Sprite skillIconB;

    [Header("프리팹 참조")]
    [Tooltip("전투용 캐릭터 프리팹 (SpumPlatformerController 포함)")]
    public GameObject prefab;
}


