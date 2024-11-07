using System;
using UnityEngine;
using UnityEngine.Serialization;

public class AnimatorManager : MonoBehaviour
{
    [SerializeField] private Transform graphics;
    [NonSerialized] public Animator AnimatorComponent;
    private int _horizontal;
    private int _vertical;
    private static readonly int IsInteracting = Animator.StringToHash("isInteracting");

    private PlayerManager _playerManager;
    private void Awake()
    {
        AnimatorComponent = GetComponentInChildren<Animator>();
        _horizontal = Animator.StringToHash("Horizontal");
        _vertical = Animator.StringToHash("Vertical");
        _playerManager = FindObjectOfType<PlayerManager>();
    }

    public void UpdateAnimatorValues(float horizontalMovement, float verticalMovement, bool isSprinting)
    {
        float snappedHorizontal;
        float snappedVertical;

        #region Snapped Horizontal
        if (horizontalMovement is > 0 and < 0.55f)
        {
            snappedHorizontal = 0.5f;
        }
        else if (horizontalMovement > 0.55f)
        {
            snappedHorizontal = 1f;
        }
        
        else if (horizontalMovement is < 0 and > -0.55f)
        {
            snappedHorizontal = -0.5f;
        }
        else if (horizontalMovement is < -0.55f)
        {
            snappedHorizontal = -1;
        }
        else
        {
            snappedHorizontal = 0f;
        }
        #endregion
        
        #region Snapped Vertical
        if (verticalMovement is > 0 and < 0.55f)
        {
            snappedVertical = 0.5f;
        }
        else if (verticalMovement > 0.55f)
        {
            snappedVertical = 1f;
        }
        
        else if (verticalMovement is < 0 and > -0.55f)
        {
            snappedVertical = -0.5f;
        }
        else if (verticalMovement is < -0.55f)
        {
            snappedVertical = -1;
        }
        else
        {
            snappedVertical = 0f;
        }
        

        #endregion

        if (isSprinting)
        {
            snappedHorizontal = horizontalMovement;
            snappedVertical = 2f;
        }
        
        graphics.transform.localPosition = Vector3.zero;
        AnimatorComponent.SetFloat(_horizontal, snappedHorizontal, 0.1f, Time.deltaTime);
        AnimatorComponent.SetFloat(_vertical, snappedVertical, 0.1f, Time.deltaTime);

    }

    public void PlayTargetAnimation(string targetAnimation, bool isInteracting)
    {
        AnimatorComponent.SetBool(IsInteracting, isInteracting);
        AnimatorComponent.CrossFade(targetAnimation, 0.2f);
    }
}
