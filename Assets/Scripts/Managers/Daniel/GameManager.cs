using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int health;
    public PlayerSceneTrigger playerScene;
    public SceneManager sceneManager;

    void Awake()
    {
        
    }

    void Update()
    {
        
    }

    public void HealthDown()
    {
        if (health > 0)
        {
            health--;
            Debug.Log("Player Health: " + health);
        }
        else
        {
            playerScene.OnDie();
            Debug.Log("Wasted");
            sceneManager.Respawn();
            health = 1;
        }
    }
}
