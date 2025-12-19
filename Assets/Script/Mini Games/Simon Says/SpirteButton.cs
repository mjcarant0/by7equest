using UnityEngine;
using System.Collections;

public class SpriteButton : MonoBehaviour
{
    public Sprite[] sprites;   // your 4 images
    public float speed = 0.1f; // how fast it animates

    SpriteRenderer sr;
    bool playing = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = sprites[0]; // start at image 1
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !playing)
        {
            StartCoroutine(Animate());
        }
    }

    IEnumerator Animate()
    {
        playing = true;

        for (int i = 0; i < sprites.Length; i++)
        {
            sr.sprite = sprites[i];
            yield return new WaitForSeconds(speed);
        }

        sr.sprite = sprites[0]; // back to image 1
        playing = false;
    }
}
