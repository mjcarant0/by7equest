using UnityEngine;
using System.Collections;

public class SpriteButton : MonoBehaviour
{
    public Sprite[] sprites;   // Sprites for animation
    public float speed = 0.1f;
    
    public AudioClip clickSFX;

    SpriteRenderer sr;
    bool playing = false;
    bool armed = false;   // allows one animation per command

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("[SpriteButton] SpriteRenderer not found!");
            return;
        }
        
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogError("[SpriteButton] Sprites array is empty!");
            return;
        }
        
        sr.sprite = sprites[0];
        armed = true; // Arm button initially so first command works
    }

    public void ArmForNewCommand()
    {
        armed = true;
        playing = false;
        if (sr != null && sprites != null && sprites.Length > 0)
        {
            sr.sprite = sprites[0];
        }
    }

    public void TryPlay()
    {
        if (!armed || playing)
            return;

        if (clickSFX != null && SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(clickSFX);

        StartCoroutine(Animate());
    }

    public void OnButtonClicked()
    {
        TryPlay();
    }

    IEnumerator Animate()
    {
        playing = true;
        armed = false; // lock after first press

        for (int i = 0; i < sprites.Length; i++)
        {
            if (sr != null && sprites[i] != null)
            {
                sr.sprite = sprites[i];
            }
            yield return new WaitForSeconds(speed);
        }

        if (sr != null)
            sr.sprite = sprites[0];
        
        playing = false;
    }
}
