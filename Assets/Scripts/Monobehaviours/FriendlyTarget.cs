using UnityEngine;

public class FriendlyTarget : MonoBehaviour
{
    public FriendlyTargetData targetData;  // Assign a unique ScriptableObject instance in the Inspector
    public float maxHP = 100f;

    private void Start()
    {
        targetData.maxHP = maxHP;
        targetData.currentHP = maxHP;
        targetData.position = transform.position;
    }

    private void Update()
    {
        // Update position and currentHP in the targetData to keep it up-to-date
        targetData.position = transform.position;
    }

    public void TakeDamage(float amount)
    {
        targetData.currentHP = Mathf.Max(0, targetData.currentHP - amount);
    }

    public void Heal(float amount)
    {
        targetData.currentHP = Mathf.Min(targetData.maxHP, targetData.currentHP + amount);
    }
}