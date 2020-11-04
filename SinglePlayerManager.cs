using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;



public class SinglePlayerManager : MonoBehaviour
{
    #region Public Fields
    public GameObject target = null;
    public GameObject piece = null;
    public GameObject victim = null;

    private GameObject targetMov = null;
    private GameObject pieceMov = null;

    public RaycastHit hit;
    public int turnTag;
    public int playerNumber = 1;
    public string end;
    public int teamTag = 1;
    public bool blueNpc = true, greenNpc = true, yellowNpc = true;
    public bool endgame;

    private float speed = 10;

    public bool movingCheck = false;
    
    GameObject bigShader;
    
    public GameObject hittingSound, slidingSound;
         
    public GameObject resultText, resumeButton, pauseMenu, pauseWall, menuAvatar, menuName;

    public GameObject playerNameUI, playerAvatarUI;

    #endregion


    #region Mono CallBacks
    //called only in the first frame
    void Start()
    {
        menuAvatar.GetComponent<AvatarHandler>().avatarid = Convert.ToInt32(PlayerPrefs.GetString("AvatarID"));
        menuName.GetComponent<Text>().text = PlayerPrefs.GetString("PlayerName");
        playerAvatarUI.GetComponent<AvatarHandler>().avatarid = Convert.ToInt32(PlayerPrefs.GetString("AvatarID"));
        playerNameUI.GetComponent<Text>().text = PlayerPrefs.GetString("PlayerName");

        bigShader = GameObject.FindGameObjectWithTag("shader");
        
        turnTag = 1;
        endgame = false;
    }

    //called in every frame update
    void Update()
    {
        if (movingCheck == false)
        {
            CheckPiecesForMoves();

            TurnSkipper(GetTurnCollor(turnTag));

            if (!endgame)
            {
                CheckNpcTurn(GetTurnCollor(turnTag));
            }

            if (CheckPlayer(1, turnTag))
            {
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
                            MovePiece(piece, target);

                            target = null;
                            piece = null;
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
        }        
    }

    #endregion


    #region Input Gathering
    //input check(editor,windows,android)
    private bool InputCheck()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor && Input.GetMouseButtonDown(0))
        {
            return true;
        }
        if (Application.platform == RuntimePlatform.Android && Input.touchCount > 0)
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
        else if (Application.platform == RuntimePlatform.Android)
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
            nOfSteps = (xd - xt + zd - zt) / 2;

            if (nOfSteps < 0)
            {
                nOfSteps = -nOfSteps;
            }
        }

        if ((xt - xd) * (xt - xd) - (zt - zd) * (zt - zd) == 0)
        {
            nOfSteps = (xd - xt) / 2;

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

    private bool CheckValidity(int thingTeam,GameObject thing, GameObject origin, GameObject destination)
    {
        float dist = 0;
        float xt = 0, xd = 0, zt = 0, zd = 0;

        xt = origin.transform.position.x;
        zt = origin.transform.position.z;
        xd = destination.transform.position.x;
        zd = destination.transform.position.z;


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

        ray = new Ray(tile.transform.position + new Vector3(0, 5, 0), -Vector3.up);

        if (Physics.Raycast(ray, out hit, 7f))
        {
            if (hit.transform != null)
            {
                sensor = hit.transform.gameObject;
            }
        }

        if (sensor == null)
        {
            return false;
        }

        if (CheckPiece(sensor))
        {
            if(GetTeam(sensor) == moverTeam)
            {
                return (false);
            }
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

    #endregion


    #region Moving Methods

    private void MovePiece(GameObject thing, GameObject destination)
    {
        movingCheck = true;

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

        ray = new Ray(thing.transform.position + new Vector3(0, -5, 0), Vector3.up);

        if (Physics.Raycast(ray, out hit, 10))
        {
            origin = hit.transform.gameObject;
            
            origin.GetComponent<TileCleaner>().enabled = true;

            origin.GetComponent<BoxCollider>().enabled = false;
        }
    }

    private void DestroyPiece(GameObject thing)
    {
        Destroy(thing);
    }

    private void HighlightValidMoves(GameObject thing)
    {
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("tile");

        int tick = 0;

        if (thing==null)
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
        string result;


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
                if (CheckValidity(0, thing[thingcount], thing[thingcount], thing[subthingcount]))
                {
                    counter++;
                }
                subthingcount++;
            }

            subthingcount = 0;

            if (counter == 0 && thing[thingcount].transform.position.y != 10)
            {                
                DestroyPiece(thing[thingcount]);                
            }
            counter = 0;

            thingcount++;
        }

        if (reds != 0)
        {
            aliveTeams++;
        }
        if (blues != 0)
        {
            aliveTeams++;
        }
        if (greens != 0)
        {
            aliveTeams++;
        }
        if (yellows != 0)
        {
            aliveTeams++;
        }


        if (aliveTeams < 2)
        {
            endgame = true;

            if (thing[0] == null)
            {
                result = "Draw Game.";
            }
            else if (GetTeam(thing[0]) == 1)
            {
                result = "You Won!!!";
            }
            else
            {
                result = "You Lost...";
            }
            
            resultText.GetComponent<Text>().text = result;

            EndGameUI();
        }
        else
        {
            endgame = false;
        }
    }
    
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
            turnTag++;
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
        if (tempPiece != null && tempTarget != null)
        {
            piece = tempPiece;
            target = tempTarget;

            MovePiece(piece, target);
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








    #region Manager Substitute Methods
    
    public void ResetSmall()
    {
        SceneManager.LoadScene("SPScene1");
    }

    public void ResetBig()
    {
        SceneManager.LoadScene("SPScene2");
    }

    private void EndGameUI()
    {
        resumeButton.SetActive(false);
        pauseMenu.SetActive(true);
        pauseWall.SetActive(true);

    }
    
    public void LeaveMatch()
    {
        SceneManager.LoadScene("MenuScene");
    }
    
    #endregion
}