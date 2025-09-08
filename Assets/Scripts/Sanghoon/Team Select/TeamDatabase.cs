// FILE: TeamDatabase.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TeamDatabase", menuName = "Game/Team Database")]
public class TeamDatabase : ScriptableObject
{
    public List<TeamMemberSO> All = new List<TeamMemberSO>();
}
