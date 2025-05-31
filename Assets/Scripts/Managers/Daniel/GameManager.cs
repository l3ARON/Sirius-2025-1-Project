using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int health;
    private int maxHealth;
    public PlayerSceneTrigger playerScene;
    public SceneManager sceneManager;
    public Slider slider;
    public Sprite emptyHeart;
    public Sprite fullHeart;
    public Image[] hearts;

    void Awake()
    {
        maxHealth = health;
        slider.maxValue = maxHealth;
        slider.value = health;
    }

    void Update()
    {
        slider.value = health;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < health)
            {
                hearts[i].sprite = fullHeart;
            }
            else
            {
                hearts[i].sprite = emptyHeart;
            }
        }
    }

    public void HealthDown()
    {
        if (health > 1)
        {
            health--;
            Debug.Log("Player Health: " + health);
        }
        else
        {
            playerScene.OnDie();
            Debug.Log("Wasted");
            playerScene.SmallEnemyMoveReset();
            sceneManager.Respawn();
            health = 5;
        }
    }
}
