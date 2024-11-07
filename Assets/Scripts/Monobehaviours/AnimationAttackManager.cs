using System;
using UnityEngine;

public class AnimationAttackManager : MonoBehaviour
{
    private ParticleManager _particleManager;

    private void Awake()
    {
        _particleManager = FindObjectOfType<ParticleManager>();
    }

    public void TriggerHeavyAttackEffect()
    {
        _particleManager.ToggleParticleSystem("heavyAttack", true);  
    }

    public void DestroyHeavyAttackEffect()
    {
        _particleManager.ToggleParticleSystem("heavyAttack", false); 
    }
}
