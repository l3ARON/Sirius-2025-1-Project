using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float maxSpeed;
    public float jumpForce;
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator anim;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        // Jump
        if(Input.GetButtonDown("Jump") && !anim.GetBool("isJumping")){
            rigid.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            anim.SetBool("isJumping", true);
        }
        
        // stop Speed
        if(Input.GetButtonUp("Horizontal")){
            rigid.velocity = new Vector2(rigid.velocity.normalized.x*0.5f, rigid.velocity.y);
        }
        // 방향전환
        if(Input.GetButtonDown("Horizontal"))
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;

        // 애니메이션
        if(Mathf.Abs(rigid.velocity.x) < 0.5)
            anim.SetBool("isWalking", false);
        else
            anim.SetBool("isWalking", true);
    }
    void FixedUpdate()
    {
        // move by key control
        float h = Input.GetAxisRaw("Horizontal");

        // move spped
        rigid.AddForce(Vector2.right * h * 10f, ForceMode2D.Impulse);

        // max speed
        if(rigid.velocity.x > maxSpeed) // right Speed
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        if(rigid.velocity.x < (-1)*maxSpeed) // left Speed
            rigid.velocity = new Vector2((-1)*maxSpeed, rigid.velocity.y);

        // 착지
        if(rigid.velocity.y < -0.3){
            Debug.DrawRay(rigid.position, Vector3.down, new Color(0, 1, 0));
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1);
            if(rayHit.collider != null){
                if(rayHit.distance < 0.5f)
                    Debug.Log(rayHit.collider.name);
                    anim.SetBool("isJumping", false);
            }else
                Debug.Log("NULL");
        }
        
    }
}
