using UnityEngine;

public class PlayerTestInput : MonoBehaviour
{
    PlayerStats s;
    void Start() { s = GetComponent<PlayerStats>(); }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) s.Damage(10);
        if (Input.GetKeyDown(KeyCode.Alpha2)) s.Heal(10);
        if (Input.GetKeyDown(KeyCode.Alpha3)) s.UseMP(5);
        if (Input.GetKeyDown(KeyCode.Alpha4)) s.RestoreMP(5);
    }
}
