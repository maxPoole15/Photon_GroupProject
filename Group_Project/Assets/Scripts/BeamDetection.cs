using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MyCompany.MyGame
{
    public class BeamDetection : MonoBehaviour
    {
        private PlayerManager player;

        private void Awake()
        {
            player = GetComponentInParent<PlayerManager>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.tag == "Player")
            {
                Debug.Log("hit player");
                player.kills++;
            }
        }



    }
}

