using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TeamDatabase", menuName = "Game/Team Database", order = 2)]
public class TeamDatabase : ScriptableObject
{
    public List<TeamMemberSO> All;
}
