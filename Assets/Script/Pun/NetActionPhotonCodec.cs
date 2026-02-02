public static class NetActionPhotonCodec
{
    // 보내기: NetAction -> object[]
    public static object[] Encode(NetAction net)
    {
        return new object[]
        {
            net.pieceId,
            net.fromX, net.fromY,
            net.toX, net.toY,
            (int)net.netMoveType
        };
    }

    // 받기: object[] -> NetAction
    public static NetAction Decode(object[] data)
    {
        return new NetAction
        {
            pieceId = (int)data[0],
            fromX = (int)data[1],
            fromY = (int)data[2],
            toX = (int)data[3],
            toY = (int)data[4],
            netMoveType = (NetMoveType)(int)data[5]
        };
    }
}
