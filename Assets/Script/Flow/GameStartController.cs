using UnityEngine;
using System.Collections;

namespace Flow
{
    public class GameStartController : MonoBehaviour
    {
        [Header("Animation Reference")]
        public GameStartAnimation animationController;
        
        void Start()
        {
            if (animationController == null)
            {
                Debug.LogWarning("[GameStartController] GameStartAnimation not assigned!");
                return;
            }

            StartCoroutine(WaitForRandomizerAndStart());
        }
        
        private IEnumerator WaitForRandomizerAndStart()
        {
            Debug.Log("[GameStartController] Starting Game Start sequence");
            
            // Wait up to 2 seconds for MinigameRandomizer to initialize
            float waitTime = 0f;
            while (MinigameRandomizer.Instance == null && waitTime < 2f)
            {
                yield return new WaitForSeconds(0.1f);
                waitTime += 0.1f;
            }
            
            if (MinigameRandomizer.Instance == null)
            {
                Debug.LogError("[GameStartController] MinigameRandomizer still not found after waiting! Using fallback title.");
                // Use first available title as fallback
                animationController.SetTitle("Karate");
                animationController.OnAnimationFinished += OnAnimationComplete;
                animationController.StartAnimation();
                yield break;
            }
            
            Debug.Log("[GameStartController] MinigameRandomizer found, getting title");
            
            string minigameTitle = MinigameRandomizer.Instance.GetNextMinigameName();
            Debug.Log($"[GameStartController] Got title from randomizer: {minigameTitle}");
            
            // Set the title before starting
            animationController.SetTitle(minigameTitle);
            
            // Subscribe to animation finished event
            animationController.OnAnimationFinished += OnAnimationComplete;
            
            Debug.Log("[GameStartController] Starting animation with title: " + minigameTitle);
            animationController.StartAnimation();
        }
        
        private void OnAnimationComplete()
        {
            Debug.Log("[GameStartController] Animation completed, loading minigame");
            
            if (GameModeManager.Instance != null)
            {
                GameModeManager.Instance.LoadNextMinigame();
            }
            else
            {
                Debug.LogError("[GameStartController] GameModeManager instance not found!");
            }
        }
        
        private void OnDestroy()
        {
            if (animationController != null)
            {
                animationController.OnAnimationFinished -= OnAnimationComplete;
            }
        }
    }
}
