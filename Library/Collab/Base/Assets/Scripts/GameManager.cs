using Photon.Pun.Demo.PunBasics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Photon.Pun
{
    public class GameManager : MonoBehaviour
    {
        public GameObject gameCanvas;

        public GameObject instruction, titlePanel;
        public Animator titleAnimator;

        public GameObject newPlayerPanel;
        public GameObject welcomeBack;

        public PlayerNameInputField playerNameInputField;

        private void Start()
        {
            titlePanel.SetActive(true);
            instruction.SetActive(false);
            newPlayerPanel.SetActive(false);


            StartCoroutine(GameStart());
        }
        IEnumerator GameStart()
        {
            titlePanel.SetActive(true);
            yield return new WaitForSeconds(2);
            instruction.SetActive(true);
            yield return new WaitWhile(() => !Input.GetMouseButton(0));

            titleAnimator.SetTrigger("MoveUp");
            instruction.SetActive(false);
            yield return new WaitForSeconds(1f);
            if (playerNameInputField.firstEntry)
            {
                newPlayerPanel.SetActive(true);
            }
            else
            {
                welcomeBack.SetActive(true);
            }

        }
        public void Restart()
        {
            PhotonNetwork.Disconnect();
            SceneManager.LoadScene(0);
        }
    }

}

