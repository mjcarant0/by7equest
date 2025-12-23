using UnityEngine;
using System.Collections;

public class GameStartController : MonoBehaviour
{
    [Header("Animation Reference")]
    public GameStartAnimation animationController; // assign in Inspector

    [Header("Description Objects Only")]
    public GameObject karateDescription;
    public GameObject simonSaysDescription;
    public GameObject sliceEmAllDescription;

    [Header("Wait Time After Zoom (seconds)")]
    public float waitAfterZoom = 3f;

    IEnumerator Start()
    {
        // Wait a frame to ensure MinigameRandomizer exists
        yield return new WaitForEndOfFrame();

        if (MinigameRandomizer.Instance == null)
        {
            GameObject temp = new GameObject("MinigameRandomizer");
            temp.AddComponent<MinigameRandomizer>();
        }

        // Subscribe to animation finished event
        if (animationController != null)
        {
            animationController.OnAnimationFinished += OnIntroFinished;
        }
        else
        {
            Debug.LogWarning("GameStartAnimation not assigned in Inspector!");
            OnIntroFinished(); // fallback
        }
    }

    public void OnIntroFinished()
    {
        string nextMinigame = MinigameRandomizer.Instance.GetNextMinigameName();

        StartCoroutine(ShowDescriptionCoroutine(nextMinigame));
    }

    IEnumerator ShowDescriptionCoroutine(string minigameName)
    {
        DisableAllDescriptions();

        switch (minigameName)
        {
            case "Karate":
                if (karateDescription != null)
                {
                    karateDescription.SetActive(true);
                    yield return new WaitForSeconds(waitAfterZoom);
                    karateDescription.SetActive(false);
                }
                break;

            case "SimonSays":
                if (simonSaysDescription != null)
                {
                    simonSaysDescription.SetActive(true);
                    yield return new WaitForSeconds(waitAfterZoom);
                    simonSaysDescription.SetActive(false);
                }
                break;

            case "SliceEmAll":
                if (sliceEmAllDescription != null)
                {
                    sliceEmAllDescription.SetActive(true);
                    yield return new WaitForSeconds(waitAfterZoom);
                    sliceEmAllDescription.SetActive(false);
                }
                break;
        }

        // Load the minigame scene
        MinigameRandomizer.Instance.LoadNextMinigame();
    }

    void DisableAllDescriptions()
    {
        if (karateDescription != null) karateDescription.SetActive(false);
        if (simonSaysDescription != null) simonSaysDescription.SetActive(false);
        if (sliceEmAllDescription != null) sliceEmAllDescription.SetActive(false);
    }
}
