using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public Transform poolContainer;
        public int size;
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    public static ObjectPooler Instance;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject storedObject = Instantiate(pool.prefab);
                storedObject.transform.parent = pool.poolContainer;
                storedObject.name = pool.prefab.name;
                storedObject.SetActive(false);
                objectPool.Enqueue(storedObject);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string poolTag, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (!poolDictionary.ContainsKey(poolTag))
        {
            Debug.LogError("Pool with tag " + poolTag + " doesn't exist!");
            return null;
        }

        GameObject objectToSpawn = poolDictionary[poolTag].Dequeue();
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        if (parent != null)
        { 
            objectToSpawn.transform.SetParent(parent);
        }

        IPooledObject pooledObject = objectToSpawn.GetComponent<IPooledObject>();

        if(pooledObject != null)
        {
            pooledObject.OnObjectSpawn();
        }

        poolDictionary[poolTag].Enqueue(objectToSpawn); // Requeue the object after you start using it

        return objectToSpawn;
    }
}
