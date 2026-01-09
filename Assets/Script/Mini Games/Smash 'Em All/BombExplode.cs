using UnityEngine;

public class BombExplode : MonoBehaviour
{
    public GameObject explosionPrefab;
    public GameObject sliceVFXPrefab;
    public AudioClip sliceSFX;
    public AudioClip explosionSFX;

    private bool canExplode;
    private bool exploded;

    public static BombExplode currentCenterBomb;

    void Update()
    {
        if (canExplode && currentCenterBomb == this && !exploded && Input.GetKeyDown(KeyCode.Space))
            Explode();
    }

    public void EnableExplosion()
    {
        canExplode = true;
        currentCenterBomb = this;
    }

    public void DisableExplosion()
    {
        canExplode = false;
        if (currentCenterBomb == this)
            currentCenterBomb = null;
    }

    void Explode()
    {
        exploded = true;

        if (sliceVFXPrefab != null)
            Instantiate(sliceVFXPrefab, transform.position, Quaternion.identity);

        if (sliceSFX != null && SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(sliceSFX);

        if (explosionPrefab != null)
        {
            GameObject explosion =
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            if (explosionSFX != null && SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(explosionSFX);

            Destroy(explosion, 1f);
        }

        if (SmashEmAllManager.Instance != null)
            SmashEmAllManager.Instance.LoseGame();

        DisableExplosion();
        Destroy(gameObject);
    }
}
