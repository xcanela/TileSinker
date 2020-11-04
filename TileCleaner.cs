using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class TileCleaner : MonoBehaviourPun
{
    public Vector3 anchor;
    public float speed;


    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, anchor, speed * Time.deltaTime);

        if (transform.position.y < -15)
        {
            if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }    
    }
}
