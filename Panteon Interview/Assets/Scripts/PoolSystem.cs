using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class PoolSystem : MonoSingleton<PoolSystem>
{
    Dictionary<Object, Queue<Object>> pools = new Dictionary<Object, Queue<Object>>();

    public static void Create(Transform parent)
    {
        GameObject obj = new GameObject("PoolSystem");
        obj.transform.parent = parent;
        obj.AddComponent<PoolSystem>();
    }

    public void CreateObject(UnityEngine.Object prefab) => CreateObject(prefab, 1);
    public void CreateObject(UnityEngine.Object prefab, int size)
    {
        if (pools.ContainsKey(prefab))
            return;

        Queue<Object> queue = new Queue<Object>();

        for (int i = 0; i < size; ++i)
        {
            var o = Instantiate(prefab);
            SetActive(o, false);
            queue.Enqueue(o);
        }

        pools[prefab] = queue;
    }

    public T GetObject<T>(Object prefab) where T : Object
    {
        Queue<Object> queue;
        if (pools.TryGetValue(prefab, out queue))
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
            return obj as T;
        }
        else
        {
            CreateObject(prefab);
            return GetObject<T>(prefab);
        }
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

    public void HideObject<T>(Object objectPrefab, Object currentObject) where T : Object
    {
        Queue<Object> queue;
        if (pools.TryGetValue(objectPrefab, out queue))
        {
            SetActive(currentObject, false);
            queue.Enqueue(currentObject);
        }
        else
        {
            Debug.LogError("No pool was init with this prefab !!!");
        }
    }

    public void SpawnObject(GameObject prefab, Vector3 position) => StartCoroutine(SpawnNewObject(prefab, position, 0)); 
    public void SpawnObject(GameObject prefab, Vector3 position, float duration) => StartCoroutine(SpawnNewObject(prefab, position, duration));
    private IEnumerator SpawnNewObject(GameObject prefab, Vector3 position, float duration)
    {
        GameObject newObject = GetObject<GameObject>(prefab);
        newObject.transform.position = position;
        if (duration <= 0) yield break;
        yield return new WaitForSeconds(duration);
        HideObject<GameObject>(prefab, newObject);
    }
}
