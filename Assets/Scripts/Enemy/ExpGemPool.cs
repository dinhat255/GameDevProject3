using UnityEngine;
using System.Collections.Generic;

public class ExpGemPool
{
    private readonly Dictionary<GameObject, Queue<ExpGemPickup>> pools = new Dictionary<GameObject, Queue<ExpGemPickup>>();
    private readonly Dictionary<ExpGemPickup, GameObject> instanceToPrefab = new Dictionary<ExpGemPickup, GameObject>();

    public void PrewarmFromEnemies(IEnumerable<GameObject> enemyPrefabs, int countPerGem)
    {
        if (enemyPrefabs == null || countPerGem <= 0)
        {
            return;
        }

        foreach (GameObject enemyPrefab in enemyPrefabs)
        {
            if (enemyPrefab == null)
            {
                continue;
            }

            EnemyAI enemyAI = enemyPrefab.GetComponent<EnemyAI>();
            if (enemyAI == null || enemyAI.expGem == null)
            {
                continue;
            }

            EnsurePoolExists(enemyAI.expGem);

            for (int i = 0; i < countPerGem; i++)
            {
                ExpGemPickup pickup = CreatePooledGem(enemyAI.expGem);
                if (pickup != null)
                {
                    pools[enemyAI.expGem].Enqueue(pickup);
                }
            }
        }
    }

    public void Spawn(GameObject expPrefab, Vector3 position)
    {
        if (expPrefab == null)
        {
            return;
        }

        ExpGemPickup pickup = Rent(expPrefab);
        if (pickup == null)
        {
            return;
        }

        pickup.transform.SetPositionAndRotation(position, Quaternion.identity);
    }

    private ExpGemPickup Rent(GameObject expPrefab)
    {
        EnsurePoolExists(expPrefab);
        Queue<ExpGemPickup> pool = pools[expPrefab];

        while (pool.Count > 0)
        {
            ExpGemPickup pickup = pool.Dequeue();
            if (pickup != null)
            {
                pickup.gameObject.SetActive(true);
                return pickup;
            }
        }

        return CreatePooledGem(expPrefab);
    }

    private void Return(ExpGemPickup pickup)
    {
        if (pickup == null)
        {
            return;
        }

        if (!instanceToPrefab.TryGetValue(pickup, out GameObject expPrefab) || expPrefab == null)
        {
            Object.Destroy(pickup.gameObject);
            return;
        }

        EnsurePoolExists(expPrefab);
        pickup.gameObject.SetActive(false);
        pools[expPrefab].Enqueue(pickup);
    }

    private void EnsurePoolExists(GameObject expPrefab)
    {
        if (!pools.ContainsKey(expPrefab))
        {
            pools[expPrefab] = new Queue<ExpGemPickup>();
        }
    }

    private ExpGemPickup CreatePooledGem(GameObject expPrefab)
    {
        GameObject expObj = Object.Instantiate(expPrefab);
        expObj.SetActive(false);

        ExpGemPickup pickup = expObj.GetComponent<ExpGemPickup>();
        if (pickup == null)
        {
            pickup = expObj.AddComponent<ExpGemPickup>();
        }

        pickup.ConfigureReturnToPool(Return);
        instanceToPrefab[pickup] = expPrefab;
        return pickup;
    }
}
