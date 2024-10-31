using UnityEngine;
using System.Collections.Generic;

public class TargetManager : MonoBehaviour
{
    public List<FriendlyTargetData> targets;  // Assign all FriendlyTargetData instances here in the Inspector

    private void Awake()
    {
        // Ensure the targets list is populated in the Inspector with each target's FriendlyTargetData
    }

    public FriendlyTargetData GetLowestHPTargetData()
    {
        FriendlyTargetData lowestHPTarget = null;
        float lowestHP = float.MaxValue;

        foreach (var target in targets)
        {
            if (target.currentHP > 0 && target.currentHP < lowestHP)
            {
                lowestHP = target.currentHP;
                lowestHPTarget = target;
            }
        }

        return lowestHPTarget;
    }
}