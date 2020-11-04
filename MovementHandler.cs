using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;



public class MovementHandler : MonoBehaviour
{    
    public GameObject target = null;
    public float speed;
    public GameObject hittingSound,slidingSound;
    public GameObject victim;
    public GameObject controllerScr;
    
    bool justStartedMoving = true;

    Ray ray;
    RaycastHit hit;

    void Update()
    {
        if (target != null)
        {
            if (justStartedMoving == true)
            {
                if (!name.Contains("knight"))
                {
                    slidingSound.GetComponent<AudioSource>().Play();
                }

                justStartedMoving = false;
            }

            transform.position = Vector3.MoveTowards(transform.position, target.transform.position + new Vector3(0, 10, 0), speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target.transform.position + new Vector3(0, 10, 0)) < 0.01f)
            {
                if (name.Contains("knight"))
                {
                    hittingSound.GetComponent<AudioSource>().Play();
                }
                else
                {
                    slidingSound.GetComponent<AudioSource>().Stop();
                }

                transform.position = target.transform.position + new Vector3(0, 10, 0);

                ray = new Ray(transform.position + new Vector3(0, -5, 0), -Vector3.up);

                if (Physics.Raycast(ray, out hit, 50.0f))
                {
                    if (hit.transform != null)
                    {
                        victim = hit.transform.gameObject;
                    }
                }

                if (victim != null)
                {
                    if (victim.CompareTag("piece"))
                    {
                        if (PhotonNetwork.IsConnectedAndReady)
                        {
                            PhotonNetwork.Destroy(victim);
                        }
                        else
                        {
                            Destroy(victim);
                        }
                    }
                }

                transform.position = target.transform.position;

                if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.IsMasterClient)
                {
                    GameObject.Find("TurnClock").transform.position += new Vector3(0, 1, 0);

                    GameObject.FindGameObjectWithTag("mover").transform.position = new Vector3(0, 0, 0);
                }
                else
                {
                    controllerScr.GetComponent<SinglePlayerManager>().turnTag++;

                    controllerScr.GetComponent<SinglePlayerManager>().movingCheck = false;
                }                

                target = null;
                justStartedMoving = true;
                enabled = false;
            }
        }        
    }
}
