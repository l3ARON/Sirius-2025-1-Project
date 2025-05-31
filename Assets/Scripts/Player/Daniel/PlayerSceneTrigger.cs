using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSceneTrigger : MonoBehaviour
{
    Rigidbody2D rigid;
    public SceneManager sceneManager;
    SpriteRenderer spriteRender;
    public GameManager gameManager;
    public smallEnemyDash dashScript;          // 평소 이동 스크립트 참조

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRender = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "MoveStage")
        {
            int stagenum = int.Parse(collision.gameObject.name);
            //Next Stage
            sceneManager.NextStage(stagenum);
        } 
        
        if (collision.gameObject.tag == "RespawnTag")
        {
            sceneManager.SetRespawn(collision.transform.position);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            dashScript = collision.gameObject.GetComponent<smallEnemyDash>();
            OnDamaged(collision.transform.position);
            
        }
    }

    public void VelocityZero()
    {
        rigid.velocity = Vector2.zero;
    }



    void OnDamaged(Vector2 targetPos)
    {
        //HP Down
        gameManager.HealthDown();

        gameObject.layer = 11;
        //view alpha
        spriteRender.color = new Color(1,1,1,0.4f);
        //reaction Force
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc,1)*7, ForceMode2D.Impulse);
        
        Invoke("OffDamaged",3);
    }

    void OffDamaged()
    {
        gameObject.layer = 10;
        spriteRender.color = new Color(1,1,1,1);
    }

    public void OnDie()
    {
        spriteRender.color = new Color(1,1,1,0.4f);
        //Flip Y
        // spriteRender.flipY = true;
        // //Collider Disable
        // colli.enabled = false;
        // //Die Effect Jump
        // rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse); 
    }

    public void SmallEnemyMoveReset() //소형 몬스터가 유저를 죽이면 움직임 활성화
    {
        dashScript.StopDash(); // 이동 재개
    }
}