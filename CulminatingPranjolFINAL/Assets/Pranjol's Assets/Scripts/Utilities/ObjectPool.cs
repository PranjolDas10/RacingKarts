using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic object pool that eliminates per-frame Instantiate/Destroy overhead
/// for frequently spawned objects such as particle bursts and hit sparks.
///
/// Usage:
///     var pool = new ObjectPool&lt;ParticleSystem&gt;(prefab, initialSize: 8, parent);
///     ParticleSystem ps = pool.Get();   // activates an idle instance
///     pool.Return(ps);                  // deactivates and re-queues it
/// </summary>
public class ObjectPool<T> where T : Component
{
    private readonly T _prefab;
    private readonly Transform _parent;
    private readonly Queue<T> _idle = new Queue<T>();

    public int CountIdle   => _idle.Count;
    public int CountActive { get; private set; }

    public ObjectPool(T prefab, int initialSize, Transform parent = null)
    {
        _prefab = prefab;
        _parent = parent;

        for (int i = 0; i < initialSize; i++)
            Enqueue(CreateInstance());
    }

    /// <summary>Returns a ready-to-use, active instance from the pool.</summary>
    public T Get()
    {
        T obj = _idle.Count > 0 ? _idle.Dequeue() : CreateInstance();
        obj.gameObject.SetActive(true);
        CountActive++;
        return obj;
    }

    /// <summary>Deactivates <paramref name="obj"/> and returns it to the pool.</summary>
    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        CountActive--;
        Enqueue(obj);
    }

    private T CreateInstance()
    {
        T obj = Object.Instantiate(_prefab, _parent);
        obj.gameObject.SetActive(false);
        return obj;
    }

    private void Enqueue(T obj) => _idle.Enqueue(obj);
}
