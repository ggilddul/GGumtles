using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public static AnimationManager Instance { get; private set; }

    public Animator animator;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShakeTree()
    {
        animator.SetTrigger("ShakeTree");
    }

    public void DropAcorn()
    {
        animator.SetTrigger("DropAcorn");
    }

    public void DropDiamond()
    {
        animator.SetTrigger("DropDiamond");
    }

    public void PlayThrowAcornAnimation()
    {
        animator.SetTrigger("ThrowAcorn");
    }

    public void HatchEgg()
    {
        animator.SetTrigger("HatchEgg");
    }
}
