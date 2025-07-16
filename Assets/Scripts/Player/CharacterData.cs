using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Character/CharacterData")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public CharacterType type;
    public float moveSpeed;
    public float attackPower;
    public GameObject skillPrefab;
}
