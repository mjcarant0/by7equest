using UnityEngine;
using System.Collections;

public class GameEndController : MonoBehaviour
{
    [Header("Animation Reference")]
    public GameEndAnimation animationController;

    [Header("Auto Progress")]
    public bool autoProgress = false;
    public float autoProgressDelay = 2f;

    void Start()
    {
        if (animationController == null)
        {
            Debug.LogWarning("[v0] GameEndAnimation not assigned in GameEndController!");
        }
        else
        {
            Debug.Log("[v0] Game End Animation starting");
            
            if (autoProgress)
            {
                animationController.OnAnimationFinished += OnAnimationComplete;
            }
        }
    }

    private void OnAnimationComplete()
    {
        Debug.Log("[v0] Game End Animation completed");
    }

    private void OnDestroy()
    {
        if (animationController != null && autoProgress)
        {
            animationController.OnAnimationFinished -= OnAnimationComplete;
        }
    }
}
