using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public int totalPoint;
    public int stagePoint;
    public int stageIndex;
    public int health;
    public PlayerSceneTrigger player;
    public GameObject[] Stages;
    public Animator transitionAnim;
    int temp;
    
    public void NextStage(int num)
    {
        if(stageIndex < Stages.Length)
        {
            StartCoroutine(LoadLevel());
            temp = num;
        }
        else
        {
            //GameOver
            Time.timeScale = 0;
            Debug.Log("게임 클리어!");
        }
        totalPoint += stagePoint;
        stagePoint = 0;
    }

    IEnumerator LoadLevel()
    {
        Debug.Log("Anim Start");
        transitionAnim.SetTrigger("End");
        yield return new WaitForSeconds(1);
        ChangeMap();
        transitionAnim.SetTrigger("Start");
    }

    void Update()
    {
        
    }

    void ChangeMap()
    {
        PlayerReposition();
        Stages[stageIndex].SetActive(false);
        Debug.Log("Next Stage");
        stageIndex = temp;
        Stages[stageIndex].SetActive(true);
    }

    void PlayerReposition()
    {
        player.transform.position = new Vector3(0,0,0);
        player.VelocityZero();
    }

    // public void HealthDown()
    // {
    //     if (health > 0)
    //     {
    //         health--;
    //     }
    //     else
    //     {
    //         //Player Die Effect
    //         player.OnDie();
    //         //Result UI
    //         Debug.Log("Wasted");
    //         //Retry Button UI

    //     }
    // }

    // void OnTriggerEnter2D(Collider2D collision)
    // {
    //     if(collision.gameObject.tag == "Player") 
    //     {   
    //         if (health > 1)
    //         {
    //             PlayerReposition();
    //         }
    //         HealthDown();
    //     }
        
    // }  
}
