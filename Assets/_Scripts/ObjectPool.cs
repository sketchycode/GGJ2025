using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class ObjectPool<T> : MonoBehaviour where T : Behaviour
{
    [SerializeField] private T prefab;
    [SerializeField] private int initialPoolSize = 10;

    private Stack<T> pool;
    
    public Transform OverrideParentTransform { get; set; }

    public void Despawn(T obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(transform);
        pool.Push(obj);
    }

    protected T Spawn()
    {
        if (!pool.Any()) CreateNewObject();

        var tObj = pool.Pop();
        tObj.gameObject.SetActive(true);
        if (OverrideParentTransform != null) tObj.transform.SetParent(OverrideParentTransform);
        return tObj;
    }

    private void Awake()
    {
        // Initialize the pool
        pool = new Stack<T>();

        // Populate the pool with the initial number of objects
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewObject();
        }
    }

    // Helper method to create a new object
    private void CreateNewObject()
    {
        var tObj = Instantiate(prefab, transform, true);
        tObj.gameObject.SetActive(false);
        pool.Push(tObj);
    }
}