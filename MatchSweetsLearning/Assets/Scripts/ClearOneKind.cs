using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearOneKind : SweetsClear
{
    private SweetsAdd.SweetsType clearType;

    public SweetsAdd.SweetsType ClearType { get => clearType; set => clearType = value; }

    public override void Clear()
    {
        base.Clear();
        sweet.gameManager.ClearOneKind(clearType);
    }
}
