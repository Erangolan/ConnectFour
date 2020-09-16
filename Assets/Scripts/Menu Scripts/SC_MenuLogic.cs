using AssemblyCSharp;
using com.shephertz.app42.gaming.multiplayer.client;
using com.shephertz.app42.gaming.multiplayer.client.events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SC_MenuLogic : MonoBehaviour
{
    private string apiKey;
    private string secretKey;
    public Dictionary<string, GameObject> unityObjects;
    private Dictionary<string, object> matchRoomData;
    public Listener listen;
    private List<string> roomIds;
    private int roomIdx = 0;
    private string roomId = string.Empty;

    #region Singleton
    static SC_MenuLogic instance;
    public static SC_MenuLogic Instance
    {
        get
        {
            if (instance == null)
                instance = GameObject.Find("SC_MenuLogic").GetComponent<SC_MenuLogic>();

            return instance;
        }
    }
    #endregion

    #region Monobehaviour

    private void OnEnable()
    {
        Listener.OnConnect += OnConnect;
        Listener.OnRoomsInRange += OnRoomsInRange;
        Listener.OnCreateRoom += OnCreateRoom;
        Listener.OnGetLiveRoomInfo += OnGetLiveRoomInfo;
        Listener.OnUserJoinRoom += OnUserJoinRoom;
        Listener.OnJoinRoom += OnJoinRoom;
        Listener.OnGameStarted += OnGameStarted;
    }

    private void OnDisable()
    {
        Listener.OnConnect -= OnConnect;
        Listener.OnRoomsInRange -= OnRoomsInRange;
        Listener.OnCreateRoom -= OnCreateRoom;
        Listener.OnGetLiveRoomInfo -= OnGetLiveRoomInfo;
        Listener.OnUserJoinRoom -= OnUserJoinRoom;
        Listener.OnJoinRoom -= OnJoinRoom;
        Listener.OnGameStarted -= OnGameStarted;
    }

    void Start()
    {
        MenuInit();
    }

    #endregion

    #region Logic
    private void MenuInit()
    {
        unityObjects = new Dictionary<string, GameObject>();
        GameObject[] _unityObjects = GameObject.FindGameObjectsWithTag("UnityObject");
        foreach (GameObject g in _unityObjects)
            unityObjects.Add(g.name, g);

        unityObjects["Game"].SetActive(false);

        if (listen == null)
            listen = new Listener();

        WarpClient.initialize(apiKey, secretKey);
        WarpClient.GetInstance().AddConnectionRequestListener(listen);
        WarpClient.GetInstance().AddChatRequestListener(listen);
        WarpClient.GetInstance().AddUpdateRequestListener(listen);
        WarpClient.GetInstance().AddLobbyRequestListener(listen);
        WarpClient.GetInstance().AddNotificationListener(listen);
        WarpClient.GetInstance().AddRoomRequestListener(listen);
        WarpClient.GetInstance().AddZoneRequestListener(listen);
        WarpClient.GetInstance().AddTurnBasedRoomRequestListener(listen);

        matchRoomData = new Dictionary<string, object>();
        matchRoomData.Add("Password", "Shenkar2020");

        GlobalVariables.userId = System.DateTime.Now.Ticks.ToString();
        unityObjects["Txt_Menu_UserId"].GetComponent<Text>().text = "UserId: " + GlobalVariables.userId;
        UpdateStatus("Connecting...");
        WarpClient.GetInstance().Connect(GlobalVariables.userId);
    }

    private void UpdateStatus(string _NewStatus)
    {
        unityObjects["Txt_Menu_CurrentStatus"].GetComponent<Text>().text = _NewStatus;
    }

    private void DoRoomSearchLogic()
    {
        if (roomIds.Count > 0 && roomIds.Count > roomIdx)
        {
            //Get Room Information
            UpdateStatus("Getting room Details: " + roomIds[roomIdx]);
            WarpClient.GetInstance().GetLiveRoomInfo(roomIds[roomIdx]);
        }
        else
        {
            //Create Room
            UpdateStatus("Creating a room...");
            WarpClient.GetInstance().CreateTurnRoom("Test", GlobalVariables.userId, 2, matchRoomData, GlobalVariables.turnTime);
        }
    }

    #endregion

    #region Events

    private void OnConnect(bool _IsSucces)
    {
        if (_IsSucces)
        {
            UpdateStatus("Connected.");
            unityObjects["Btn_Menu_Play"].GetComponent<Button>().interactable = true;
        }
        else UpdateStatus("Failed to connect.");
    }

    private void OnRoomsInRange(bool _IsSucces, MatchedRoomsEvent eventObj)
    {
        Debug.Log(_IsSucces + " " + eventObj.getRoomsData().Length);
        if (_IsSucces)
        {
            UpdateStatus("Parsing Rooms...");
            roomIds = new List<string>();
            foreach (var roomData in eventObj.getRoomsData())
            {
                Debug.Log("Room Id " + roomData.getId());
                Debug.Log("Room Owner " + roomData.getRoomOwner());
                roomIds.Add(roomData.getId());
            }

            roomIdx = 0;
            DoRoomSearchLogic();
        }
        else
        {
            UpdateStatus("Error fetching room data");
            unityObjects["Btn_Menu_Play"].GetComponent<Button>().interactable = true;
        }
    }

    private void OnCreateRoom(bool _IsSuccess, string _RoomId)
    {
        Debug.Log(_IsSuccess + " " + _RoomId);
        if (_IsSuccess)
        {
            unityObjects["Txt_Menu_RoomId"].GetComponent<Text>().text = "RoomId: " + _RoomId;
            UpdateStatus("Room Created, Waiting for an opponent...");
            roomId = _RoomId;
            WarpClient.GetInstance().JoinRoom(roomId);
            WarpClient.GetInstance().SubscribeRoom(roomId);
        }
    }

    private void OnGetLiveRoomInfo(LiveRoomInfoEvent _EventObj)
    {
        Dictionary<string, object> _params = _EventObj.getProperties();
        if (_params.ContainsKey("Password") == true && _params["Password"].ToString() == matchRoomData["Password"].ToString())
        {
            roomId = _EventObj.getData().getId();
            WarpClient.GetInstance().JoinRoom(roomId);
            WarpClient.GetInstance().SubscribeRoom(roomId);
        }
        else
        {
            roomIdx++;
            DoRoomSearchLogic();
        }
    }

    private void OnUserJoinRoom(RoomData eventObj, string _UserId)
    {
        UpdateStatus("User: " + _UserId + " has joined the room");
        if (_UserId != GlobalVariables.userId)
        {
            //Start Game
            //Debug.Log("Start Game");
            UpdateStatus("Starting Game..");
            WarpClient.GetInstance().startGame();
        }
    }

    private void OnJoinRoom(bool _IsSuccess, string _RoomId)
    {
        if (_IsSuccess)
        {
            UpdateStatus("Joined Room: " + _RoomId);
            unityObjects["Txt_Menu_RoomId"].GetComponent<Text>().text = "RoomId: " + _RoomId;
        }
    }

    private void OnGameStarted(string _Sender, string _RoomId, string _NextTurn)
    {
        UpdateStatus("Game Started, Turn: " + _NextTurn);
        unityObjects["Menu"].SetActive(false);
        unityObjects["Game"].SetActive(true);
    }

    #endregion

    #region Controller

    public void Btn_Menu_Play()
    {
        UpdateStatus("Searching for room...");
        unityObjects["Btn_Menu_Play"].GetComponent<Button>().interactable = false;
        WarpClient.GetInstance().GetRoomsInRange(1, 2);
        GlobalVariables.curOpponent = GlobalEnums.Opponent.Player;
    }

    #endregion
}
