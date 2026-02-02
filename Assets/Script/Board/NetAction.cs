using System;
using UnityEngine;

[Serializable]
public struct NetAction
{
    // ¸»ÀÇ °íÀ¯ ID
    public int pieceId;
    // ½ÃÀÛ Ä­
    public GridIndex from;
    // µµÂø Ä­
    public GridIndex to;
    public NetMoveType netMoveType;
}
