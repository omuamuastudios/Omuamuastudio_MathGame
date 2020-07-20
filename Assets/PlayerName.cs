using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Photon.Pun.Demo.PunBasics
{
    public class PlayerName : MonoBehaviour
    {
        
        private void Update()
        {
           Text playerName = this.gameObject.GetComponent<Text>();
            playerName.text = PhotonNetwork.NickName;
        }
    }
}