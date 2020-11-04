using UnityEngine;
using UnityEngine.UI;
using System;
using Photon.Pun;


public class PlayerManager : MonoBehaviourPun
{
    #region Public Fields
    public GameObject target=null;
    public GameObject piece=null;
    public int targetID;
    public int pieceID;

    GameObject movingCheck;
        
    public RaycastHit hit;
    public int turnTag;
    public int playerNumber;
    public string end;
    public int teamTag;
    public bool blueNpc = true, greenNpc = true, yellowNpc = true;
    public bool redDead = false, blueDead = false, greenDead = false, yellowDead = false;

    public bool endgame;

    private float speed=10;

    private bool npcPlaying = false;

    GameObject bigShader; 

    public float gravity=0.02f;
    
    public GameObject hittingSound, slidingSound;

    private GameObject timer;
    private float timerValue, timePerTurn = 30;
    
    #endregion
    

    #region Mono CallBacks
    //called only in the first frame
    void Start()
    {      
        timer = GameObject.FindGameObjectWithTag("timer");
        timer.SetActive(false);

        bigShader = GameObject.FindGameObjectWithTag("shader");
        movingCheck = GameObject.FindGameObjectWithTag("mover");

        turnTag = 1;        
        endgame = false;
    }
    
    //called in every frame update
    void Update()
    {
        if (movingCheck.transform.position.x == 0)
        {
            turnTag = Convert.ToInt16(GameObject.Find("TurnClock").transform.position.y);

            if (turnTag == 1)
            {
                GetPlayerNumber();
                SetNpcs(playerNumber);
            }

            if (PhotonNetwork.IsMasterClient)
            {
                CheckPiecesForMoves();

                TurnSkipper(GetTurnCollor(turnTag));

                if (!endgame)
                {
                    CheckNpcTurn(GetTurnCollor(turnTag));
                }
            }

            if (CheckPlayer(teamTag, turnTag) && photonView.IsMine && !CheckGameOver())
            {
                if (!timer.activeSelf)
                {
                    timer.SetActive(true);

                    timer.GetComponent<TimerHandler>().timeRemaining = timePerTurn;
                }

                timerValue = timer.GetComponent<TimerHandler>().timeRemaining;

                if (timerValue == 0)
                {
                    timer.SetActive(false);

                    NpcPlays(teamTag);
                }

                if (InputCheck())
                {
                    GetTarget();

                    if (target != null)
                    {
                        HighlightValidMoves(piece);

                        if (CheckPiece(target) && CheckTeam(target, turnTag))
                        {
                            piece = target;
                            target = null;
                        }
                        else if (piece != null && CheckValidity(GetTeam(piece), piece, piece, target))
                        {
                            pieceID = piece.GetPhotonView().ViewID;
                            targetID = target.GetPhotonView().ViewID;

                            photonView.RPC("MovePiece", RpcTarget.MasterClient, pieceID, targetID);

                            timer.SetActive(false);

                            piece = null;
                            target = null;
                        }
                        else
                        {
                            target = null;
                            piece = null;
                        }

                        HighlightValidMoves(piece);
                    }
                }
            }
            else if (photonView.IsMine)
            {
                timer.SetActive(false);
            }
        }        
    }

    #endregion
    

    #region Input Gathering
    //input check(editor,windows,android)
    private bool InputCheck()
    {        
        if(Application.platform == RuntimePlatform.WindowsEditor && Input.GetMouseButtonDown(0))
        {
            return true;
        }
        if(Application.platform == RuntimePlatform.Android && Input.touchCount > 0)
        {
            return true;
        }
        if (Application.platform == RuntimePlatform.WindowsPlayer && Input.GetMouseButtonDown(0))
        {
            return true;
        }

        return false;
    }
    //input gathering(editor,windows,android)
    private void GetTarget()
    {
        Ray ray;
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        }
        else if(Application.platform == RuntimePlatform.Android)
        {
            ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
        }
        else if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        }
        else
        {
            target = null;
            return;
        }            

        if (Physics.Raycast(ray, out hit, 100.0f))
        {
            if (hit.transform != null)
            {
                target = hit.transform.gameObject;
            }
        }
    }

    #endregion


    #region Verifier Methods 

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

    private bool CheckPath(GameObject thing, GameObject destination)
    {
        Ray ray;
        Vector3 path = destination.transform.position - thing.transform.position;
        Vector3 step;
        float beacon = 1, dist = path.magnitude;
        float nOfSteps = 1;
        GameObject sensor = null;
        float xt = 0, xd = 0, zt = 0, zd = 0;

        xt = thing.transform.position.x;
        zt = thing.transform.position.z;
        xd = destination.transform.position.x;
        zd = destination.transform.position.z;

        if ((xt == xd) ^ (zt == zd))
        {
            nOfSteps = (xd - xt + zd - zt)/2;

            if (nOfSteps < 0)
            {
                nOfSteps = -nOfSteps;
            }
        }

        if ((xt - xd) * (xt - xd) - (zt - zd) * (zt - zd) == 0)
        {
            nOfSteps = (xd - xt)/2;

            if (nOfSteps < 0)
            {
                nOfSteps = -nOfSteps;
            }
        }

        step = path / nOfSteps;

        while (beacon < nOfSteps)
        {  
            ray = new Ray(thing.transform.position + new Vector3(0, -5, 0) + (step * beacon), Vector3.up);

            if (Physics.Raycast(ray, out hit, 150.0f))
            {
                if (hit.transform != null)
                {
                    sensor = hit.transform.gameObject;
                }
            }

            if (sensor.CompareTag("sensor"))
            {
                return (false);
            }

            beacon++;
        }
        beacon = 1;

        while (beacon < nOfSteps)
        {
            

            ray = new Ray(thing.transform.position + new Vector3(0, 5, 0) + (step * beacon), -Vector3.up);

            if (Physics.Raycast(ray, out hit, 150.0f))
            {
                if (hit.transform != null)
                {
                    sensor = hit.transform.gameObject;
                }
            }

            if (CheckPiece(sensor))
            {
                return (false);
            }
            beacon++;
        }

        return (true);
    }

    private bool CheckPiece(GameObject thing)
    {
        if (thing.CompareTag("piece"))
        {
            return (true);
        }

        return (false);
    }

    private bool CheckValidity(int thingTeam, GameObject thing, GameObject origin, GameObject destination)
    {
        float dist = 0;
        float xt = 0, xd = 0, zt = 0, zd = 0;

        xt = origin.transform.position.x;
        zt = origin.transform.position.z;
        xd = destination.transform.position.x;
        zd = destination.transform.position.z;

        if (!destination.GetComponent<BoxCollider>().enabled)
        {
            return false;
        }

        if (thing.name.Contains("rook"))
        {
            if (((xt == xd) ^ (zt == zd)) && CheckPath(origin, destination))
            {
                return (CheckTile(destination, thingTeam));
            }
        }

        else if (thing.name.Contains("knight"))
        {
            dist = ((xd - xt) * (zd - zt) * (xd - xt) * (zd - zt)) - 64;

            if (dist == 0)
            {
                return (CheckTile(destination, thingTeam));
            }
        }

        else if (thing.name.Contains("bishop"))
        {
            dist = (xt - xd) * (xt - xd) - (zt - zd) * (zt - zd);


            if (dist == 0 && CheckPath(origin, destination) && xt != xd)
            {
                return (CheckTile(destination, thingTeam));
            }
        }

        return (false);
    }

    private bool CheckTile(GameObject tile, int moverTeam)
    {
        Ray ray;

        GameObject sensor = null;

        if (!tile.GetComponent<BoxCollider>().enabled)
        {
            return false;
        }

        ray = new Ray(tile.transform.position + new Vector3(0, 5, 0), -Vector3.up);

        if (Physics.Raycast(ray, out hit, 7f))
        {
            if (hit.transform != null)
            {
                sensor = hit.transform.gameObject;
            }
        }

        if (sensor.CompareTag("piece") && GetTeam(sensor) == moverTeam)
        {
            return (false);
        }

        return (true);
    }

    private string GetTurnCollor(int turn)
    {
        int collorTag;
        string turnCollor;

        collorTag = ((turn - 1) % 4) + 1;

        if (collorTag == 1)
        {
            turnCollor = "red";
        }
        else if (collorTag == 2)
        {
            turnCollor = "blue";
        }
        else if (collorTag == 3)
        {
            turnCollor = "green";
        }
        else
        {
            turnCollor = "yellow";
        }

        return turnCollor;
    }

    private bool CheckTeam(GameObject thing, int turn)
    {
        if (thing.name.Contains(GetTurnCollor(turn)))
        {
            return (true);
        }
        else
        {
            return (false);
        }
    }

    private bool CheckPlayer(int myTeam, int turn)
    {
        if (((turn - 1) % 4) + 1 == myTeam)
        {
            return (true);
        }
        else
        {
            return (false);
        }
    }
    
    private bool CheckGameOver()
    {
        if (GameObject.Find("TurnClock").transform.position.x == 100)
        {
            return true;
        }

        return false;
    }

    #endregion


    #region Moving Methods
    
    [PunRPC]
    private void MovePiece(int thingID, int destinationID)
    {
        GameObject thing, destination;

        thing = PhotonView.Find(thingID).gameObject;
        destination = PhotonView.Find(destinationID).gameObject;

        movingCheck.transform.position = new Vector3(1, 0, 0);


        DestroyOrigin(thing);
        
        if (!thing.name.Contains("knight"))
        {
            slidingSound.GetComponent<AudioSource>().Play();
        }

        thing.transform.position += new Vector3(0, 10, 0);

        thing.GetComponent<MovementHandler>().enabled = true;
        thing.GetComponent<MovementHandler>().target = destination;
    }
    
    private void DestroyOrigin(GameObject thing)
    {
        Ray ray;
        GameObject origin;

        ray = new Ray(thing.transform.position + new Vector3(0, -1, 0), Vector3.up);

        if (Physics.Raycast(ray, out hit, 5))
        {
            origin = hit.transform.gameObject;

            origin.GetComponent<TileCleaner>().enabled = true;

            origin.GetComponent<BoxCollider>().enabled = false;
        }
    }
    
    private void DestroyPiece(GameObject thing)
    {

        PhotonNetwork.Destroy(thing);

    }
          
    private void HighlightValidMoves(GameObject thing)
    {
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("tile");
        
        int tick = 0;

        if (thing == null)
        {
            bigShader.SetActive(false);
        }
        else
        {
            bigShader.SetActive(true);
        }

        while (tick < tiles.Length)
        {
            tiles[tick].transform.GetChild(0).gameObject.SetActive(false);
            if (thing == null)
            {
            }
            else if (tiles[tick].transform.position == thing.transform.position)
            {
                tiles[tick].transform.GetChild(0).gameObject.SetActive(false);
            }
            else if (!CheckValidity(GetTeam(thing), thing, thing, tiles[tick]))
            {
                tiles[tick].transform.GetChild(0).gameObject.SetActive(true);                
            }

            tick++;
        }
    }

    
    #endregion


    #region Turn Fixing Methods

    private void CheckPiecesForMoves()
    {
        GameObject[] thing, place;
        int thingcount = 0, subthingcount = 0, placecount = 0, counter = 0, reds = 0, blues = 0, greens = 0, yellows = 0, aliveTeams = 0;

        endgame = false;

        thing = GameObject.FindGameObjectsWithTag("piece");

        place = GameObject.FindGameObjectsWithTag("tile");

        while (thingcount < thing.Length)
        {
            if (thing[thingcount].name.Contains("red"))
            {
                reds++;
            }
            else if (thing[thingcount].name.Contains("blue"))
            {
                blues++;
            }
            else if (thing[thingcount].name.Contains("green"))
            {
                greens++;
            }
            else if (thing[thingcount].name.Contains("yellow"))
            {
                yellows++;
            }
            
            while (placecount < place.Length)
            {
                if (CheckValidity(GetTeam(thing[thingcount]), thing[thingcount], thing[thingcount], place[placecount]))
                {
                    counter++;
                }
                placecount++;
            }

            placecount = 0;

            while (subthingcount < thing.Length)
            {
                if(CheckValidity(0, thing[thingcount], thing[thingcount], thing[subthingcount]))
                {
                    counter++;
                }
                subthingcount++;
            }

            subthingcount = 0;

            if (counter == 0 && thing[thingcount].transform.position.y != 10)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    DestroyPiece(thing[thingcount]);
                }
            }
            counter = 0;

            thing = GameObject.FindGameObjectsWithTag("piece");

            thingcount++;
        }

        if (reds != 0)
        {
            aliveTeams++;
        }
        else
        {
            redDead = true;
        }
        if (blues != 0)
        {
            aliveTeams++;
        }
        else
        {
            blueDead = true;
        }
        if (greens != 0)
        {
            aliveTeams++;
        }
        else
        {
            greenDead = true;
        }
        if (yellows != 0)
        {
            aliveTeams++;
        }
        else
        {
            yellowDead = true;
        }
                       
        if (aliveTeams < 2)
        {
            GameObject.Find("TurnClock").transform.position = new Vector3(100, 0, 0);
        }
    }

    public void GetPlayerNumber()
    {
        int playerN;
        int tick = 0;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        PlayerManager scr;
        
        playerN = players.Length;

        while (tick < playerN)
        {
            scr = players[tick].GetComponent<PlayerManager>();                                                                    
            scr.teamTag = players[tick].GetPhotonView().ViewID/1000;
            scr.playerNumber = playerN;
            tick++;
        }
    }
    
    private void SetNpcs(int pNumber)
    {
        if (pNumber > 1)
        {
            blueNpc = false;
            if (pNumber > 2)
            {
                greenNpc = false;
                if (pNumber > 3)
                {
                    yellowNpc = false;
                }
            }
        }
    }
    [PunRPC]
    private void TurnSkipper(string turnColl)
    {
        GameObject[] myPieces;
        int tick = 0, counter = 0;

        myPieces = GameObject.FindGameObjectsWithTag("piece");
        while (tick < myPieces.Length)
        {
            if (myPieces[tick].name.Contains(turnColl))
            {
                counter++;
            }
            tick++;
        }

        if (counter == 0)
        {
            GameObject.Find("TurnClock").transform.position += new Vector3(0, 1, 0);
        }

    }
    
    private void CheckNpcTurn(string turnColl)
    {
        if (turnColl.Contains("blue") && blueNpc)
        {
            NpcPlays(2);
        }
        else if (turnColl.Contains("green") && greenNpc)
        {
            NpcPlays(3);
        }
        else if (turnColl.Contains("yellow") && yellowNpc)
        {
            NpcPlays(4);
        }
    }

    #endregion

    

    #region PseudoAI Methods

    public void NpcPlays(int myTeam)
    {
        int pieceCounter = 0, spotCounter = 0, tempscore = -500;
        GameObject[] myPieces, spots;
        GameObject tempPiece = null, tempTarget = null;

        myPieces = GameObject.FindGameObjectsWithTag("piece");
        spots = GameObject.FindGameObjectsWithTag("tile");

        while (pieceCounter < myPieces.Length)
        {
            if (GetTeam(myPieces[pieceCounter]) == myTeam)
            {
                spotCounter = 0;

                while (spotCounter < spots.Length)
                {
                    if (CheckValidity(GetTeam(myPieces[pieceCounter]), myPieces[pieceCounter], myPieces[pieceCounter], spots[spotCounter]))
                    {
                        if (MoveScore(spots[spotCounter], myPieces[pieceCounter], spots) > tempscore)
                        {
                            tempscore = MoveScore(spots[spotCounter], myPieces[pieceCounter], spots);
                            tempPiece = myPieces[pieceCounter];
                            tempTarget = spots[spotCounter];
                        }
                    }
                    spotCounter++;
                }
            }
            pieceCounter++;
        }
        if (tempPiece != null && tempTarget!=null)
        {
            pieceID = tempPiece.GetPhotonView().ViewID;
            targetID = tempTarget.GetPhotonView().ViewID;

            npcPlaying = true;

            photonView.RPC("MovePiece", RpcTarget.MasterClient,pieceID, targetID);
        }
    }

    public int MoveScore(GameObject spot, GameObject mover, GameObject[] targets)
    {
        int tick = 0, movescore = 0;
        GameObject[] myPieces = GameObject.FindGameObjectsWithTag("piece");

        while (tick < targets.Length)
        {
            if (CheckValidity(GetTeam(mover), mover, spot, targets[tick]))
            {
                movescore++;
            }
            tick++;
        }
        tick = 0;

        while (tick < myPieces.Length)
        {
            if (GetTeam(myPieces[tick]) != GetTeam(mover) && myPieces[tick].transform.position == spot.transform.position)
            {
                movescore = movescore + 5;
            }
            if (CheckValidity(GetTeam(myPieces[tick]), myPieces[tick], myPieces[tick], mover))
            {
                if (GetTeam(myPieces[tick]) != GetTeam(mover))
                {
                    movescore = movescore + 5;
                }
                else
                {
                    movescore = movescore - 6;
                }

            }
            if (CheckValidity(GetTeam(myPieces[tick]), myPieces[tick], myPieces[tick], spot))
            {
                if (GetTeam(myPieces[tick]) != GetTeam(mover))
                {
                    movescore = movescore - 8;
                }
                else
                {
                    movescore = movescore + 5;
                }

            }
            tick++;
        }
        tick = 0;

        return (movescore);
    }

    #endregion
             
}