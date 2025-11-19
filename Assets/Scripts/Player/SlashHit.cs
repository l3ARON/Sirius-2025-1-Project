using UnityEngine;

public class SlashHit : MonoBehaviour
{
    private PlayerAttack2D playerAttack;
    // private bool hasHit = false;

    void Awake()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            playerAttack = playerObj.GetComponent<PlayerAttack2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && playerAttack != null)
        {
            playerAttack.DealDamageToEnemy(collision.gameObject);
            Debug.Log(collision.gameObject);
        }
    }
}
