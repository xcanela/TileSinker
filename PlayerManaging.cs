using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class PlayerManaging : MonoBehaviourPunCallbacks
{
    #region Public Fields

    [Tooltip("The prefab to use for representing the player")]
    public GameObject playerPrefab;

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;

    public GameObject playerProfileUI, playerCollor, playermmr, playerAvatar;

    public GameObject enemyProfileUI1, enemyProfileUI2, enemyProfileUI3;

    public GameObject enemyCollor1,enemyAvatar1, enemyCollor2, enemyAvatar2, enemyCollor3, enemyAvatar3;

    public GameObject narrator, turnClock, thugGlasses, billboard, winnerAvatar, winnerName, winDeltaTag, lostDeltaTag, loserTag, loserAvatar, resultText;

    public GameObject pauseMenu, resumeButton;

    public float newMMR, myTeam, myMMR, e1MMR = 0, e2MMR = 0, e3MMR = 0, e1team = 0, e2team = 0, e3team = 0;
    
    public float minMMR = 100, maxMMR = 4000, avgMMRvar = 30;

    public float winnerTeam = 0;

    int eAvatarID1, eAvatarID2, eAvatarID3;

    string eName1, eName2, eName3;

    bool matchendedok = false;

    bool npcgreen = false, npcyellow = false;

    #endregion


    #region Mono Callbacks

    public void Start()
    {
        InstantiateMyPrefab();

        CloseRoom();
        
        ShowNames();

        SetNpcs();
    }

    public void Update()
    {
        if (CheckIfGameIsOver())
        {
            EndGameUI();
                                 
            GetNewMMR();

            PlayerPrefs.SetString("PlayerMMR", Convert.ToString(newMMR));

        }
        else
        {
            ShowNames();

            Narrator();
        }                     
    }
    
    #endregion


    #region Manager Methods
    //instatiate if not already instatiated
    private void InstantiateMyPrefab()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        int tick = 0;
        bool toDo = true;

        while (tick < players.Length)
        {
            if (players[tick].GetPhotonView().IsMine)
            {
                toDo = false;
            }
            tick++;
        }
        tick = 0;

        if (toDo)
        {
            PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
        }        
    }

    public void ShowNames()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        int tick = 0, teck = 0;
        string tag;
        string[] itens;

        while (tick < players.Length)
        {
            if (players[tick].GetPhotonView().IsMine)
            {                
                tag = players[tick].GetPhotonView().Owner.NickName;
                itens = tag.Split('|');

                playerProfileUI.SetActive(true);

                myMMR = Convert.ToInt32(itens[0]);
                myTeam = players[tick].GetPhotonView().ViewID / 1000;

                playermmr.GetComponent<Text>().text = itens[0];

                playerProfileUI.GetComponent<Text>().text = itens[1]; 

                                
                playerCollor.GetComponent<Image>().color = GetPlayerCollor(players[tick].GetPhotonView().ViewID / 1000);

                loserAvatar.GetComponent<AvatarHandler>().avatarid = Convert.ToInt32(PlayerPrefs.GetString("AvatarID"));
                playerAvatar.GetComponent<AvatarHandler>().avatarid = Convert.ToInt32(PlayerPrefs.GetString("AvatarID"));
            }
            else
            {
                if (teck == 0)
                {
                    enemyProfileUI1.SetActive(true);

                    tag = players[tick].GetPhotonView().Owner.NickName;
                    itens = tag.Split('|');

                    e1MMR = Convert.ToInt32(itens[0]);
                    e1team = players[tick].GetPhotonView().ViewID / 1000;
                    enemyProfileUI1.GetComponent<Text>().text = itens[1];
                    eName1 = itens[1];

                    enemyAvatar1.GetComponent<AvatarHandler>().avatarid = Convert.ToInt32(itens[2]);
                    eAvatarID1 = Convert.ToInt32(itens[2]);
                    enemyCollor1.GetComponent<Image>().color = GetPlayerCollor(players[tick].GetPhotonView().ViewID / 1000);
                }
                else if (teck == 1 && enemyProfileUI2 != null)
                {
                    enemyProfileUI2.SetActive(true);

                    tag = players[tick].GetPhotonView().Owner.NickName;
                    itens = tag.Split('|');

                    e2MMR = Convert.ToInt32(itens[0]);
                    e2team = players[tick].GetPhotonView().ViewID / 1000;
                    enemyProfileUI2.GetComponent<Text>().text = itens[1];
                    eName2 = itens[1];

                    enemyAvatar2.GetComponent<AvatarHandler>().avatarid = Convert.ToInt32(itens[2]);
                    eAvatarID2 = Convert.ToInt32(itens[2]);
                    enemyCollor2.GetComponent<Image>().color = GetPlayerCollor(players[tick].GetPhotonView().ViewID / 1000);
                }
                else if (teck == 2 && enemyProfileUI3 != null)
                {
                    enemyProfileUI3.SetActive(true);
                    
                    tag = players[tick].GetPhotonView().Owner.NickName;
                    itens = tag.Split('|');

                    e3MMR = Convert.ToInt32(itens[0]);
                    e3team = players[tick].GetPhotonView().ViewID / 1000;
                    enemyProfileUI1.GetComponent<Text>().text = itens[1];
                    eName3 = itens[1];

                    enemyAvatar3.GetComponent<AvatarHandler>().avatarid = Convert.ToInt32(itens[2]);
                    eAvatarID3 = Convert.ToInt32(itens[2]);
                    enemyCollor3.GetComponent<Image>().color = GetPlayerCollor(players[tick].GetPhotonView().ViewID / 1000);
                }

                teck++;
            }
            tick++;
        }
    }

    public void Narrator()
    {
        int turnTag;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        int tick = 0;
        string tag;
        string[] itens;
        
        turnTag = Convert.ToInt16(turnClock.transform.position.y);

      
        while (tick < players.Length)
        {
            if (((turnTag - 1) % 4) + 1 == players[tick].GetPhotonView().ViewID / 1000)
            {
                if (players[tick].GetPhotonView().IsMine)
                {
                    myTeam = players[tick].GetPhotonView().ViewID / 1000;
                    narrator.GetComponent<Text>().text = "Your Turn!";
                    return;
                }
                else
                {
                    tag = players[tick].GetPhotonView().Owner.NickName;
                    itens = tag.Split('|');
                    
                    narrator.GetComponent<Text>().text = itens[1] + "'s Turn...";

                    return;
                }
            }
            tick++;
        }

        narrator.GetComponent<Text>().text = "Npc's Turn...";
    }
    
    private Color GetPlayerCollor(int teamTag)
    {
        int collorTag;
        Color teamCollor;

        collorTag = ((teamTag - 1) % 4) + 1;

        if (collorTag == 1)
        {
            teamCollor = new Color(0.57f, 0.06f, 0.06f);
        }
        else if (collorTag == 2)
        {
            teamCollor = new Color(0.08f, 0.19f, 0.29f);
        }
        else if (collorTag == 3)
        {
            teamCollor = new Color(0.08f, 0.22f, 0.11f);
        }
        else
        {
            teamCollor = new Color(0.5f, 0.3f, 0);
        }

        return teamCollor;
    }

    private void CloseRoom()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }
    }

    private void EndGameUI()
    {
        winnerAvatar.SetActive(true);
        billboard.SetActive(false);       
        pauseMenu.SetActive(true);
        resumeButton.SetActive(false);        
    }

    private void GetNewMMR()
    {
        float winMMR=0, eMMr=0;
        
        int deltaMMR=0, n=0;

        GameObject[] pieces = GameObject.FindGameObjectsWithTag("piece");

        if (pieces[0] == null)
        {
            resultText.GetComponent<Text>().text = "Draw Game.";

            winnerAvatar.GetComponent<AvatarHandler>().avatarid = Convert.ToInt32(PlayerPrefs.GetString("AvatarID"));
            winnerName.GetComponent<Text>().text = PlayerPrefs.GetString("PlayerName");
            thugGlasses.SetActive(false);
        }
        else
        {
            winnerTeam = GetTeam(pieces[0]);

            if (myTeam == winnerTeam)
            {
                resultText.GetComponent<Text>().text = "You Won!!!";

                winMMR = myMMR;
                winnerAvatar.GetComponent<AvatarHandler>().avatarid = Convert.ToInt32(PlayerPrefs.GetString("AvatarID"));
                winnerName.GetComponent<Text>().text = PlayerPrefs.GetString("PlayerName");

            }
            else if (e1team == winnerTeam)
            {
                resultText.GetComponent<Text>().text = "You Lost...";

                winMMR = e1MMR;
                winnerAvatar.GetComponent<AvatarHandler>().avatarid = eAvatarID1;

                winnerName.GetComponent<Text>().text = eName1;

                loserTag.SetActive(true);
                loserTag.GetComponent<Text>().text = PlayerPrefs.GetString("PlayerName");
            }
            else if (e2team == winnerTeam)
            {
                resultText.GetComponent<Text>().text = "You Lost...";


                winMMR = e2MMR;
                winnerAvatar.GetComponent<AvatarHandler>().avatarid = eAvatarID2;

                winnerName.GetComponent<Text>().text = eName2;

                loserTag.SetActive(true);
                loserTag.GetComponent<Text>().text = PlayerPrefs.GetString("PlayerName");
            }
            else if (e3team == winnerTeam)
            {
                resultText.GetComponent<Text>().text = "You Lost...";

                winMMR = e3MMR;
                winnerAvatar.GetComponent<AvatarHandler>().avatarid = eAvatarID3;

                winnerName.GetComponent<Text>().text = eName3;

                loserTag.SetActive(true);
                loserTag.GetComponent<Text>().text = PlayerPrefs.GetString("PlayerName");
            }
            else
            {
                resultText.GetComponent<Text>().text = "A.I. Won...";

                winnerAvatar.GetComponent<AvatarHandler>().avatarid = Convert.ToInt32(PlayerPrefs.GetString("AvatarID"));
                winnerName.GetComponent<Text>().text = PlayerPrefs.GetString("PlayerName");
                thugGlasses.SetActive(false);
            }
        }
        
        if (e1MMR != 0)
        {
            n = 1;
            if (e2MMR != 0)
            {
                n = 2;
                if (e3MMR != 0)
                {
                    n = 3;
                }
            }
        }

        if (n > 0)
        {
            eMMr = (myMMR + e1MMR + e2MMR + e3MMR - winMMR) / n;
        }

        if (winMMR != 0)
        {
            deltaMMR = Convert.ToInt32((eMMr / winMMR) * avgMMRvar * ((maxMMR - winMMR) / maxMMR) * ((eMMr - minMMR) / eMMr));
        }
        else
        {
            newMMR = myMMR;

            return;
        }        

        if (myTeam == winnerTeam)
        {
            newMMR = Convert.ToInt32(myMMR) + deltaMMR;
        }
        else
        {
            newMMR = Convert.ToInt32(myMMR) - deltaMMR/n;
        }
                                    
        winDeltaTag.GetComponent<Text>().text = "+" + Convert.ToString(deltaMMR);

        lostDeltaTag.GetComponent<Text>().text = "-" + Convert.ToString(deltaMMR/n);
    }
    
    private int GetTeam(GameObject thing)
    {
        if (thing.name.Contains("red"))

        {
            return (1);
        }

        if (thing.name.Contains("blue"))

        {
            return (2);
        }

        if (thing.name.Contains("green"))

        {
            return (3);
        }

        if (thing.name.Contains("yellow"))

        {
            return (4);
        }

        return (0);
    }
    
    private void ForfeitMatch()
    {
        float eMMr;
        int deltaMMr = 0, n = 0;
        string newMMr;

        if (e1MMR != 0)
        {
            n = 1;
            if (e2MMR != 0)
            {
                n = 2;
                if (e3MMR != 0)
                {
                    n = 3;
                }
            }
        }
        if (n != 0)
        {
            eMMr = (e1MMR + e2MMR + e3MMR) / n;

            deltaMMr = Convert.ToInt32((myMMR / eMMr) * avgMMRvar * ((maxMMR - eMMr) / maxMMR) * ((myMMR - minMMR) / eMMr));

            newMMr = Convert.ToString(myMMR - deltaMMr/n);

            PlayerPrefs.SetString("PlayerMMR", newMMr);
        }        
    }

    private void WinByDefault()
    {
        float eMMr;
        int deltaMMr = 0, n = 0;
        string newMMr;

        if (e1MMR != 0)
        {
            n = 1;
            if (e2MMR != 0)
            {
                n = 2;
                if (e3MMR != 0)
                {
                    n = 3;
                }
            }
        }
        if (n != 0)
        {
            eMMr = (e1MMR + e2MMR + e3MMR) / n;

            deltaMMr = Convert.ToInt32((eMMr / myMMR) * avgMMRvar * ((maxMMR - myMMR) / maxMMR) * ((eMMr - minMMR) / eMMr));

            newMMr = Convert.ToString(myMMR + deltaMMr);

            PlayerPrefs.SetString("PlayerMMR", newMMr);
        }
    }
    
    private void SetNpcs()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        if (players.Length < 4)
        {
            npcyellow = true;
            if (players.Length < 3)
            {
                npcgreen = true;
            }
        }
    }

    private bool CheckIfGameIsOver()
    {
        if (turnClock.transform.position.x == 100)
        {
            return true;
        }

        return false;
    }
    
    #endregion


    #region Public Methods
    
    public void LeaveMatch()
    {
        float endtag = turnClock.transform.position.x;

        if (PhotonNetwork.IsConnectedAndReady && endtag == 100)
        {
            PhotonNetwork.Disconnect();

            return;
        }

        if (PhotonNetwork.IsConnectedAndReady)
        {
            ForfeitMatch();

            PhotonNetwork.Disconnect();

            return;
        }
        else
        {
            SceneManager.LoadScene("MenuScene");
        }
    }

    public void ResetSmall()
    {
        SceneManager.LoadScene("SPScene1");
    }

    public void ResetBig()
    {
        SceneManager.LoadScene("SPScene2");
    }

    #endregion
    

    #region PUN Callbacks

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);

        SceneManager.LoadScene("MenuScene");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        if (PhotonNetwork.PlayerListOthers.Length == 0)
        {
            if(turnClock.transform.position.x == 100)
            WinByDefault();

            PhotonNetwork.Disconnect();

            SceneManager.LoadScene("MenuScene");
        }
    }

    #endregion
}


