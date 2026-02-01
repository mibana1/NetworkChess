using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;



public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_InputField createRoomInput;
    [SerializeField] TMP_InputField joinRoomInput;

    [SerializeField] GameObject roomPrefab;
    [SerializeField] Transform roomListpanel;

    private void Start()
    {
        PhotonNetwork.JoinLobby();
    }

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(createRoomInput.text, new RoomOptions { MaxPlayers = 2 });
        Debug.Log(createRoomInput.text + "방이 생성 되었습니다");
    }

    public void joinRoom()
    {
        PhotonNetwork.JoinRoom(joinRoomInput.text);
    }

    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomOrCreateRoom();
    }

    public void ExitLobby()
    {
        PhotonNetwork.LeaveLobby();
        SceneManager.LoadScene(0);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("로비에 입장하였습니다");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("방에 입장하여 룸 씬으로 전환요청까지 해둠");
        SceneManager.LoadScene("RoomScene");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform child in roomListpanel)
        {
            Destroy(child.gameObject);
        }
        foreach (RoomInfo roomInfo in roomList)
        {
            if (roomInfo.RemovedFromList)
            {
                continue;
            }
            var room = Instantiate(roomPrefab, roomListpanel);
            room.GetComponentInChildren<TMP_Text>().text = roomInfo.Name;
        }
    }
}
