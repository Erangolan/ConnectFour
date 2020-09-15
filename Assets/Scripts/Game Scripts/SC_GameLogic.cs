using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using AssemblyCSharp;
using com.shephertz.app42.gaming.multiplayer.client;
using com.shephertz.app42.gaming.multiplayer.client.events;

public class SC_GameLogic : MonoBehaviour
{
    public Dictionary<string, GameObject> unityObjects;
    static SC_GameLogic instance;
    private GlobalEnums.SlotState[,] currBoard;
    private GlobalEnums.SlotState currState;
    public Sprite Red, Yellow, Black;
    private int moveCounter = 0;
    private float startTime = 0;
    private bool gameStarted = false;
    private string curTurn = string.Empty;
    private bool isMyTurn = false;
    private GlobalEnums.SlotState playerState;


    #region Singleton
    public static SC_GameLogic Instance
    {
        get
        {
            if (instance == null)
                instance = GameObject.Find("SC_GameLogic").GetComponent<SC_GameLogic>();

            return instance;
        }
    }
    #endregion

    #region MonoBehaviour
    private void OnEnable()
    {
        Listener.OnGameStarted += OnGameStarted;
        Listener.OnMoveCompleted += OnMoveCompleted;
        Listener.OnGameStopped += OnGameStopped;
    }

    private void OnDisable()
    {
        Listener.OnGameStarted -= OnGameStarted;
        Listener.OnMoveCompleted -= OnMoveCompleted;
        Listener.OnGameStopped -= OnGameStopped;
    }
    void Awake()
    {
        Init();
    }

    private void Update()
    {
        Timer();
    }

    #endregion

    #region Logic
    public void Init()
    {
        unityObjects = new Dictionary<string, GameObject>();
        GameObject[] _objects = GameObject.FindGameObjectsWithTag("UnityObject");
        foreach (GameObject g in _objects)
            unityObjects.Add(g.name, g);

        unityObjects["PopUp_GameOver"].SetActive(false);
    }

    private void RestartGame()
    {
        moveCounter = 0;
        currBoard = new GlobalEnums.SlotState[GlobalVariables.rowsAmount, GlobalVariables.collumnsAmount];
        for (int i = 0; i < GlobalVariables.rowsAmount; i++)
        {
            for (int j = 0; j < GlobalVariables.collumnsAmount; j++)
            {
                int tmp = i * 10 + j;
                unityObjects["Btn_Slot" + tmp].GetComponent<SC_Slot>().ChangeSlotState(GlobalEnums.SlotState.Empty);
                currBoard[i, j] = GlobalEnums.SlotState.Empty;
                if (i == 0)
                    unityObjects["Btn_Slot" + tmp].GetComponent<Button>().interactable = true;
                else
                    unityObjects["Btn_Slot" + tmp].GetComponent<Button>().interactable = false;
            }
        }

        if (GlobalVariables.curOpponent == GlobalEnums.Opponent.AI)
        {
            if (UnityEngine.Random.Range(0, 2) == 0)
            {
                //AI
                currState = GlobalEnums.SlotState.Red;
                unityObjects["Img_currState"].GetComponent<Image>().sprite = Red;
                string _idx = GetRandomSlot();
                PlacementLogic(_idx);
            }
            else
            {
                currState = GlobalEnums.SlotState.Yellow;
                unityObjects["Img_currState"].GetComponent<Image>().sprite = Yellow;
            }
        }
    }

    private void PassTurn()
    {
        if (currState == GlobalEnums.SlotState.Red)
        {
            currState = GlobalEnums.SlotState.Yellow;
            unityObjects["Img_CurrState"].GetComponent<Image>().sprite = Yellow;
        }
        else
        {
            currState = GlobalEnums.SlotState.Red;
            unityObjects["Img_CurrState"].GetComponent<Image>().sprite = Red;
        }
    }

    private void ChangeInteraction(bool _IsActive)
    {
        for (int i = 0; i < GlobalVariables.rowsAmount; i++)
            for (int j = 0; j < GlobalVariables.collumnsAmount; j++)
            {
                int tmp = i * 10 + j;
                unityObjects["Btn_Slot" + tmp].GetComponent<Button>().interactable = _IsActive;
            }
    }

    private GlobalEnums.MatchState IsMatchOver()
    {
        //row win
        for (int i = 0; i < 6; i++)
            for (int j = 0; j < 4; j++)
                if (currBoard[i, j] != GlobalEnums.SlotState.Empty && currBoard[i, j] == currBoard[i, j + 1] && currBoard[i, j] == currBoard[i, j + 2] && currBoard[i, j] == currBoard[i, j + 3])
                    return GlobalEnums.MatchState.Winner;

        //right to up diagonal win
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 4; j++)
                if (currBoard[i, j] != GlobalEnums.SlotState.Empty && currBoard[i, j] == currBoard[i + 1, j + 1] && currBoard[i, j] == currBoard[i + 2, j + 2] && currBoard[i, j] == currBoard[i + 3, j + 3])
                    return GlobalEnums.MatchState.Winner;

        //right to down diagonal win
        for (int i = 5; i > 2; i--)
            for (int j = 0; j < 4; j++)
                if (currBoard[i, j] != GlobalEnums.SlotState.Empty && currBoard[i, j] == currBoard[i - 1, j + 1] && currBoard[i, j] == currBoard[i - 2, j + 2] && currBoard[i, j] == currBoard[i - 3, j + 3])
                    return GlobalEnums.MatchState.Winner;

        //left to up diagonal win
        for (int i = 0; i < 3; i++)
            for (int j = 3; j < 7; j++)
                if (currBoard[i, j] != GlobalEnums.SlotState.Empty && currBoard[i, j] == currBoard[i + 1, j - 1] && currBoard[i, j] == currBoard[i + 2, j - 2] && currBoard[i, j] == currBoard[i + 3, j - 3])
                    return GlobalEnums.MatchState.Winner;

        //left to down diagonal win
        for (int i = 3; i > 6; i++)
            for (int j = 3; j < 7; j++)
                if (currBoard[i, j] != GlobalEnums.SlotState.Empty && currBoard[i, j] == currBoard[i - 1, j - 1] && currBoard[i, j] == currBoard[i - 2, j - 2] && currBoard[i, j] == currBoard[i - 3, j - 3])
                    return GlobalEnums.MatchState.Winner;

        //collumn win
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 7; j++)
                if (currBoard[i, j] != GlobalEnums.SlotState.Empty && currBoard[i, j] == currBoard[i + 1, j] && currBoard[i, j] == currBoard[i + 2, j] && currBoard[i, j] == currBoard[i + 3, j])
                    return GlobalEnums.MatchState.Winner;

        if (moveCounter == GlobalVariables.slotAmount)
            return GlobalEnums.MatchState.Tie;

        return GlobalEnums.MatchState.NoWinner;
    }

    private GlobalEnums.MatchState PlacementLogic(string _Index)
    {
        int row = (int)Char.GetNumericValue(_Index, 0);
        int col = (int)Char.GetNumericValue(_Index, 1);
        GlobalEnums.MatchState _currentState = GlobalEnums.MatchState.NoWinner;

        if (currBoard[row, col] == GlobalEnums.SlotState.Empty)
        {
            moveCounter++;
            currBoard[row, col] = currState;
            int tmp = row * 10 + col;
            unityObjects["Btn_Slot" + tmp].GetComponent<SC_Slot>().ChangeSlotState(currState);
            unityObjects["Btn_Slot" + tmp].GetComponent<Button>().interactable = false;
            if (row < 5)
                unityObjects["Btn_Slot" + ((row + 1) * 10 + col)].GetComponent<Button>().interactable = true;

            //GlobalEnums.MatchState _isOver = IsMatchOver();
            _currentState = IsMatchOver();
            if (_currentState == GlobalEnums.MatchState.Tie || _currentState == GlobalEnums.MatchState.Winner)
            {
                ChangeInteraction(false);
                unityObjects["PopUp_GameOver"].SetActive(true);
                unityObjects["Btn_PopUp_Restart"].GetComponent<Button>().interactable = false;
                if (_currentState == GlobalEnums.MatchState.Winner)
                {
                    if (currState == GlobalEnums.SlotState.Red)
                        unityObjects["Img_PopUp_Winner_CurrState"].GetComponent<Image>().sprite = Red;
                    else
                        unityObjects["Img_PopUp_Winner_CurrState"].GetComponent<Image>().sprite = Yellow;

                    unityObjects["Img_PopUp_Winner_CurrState"].GetComponent<Image>().enabled = true;
                    unityObjects["Txt_PopUp_Winner"].GetComponent<Text>().text = "The Winner is: ";
                }
                else
                {
                    unityObjects["Img_PopUp_Winner_CurrState"].GetComponent<Image>().enabled = false;
                    unityObjects["Txt_PopUp_Winner"].GetComponent<Text>().text = "The game is Tied";
                }
                gameStarted = false;
                unityObjects["Txt_TurnTime"].GetComponent<Text>().text = string.Empty;
                //WarpClient.GetInstance().stopGame();
            }
            else 
                PassTurn();

            if (GlobalVariables.curOpponent == GlobalEnums.Opponent.AI &&
                currState == GlobalEnums.SlotState.Red && _currentState == GlobalEnums.MatchState.NoWinner)
                StartCoroutine(PlayAI());
        }
        return _currentState;
    }

    private string GetRandomSlot()
    {
        List<List<int>> _list = new List<List<int>>();

        for (int i = 0; i < GlobalVariables.rowsAmount; i++)
            for (int j = 0; j < GlobalVariables.collumnsAmount; j++)
            {
                int tmp = i * 10 + j;
                if (unityObjects["Btn_Slot" + tmp].GetComponent<Button>().interactable == true)
                    _list.Add(new List<int> { i, j });
            }

        int _rand = UnityEngine.Random.Range(0, _list.Count);
        return _list[_rand][0].ToString() + _list[_rand][1].ToString();
    }

    private IEnumerator PlayAI()
    {
        int _rand = UnityEngine.Random.Range(1, 3);
        yield return new WaitForSeconds(_rand);

        string _idx = GetRandomSlot();
        PlacementLogic(_idx);
    }

    private void Timer()
    {
        if (gameStarted)
        {
            float _curTime = Time.time - startTime;
            float _leftTime = GlobalVariables.turnTime - _curTime;
            if (_leftTime > 0)
                unityObjects["Txt_TurnTime"].GetComponent<Text>().text = ((int)_leftTime).ToString();
            else unityObjects["Txt_TurnTime"].GetComponent<Text>().text = "0";
        }
    }

    #endregion

    #region Controller

    public void Btn_Slot(string _Index)
    {
        if ((currState == GlobalEnums.SlotState.Yellow && GlobalVariables.curOpponent == GlobalEnums.Opponent.AI) ||
            isMyTurn == true && GlobalVariables.curOpponent == GlobalEnums.Opponent.Player)
        {
            Debug.Log("curOpponent " + GlobalVariables.curOpponent);
            PlacementLogic(_Index);

            Dictionary<string, object> _toSend = new Dictionary<string, object>();
            _toSend.Add("Index", _Index);
            string _sendData = MiniJSON.Json.Serialize(_toSend);
            WarpClient.GetInstance().sendMove(_sendData);
            Debug.Log("Sent Data: " + _sendData);
        }
    }

    public void Btn_PopUp_Restart()
    {
        if (GlobalVariables.curOpponent == GlobalEnums.Opponent.Player)
        {
            unityObjects["PopUp_GameOver"].SetActive(false);
            WarpClient.GetInstance().startGame();
        }
        else 
            RestartGame();
    }

    public void PointerEnter(string _Index)
    {
        if (unityObjects["Btn_Slot" + _Index].GetComponent<Button>().interactable == true)
            unityObjects["Btn_Slot" + _Index].GetComponent<SC_Slot>().ChangeSlotState(GlobalEnums.SlotState.Black);
    }

    public void PointerExit(string _Index)
    {
        if (unityObjects["Btn_Slot" + _Index].GetComponent<Button>().interactable == true)
            unityObjects["Btn_Slot" + _Index].GetComponent<SC_Slot>().ChangeSlotState(GlobalEnums.SlotState.Empty);
    }

    #endregion

    #region Events
    private void OnGameStarted(string _Sender, string _RoomId, string _NextTurn)
    {
        RestartGame();

        startTime = Time.time;
        gameStarted = true;
        curTurn = _NextTurn;
        currState = GlobalEnums.SlotState.Yellow;
        unityObjects["Img_CurrState"].GetComponent<Image>().sprite = Yellow;

        if (curTurn == GlobalVariables.userId)
        {
            isMyTurn = true;
            playerState = GlobalEnums.SlotState.Yellow;
            unityObjects["Img_MyState"].GetComponent<Image>().sprite = Yellow;
        }
        else
        {
            isMyTurn = false;
            playerState = GlobalEnums.SlotState.Red;
            unityObjects["Img_MyState"].GetComponent<Image>().sprite = Red;
        }

    }

    private void OnMoveCompleted(MoveEvent _Move)
    {
        if (_Move.getSender() != GlobalVariables.userId)
        {
            Dictionary<string, object> _data = (Dictionary<string, object>)MiniJSON.Json.Deserialize(_Move.getMoveData());
            if (_data != null && _data.ContainsKey("Index"))
            {
                string _index = _data["Index"].ToString();
                GlobalEnums.MatchState _currentState = PlacementLogic(_index);
                if (_currentState == GlobalEnums.MatchState.Tie || _currentState == GlobalEnums.MatchState.Winner)
                    WarpClient.GetInstance().stopGame();
            }
        }

        startTime = Time.time;
        if (_Move.getNextTurn() == GlobalVariables.userId)
            isMyTurn = true;
        else 
            isMyTurn = false;
    }

    private void OnGameStopped(string _Sender, string _RoomId)
    {
        Debug.Log("OnGameStopped " + _Sender + " " + _RoomId);
        unityObjects["Btn_PopUp_Restart"].GetComponent<Button>().interactable = true;
    }
    #endregion
}