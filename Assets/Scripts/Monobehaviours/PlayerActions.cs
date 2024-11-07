using Unity.VisualScripting;
using UnityEngine;

public class PlayerActions : MonoBehaviour
{
    [SerializeField] private Transform attackPoint;
    
    
    private AnimatorManager _animatorManager;
    private PlayerManager _playerManager;
    private PlayerLocomotion _playerLocomotion;
    private ParticleManager _particleManager;
    

    private void Awake()
    {
        _animatorManager = GetComponent<AnimatorManager>();
        _playerLocomotion = GetComponent<PlayerLocomotion>();
        _playerManager = GetComponent<PlayerManager>();
        _particleManager = GetComponent<ParticleManager>();
    }

    public void HandlePrimaryAction()
    {
        if(_playerLocomotion.isSprinting || _playerManager.isInteracting) return;
        _particleManager.SpawnTemporaryParticle("magicProjectile", attackPoint.position, attackPoint.rotation );
    }

    public void HandleUltimateAction()
    {
        if (_playerManager.isInteracting || _playerManager.isHeavyAttacking) return;
        _playerLocomotion.RotateAtLookAt();
        _playerLocomotion.StopAllMovement();
        _animatorManager.PlayTargetAnimation("SayainAttack", true);
        _particleManager.ToggleParticleSystem("heavyAttackAura", true); 
    
    }

    public void HandleSecondaryAction()
    {
        if (_playerManager.isInteracting) return;
    }
}