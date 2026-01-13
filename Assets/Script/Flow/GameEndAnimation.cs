using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class GameEndAnimation : MonoBehaviour
{
    [Header("Sliced sprites (in order)")]
    public Sprite[] frames;

    [Header("Animation Settings")]
    public float frameDelay = 0.08f;
    public bool loop = false;

    [Header("Zoom Out Start")]
    public float startScale = 2.5f;
    public float zoomDuration = 0.5f;

    [Header("Door Open Delay")]
    public float doorOpenDelay = 1f;

    private SpriteRenderer sr;
    private Vector3 originalScale;

    public System.Action OnAnimationFinished;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        if (sr == null)
        {
            Debug.LogError("SpriteRenderer component missing on GameEndAnimation!");
            return;
        }

        originalScale = transform.localScale;
        transform.localScale = originalScale * startScale;

        if (frames == null || frames.Length == 0)
        {
            Debug.LogError("GameEndAnimation: No frames assigned in Inspector!");
            OnAnimationFinished?.Invoke();
            return;
        }

        // Set the first frame immediately
        sr.sprite = frames[0];

        // Start the animation coroutine
        StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        // 1. Hold the first frame (door fully open) before zoom
        yield return new WaitForSeconds(doorOpenDelay);

        // 2. Zoom out while door is still fully open
        yield return StartCoroutine(ZoomOut());

        // 3. Play the closing frames
        for (int i = 1; i < frames.Length; i++)
        {
            sr.sprite = frames[i];
            yield return new WaitForSeconds(frameDelay);
        }

        OnAnimationFinished?.Invoke();

        if (loop)
        {
            transform.localScale = originalScale * startScale;
            StartCoroutine(PlayAnimation());
        }
    }

    IEnumerator ZoomOut()
    {
        float elapsed = 0f;
        Vector3 start = originalScale * startScale;
        Vector3 end = originalScale;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomDuration;
            transform.localScale = Vector3.Lerp(start, end, t);
            yield return null;
        }

        transform.localScale = originalScale;
    }
}
