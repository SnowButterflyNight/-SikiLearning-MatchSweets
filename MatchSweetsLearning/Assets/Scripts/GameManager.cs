using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //枚举格子类型，名字规范化为全部大写。
    public enum GridType
    {
        EMPTY,//空
        NORMAL,//正常的甜品
        BARRIER,//障碍甜品
        ROW_CLEAR,//行消除
        COLUMN_CLEAR,//列消除
        RAINBOWCANDY,//彩虹糖道具
        COUNT//标记类型
    }

    //定义一个字典，将类型和对应的游戏预制体绑定，以便之后通过类型来查询使用
    public Dictionary<GridType, GameObject> gridPrefabDict;

    //给游戏预制体定义结构体，方便监视面板赋值
    [System.Serializable]
    public struct GridPrefab
    {
        public GridType type; //类型
        public GameObject prefab; //预制体
    }
    //格子的预制体有多个，因此需要一个结构体数组
    public GridPrefab[] gridPrefabs;

    //实例化出来的甜品数组
    //为了后面频繁对游戏物体进行操作，这里需要将游戏物体改为它的基础脚本。
    //public GameObject[,] sweets;
    public SweetsControl[,] sweets;

    //只用实例化一次，就定义为单例。
    private static GameManager _instance;

    //给属性定义get set方法。
    //ctrl + E + R
    public static GameManager Instance { get => _instance; set => _instance = value; }

    //行列格子数
    public int Column;
    public int Row;
    //格子预制体
    public GameObject girdPrefab;
    //填充时间
    public float fillTime;

    //定义两个需要交换位置的对象，一个是主动拖的，一个是被交换的
    private SweetsControl pressSweet;
    private SweetsControl enterSweet;

    //UI显示的时间
    public Text textTime;
    //用于计算的时间
    private float gameTime = 60;
    //游戏是否结束
    private bool isOver;
    //得分
    public int gameScore;
    //显示分数
    public Text textScore;
    //增加分数的计时器
    private float addScoreTimer;
    //当前的分数
    private float currentScore;
    //结算面板
    public GameObject gameOverPanel;
    //结算得分
    public Text finalScore;

    //在所有物体加载前调用就写在Awake方法里
    private void Awake()
    {
        //实例化单例
        _instance = this;
    }

    void Start()
    {
        //实例化字典
        gridPrefabDict = new Dictionary<GridType, GameObject>();
        //为字典添加内容
        for (int i = 0; i < gridPrefabs.Length; i++)
        {
            //如果不包含这个key值，就添加进去
            if(!gridPrefabDict.ContainsKey(gridPrefabs[i].type))
            {
                gridPrefabDict.Add(gridPrefabs[i].type, gridPrefabs[i].prefab);
            }
        }
        //双重循环实例化所有格子
        for (int x = 0; x < Column; x++)
        {
            for (int y = 0; y < Row; y++)
            {
                GameObject grid = Instantiate(girdPrefab, CorrectPos(x,y), Quaternion.identity);
                //将实例化的格子都放在GameManager下面，调用transform里的设置父级的方法，把GameManager自身的transform传进去
                grid.transform.SetParent(transform);
            }
        }

        //根据行列来实例化甜品
        //此处也修改为实例化基础脚本
        //sweets = new GameObject[Column, Row];
        sweets = new SweetsControl[Column, Row];
        for (int x = 0; x < Column; x++)
        {
            for (int y = 0; y < Row; y++)
            {
                CreateNewSweet(x, y, GridType.EMPTY);
                ////这里实例化的对象就是字典中的NORMAL类型预制体
                ////这里要改为实例化一个新的游戏物体
                ////sweets[x,y] = Instantiate(gridPrefabDict[GridType.NORMAL], CorrectPos(x, y), Quaternion.identity);
                ////sweets[x,y].transform.SetParent(transform);
                //GameObject newSweet = Instantiate(gridPrefabDict[GridType.NORMAL], CorrectPos(x, y), Quaternion.identity);
                //newSweet.transform.SetParent(transform);

                ////对基础脚本进行初始化
                ////先通过GO获取它身上的脚本
                //sweets[x, y] = newSweet.GetComponent<SweetsControl>();
                ////调用基础脚本里的初始化方法进行初始化赋值
                //sweets[x, y].Init(x, y, this, GridType.NORMAL);

                ////安全校验
                //if (sweets[x,y].CanMove())
                //{
                //    sweets[x, y].moveComponent.Move(x, y);
                //}
                //if(sweets[x,y].CanChangeType())
                //{
                //    //(SweetsAdd.SweetsType)是将int类强转为枚举类
                //    //这里Range的右边界调用类里的类型数量，方便以后的动态更改
                //    sweets[x, y].typeComponent.SetType((SweetsAdd.SweetsType)(Random.Range(0, sweets[x, y].typeComponent.TypeNum)));
                //}

                //测试代码，移动到自己原来的位置。
                //if (sweets[x, y].CanMove())
                //{
                //    sweets[x, y].moveComponent.Move(x, y);
                //}
            }
        }
        //障碍甜品产生测试
        Destroy(sweets[4, 4].gameObject);
        CreateNewSweet(4, 4, GridType.BARRIER);

        //启用协程
        StartCoroutine(AllFill());
    }

    void Update()
    {
        gameTime -= Time.deltaTime;
        if(gameTime <= 0)
        {
            gameTime = 0;//防止时间为负数
            gameOverPanel.SetActive(true);//弹出失败界面
            finalScore.text = gameScore.ToString();//赋值最终得分
            isOver = true;
            //return;
        }
        textTime.text = gameTime.ToString("0");//引号里的是格式化，0.0代表保留一位小数
        //用一个计时器来决定分数增加的时间
        if (addScoreTimer <= 0.06f)
        {
            addScoreTimer += Time.deltaTime;
        }
        else
        {
            if (currentScore < gameScore)
            {
                currentScore++;
                textScore.text = currentScore.ToString();//得分显示
                addScoreTimer = 0;
            }
        }
    }

    //位置纠正方法
    public Vector3 CorrectPos(int x, int y)
    {
        //格子的顺序为从左往右，从上到下。
        //x坐标=GameManger的x - 大网格长度的一半 + 行列对应的x。
        //y坐标=GameManger的y + 大网格长度的一半 - 行列对应的y。
        //除以2的后面加f是考虑到float类型的结果。
        return new Vector3(transform .position.x - Column / 2f + x, transform.position.y + Row / 2f - y, 0);
    }

    //产生甜品的方法，传递甜品的位置和格子类型参数，返回类型为甜品基类
    public SweetsControl CreateNewSweet(int x, int y, GridType type)
    {
        //实例化并赋值
        GameObject newSweet = Instantiate(gridPrefabDict[type], CorrectPos(x, y), Quaternion.identity);
        //设置生成位置的父类
        newSweet.transform.parent = transform;
        //获取基类
        sweets[x, y] = newSweet.GetComponent<SweetsControl>();
        //调用初始化方法
        sweets[x, y].Init(x, y, this, type);

        return sweets[x, y];
    }

    //全部填充
    //通过匹配传递回来的needRefill参数来判定是否需要调用填充方法
    public IEnumerator AllFill()
    {
        bool needRefill = true;
        while (needRefill)
        {
            yield return new WaitForSeconds(fillTime);
            //没填完就继续调用自身
            while (Fill())
            {
                //两次填充之间的等待时间
                yield return new WaitForSeconds(fillTime);
            }
            needRefill = MatchedSweetClear();
        }   
    }
    //分步填充
    public bool Fill()
    {
        //需要判断本次的填充是否完成
        bool filledNotOver = false;
        //遍历填充
        //为什么必须要从下往上判断，因为最下面的处理完后就不会再因为上面的情况改变而再改变
        for (int y = Row - 2; y >= 0; y--)
        {
            for(int x = 0; x < Column; x++)
            {
                SweetsControl sweet = sweets[x, y];//当前元素位置
                //判断是否可以往下落
                if(sweet.CanMove())
                {
                    SweetsControl sweetBelow = sweets[x, y + 1];//正下方的元素位置
                    //如果正下方的是空甜品，那么就改变该元素的位置至其正下方。
                    if(sweetBelow.Type == GridType.EMPTY)
                    {
                        //先消除原来的空格子对象再填充
                        Destroy(sweetBelow.gameObject);
                        //调用move方法，然后改变位置参数变量
                        sweet.MoveComponent.Move(x, y + 1, fillTime);
                        sweets[x, y + 1] = sweet;
                        //自身的位置就空了，因此要调用方法创建一个空甜品保证它上面的还能往下掉
                        CreateNewSweet(x, y, GridType.EMPTY);
                        filledNotOver = true;
                    }
                    //斜向填充
                    else
                    {
                        //down为-1表示左下，0表示正下，1表示右下
                        for (int down = -1; down <= 1; down++)
                        {
                            //排除正下
                            if (down != 0)
                            {
                                //累加得到左下和右下
                                int downX = x + down;
                                //边界检测
                                //判断是否需要进行斜向填充
                                if (downX >= 0 && downX < Column)
                                {
                                    SweetsControl downSweet = sweets[downX, y + 1];
                                    if (downSweet.Type == GridType.EMPTY)
                                    {
                                        bool canFill = true;//垂直填充是否满足填充条件
                                        for (int upY = y; upY >= 0; upY--)
                                        {
                                            SweetsControl upSweet = sweets[downX, upY];
                                            //如果空格子的上方格子是可以移动的甜品，那么就退出
                                            if (upSweet.CanMove())
                                            {
                                                break;
                                            }
                                            //如果空格子的上方是不能移动的且不为空的甜品，说明是障碍甜品
                                            else if (!upSweet.CanMove() && upSweet.Type != GridType.EMPTY)
                                            {
                                                canFill = false;
                                                break;
                                            }
                                        }
                                        //填充
                                        if (!canFill)
                                        {
                                            //先消除原来的空格子对象再填充
                                            Destroy(downSweet.gameObject);
                                            sweet.MoveComponent.Move(downX, y + 1, fillTime);
                                            sweets[downX, y + 1] = sweet;
                                            CreateNewSweet(x, y, GridType.EMPTY);
                                            filledNotOver = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        //最上面的一行，基本逻辑差不多，就是将下落改为新生成一个甜品
        for(int x = 0; x < Column; x++)
        {
            SweetsControl sweet = sweets[x, 0];//当前元素位置

            if (sweet.Type == GridType.EMPTY)
            {
                GameObject newSweet = Instantiate(gridPrefabDict[GridType.NORMAL], CorrectPos(x, -1), Quaternion.identity);
                newSweet.transform.parent = transform;

                //生成新的甜品，这里不能调用之前的方法，因为二维数组不包含这多的一行
                sweets[x, 0] = newSweet.GetComponent<SweetsControl>();
                sweets[x, 0].Init(x, -1, this, GridType.NORMAL);
                //移动并设置sprite
                sweets[x, 0].MoveComponent.Move(x, 0, fillTime);
                sweets[x, 0].TypeComponent.SetType((SweetsAdd.SweetsType)Random.Range(0, sweets[x, 0].TypeComponent.TypeNum));
                filledNotOver = true;
            }
        }
        return filledNotOver;
    }

    //判断甜品是否相邻的方法
    private bool IsNeighbor(SweetsControl sweet1, SweetsControl sweet2)
    {
        //x相等，判断y是否相差1
        //y相等，判断x是否相差1
        return (sweet1.X == sweet2.X && Mathf.Abs(sweet1.Y - sweet2.Y) == 1) || (sweet1.Y == sweet2.Y && Mathf.Abs(sweet1.X - sweet2.X) == 1);
    }
    //甜品交换方法
    private void ExchangeSweets(SweetsControl sweet1, SweetsControl sweet2)
    {
        if(sweet1.CanMove() && sweet2.CanMove())
        {
            //更新数组中的位置坐标信息
            sweets[sweet1.X, sweet1.Y] = sweet2;
            sweets[sweet2.X, sweet2.Y] = sweet1;

            //满足行匹配或列匹配的时候交换位置
            if(MatchSweets(sweet1,sweet2.X,sweet2.Y)!= null || MatchSweets(sweet2, sweet1.X, sweet1.Y) != null || sweet1.Type == GridType.RAINBOWCANDY || sweet2.Type == GridType.RAINBOWCANDY)
            {
                int tempX = sweet1.X;
                int tempY = sweet1.Y;
                //用移动方法更改游戏物体的位置
                sweet1.MoveComponent.Move(sweet2.X, sweet2.Y, fillTime);
                sweet2.MoveComponent.Move(tempX, tempY, fillTime);

                //判断彩虹糖要消除的是哪一类的甜品
                if (sweet1.Type == GridType.RAINBOWCANDY && sweet1.CanChangeType() && sweet2.CanChangeType())
                {
                    ClearOneKind clearType = sweet1.GetComponent<ClearOneKind>();
                    if(clearType != null)
                    {
                        clearType.ClearType = sweet2.TypeComponent.Type;
                    }
                    SweetClear(sweet1.X, sweet1.Y);
                }
                if (sweet2.Type == GridType.RAINBOWCANDY && sweet2.CanChangeType() && sweet1.CanChangeType())
                {
                    ClearOneKind clearType = sweet2.GetComponent<ClearOneKind>();
                    if (clearType != null)
                    {
                        clearType.ClearType = sweet1.TypeComponent.Type;
                    }
                    SweetClear(sweet2.X, sweet2.Y);
                }

                //在交换的时候调用清除方法
                MatchedSweetClear();
                //别忘记在交换后调用填充
                StartCoroutine(AllFill());
                //防止二次交换
                pressSweet = null;
                enterSweet = null;
            }
            else//没法交换的就复原位置
            {
                sweets[sweet1.X, sweet1.Y] = sweet1;
                sweets[sweet2.X, sweet2.Y] = sweet2;
            }
        }
    }

    /// <summary>
    /// 甜品交互方法
    /// </summary>
    #region
    // 点击甜品的赋值方法
    public void PressSweet(SweetsControl sweet)
    {
        if (isOver)
        {
            return;
        }
        pressSweet = sweet;
    }
    // 被交换甜品的赋值方法
    public void EnterSweet(SweetsControl sweet)
    {
        if (isOver)
        {
            return;
        }
        enterSweet = sweet;
    }
    //当鼠标抬起时交换两个甜品的位置
    public void ReleaseMouse()
    {
        if (isOver)
        {
            return;
        }
        if (IsNeighbor(pressSweet, enterSweet))
        {
            ExchangeSweets(pressSweet,enterSweet);
        }    
    }
    #endregion

    /// <summary>
    /// 清除匹配方法
    /// 问题一：为啥唯独甜甜圈的预制体不满足算法消除
    /// </summary>
    #region
    //匹配方法
    //传递的位置参数是移动后的甜品位置，该位置是遍历的起点。
    public List<SweetsControl> MatchSweets(SweetsControl sweet, int desX, int desY)
    {
        //能改变类型的甜品才是可以匹配的
        if(sweet.CanChangeType())
        {
            SweetsAdd.SweetsType type = sweet.TypeComponent.Type;//先赋值类型
            List<SweetsControl> matchRowSweets = new List<SweetsControl>();//行遍历列表
            List<SweetsControl> matchColumnSweets = new List<SweetsControl>();//列遍历列表
            List<SweetsControl> finishMatchSweets = new List<SweetsControl>();//完成匹配列表

            //行匹配
            matchRowSweets.Add(sweet);
            //用i来表示判断方向，0是往左，1是往右
            for (int i = 0; i <= 1; i++)
            {
                for (int xDistance = 1; xDistance < Column; xDistance++)
                {
                    int x;
                    //向左移动
                    if (i == 0)
                    {
                        x = desX - xDistance;
                    }
                    else//向右移动
                    {
                        x = desX + xDistance;
                    }
                    //边界检测
                    if (x < 0 || x>= Column)
                    {
                        break;
                    }
                    //对于满足匹配条件的甜品要加到列表中
                    //既能改变类型同时和起始判断的甜品一样
                    if (sweets[x,desY].CanChangeType()&&sweets[x,desY].TypeComponent.Type == type)
                    {
                        matchRowSweets.Add(sweets[x,desY]);
                    }
                    else
                    {
                        break;
                    }    
                }
            }
            //将行的甜品先加到消除列表中
            //这里的3写成0了，wtf
            if(matchRowSweets.Count >= 3)
            {
                for(int i = 0; i < matchRowSweets.Count; i++)
                {
                    finishMatchSweets.Add(matchRowSweets[i]);
                }
            }
            //检查行遍历列表中的元素数量是否大于等于3，满足就进行下一步
            //考虑到需要对L型和T型情况的消除，满足后需要继续按照列方向进行判断
            if(matchRowSweets.Count >= 3)
            {
                for(int i = 0; i < matchRowSweets.Count; i++)
                {
                    //满足行匹配后继续进行列匹配判断
                    //0代表上，1代表下
                    for(int j = 0; j <= 1; j++)
                    {
                        for(int yDistance = 1; yDistance < Row; yDistance++)
                        {
                            int y;
                            if(j == 0)
                            {
                                y = desY - yDistance;
                            }
                            else
                            {
                                y = desY + yDistance;
                            }
                            if(y < 0 || y >= Row)
                            {
                                break;
                            }
                            if(sweets[matchRowSweets[i].X,y].CanChangeType() && sweets[matchRowSweets[i].X,y].TypeComponent.Type == type)
                            {
                                //满足条件后是加入到列匹配列表中
                                matchColumnSweets.Add(sweets[matchRowSweets[i].X,y]);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    //如果列匹配列表数量小于2，说明不满足消除条件，则清空列表
                    //如果满足消除条件，就把行和列的格子都加入消除队列中
                    if(matchColumnSweets.Count < 2)
                    {
                        matchColumnSweets.Clear();
                    }
                    else
                    {
                        for(int j = 0; j < matchColumnSweets.Count; j++)
                        {
                            finishMatchSweets.Add(matchColumnSweets[j]);
                        }
                        break;
                    }
                }
            }
            //如果匹配完成，就返回匹配完成列表
            if(finishMatchSweets.Count >= 3)
            {
                return finishMatchSweets;
            }
            //结束后清空三个列表
            matchColumnSweets.Clear();
            matchRowSweets.Clear();
            //finishMatchSweets.Clear();

            //列匹配
            matchColumnSweets.Add(sweet);
            //用i来表示判断方向，0是往下，1是往上
            for (int i = 0; i <= 1; i++)
            {
                for (int yDistance = 1; yDistance < Row; yDistance++)
                {
                    int y;
                    //向下移动
                    if (i == 0)
                    {
                        y = desY - yDistance;
                    }
                    else//向上移动
                    {
                        y = desY + yDistance;
                    }
                    //边界检测
                    if (y < 0 || y >= Row)
                    {
                        break;
                    }
                    //对于满足匹配条件的甜品要加到列表中
                    //既能改变类型同时和起始判断的甜品一样
                    if (sweets[desX, y].CanChangeType() && sweets[desX, y].TypeComponent.Type == type)
                    {
                        matchColumnSweets.Add(sweets[desX, y]);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            //将列的甜品先加到消除列表中
            //这里也是一样3写成了0
            if (matchColumnSweets.Count >= 3)
            {
                for (int i = 0; i < matchColumnSweets.Count; i++)
                {
                    finishMatchSweets.Add(matchColumnSweets[i]);
                }
            }
            //检查行遍历列表中的元素数量是否大于等于3，满足就把它们添加到匹配完成列表中
            if (matchColumnSweets.Count >= 3)
            {
                for (int i = 0; i < matchColumnSweets.Count; i++)
                {
                    //满足行匹配后继续进行列匹配判断
                    //0代表上，1代表下
                    for (int j = 0; j <= 1; j++)
                    {
                        for (int xDistance = 1; xDistance < Column; xDistance++)
                        {
                            int x;
                            if (j == 0)
                            {
                                x = desX - xDistance;
                            }
                            else
                            {
                                x = desX + xDistance;
                            }
                            if (x < 0 || x >= Column)
                            {
                                break;
                            }
                            if (sweets[x,matchColumnSweets[i].Y].CanChangeType() && sweets[x, matchColumnSweets[i].Y].TypeComponent.Type == type)
                            {
                                //满足条件后是加入到列匹配列表中
                                matchRowSweets.Add(sweets[x, matchColumnSweets[i].Y]);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    //如果列匹配列表数量小于2，说明不满足消除条件，则清空列表
                    //如果满足消除条件，就把行和列的格子都加入消除队列中
                    //教程是不是忘记加行匹配列表了？
                    if (matchRowSweets.Count < 2)
                    {
                        matchRowSweets.Clear();
                    }
                    else
                    {
                        for (int j = 0; j < matchRowSweets.Count; j++)
                        {
                            finishMatchSweets.Add(matchRowSweets[j]);
                        }
                        break;
                    }
                }
            }
            //如果匹配完成，就返回匹配完成列表
            if (finishMatchSweets.Count >= 3)
            {
                return finishMatchSweets;
            }
        }
        return null;
    }

    //清除方法
    public bool SweetClear(int x, int y)
    {
        //如果能清除并且没有正在清除
        if (sweets[x,y].CanClear() && !sweets[x,y].ClearComponent.IsClearing)
        {
            sweets[x, y].ClearComponent.Clear();
            CreateNewSweet(x, y, GridType.EMPTY);//别忘了消除后创新空甜品

            ClearBiscuit(x, y);//在消除甜品后调用消除饼干的方法
            return true;
        }
        return false;
    }
    //清除饼干的方法
    private void ClearBiscuit(int x, int y)
    {
        //左右遍历
        for (int neighborX = x-1; neighborX <= x+1; neighborX++)
        {
            //自身判断+边界判断
            if(neighborX != x && neighborX >= 0 && neighborX < Column)
            {
                //是否可以消除
                if(sweets[neighborX,y].Type == GridType.BARRIER && sweets[neighborX,y].CanClear())
                {
                    sweets[neighborX, y].ClearComponent.Clear();
                    CreateNewSweet(neighborX, y, GridType.EMPTY);//别忘记在清除后创建空的甜品
                }
            }
        }
        //上下遍历
        for (int neighborY = y - 1; neighborY <= y + 1; neighborY++)
        {
            //自身判断+边界判断
            if (neighborY != y && neighborY >= 0 && neighborY < Row)
            {
                //是否可以消除
                if (sweets[x, neighborY].Type == GridType.BARRIER && sweets[x, neighborY].CanClear())
                {
                    sweets[x, neighborY].ClearComponent.Clear();
                    CreateNewSweet(x, neighborY, GridType.EMPTY);//别忘记在清除后创建空的甜品
                }
            }
        }
    }

    //清除完成匹配的甜品
    private bool MatchedSweetClear()
    {
        bool needRefill = false;
        for (int y = 0; y < Row; y++)
        {
            for (int x = 0; x < Column; x++)
            {
                if (sweets[x, y].CanClear())
                {
                    List<SweetsControl> matchList = MatchSweets(sweets[x, y], x, y);

                    //如果匹配列表不为空，就调用消除方法
                    if (matchList != null)
                    {
                        //调用消除方法前判断是否生成特殊类型的甜品（道具）
                        GridType specialGridType = GridType.COUNT;//特殊甜品
                        SweetsControl randomSweet = matchList[Random.Range(0, matchList.Count)];//随机选取消除的格子
                        //赋值甜品位置
                        int specialSweetX = randomSweet.X;
                        int specialSweetY = randomSweet.Y;
                        //当同时消除的甜品数等于4时生成行列消除
                        if (matchList.Count == 4)
                        {
                            //先将行列消除添加到特殊甜品的枚举类里面
                            specialGridType = (GridType)Random.Range((int)GridType.ROW_CLEAR, (int)GridType.COLUMN_CLEAR);

                        }
                        //当同时消除的甜品数大于4时生成彩虹糖
                        if (matchList.Count >= 5)
                        {
                            specialGridType = GridType.RAINBOWCANDY;
                        }
;
                        for (int i = 0; i < matchList.Count; i++)
                        {
                            if(SweetClear(matchList[i].X, matchList[i].Y))
                            {
                                needRefill = true;
                            }
                        }
                        //如果它的类型不是特殊甜品就销毁并重新生成
                        if(specialGridType != GridType.COUNT)
                        {
                            Destroy(sweets[specialSweetX, specialSweetY]);
                            SweetsControl newSweet = CreateNewSweet(specialSweetX, specialSweetY, specialGridType);
                            //判断特殊甜品的类型
                            //以及是否能赋值甜品样式
                            if(specialGridType == GridType.ROW_CLEAR || specialGridType == GridType.COLUMN_CLEAR && newSweet.CanChangeType()&&matchList[0].CanChangeType())
                            {
                                //生成特殊甜品，它的甜品样式和消除列表的第一个甜品一样
                                newSweet.TypeComponent.SetType(matchList[0].TypeComponent.Type);
                            }
                            else if (specialGridType == GridType.RAINBOWCANDY && newSweet.CanChangeType())
                            {
                                newSweet.TypeComponent.SetType(SweetsAdd.SweetsType.RAINBOW);
                            }
                        }
                    }
                }
            }
        }
        return true;
    }
    #endregion

    //返回主界面功能
    public void RetureToMain()
    {
        SceneManager.LoadScene(0);
    }
    //重玩功能
    public void Restart()
    {
        SceneManager.LoadScene(1);
    }
    //清除行的方法
    public void ClearRow(int row)
    {
        for (int x = 0; x < Column; x++)
        {
            SweetClear(x, row);
        }
    }
    //清除列的方法
    public void ClearColumn(int column)
    {
        for (int y = 0; y < Row; y++)
        {
            SweetClear(column, y);
        }
    }
    //清除某一类甜品的方法
    public void ClearOneKind(SweetsAdd.SweetsType type)
    {
        //遍历全局消除某一类甜品
        for(int x = 0; x < Column; x++)
        {
            for(int y = 0; y < Row; y++)
            {
                if(sweets[x,y].CanChangeType() && (sweets[x,y].TypeComponent.Type == type || type == SweetsAdd.SweetsType.RAINBOW))
                {
                    SweetClear(x, y);
                }
            }
        }
    }
}
