using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Chat;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Net;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    #region Private Serializable Fields

    [Tooltip("The maximum number of players per room")]
    public byte maxPlayersPerRoom = 2;
        
    private string board = "Scene1";


    private int numberOfBoards = 2;
    #endregion
    
    #region Private Fields

    bool isConnecting = false;
    bool isQuickConnecting = false;
    bool isCreating = false;
    bool isJoining = false;

    bool roomIsSecret = true;

    bool isGoingSolo = false;

    public string roomName;

    string gameVersion = "1";

    #endregion



    #region MonoBehaviour CallBacks

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    #endregion
    

    #region Public Methods

    public void SecretRoom(bool checkMark)
    {
        if (checkMark)
        {
            roomIsSecret = true;
        }
        else
        {
            roomIsSecret = false;
        }
    }



    //connect or disconnect from photon server
    public void Connect()
    {
        // keep track of the will to join a room, because when we come back from the game we will get a callback that we are connected, so we need to know what to do then
        isConnecting = true;        
           
        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (!PhotonNetwork.IsConnected)
        {
            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = this.gameVersion;
        }
    }

    public void Disconnect()
    {
        PhotonNetwork.Disconnect();

        isConnecting = false;
    }

    public void QuickConnect()
    {
        isConnecting = true;
        isQuickConnecting = true;
        Connect();
    }

    public void CreateRoom()
    {
        isConnecting = true;
        isCreating = true;
        Connect();
    }

    public void JoinCustomRoom()
    {
        isJoining = true;
        Connect();
    }

    public void StartSPMatch()
    {
        SceneManager.LoadScene("SP" + board);
    }

    public void StartMatch()
    {
        if (PhotonNetwork.PlayerList.Length < 2)
        {
            isGoingSolo = true;
            PhotonNetwork.Disconnect();
            return;
        }

        PhotonNetwork.LoadLevel(board);        
    }


    public void GetTargetRoom(string input)
    {
        roomName = input;
    }
    
    public void SelectSmallBoard()
    {
        maxPlayersPerRoom = 2;

        board = "Scene1";               
    }
    
    public void SelectBigBoard()
    {
        maxPlayersPerRoom = 4;

        board = "Scene2";
    }
     
    #endregion


    #region PunCallbacks CallBacks

    /// Called after the connection to the master is established and authenticated
    public override void OnConnectedToMaster()
    {        
        if (isConnecting)
        {          
            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
            if (isQuickConnecting)
            {
                PhotonNetwork.JoinRandomRoom();
            }
            else if (isCreating)
            {
                if (roomName == null)
                {
                    roomName = PhotonNetwork.NickName;
                }

                PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = this.maxPlayersPerRoom,IsVisible = roomIsSecret });
            }
            else if (isJoining)
            {
                PhotonNetwork.JoinRoom(roomName);
            }
        }
    }
    
    
    /// Called when a JoinRandom() call failed. The parameter provides ErrorCode and message.
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        if (roomName == null)
        {
            roomName = PhotonNetwork.NickName;
        }
        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = maxPlayersPerRoom });        
    }

    /// Called after disconnecting from the Photon server.
    public override void OnDisconnected(DisconnectCause cause)
    {
        if (isGoingSolo)
        {
            StartSPMatch();
            return;
        }       

        SceneManager.LoadScene("MenuScene");
    }

    
    #endregion
}
