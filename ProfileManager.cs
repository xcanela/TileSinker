using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using System.Net;
using UnityEngine.Networking;
using System.Collections;

public class ProfileManager : MonoBehaviourPunCallbacks
{        
    #region Public Fields

    [Tooltip("The prefab to use for representing the player")]
    public GameObject playerPrefab;

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;

    public GameObject playerProfileUI, playerProfileUIMMRtag, playerUIavatar;

    public GameObject startMatchButton;

    public GameObject inputField;

    public string playerName = playerNamePrefKey;
    
    public GameObject roomLabel;

    public GameObject signMenu, mainMenu;
    
    public string starterMMR = "1000";

    public GameObject rankTagPrefab;
    public Transform rankContainer;

    public GameObject testtext;

    public GameObject rules;

    #endregion
    
    #region Private Constants

    const string playerMMRPrefKey = "PlayerMMR";
    // Store the PlayerPref Key to avoid typos
    const string playerNamePrefKey = "PlayerName";

    const string playerAvatarID = "AvatarID";
    
    #endregion

    
    #region Mono CallBacks


    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during initialization phase.
    /// </summary>
    void Start()
    {
        string defaultName = string.Empty;
        InputField _inputField = inputField.GetComponent<InputField>();

        

        if (!PlayerPrefs.HasKey(playerNamePrefKey))
        {
            signMenu.SetActive(true);
            mainMenu.SetActive(false);

            PlayerPrefs.SetString("PlayerName", "NoNameGuest");
            PlayerPrefs.SetString(playerMMRPrefKey, starterMMR);
            PlayerPrefs.SetString(playerAvatarID, "1");
        }
        else if (PlayerPrefs.GetString(playerAvatarID).Equals("0"))
        {
            PlayerPrefs.SetString(playerAvatarID, "1");
        }

        if (_inputField != null)
        {
            if (PlayerPrefs.HasKey(playerNamePrefKey))
            {
                defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                _inputField.text = defaultName;
            }
        }
        else
        {

        }

        PhotonNetwork.NickName = PlayerPrefs.GetString(playerMMRPrefKey) + "|" + PlayerPrefs.GetString(playerNamePrefKey) + "|" + PlayerPrefs.GetString(playerAvatarID);

        SendMMR();
        
        ShowName();
    }

    private void Update()
    {
    }


    #endregion


    #region Public Methods

    public void ShowName()
    {
        playerProfileUI.SetActive(true);
        playerProfileUI.GetComponent<Text>().text = PlayerPrefs.GetString(playerNamePrefKey);
        playerProfileUIMMRtag.GetComponent<Text>().text = PlayerPrefs.GetString(playerMMRPrefKey);
        if (PlayerPrefs.HasKey(playerAvatarID))
        {
            playerUIavatar.GetComponent<AvatarHandler>().avatarid = Convert.ToInt32(PlayerPrefs.GetString(playerAvatarID));
        }
        else
        {
            PlayerPrefs.SetString(playerAvatarID, "0");
        }
    }    
    /// Sets the name of the player, and save it in the PlayerPrefs for future sessions.
    public void SetPlayerName(string value)
    {
        // #Important
        if (string.IsNullOrEmpty(value))
        {
            value = "NoNameGuest";
        }

        PhotonNetwork.NickName = PlayerPrefs.GetString(playerMMRPrefKey) + "|" + value + "|" + PlayerPrefs.GetInt(playerAvatarID);

        PlayerPrefs.SetString("PlayerName", value);

        playerName = PlayerPrefs.GetString(playerNamePrefKey);

        ShowName();
    }
   
    private void InstantiateMyPrefab()
    {
        PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);      
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RefreshRank()
    {
        GetRankingList();        
    }


    public void EraseProfile(string check)
    {
        if (Equals(check, "Sure"))
        {
            PlayerPrefs.DeleteAll();

            StartCoroutine(GetRequest("http://dreamlo.com/lb/pm3oYHCDf0m8D5-WLp6A3gIQLWB8LGVUm3EgniK_QODA/delete/" + SystemInfo.deviceUniqueIdentifier));

            QuitGame();
        }
    }
    
    #endregion


    #region PUN Callbacks

    public override void OnJoinedRoom()
    {
        string tag;
        string[] itens;

        base.OnJoinedRoom();

        tag = PhotonNetwork.MasterClient.NickName;
        itens = tag.Split('|');

        roomLabel.GetComponent<Text>().text = itens[1] + "'s Room";

        InstantiateMyPrefab();

        if (PhotonNetwork.IsMasterClient)
        {
            startMatchButton.SetActive(true);
        }        
    }

    #endregion
    

    #region Leaderboard Methods

    public void GetRankingList()
    {
        var textFromFile = (new WebClient()).DownloadString("http://dreamlo.com/lb/5f84c68feb371809c47c2135/quote");        
        string[] lines = textFromFile.Split(new[] { '\r', '\n' });
        string[] parts;
        GameObject thisTag;
        int tick = 0;

        foreach (Transform child in rankContainer)
        {
            Destroy(child.gameObject);
        }

        while (tick < lines.Length)
        {  
            parts = lines[tick].Split(',');

            if(parts.Length > 1)
            {
                if (!parts[3].Equals(""))
                {
                    thisTag = Instantiate(rankTagPrefab, rankContainer);

                    thisTag.transform.GetChild(0).GetComponent<Text>().text = parts[3].Trim('"'); ;
                    thisTag.transform.GetChild(1).GetComponent<Text>().text = parts[1].Trim('"');
                    thisTag.transform.GetChild(2).GetComponent<Text>().text = Convert.ToString(tick + 1);
                    thisTag.transform.GetChild(3).GetComponent<AvatarHandler>().avatarid = Convert.ToInt32(parts[2].Trim('"'));
                }               
            }

            tick++;     
        }
    }

    public void SendMMR()
    {        
        string mmr = PlayerPrefs.GetString(playerMMRPrefKey);
        
        StartCoroutine(GetRequest("http://dreamlo.com/lb/pm3oYHCDf0m8D5-WLp6A3gIQLWB8LGVUm3EgniK_QODA/delete/" + SystemInfo.deviceUniqueIdentifier));
        StartCoroutine(GetRequest("http://dreamlo.com/lb/pm3oYHCDf0m8D5-WLp6A3gIQLWB8LGVUm3EgniK_QODA/add/" + SystemInfo.deviceUniqueIdentifier + "/" + mmr + "/" + PlayerPrefs.GetString(playerAvatarID) + "/" + PlayerPrefs.GetString(playerNamePrefKey)));
    }


    IEnumerator GetRequest(string url)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
        }
    }

    #endregion





    #region Avatar Mess

    public void SetBear()
    {
        ChangeMyAvatar(1);
    }

    public void SetCat()
    {
        ChangeMyAvatar(2);
    }

    public void SetCow()
    {
        ChangeMyAvatar(3);
    }

    public void SetDog()
    {
        ChangeMyAvatar(4);
    }

    public void SetFox()
    {
        ChangeMyAvatar(5);
    }

    public void SetMonkey()
    {
        ChangeMyAvatar(6);
    }

    public void SetMice()
    {
        ChangeMyAvatar(7);
    }

    public void SetOwl()
    {
        ChangeMyAvatar(8);
    }

    public void SetPanda()
    {
        ChangeMyAvatar(9);
    }

    public void SetPig()
    {
        ChangeMyAvatar(10);
    }

    public void SetBunny()
    {
        ChangeMyAvatar(11);
    }

    public void SetWolf()
    {
        ChangeMyAvatar(12);
    }

    private void ChangeMyAvatar(int selection)
    {
        PlayerPrefs.SetString(playerAvatarID, Convert.ToString(selection));

        PhotonNetwork.NickName = PlayerPrefs.GetString(playerMMRPrefKey) + "|" + PlayerPrefs.GetString(playerNamePrefKey) + "|" + PlayerPrefs.GetString(playerAvatarID);

        playerUIavatar.GetComponent<AvatarHandler>().avatarid = selection;
    }

    #endregion
}
