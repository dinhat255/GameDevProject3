using UnityEngine;
using System;

public class SlashAnim : MonoBehaviour
{
    private Animator animatorRef;
    private Action<GameObject> returnToPoolAction;

    private void Awake()
    {
        animatorRef = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (animatorRef != null)
        {
            animatorRef.Rebind();
            animatorRef.Update(0f);
        }
    }

    public void SetReturnToPoolAction(Action<GameObject> returnAction)
    {
        returnToPoolAction = returnAction;
    }

    public void DestroySelf()
    {
        if (returnToPoolAction != null)
        {
            returnToPoolAction.Invoke(gameObject);
            return;
        }

        Destroy(gameObject);
    }
}
