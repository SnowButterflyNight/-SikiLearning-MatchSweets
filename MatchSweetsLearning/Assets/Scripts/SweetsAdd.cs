using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweetsAdd : MonoBehaviour
{
    public enum SweetsType
    {
        BLUE,
        GREEN,
        PINK,
        PURPLE,
        RED,
        YELLOW,
        RAINBOW,
        COUNT
    }

    //甜品类型的结构体
    [System.Serializable]
    public struct SweetSprite
    {
        public SweetsType type;
        public Sprite sprite;
    }
    //结构体数组
    public SweetSprite[] SweetSprites;

    //需要一个字典来建立甜品种类和甜品sprite的关系
    private Dictionary<SweetsType, Sprite> typeSpriteDict;

    //sprite渲染器
    private SpriteRenderer sprite;
    
    //类型的成员变量，用来做安全检测。
    private SweetsType type;
    public SweetsType Type
    {
        get
        {
            return type;
        }
        set
        {
            SetType(value);
        }
    }

    //种类数量
    public int TypeNum
    {
        get { return SweetSprites.Length; }
    }
    private void Awake()
    {
        //注意这里是渲染层的获取，所以Find的对象是渲染层。
        sprite = transform.Find("SweetsRender").GetComponent<SpriteRenderer>();

        typeSpriteDict = new Dictionary<SweetsType, Sprite>();

        for (int i = 0; i < SweetSprites.Length; i++)
        {
            if (!typeSpriteDict.ContainsKey(SweetSprites[i].type))
            {
                typeSpriteDict.Add(SweetSprites[i].type, SweetSprites[i].sprite);
            }
        }
    }

    //安全检验。如果包含这个类型，就用这个类型对应的sprite来渲染
    public void SetType(SweetsType newType)
    {
        type = newType;
        if(typeSpriteDict.ContainsKey(newType))
        {
            sprite.sprite = typeSpriteDict[newType];
        }
    }
}
