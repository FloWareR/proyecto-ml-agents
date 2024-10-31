using UnityEngine;
using System.Collections.Generic;

public class TargetManager : MonoBehaviour
{
    public List<FriendlyTargetData> targets; 

    private void Awake()
    {

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