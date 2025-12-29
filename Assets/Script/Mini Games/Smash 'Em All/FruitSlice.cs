using UnityEngine;

public class FruitSlice : MonoBehaviour
{
    public Sprite slicedSprite;
    public GameObject sliceVFXPrefab;

    private SpriteRenderer sr;
    private bool isSliced;
    private bool canSlice;

    public bool IsSliced => isSliced;
    public static FruitSlice currentCenterFruit;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (canSlice && currentCenterFruit == this && !isSliced && Input.GetKeyDown(KeyCode.Space))
            Slice();
    }

    public void EnableSlicing()
    {
        canSlice = true;
        currentCenterFruit = this;
    }

    public void DisableSlicing()
    {
        canSlice = false;
        if (currentCenterFruit == this)
            currentCenterFruit = null;
    }

    void Slice()
    {
        isSliced = true;

        if (slicedSprite != null)
            sr.sprite = slicedSprite;

        if (sliceVFXPrefab != null)
            Instantiate(sliceVFXPrefab, transform.position, Quaternion.identity);

        if (SmashEmAllManager.Instance != null)
            SmashEmAllManager.Instance.FruitSliced();

        DisableSlicing();
    }
}
