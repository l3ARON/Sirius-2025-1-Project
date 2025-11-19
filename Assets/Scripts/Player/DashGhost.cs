using UnityEngine;

public class DashGhost : MonoBehaviour
{
    public float fadeSpeed = 5f;   // 숫자 클수록 빨리 사라짐
    SpriteRenderer sr;
    Color startColor;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        startColor = sr.color;
    }

    void Update()
    {
        Color c = sr.color;
        c.a -= fadeSpeed * Time.deltaTime;
        sr.color = c;

        if (c.a <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
