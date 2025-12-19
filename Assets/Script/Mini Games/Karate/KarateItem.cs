using UnityEngine;

public class KarateItem : MonoBehaviour
{
    public bool isWood = true;
    public GameObject choppedWoodPrefab;

    void Start() { }

    public void ChopWood()
    {
        if (choppedWoodPrefab != null)
        {
            GameObject pieces = Instantiate(choppedWoodPrefab, transform.position, transform.rotation);
            pieces.transform.localScale = transform.localScale;
            Destroy(pieces, 0.5f);
        }
        Destroy(gameObject);
    }
}