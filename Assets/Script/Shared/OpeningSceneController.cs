using UnityEngine;
using UnityEngine.SceneManagement;

public class OpeningSceneController : MonoBehaviour
{
    [Header("Description Objects")]
    public GameObject karateDescription;
    public GameObject simonSaysDescription;
    public GameObject sliceEmAllDescription;

    [Header("Animation")]
    public Animator trainAnimator; 
    public string animationName = "TrainAnimation";  
    
    [Header("Wait Time (seconds)")]
    public float displayDuration = 3f;
    public bool waitForAnimation = true; 

    void Start()
    {
        Debug.Log("[OpeningScene] Opening scene started - showing train animation");
        
        if (MinigameRandomizer.Instance == null)
        {
            GameObject temp = new GameObject("MinigameRandomizer");
            temp.AddComponent<MinigameRandomizer>();
        }

        string nextMinigame = MinigameRandomizer.Instance.GetNextMinigameName();
        Debug.Log($"[OpeningScene] Next minigame will be: {nextMinigame}");

        ShowInstructions(nextMinigame);
        
        // Wait for animation if enabled
        if (waitForAnimation && trainAnimator != null)
        {
            float animationLength = GetAnimationLength();
            Debug.Log($"[OpeningScene] Waiting for animation: {animationLength} seconds");
            Invoke(nameof(LoadModeDisplay), animationLength);
        }
        else
        {
            Invoke(nameof(LoadModeDisplay), displayDuration);
        }
    }

    float GetAnimationLength()
    {
        if (trainAnimator == null) return displayDuration;
        
        AnimationClip[] clips = trainAnimator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == animationName)
            {
                return clip.length;
            }
        }
        
        return displayDuration;
    }

    void ShowInstructions(string minigameName)
    {
        if (karateDescription != null) karateDescription.SetActive(false);
        if (simonSaysDescription != null) simonSaysDescription.SetActive(false);
        if (sliceEmAllDescription != null) sliceEmAllDescription.SetActive(false);

        switch (minigameName)
        {
            case "Karate":
                if (karateDescription != null) karateDescription.SetActive(true);
                break;
            case "SimonSays":
                if (simonSaysDescription != null) simonSaysDescription.SetActive(true);
                break;
            case "SliceEmAll":
                if (sliceEmAllDescription != null) sliceEmAllDescription.SetActive(true);
                break;
            default:
                Debug.LogWarning($"[OpeningScene] Unknown minigame: {minigameName}");
                break;
        }
    }

    void LoadModeDisplay()
    {
        Debug.Log("[OpeningScene] Loading Mode Display");
        SceneManager.LoadScene("ModeDisplay");
    }
}