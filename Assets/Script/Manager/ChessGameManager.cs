using Photon.Pun;
using UnityEngine;

public class ChessGameManager : MonoBehaviourPun
{
    public static ChessGameManager Instance;

    public ChessTeam MyColor { get; private set; }

    private void Awake()
    {
        Instance = this;

        MyColor = PhotonNetwork.IsMasterClient
            ? ChessTeam.White
            : ChessTeam.Black;

        Debug.Log($"³» »ö»ó: {MyColor}");
    }
}
