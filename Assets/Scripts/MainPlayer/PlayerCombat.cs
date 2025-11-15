using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private int currentEXP = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Experience"))
        {
            currentEXP++;
            Destroy(other.gameObject);

            Debug.Log("EXP: " + currentEXP);
        }
    }
}
