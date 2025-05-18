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
    public Camera myCamera;
    int temp;

    [SerializeField] Vector3[][] Teleport = new Vector3[][]
    {       
    new Vector3[] { new Vector3(0, 0, 0), new Vector3(42, 8, 0) }, // Teleport[0]
    new Vector3[] { new Vector3(0, 0, 0), new Vector3(50, 0, 0), new Vector3(49, 30, 0)}, // Teleport[1]
    new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, -44, 0)}, // Teleport[2]
    new Vector3[] { new Vector3(0, 0, 0), new Vector3(85, 0, 0)}, // Teleport[3]
    new Vector3[] { new Vector3(0, 0, 0)}  // Teleport[4]
    };

    [SerializeField] Vector3[][] cameraPosition = new Vector3[][]
    {       
    new Vector3[] { new Vector3(13.69696f, 2.96734f, -10), new Vector3(-14.15286f, 0.02492642f, -10) }, // Teleport[0]
    new Vector3[] { new Vector3(11.36958f, 2.962502f, -10), new Vector3(-14.23454f, 3.031161f, -10), new Vector3(-14.36533f, 0.06812f, -10)}, // Teleport[1]
    new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, -44, 0)}, // Teleport[2]
    new Vector3[] { new Vector3(0, 0, 0), new Vector3(85, 0, 0)}, // Teleport[3]
    new Vector3[] { new Vector3(0, 0, 0)}  // Teleport[4]
    };


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
        yield return new WaitForSeconds(0.8f);
        ChangeMap();
        yield return new WaitForSeconds(1.3f);
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
        stageIndex = temp/10;
        Stages[stageIndex].SetActive(true);
    }

    void PlayerReposition()
    {
        int a = temp/10;
        int b = temp%10;
        player.transform.position = Teleport[a][b];
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
