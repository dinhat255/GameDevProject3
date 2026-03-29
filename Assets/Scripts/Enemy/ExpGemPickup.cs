using UnityEngine;
using System;

public class ExpGemPickup : MonoBehaviour
{
    [SerializeField] private int expValue = 1;

    private Action<ExpGemPickup> returnToPoolAction;

    public int ExpValue => expValue;

    public void ConfigureReturnToPool(Action<ExpGemPickup> onReturnToPool)
    {
        returnToPoolAction = onReturnToPool;
    }

    public void Collect()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayExpPickupSfx();
        }

        if (returnToPoolAction != null)
        {
            returnToPoolAction.Invoke(this);
            return;
        }

        Destroy(gameObject);
    }
}
