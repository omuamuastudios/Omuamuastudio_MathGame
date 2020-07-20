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

        public GameObject instruction, titlePanel,playOnline,connectionPanel;
        public Animator titleAnimator;

        public GameObject newPlayerPanel;
        public GameObject welcomeBack;

        public PlayerNameInputField playerNameInputField;

        private void Start()
        {

            Refresher(false);

            
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
        public void Refresher(bool netrefresh)
        {
            Debug.Log("refering");
            titlePanel.SetActive(true);
            instruction.SetActive(false);
            newPlayerPanel.SetActive(false);
            playOnline.SetActive(false);
            connectionPanel.SetActive(false);
            StartCoroutine(GameStart());
            if (netrefresh)
            {
                Application.Quit();            
            }
        }
    }

}

