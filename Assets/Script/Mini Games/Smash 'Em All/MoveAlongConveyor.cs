using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoveAlongConveyor : MonoBehaviour
{
    public float timeAtSpawn = 0.3f;
    public float maxTimeAtCenter = 1.5f;
    public float sliceDisplayTime = 0.35f;
    public float timeAtEnd = 0.6f;

    private static Queue<MoveAlongConveyor> centerQueue = new Queue<MoveAlongConveyor>();
    private static bool centerBusy;

    private Transform spawnPoint;
    private Transform centerPoint;
    private Transform endPoint;

    private FruitSlice fruitSlice;
    private BombExplode bomb;
    private FruitSpawner spawner;

    void Start()
    {
        spawnPoint = GameObject.Find("SpawnPoint").transform;
        centerPoint = GameObject.Find("CenterPoint").transform;
        endPoint = GameObject.Find("EndPoint").transform;

        fruitSlice = GetComponent<FruitSlice>();
        bomb = GetComponent<BombExplode>();

        // Join the queue to wait for a turn at the center
        centerQueue.Enqueue(this);

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

        // Wait until center is free and the item is infront of the line
        while (centerBusy || centerQueue.Peek() != this)
            yield return null;

        // Mark center as busy and leave the queue to move forward
        centerBusy = true;
        centerQueue.Dequeue();

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