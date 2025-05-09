using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float maxSpeed;
    Rigidbody2D rigid;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // move by key control
        float h = Input.GetAxisRaw("Horizontal");

        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

        if(rigid.velocity.x > maxSpeed) // right Speed
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        if(rigid.velocity.x < (-1)*maxSpeed) // left Speed
            rigid.velocity = new Vector2((-1)*maxSpeed, rigid.velocity.y);
    }
}
