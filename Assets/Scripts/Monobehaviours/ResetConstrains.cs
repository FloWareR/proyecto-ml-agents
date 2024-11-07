using UnityEngine;

public class ResetConstrains : StateMachineBehaviour
{
    private static readonly int IsHeavyAttacking = Animator.StringToHash("isHeavyAttacking");

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(IsHeavyAttacking, true);

    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        FindObjectOfType<PlayerLocomotion>().ReinstateMovement();
        animator.SetBool(IsHeavyAttacking, false);
        FindObjectOfType<ParticleManager>().ToggleParticleSystem("heavyAttack", false);  // To play

    }

}
