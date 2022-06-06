using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class PoolSystem : MonoBehaviour
{
    public static PoolSystem Instance { get; private set; }

    public static void Create(Transform parent)
    {
        GameObject obj = new GameObject("PoolSystem");
        obj.transform.parent = parent;
        Instance = obj.AddComponent<PoolSystem>();
    }

    Dictionary<Object, Queue<Object>> m_Pools = new Dictionary<Object, Queue<Object>>();
    Queue<Object> output = new Queue<Object>();

    public void InitPool(UnityEngine.Object prefab, int size)
    {
        if (m_Pools.ContainsKey(prefab))
            return;

        Queue<Object> queue = new Queue<Object>();

        for (int i = 0; i < size; ++i)
        {
            var o = Instantiate(prefab);
            SetActive(o, false);
            queue.Enqueue(o);
        }

        m_Pools[prefab] = queue;
    }

    public T GetInstance<T>(Object prefab) where T : Object
    {
        Queue<Object> queue;
        if (m_Pools.TryGetValue(prefab, out queue))
        {
            Object obj;

            if (queue.Count > 0)
            {
                obj = queue.Dequeue();
            }
            else
            {
                obj = Instantiate(prefab);
            }

            SetActive(obj, true);
            queue.Enqueue(obj);

            output.Enqueue(obj);
            return obj as T;
        }

        UnityEngine.Debug.LogError("No pool was init with this prefab");
        return null;
    }

    static void SetActive(Object obj, bool active)
    {
        GameObject go = null;

        if (obj is Component component)
        {
            go = component.gameObject;
        }
        else
        {
            go = obj as GameObject;
        }

        go.SetActive(active);
    }

    public void ResetPool()
    {
        while (output.Count > 0)
        {
            SetActive(output.Dequeue(), false);
        }
    }
}
