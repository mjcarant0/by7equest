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
    public float startScale = 2.5f;      // initial zoomed-in size
    public float zoomDuration = 0.5f;     // time to zoom out to normal size

    private SpriteRenderer sr;
    private Vector3 originalScale;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        // Start zoomed in
        transform.localScale = originalScale * startScale;

        if (frames == null || frames.Length == 0)
        {
            Debug.LogError("GameEndAnimation: No frames assigned!");
            return;
        }

        StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        // Smoothly zoom out to original scale over zoomDuration
        yield return StartCoroutine(ZoomOut());

        // Play all frames normally
        for (int i = 0; i < frames.Length; i++)
        {
            sr.sprite = frames[i];
            yield return new WaitForSeconds(frameDelay);
        }

        // Loop if needed
        if (loop)
        {
            transform.localScale = originalScale * startScale; // reset scale
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
