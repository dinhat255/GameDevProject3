using UnityEngine;
using System.Collections.Generic;

public class EnemyPool
{
    private readonly Dictionary<GameObject, Queue<EnemyAI>> pools = new Dictionary<GameObject, Queue<EnemyAI>>();
    private readonly Dictionary<EnemyAI, GameObject> instanceToPrefab = new Dictionary<EnemyAI, GameObject>();

    public void Prewarm(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0)
        {
            return;
        }

        EnsurePoolExists(prefab);

        for (int i = 0; i < count; i++)
        {
            EnemyAI ai = CreatePooledEnemy(prefab);
            if (ai != null)
            {
                pools[prefab].Enqueue(ai);
            }
        }
    }

    public EnemyAI Rent(GameObject prefab)
    {
        if (prefab == null)
        {
            return null;
        }

        EnsurePoolExists(prefab);
        Queue<EnemyAI> pool = pools[prefab];

        while (pool.Count > 0)
        {
            EnemyAI pooledEnemy = pool.Dequeue();
            if (pooledEnemy != null)
            {
                pooledEnemy.gameObject.SetActive(true);
                return pooledEnemy;
            }
        }

        EnemyAI createdEnemy = CreatePooledEnemy(prefab);
        if (createdEnemy != null)
        {
            createdEnemy.gameObject.SetActive(true);
        }

        return createdEnemy;
    }

    public void Return(EnemyAI ai)
    {
        if (ai == null)
        {
            return;
        }

        if (!instanceToPrefab.TryGetValue(ai, out GameObject prefab) || prefab == null)
        {
            Object.Destroy(ai.gameObject);
            return;
        }

        EnsurePoolExists(prefab);
        ai.gameObject.SetActive(false);
        pools[prefab].Enqueue(ai);
    }

    private void EnsurePoolExists(GameObject prefab)
    {
        if (!pools.ContainsKey(prefab))
        {
            pools[prefab] = new Queue<EnemyAI>();
        }
    }

    private EnemyAI CreatePooledEnemy(GameObject prefab)
    {
        GameObject enemyObj = Object.Instantiate(prefab);
        enemyObj.SetActive(false);

        EnemyAI ai = enemyObj.GetComponent<EnemyAI>();
        if (ai == null)
        {
            Object.Destroy(enemyObj);
            return null;
        }

        instanceToPrefab[ai] = prefab;
        return ai;
    }
}
