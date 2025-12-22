using UnityEngine;
using System.Collections.Generic;

public class FruitSpawner : MonoBehaviour
{
    public GameObject[] fruitPrefabs;
    public GameObject bombPrefab;

    public Transform spawnPoint;

    private GameObject fruitAtSpawn;
    private Queue<GameObject> spawnQueue = new Queue<GameObject>();

    void Start()
    {
        RefillQueue();
        SpawnNext();
    }

    void Update()
    {
        if (fruitAtSpawn == null)
            SpawnNext();
    }

    void RefillQueue()
    {
        // Temporary list for shuffling items
        List<GameObject> temp = new List<GameObject>();

        foreach (GameObject fruit in fruitPrefabs)
        {
            temp.Add(fruit);
            temp.Add(fruit);
        }

        temp.Add(bombPrefab);
        temp.Add(bombPrefab);

        // Shuffle list
        for (int i = 0; i < temp.Count; i++)
        {
            int r = Random.Range(i, temp.Count);
            (temp[i], temp[r]) = (temp[r], temp[i]);
        }

        // Fill queue with items from list
        foreach (GameObject obj in temp)
            // Adds item to the BACK of the line
            spawnQueue.Enqueue(obj);
    }

    void SpawnNext()
    {
        if (fruitAtSpawn != null)
            return;

        // if line == empty, then refill immediately
        if (spawnQueue.Count == 0)
            RefillQueue();

        // Takes item from the FRONT of the line
        GameObject prefab = spawnQueue.Dequeue();

        fruitAtSpawn = Instantiate(
            prefab,
            spawnPoint.position,
            Quaternion.identity
        );

        MoveAlongConveyor mover = fruitAtSpawn.GetComponent<MoveAlongConveyor>();
        if (mover != null)
            mover.SetSpawner(this);
    }

    public void ClearSpawnSlot()
    {
        fruitAtSpawn = null;
    }
}
