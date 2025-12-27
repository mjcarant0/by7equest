using UnityEngine;
using System.Collections;

public class MoveAlongConveyor : MonoBehaviour
{
    public float timeAtSpawn = 0.3f;
    public float maxTimeAtCenter = 1.5f;
    public float sliceDisplayTime = 0.35f;
    public float timeAtEnd = 0.6f;

    private static bool centerBusy;

    private Transform spawnPoint;
    private Transform centerPoint;
    private Transform endPoint;

    private FruitSlice fruitSlice;
    private BombExplode bomb;
    private FruitSpawner spawner;

    // Called explicitly when SliceEmAll loads
    public static void ResetSliceEmAll()
    {
        centerBusy = false;
    }

    void Start()
    {
        spawnPoint = GameObject.Find("SpawnPoint").transform;
        centerPoint = GameObject.Find("CenterPoint").transform;
        endPoint = GameObject.Find("EndPoint").transform;

        fruitSlice = GetComponent<FruitSlice>();
        bomb = GetComponent<BombExplode>();

        StartCoroutine(MoveRoutine());
    }

    public void SetSpawner(FruitSpawner s)
    {
        spawner = s;
    }

    IEnumerator MoveRoutine()
    {
        // Initial position (Spawn)
        transform.position = spawnPoint.position;
        yield return new WaitForSeconds(timeAtSpawn);

        // Wait until center is free
        while (centerBusy)
            yield return null;

        // Claim center
        centerBusy = true;

        // Tell spawner that spawn is empty
        if (spawner != null)
            spawner.ClearSpawnSlot();

        // Move to center and enable interactions
        transform.position = centerPoint.position;

        if (fruitSlice != null)
            fruitSlice.EnableSlicing();

        if (bomb != null)
            bomb.EnableExplosion();

        float timer = 0f;

        // Wait in center for either set time or sliced
        while (timer < maxTimeAtCenter)
        {
            if (fruitSlice != null && fruitSlice.IsSliced)
                break;

            timer += Time.deltaTime;
            yield return null;
        }

        // Disable interactions before moving away
        if (fruitSlice != null)
            fruitSlice.DisableSlicing();

        if (bomb != null)
            bomb.DisableExplosion();

        // Short pause if the fruit was sliced
        if (fruitSlice != null && fruitSlice.IsSliced)
            yield return new WaitForSeconds(sliceDisplayTime);

        // Move to end point
        transform.position = endPoint.position;

        // Free up center for next fruit
        centerBusy = false;

        yield return new WaitForSeconds(timeAtEnd);

        // Remove obj from game
        Destroy(gameObject);
    }
}