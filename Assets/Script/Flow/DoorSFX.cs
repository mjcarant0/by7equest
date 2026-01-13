using UnityEngine;
using System.Collections;

public class DoorSFXDelay : MonoBehaviour
{
    public AudioSource doorAudioSource;
    public float delay = 2f;

    void Start()
    {
        StartCoroutine(PlayAfterDelay());
    }

    IEnumerator PlayAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        doorAudioSource.Play();
    }
}
