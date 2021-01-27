using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineClear : SweetsClear
{
    public bool isRow;//是否为行

    public override void Clear()
    {
        base.Clear();
        if (isRow)
        {
            sweet.gameManager.ClearRow(sweet.Y);
        }
        else
        {
            sweet.gameManager.ClearRow(sweet.X);
        }
    }
}
