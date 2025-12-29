using UnityEngine;

public class BombExplode : MonoBehaviour
{
    public GameObject explosionPrefab;
    public GameObject sliceVFXPrefab;

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

        if (explosionPrefab != null)
        {
            GameObject explosion =
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            Destroy(explosion, 1f);
        }

        if (SmashEmAllManager.Instance != null)
            SmashEmAllManager.Instance.LoseGame();

        DisableExplosion();
        Destroy(gameObject);
    }
}
