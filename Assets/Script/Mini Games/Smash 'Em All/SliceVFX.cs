using UnityEngine;

public class AutoDestroyAfterAnim : MonoBehaviour
{
    void Start()
    {
        float duration = GetComponent<Animator>()
            .GetCurrentAnimatorStateInfo(0).length;

        Destroy(gameObject, duration);
    }
}
