using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PunchBehavior : StateMachineBehaviour
{
    private MarioController m_MarioController;
    public MarioController.TPunchType m_PunchType;
    public float m_EnablePunchStartAnimationPct;
    public float m_EnablePunchEndAnimtionPCT;

    //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_MarioController = animator.GetComponent<MarioController>();
        m_MarioController.EnableHitCollider(m_PunchType, false);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        bool l_Enabled = stateInfo.normalizedTime >= m_EnablePunchStartAnimationPct
            && stateInfo.normalizedTime <= m_EnablePunchEndAnimtionPCT;
        m_MarioController.EnableHitCollider(m_PunchType, l_Enabled);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_MarioController.EnableHitCollider(m_PunchType, false);
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Implement code that processes and affects root motion
    }

    // OnStateIK is called right after Animator.OnAnimatorIK()
    override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Implement code that sets up animation IK (inverse kinematics)
    }
}
