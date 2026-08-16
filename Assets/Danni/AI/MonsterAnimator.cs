using System;
using System.Collections;
using UnityEngine;

public class MonsterAnimator : MonoBehaviour
{
    private Animator animator;
 
    private static readonly int IdleHash = Animator.StringToHash("Idle1");
    private static readonly int RunHash = Animator.StringToHash("run1");
    private static readonly int AttackHash = Animator.StringToHash("attack1");
    private static readonly int DeathHash = Animator.StringToHash("death1");
 
    private int currentStateHash = -1;
 
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
 
    public void PlayIdle()
    {
        PlayState(IdleHash);
    }
 
    public void PlayRun()
    {
        PlayState(RunHash);
    }
 
    public void PlayAttack(Action onFinished)
    {
        PlayState(AttackHash);
        WaitThenInvoke(AttackHash, onFinished);
    }
 
    public void PlayDeath(Action onFinished)
    {
        PlayState(DeathHash);
        WaitThenInvoke(DeathHash, onFinished);
    }
 
    public void ResetAnimator()
    {
        StopAllCoroutines();
        currentStateHash = -1;
        PlayIdle();
    }
 
    private void PlayState(int stateHash, float transitionDuration = 0.1f)
    {
        if (animator == null || currentStateHash == stateHash) return;
        animator.CrossFade(stateHash, transitionDuration, 0);
        currentStateHash = stateHash;
    }
 
    private void WaitThenInvoke(int stateHash, Action onFinished)
    {
        StopAllCoroutines();
        if (animator == null)
        {
            onFinished?.Invoke();
            return;
        }
        StartCoroutine(WaitForAnimationThen(stateHash, onFinished));
    }
 
    private IEnumerator WaitForAnimationThen(int stateHash, Action onFinished)
    {
        yield return null;
        while (true)
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            bool inTargetState = info.shortNameHash == stateHash;
            bool inTransition = animator.IsInTransition(0);
            if (inTargetState && !inTransition && info.normalizedTime >= 1f)
                break;
            yield return null;
        }
        onFinished?.Invoke();
    }
}
