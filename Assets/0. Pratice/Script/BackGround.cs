using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    private float length, startpos;
    public GameObject cam;
    public float parallaxFactor; // 0.1~1 사이 추천

    void Start()
    {
        startpos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        float temp = cam.transform.position.x * (1 - parallaxFactor);
        float dist = cam.transform.position.x * parallaxFactor;

        transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);

        if (temp > startpos + length) startpos += length;
        else if (temp < startpos - length) startpos -= length;

        // Parallax 스크립트 마지막 줄에 추가
        Vector3 fixedPos = transform.position;
        fixedPos.x = Mathf.Round(fixedPos.x * 100) / 100f; // 또는 0.01 대신 0.1 등
        fixedPos.y = Mathf.Round(fixedPos.y * 100) / 100f;
        transform.position = fixedPos;
    }



}
