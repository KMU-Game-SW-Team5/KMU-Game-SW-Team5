using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    private Dictionary<GameObject, Queue<GameObject>> poolDict = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject); 
    }

    public GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (prefab == null) return null;

        if (!poolDict.ContainsKey(prefab))
            poolDict[prefab] = new Queue<GameObject>();

        Queue<GameObject> queue = poolDict[prefab];

        GameObject obj = null;
        while (queue.Count > 0)
        {
            obj = queue.Dequeue();
            if (obj != null && !obj.activeSelf)
                break;
            obj = null;
        }

        if (obj == null)
        {
            obj = Instantiate(prefab, pos, rot);
            obj.transform.SetParent(this.transform); 
        }
        else
        {
            obj.transform.SetPositionAndRotation(pos, rot);
            obj.SetActive(true);
        }

        return obj;
    }

    public void Despawn(GameObject prefab, GameObject instance)
    {
        if (prefab == null || instance == null) return;

        instance.SetActive(false);
        instance.transform.SetParent(this.transform); 

        if (!poolDict.ContainsKey(prefab))
            poolDict[prefab] = new Queue<GameObject>();

        poolDict[prefab].Enqueue(instance);
    }
}
