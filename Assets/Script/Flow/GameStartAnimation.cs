using UnityEngine;
using System.Collections;
using TMPro;

namespace Flow
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class GameStartAnimation : MonoBehaviour
    {
        [Header("Door Opening Frames (in order)")]
        public Sprite[] doorFrames;
        
        [Header("Animation Settings")]
        public float frameDelay = 0.08f;
        
        [Header("Zoom Out Effect")]
        public bool zoomOnLastFrame = true;
        public float endScale = 2.5f;
        public float zoomDuration = 0.5f;
        
        [Header("Title Display")]
        public TextMeshProUGUI titleText;
        public CanvasGroup titleCanvasGroup;
        public string minigameTitle = "Memory Match";
        public float titleFadeInDuration = 0.5f;
        public float titleDisplayDuration = 2f;
        
        [Header("Title Description GameObjects")]
        public GameObject karateTitleDescription;
        public GameObject simonSaysTitleDescription;
        public GameObject sliceEmAllTitleDescription;
        
        [Header("Countdown")]
        public Sprite[] countdownSprites;
        public float countdownDelay = 0.8f;
        
        private SpriteRenderer sr;
        private Vector3 originalScale;
        private GameObject currentTitleObject;
        
        public System.Action OnAnimationFinished;
        
        void Start()
        {
            sr = GetComponent<SpriteRenderer>();
            originalScale = transform.localScale;
            
            // Ensure the UI is hidden at the very start
            if (titleCanvasGroup != null) titleCanvasGroup.alpha = 0f;
            
            // Turn off all description objects initially
            if (karateTitleDescription != null) karateTitleDescription.SetActive(false);
            if (simonSaysTitleDescription != null) simonSaysTitleDescription.SetActive(false);
            if (sliceEmAllTitleDescription != null) sliceEmAllTitleDescription.SetActive(false);
        }
        
        public void StartAnimation()
        {
            StartCoroutine(PlayAnimation());
        }

        IEnumerator PlayAnimation()
        {
            // STEP 0: Activate title/description immediately (outside doors)
            if (currentTitleObject != null)
                currentTitleObject.SetActive(true);

            if (titleText != null)
                titleText.gameObject.SetActive(true);

            // Start fade-in of title (non-blocking)
            StartCoroutine(FadeTitle(0f, 1f, titleFadeInDuration));

            // STEP 1: Play door opening animation frame by frame
            for (int i = 0; i < doorFrames.Length; i++)
            {
                sr.sprite = doorFrames[i];
                yield return new WaitForSeconds(frameDelay);
            }

            // STEP 2: Zoom on last frame of door
            if (zoomOnLastFrame)
            {
                yield return StartCoroutine(ZoomOut());
            }

            // STEP 3: Wait for title display duration
            yield return new WaitForSeconds(titleDisplayDuration);

            // STEP 4: Fade out title/description
            yield return StartCoroutine(FadeTitle(1f, 0f, titleFadeInDuration));

            if (titleText != null)
                titleText.gameObject.SetActive(false);
            if (currentTitleObject != null)
                currentTitleObject.SetActive(false);

            // STEP 5: Play countdown if any
            if (countdownSprites != null && countdownSprites.Length > 0)
            {
                for (int i = 0; i < countdownSprites.Length; i++)
                {
                    sr.sprite = countdownSprites[i];
                    yield return new WaitForSeconds(countdownDelay);
                }
            }

            // STEP 6: Notify animation finished
            OnAnimationFinished?.Invoke();
        }

        IEnumerator ZoomOut()
        {
            float elapsed = 0f;
            Vector3 start = originalScale;
            Vector3 end = originalScale * endScale;

            while (elapsed < zoomDuration)
            {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(start, end, elapsed / zoomDuration);
                yield return null;
            }
            
            transform.localScale = end;
        }
        
        IEnumerator FadeTitle(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                if (titleCanvasGroup != null)
                    titleCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }

            if (titleCanvasGroup != null)
                titleCanvasGroup.alpha = to;
        }
        
        public void SetTitle(string title)
        {
            minigameTitle = title;
            if (titleText != null) titleText.text = title;
            
            // Hide all descriptions first to reset
            if (karateTitleDescription != null) karateTitleDescription.SetActive(false);
            if (simonSaysTitleDescription != null) simonSaysTitleDescription.SetActive(false);
            if (sliceEmAllTitleDescription != null) sliceEmAllTitleDescription.SetActive(false);
            
            // Match the string from MinigameRandomizer
            string lowerTitle = title.ToLower();
            if (lowerTitle.Contains("karate")) currentTitleObject = karateTitleDescription;
            else if (lowerTitle.Contains("simon")) currentTitleObject = simonSaysTitleDescription;
            else if (lowerTitle.Contains("slice") || lowerTitle.Contains("smash")) currentTitleObject = sliceEmAllTitleDescription;
            
            Debug.Log($"[GameStartAnimation] Title set to: {title}. Description Object assigned: {(currentTitleObject != null ? currentTitleObject.name : "None")}");
        }
    }
}