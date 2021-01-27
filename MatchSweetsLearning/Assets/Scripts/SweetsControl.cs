using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweetsControl : MonoBehaviour
{
    private int x;
    private int y;
    private GameManager.GridType type;//获取另一个脚本类型变量的类型名格式“类名.变量名”

    [HideInInspector]
    public GameManager gameManager;//GameManager脚本

    private SweetsMove moveComponent;//SweetMove脚本

    private SweetsAdd typeComponent;//甜品类型脚本

    private SweetsClear clearComponent;//消除脚本

    public int X { get => x; set => x = value; }
    public int Y { get => y; set => y = value; }
    public GameManager.GridType Type { get => type;}//后续不用修改，就无需set方法
    public SweetsMove MoveComponent { get => moveComponent;}
    public SweetsAdd TypeComponent { get => typeComponent;}
    public SweetsClear ClearComponent { get => clearComponent;}

    //记得在awake里面初始化赋值该脚本。
    private void Awake()
    {
        moveComponent = GetComponent<SweetsMove>();
        typeComponent = GetComponent<SweetsAdd>();
        clearComponent = GetComponent<SweetsClear>();
    }
    //判断是否可以移动
    public bool CanMove()
    {
        return moveComponent != null;//直接判断移动的脚本是否被调用来判断甜品是否移动
    }

    //判断是否可以更改甜品类型
    public bool CanChangeType()
    {
        return typeComponent != null;//直接判断变型的脚本是否被调用来判断甜品是否可以变型
    }
    //判断是否可以清除
    public bool CanClear()
    {
        return clearComponent != null;
    }

    //定义一个初始化方法
    public void Init(int _x, int _y, GameManager _gameManager, GameManager.GridType _type)
    {
        x = _x;
        y = _y;
        gameManager = _gameManager;
        type = _type;
    }

    //鼠标进入
    private void OnMouseEnter()
    {
        gameManager.EnterSweet(this);
    }
    //鼠标按下
    private void OnMouseDown()
    {
        gameManager.PressSweet(this);
    }
    //鼠标抬起
    private void OnMouseUp()
    {
        gameManager.ReleaseMouse();
    }
}
