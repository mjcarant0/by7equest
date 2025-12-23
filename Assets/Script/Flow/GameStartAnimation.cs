using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class GameStartAnimation : MonoBehaviour
{
    [Header("Sliced sprites (in order)")]
    public Sprite[] frames;

    [Header("Animation Settings")]
    public float frameDelay = 0.08f;
    public bool loop = false;

    [Header("Zoom In (Last Frame)")]
    public bool zoomOnLastFrame = true;
    public float zoomScale = 2.5f;      // how big it gets
    public float zoomDuration = 0.5f;   // how fast the zoom is

    private SpriteRenderer sr;
    private Vector3 originalScale;

    // Event to notify when animation finishes
    public System.Action OnAnimationFinished;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        if (frames == null || frames.Length == 0)
        {
            Debug.LogError("GameStartAnimation: No frames assigned!");
            return;
        }

        StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        for (int i = 0; i < frames.Length; i++)
        {
            sr.sprite = frames[i];
            yield return new WaitForSeconds(frameDelay);
        }

        if (zoomOnLastFrame)
        {
            yield return StartCoroutine(ZoomIn());
        }

        // Notify subscribers that animation + zoom finished
        OnAnimationFinished?.Invoke();

        if (loop)
        {
            transform.localScale = originalScale;
            StartCoroutine(PlayAnimation());
        }
    }

    IEnumerator ZoomIn()
    {
        float elapsed = 0f;
        Vector3 targetScale = originalScale * zoomScale;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomDuration;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;
    }
}
