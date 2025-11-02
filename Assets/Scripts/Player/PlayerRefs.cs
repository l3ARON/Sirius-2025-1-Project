using UnityEngine;

/// <summary>
/// 플레이어가 공통으로 쓰는 컴포넌트/포인트/수치 모음
/// </summary>
public class PlayerRefs : MonoBehaviour
{
    [Header("Core Components")]
    public Rigidbody2D rigid;
    public SpriteRenderer spriteRenderer;
    public Animator anim;

    [Header("Attack Points")]
    public Transform attackPoint;   // 근접 공격 위치
    public Transform firePoint;     // 원거리 발사 위치

    [Header("Audio")]
    public AudioSource attackClip;
    public AudioSource dashClip;
    public AudioSource walkClip;

    void Awake()
    {
        // 비워놨을 때 자동으로 찾아줌
        if (rigid == null) rigid = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (anim == null) anim = GetComponent<Animator>();
    }
}
