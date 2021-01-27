using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweetsMove : MonoBehaviour
{
    //定义基础脚本
    private SweetsControl sweet;
    //将移动的协程定义出来，方便终止该协程
    private IEnumerator moveCoroutine;

    //在Awake方法中获取基础脚本
    private void Awake()
    {
        sweet = GetComponent<SweetsControl>();
    }

    //移动方法，传递新的坐标参数
    //开启或结束move协程
    public void Move(int desX, int desY, float time)
    {
        //保证同一个时间只有一个协程
        //防止甜品在移动过程种又收到了新的目标位置移动的命令时旧和新的协程一同执行。
        if(moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        //协程赋值和开启
        moveCoroutine = MoveCoroutine(desX, desY, time);
        StartCoroutine(moveCoroutine);
        ////前面两个是改变属性变量，最后的才是改变甜品的本地坐标。
        //sweet.X = desX;
        //sweet.Y = desY;
        //sweet.transform.position = sweet.gameManager.CorrectPos(desX, desY);
    }

    //负责移动的协程，除了要传递移动的终点坐标外，还需要提供持续的时间
    private IEnumerator MoveCoroutine(int desX,int desY, float time)
    {
        //接收终点位置
        sweet.X = desX;
        sweet.Y = desY;
        //计算起始位置
        Vector3 startPos = transform.position;
        Vector3 endPos = sweet.gameManager.CorrectPos(desX, desY);
        //平滑移动
        //用一个循环来逐帧累加t，t/动画的总持续时间 用于计算整个目前是全部距离的几分之几
        for (float t = 0; t<time; t+=Time.deltaTime)
        {
            //插值移动
            sweet.transform.position = Vector3.Lerp(startPos, endPos, t / time);
            //每一帧都有一个返回值
            yield return 0;
        }
        //防止协程结束物体还没移到指定位置，此时直接强制移动到终点
        sweet.transform.position = endPos;
    }
}
