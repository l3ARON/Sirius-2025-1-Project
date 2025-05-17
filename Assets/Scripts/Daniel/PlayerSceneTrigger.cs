using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSceneTrigger : MonoBehaviour
{
    Rigidbody2D rigid;
    public SceneManager sceneManager;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Stage")
        {
            int stagenum = int.Parse(collision.gameObject.name);
            //Next Stage
            sceneManager.NextStage(stagenum);
        } 
    }

    public void VelocityZero()
    {
        rigid.velocity = Vector2.zero;
    }
}