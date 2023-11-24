using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSMClearSignals : StateMachineBehaviour
{
    //������붯�����ź�
    public string[] clearAtEnter;
    //�����˳������Ĵ����ź�
    public string[] clearAtExit;
    //�����ź�
    public AudioClip SoundClip;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        foreach(string signal in clearAtEnter)
        {
            animator.ResetTrigger(signal);
        }
        animator.gameObject.GetComponent<AudioSource>().clip = SoundClip;
        animator.gameObject.GetComponent<AudioSource>().Play();
    }


    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        for(int i = 0; i < clearAtExit.Length; i++)
        {
            animator.ResetTrigger(clearAtExit[i]);
        }
    }
}
