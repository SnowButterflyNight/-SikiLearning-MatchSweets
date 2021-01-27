using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweetsClear : MonoBehaviour
{
    public AnimationClip clearAnimation;//消除动画
    private bool isClearing;//是否正在消除
    public bool IsClearing { get => isClearing;}
    protected SweetsControl sweet;//甜品脚本
    public AudioClip destoryAudio;//消除音效
    //忘记获取甜品脚本了
    private void Awake()
    {
        sweet = GetComponent<SweetsControl>();
    }
    //消除方法，虚函数方便后续拓展
    public virtual void Clear()
    {
        isClearing = true;
        StartCoroutine(ClearCoroutine());
    }
    //消除协程
    private IEnumerator ClearCoroutine()
    {
        //Animation animator = GetComponent<Animation>();
        //是animation不是animator！！
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(clearAnimation.name);
            //加分，记得要实例话后才能访问其它类里的变量。
            GameManager.Instance.gameScore++;
            //播放消除音效
            AudioSource.PlayClipAtPoint(destoryAudio, transform.position);
            yield return new WaitForSeconds(clearAnimation.length);
            Destroy(gameObject);
        }
    }
}
