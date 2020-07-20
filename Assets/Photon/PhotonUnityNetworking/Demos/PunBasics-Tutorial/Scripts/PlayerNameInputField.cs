// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayerNameInputField.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking Demos
// </copyright>
// <summary>
//  Let the player input his name to be saved as the network player Name, viewed by alls players above each  when in the same room. 
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Photon.Pun.Demo.PunBasics
{	
	public class PlayerNameInputField : MonoBehaviour
	{
		#region Private Constants
	    public InputField _inputField;
		public Text playername;
	


		public GameObject intro;
		public GameObject newPlayer;
		public GameObject SaveName,backButton;

		public bool firstEntry;
		

		// Store the PlayerPref Key to avoid typos
		const string ppplayerNamePrefKey="MyPlayer" ;
		const string newgame = "1";

		string playerNamePrefKey;
		#endregion

		#region MonoBehaviour CallBacks
		void Start () {
		 playerNamePrefKey = PlayerPrefs.GetString(ppplayerNamePrefKey);
			Debug.Log(playerNamePrefKey);
			Debug.Log(newgame);

		
			if (!PlayerPrefs.HasKey(newgame))
			{					
				PlayerPrefs.SetInt(newgame, 2);
				firstEntry = true;
				StartCoroutine(NewPlayer());
			}
			else
            {
				playername.text = playerNamePrefKey;
				
				firstEntry = false;
				StartCoroutine(WelcomeBack());
            }
			Debug.Log(firstEntry);
			string defaultName = string.Empty;			
			//if (_inputField==null)
			//{
			//	if (PlayerPrefs.HasKey(playerNamePrefKey))
			//	{
			//		defaultName = PlayerPrefs.GetString(playerNamePrefKey);
			//		_inputField.text = defaultName;
			//		playername.text = defaultName;					
			//	}
			//}
			PhotonNetwork.NickName = playerNamePrefKey;
		}
		private void Update()
		{
			playerNamePrefKey = PlayerPrefs.GetString(ppplayerNamePrefKey);
			playername.text = playerNamePrefKey;
			
		}
		#endregion
		IEnumerator NewPlayer()
        {
		yield return new WaitForSeconds(1);
			
			if (string.IsNullOrEmpty(PhotonNetwork.NickName))
			{
				//PlayerPrefs.SetString(playerNamePrefKey, "Tester");
				Debug.Log(PlayerPrefs.GetString(ppplayerNamePrefKey));
				Debug.Log(PhotonNetwork.NickName);
				PhotonNetwork.NickName = PlayerPrefs.GetString(ppplayerNamePrefKey);

			}

		

        }
		IEnumerator WelcomeBack()
        {
			yield return new WaitForSeconds(1);
        }

		#region Public Methods


		/// <param name="value">The name of the Player</param>
		public void SetPlayerName(string value)
		{
			// #Important
			SaveName.SetActive(true);
			backButton.SetActive(false);
		    if (string.IsNullOrEmpty(value))
		    {
                Debug.LogError("Player Name is null or empty");
		        return;
		    }
			PhotonNetwork.NickName = value;

			PlayerPrefs.SetString(ppplayerNamePrefKey, value);
		}
		
		public void Rename()
        {
			newPlayer.SetActive(true);
		}
    
        #endregion
    }

}
