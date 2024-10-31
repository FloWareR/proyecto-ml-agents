using UnityEngine;

[CreateAssetMenu(fileName = "FriendlyTargetData", menuName = "ScriptableObjects/FriendlyTargetData")]
public class FriendlyTargetData : ScriptableObject
{
    public float currentHP;
    public float maxHP;
    public Vector3 position;

    public float NormalizedHP => currentHP / maxHP; // Return HP as a normalized value
}