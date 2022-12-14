using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class Menu : MonoBehaviourPunCallbacks, ILobbyCallbacks
{
    [Header("Screens")]
    public GameObject mainScreen;
    public GameObject createRoomScreen;
    public GameObject lobbyScreen;
    public GameObject lobbyBrowserScreen;
    public GameObject tutorialScreen;
    public GameObject creditsScreen;

    [Header("Main Screen")]
    public Button createRoomButton;
    public Button findRoomButton;

    [Header("Lobby")]
    public TextMeshProUGUI playerListText;
    public TextMeshProUGUI roomInfoText;
    public Button startGameButton;

    [Header("Lobby Browser")]
    public RectTransform roomListContainer;
    public GameObject roomButtonPrefab;

    private List<GameObject> roomButtons = new List<GameObject>();
    private List<RoomInfo> roomList = new List<RoomInfo>();

    void Start ()
    {
        //disable the menu buttons at the start
        createRoomButton.interactable = false;
        findRoomButton.interactable = false;

        //enable the cursor
        Cursor.lockState = CursorLockMode.None;

        //are we in a game
        if(PhotonNetwork.InRoom)
        {
            //go to lobby
            SetScreen(lobbyScreen);
            UpdateLobbyUI();

            //make the room visible again
            PhotonNetwork.CurrentRoom.IsVisible = true;
            PhotonNetwork.CurrentRoom.IsOpen = true;
        }
    }

    void SetScreen (GameObject screen)
    {
        //disable all other screens
        mainScreen.SetActive(false);
        lobbyScreen.SetActive(false);
        createRoomScreen.SetActive(false);
        lobbyBrowserScreen.SetActive(false);
        tutorialScreen.SetActive(false);
        creditsScreen.SetActive(false);

        //activate requested screen
        screen.SetActive(true);

        if (screen == lobbyBrowserScreen)
            UpdateLobbyBrowserUI();
    }

    public void OnBackButton ()
    {
        SetScreen(mainScreen);
    }

        // MAIN SCREEN

    public void OnPlayerNameValueChanged (TMP_InputField playerNameInput)
    {
        PhotonNetwork.NickName = playerNameInput.text;
    }

    public override void OnConnectedToMaster()
    {
        createRoomButton.interactable = true;
        findRoomButton.interactable = true;
    }

    public void OnCreateRoomButton ()
    {
        SetScreen(createRoomScreen);
    }

    public void OnFindRoomButton ()
    {
        SetScreen(lobbyBrowserScreen);
    }

    public void OnTutorialButton ()
    {
        SetScreen(tutorialScreen);
    }

    public void OnCreditsButton ()
    {
        SetScreen(creditsScreen);
    }

        //CREATE ROOM SCREEN  

    public void OnCreateButton (TMP_InputField roomNameInput)
    {
        NetworkManager.instance.CreateRoom(roomNameInput.text);
    }

    //LOBBY SCREEN

    public override void OnJoinedRoom()
    {
        SetScreen(lobbyScreen);
        photonView.RPC("UpdateLobbyUI", RpcTarget.All);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateLobbyUI();
    }

    [PunRPC]
    void UpdateLobbyUI ()
    {
        // enable or disable start game button if host
        startGameButton.interactable = PhotonNetwork.IsMasterClient;

        //display all players
        playerListText.text = "";

        foreach (Player player in PhotonNetwork.PlayerList)
            playerListText.text += player.NickName + "\n";

        // set room info text
        roomInfoText.text = "<b>Room Name</b> \n" + PhotonNetwork.CurrentRoom.Name;
    }

    public void OnStartGameButton ()
    {
        //hide the room
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        //tell everyone to load into game scene
        NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, "Game");
    }

    public void OnLeaveLobbyButton ()
    {
        PhotonNetwork.LeaveRoom();
        SetScreen(mainScreen);
    }

    //LOBBY BROWSER SCREEN

    GameObject CreateRoomButton()
    {
        GameObject buttonObj = Instantiate(roomButtonPrefab, roomListContainer.transform);
        roomButtons.Add(buttonObj);

        return buttonObj;
    }

    void UpdateLobbyBrowserUI()
    {
        //disable all room buttons
        foreach (GameObject button in roomButtons)
            button.SetActive(false);

        //display all rooms in master server
        for(int x = 0; x < roomList.Count; ++x)
        {
            //get or create button object
            GameObject button = x >= roomButtons.Count ? CreateRoomButton() : roomButtons[x];

            button.SetActive(true);

            button.transform.Find("RoomNameText").GetComponent<TextMeshProUGUI>().text = roomList[x].Name;
            button.transform.Find("PlayerCountText").GetComponent<TextMeshProUGUI>().text = roomList[x].PlayerCount + " / " + roomList[x].MaxPlayers;

            //set btn on click event
            Button buttonComp = button.GetComponent<Button>();

            string roomName = roomList[x].Name;

            buttonComp.onClick.RemoveAllListeners();
            buttonComp.onClick.AddListener(() => { OnJoinRoomButton(roomName); });
        }
    }

    public void OnJoinRoomButton (string roomName)
    {
        NetworkManager.instance.JoinRoom(roomName);
    }

    public void OnRefreshButton ()
    {
        UpdateLobbyBrowserUI();
    }

    public override void OnRoomListUpdate (List<RoomInfo> allRooms)
    {
        roomList = allRooms;
    }
}
