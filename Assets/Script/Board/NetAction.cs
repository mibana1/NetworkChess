using System;
using UnityEngine;

[Serializable]
public struct NetAction
{
    // 말의 고유 ID
    public int pieceId;

    // 시작 칸
    public int fromX;
    public int fromY;

    // 도착 칸
    public int toX;
    public int toY;

    // 이동 타입
    public NetMoveType netMoveType;

    // ===== 편의 프로퍼티 (로컬용) =====
    public GridIndex From => new GridIndex(fromX, fromY);
    public GridIndex To => new GridIndex(toX, toY);

    // ===== 생성 헬퍼 =====
    public static NetAction Create(
        int pieceId,
        GridIndex from,
        GridIndex to,
        NetMoveType type
    )
    {
        return new NetAction
        {
            pieceId = pieceId,
            fromX = from.X,
            fromY = from.Y,
            toX = to.X,
            toY = to.Y,
            netMoveType = type
        };
    }
}
