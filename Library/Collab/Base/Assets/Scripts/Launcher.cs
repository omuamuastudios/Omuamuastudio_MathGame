// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Launcher.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking Demos
// </copyright>
// <summary>
//  Used in "PUN Basic tutorial" to connect, and join/create room automatically
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics; // LoaderAnime Script
using AppAdvisory.MathGame; // GameManager Script
using UnityEngine.SceneManagement;
using System.Collections;

namespace FourtyFourty
{
	#pragma warning disable 649

    /// <summary>
    /// Launch manager. Connect, join a random room or create one if none or all full.
    /// </summary>
	public class Launcher : MonoBehaviourPunCallbacks
    {

		#region Private Serializable Fields

		[Tooltip("The Ui Panel to let the user enter name, connect and play")]
		[SerializeField]
		private GameObject controlPanel;

		[Tooltip("The Ui Text to inform the user about the connection progress")]
		[SerializeField]
		private Text feedbackText;

		[Tooltip("The maximum number of players per room")]
		[SerializeField]
		private byte maxPlayersPerRoom = 4;

		[Tooltip("The UI Loader Anime")]

		#endregion

		#region Private Fields
		/// <summary>
		/// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon, 
		/// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
		/// Typically this is used for the OnConnectedToMaster() callback.
		/// </summary>
		bool isConnecting;

		/// <summary>
		/// This client's version number. Users are separated from each other by gameVersion (which allows you to make breaking changes).
		/// </summary>
		string gameVersion = "1";

		#endregion

		#region MonoBehaviour CallBacks

		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity during early initialization phase.
		/// </summary>
		void Awake()
		{

			timer.SetTimer(countDownTime);
			// #Critical
			// this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
			//PhotonNetwork.AutomaticallySyncScene = true;

			// check for internet connectivity;

			if(!CheckInternetConnection())
			{
				//ShowConnectionStatus("Not Connected to Internet.." , true);
			}
		}

		#endregion

		#region FourtyFourty Variables
		
		public AppAdvisory.MathGame.PlayButton playButtonScript;
		public GameObject gamePanelGameObj;
		public AppAdvisory.MathGame.GameManager gameManagerScript;
		public int totalQuestion = 10;
		public float countDownTime = 5f;

		public Text otherPlayerNameText;
		public Text countDownText;
		public Text gameStatusText;
		public GameObject loadinAnimObj;
		public GameObject netowrkingGameObj;
		public GameObject createRoomBtn;
		public GameObject joinRoomBtn;

		public GameObject connectionPanel;

		public InputField inputRoomName;

		public GameObject connectingStatusPanel;
		public Text connctingStatusText;

		public string localPlayerName;
		public string otherPlayerName;


		public Text matchesWonText;
		public Text matchesWonPlayedText;
		public Text matchesWonTimeText;
		bool isCreatJoinRoom = false;
		bool isRandomMatchMaking = false;
		bool isRestarting = false;
	
		Timer timer = new Timer();
		bool timeOut = false;

		bool connectedToInternet = false;

		string roomName = "";
		#endregion

		#region Public Methods

		/// <summary>
		/// Start the connection process. 
		/// - If already connected, we attempt joining a random room
		/// - if not yet connected, Connect this application instance to Photon Cloud Network
		/// </summary>
		public void Connect()
		{
			ShowConnectionStatus("Connecting.." , true);
			// we want to make sure the log is clear everytime we connect, we might have several failed attempted if connection failed.
			feedbackText.text = "";

			// keep track of the will to join a room, because when we come back from the game we will get a callback that we are connected, so we need to know what to do then
			isConnecting = true;

			// hide the Play button for visual consistency
			controlPanel.SetActive(false);


			// we check if we are connected or not, we join if we are , else we initiate the connection to the server.
			if (PhotonNetwork.IsConnected)
			{
				LogFeedback("Joining Room...");
				Debug.Log("Joining Room...");
				// #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
				PhotonNetwork.JoinRandomRoom();
			}else{

				LogFeedback("Connecting...");
				Debug.Log("Connecting..");
				
				// #Critical, we must first and foremost connect to Photon Online Server.
				PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = this.gameVersion;
				Debug.Log("GameVersion"+ this.gameVersion);
			}
		}

		/// <summary>
		/// Logs the feedback in the UI view for the player, as opposed to inside the Unity Editor for the developer.
		/// </summary>
		/// <param name="message">Message.</param>
		void LogFeedback(string message)
		{
			// we do not assume there is a feedbackText defined.
			if (feedbackText == null) {
				return;
			}

			// add new messages as a new line and at the bottom of the log.
			feedbackText.text += System.Environment.NewLine+message;
		}

        #endregion


        #region MonoBehaviourPunCallbacks CallBacks
        // below, we implement some callbacks of PUN
        // you can find PUN's callbacks in the class MonoBehaviourPunCallbacks


        /// <summary>
        /// Called after the connection to the master is established and authenticated
        /// </summary>
        public override void OnConnectedToMaster()
		{
            // we don't want to do anything if we are not attempting to join a room. 
			// this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
			// we don't want to do anything.
			if (isConnecting)
			{
				LogFeedback("OnConnectedToMaster: Next -> try to Join Random Room");
				Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN. Now this client is connected and could join a room.\n Calling: PhotonNetwork.JoinRandomRoom(); Operation will fail if no room found");
				ShowConnectionStatus("Connecting.." , false);
				// #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
				if(!isCreatJoinRoom)
					PhotonNetwork.JoinRandomRoom();
				else {
					// enable the create and join room button once connected
					joinRoomBtn.SetActive(true);
					createRoomBtn.SetActive(true);
				}
			}
			localPlayerName = PhotonNetwork.NickName;
		//	connectionPanel.SetActive(true);
			connectedToInternet = true;
		}

		/// <summary>
		/// Called when a JoinRandom() call failed. The parameter provides ErrorCode and message.
		/// </summary>
		/// <remarks>
		/// Most likely all rooms are full or no rooms are available. <br/>
		/// </remarks>
		public override void OnJoinRandomFailed(short returnCode, string message)
		{
			LogFeedback("<Color=Red>OnJoinRandomFailed</Color>: Next -> Create a new Room");
			Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

			// #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
			PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = this.maxPlayersPerRoom});
		}

		/// <summary>
		/// Called when entering a room (by creating or joining it). Called on all clients (including the Master Client).
		/// </summary>
		/// <remarks>
		/// This method is commonly used to instantiate player characters.
		/// If a match has to be started "actively", you can call an [PunRPC](@ref PhotonView.RPC) triggered by a user's button-press or a timer.
		///
		/// When this is called, you can usually already access the existing players in the room via PhotonNetwork.PlayerList.
		/// Also, all custom properties should be already available as Room.customProperties. Check Room..PlayerCount to find out if
		/// enough players are in the room to start playing.
		/// </remarks>
		public override void OnJoinedRoom()
		{
			LogFeedback("<Color=Green>OnJoinedRoom</Color> with "+PhotonNetwork.CurrentRoom.PlayerCount+" Player(s)");
			Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.\nFrom here on, your game would be running.");
			ShowConnectionStatus("Joining Room.." , false);
			// #Critical: We only load if we are the first player, else we rely on  PhotonNetwork.AutomaticallySyncScene to sync our instance scene.

			// if we have reaced the maxPlayersPerRoom then we can start the game
			if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayersPerRoom)
			{
				Debug.Log("We load the 'Room for " + PhotonNetwork.CurrentRoom.PlayerCount);

				// #Critical
				// Load Multiplayer level
				//PhotonNetwork.LoadLevel("PunBasics-Room for 1");

				// Start the game , we will use the question back which we have created
				// make sure we have the question bank synced first then call this other wise you know
				// for testing we you can use maxPlayersPerRoom as 1

				//CreateQustionBank();
				//gamePanelGameObj.SetActive(true);
				//playButtonScript.OnClicked();
			//	gameManagerScript.StartTheMultiplayerMode();

			}

			if (PhotonNetwork.IsMasterClient)
			{
				Debug.LogFormat( "OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient ); // called before OnPlayerLeftRoom
				CreateQustionBank();
			}
			// we are the oteher player joining the room after the maseter client 
			else {
				foreach (var item in PhotonNetwork.PlayerList){
					if(item.IsMasterClient)
					{
						otherPlayerNameText.text = item.NickName;
					}
				}
			}
		}

		#endregion

		#region  ConnectionTearDown
		/// <summary>
		/// Called after disconnecting from the Photon server.
		/// </summary>
		public override void OnDisconnected(DisconnectCause cause)
		{
			LogFeedback("<Color=Red>OnDisconnected</Color> "+cause);
			Debug.LogError("PUN Basics Tutorial/Launcher:Disconnected");

			isConnecting = false;
			controlPanel.SetActive(true);
			ShowConnectionStatus("Failed to Create Room.." , false);
			EndGameDueToDisconnect();

			if(isRestarting){
				SceneManager.LoadScene(0);
			}

		}
		#endregion

		 #region Rooms Photon Callbacks 

        /// <summary>
        /// Called when a Photon Player got connected. We need to then load a bigger scene.
        /// </summary>
        /// <param name="other">Other.</param>
        public override void OnPlayerEnteredRoom( Player other  )
		{
			Debug.Log( "OnPlayerEnteredRoom() " + other.NickName); // not seen if you're the player connecting
			otherPlayerNameText.text = other.NickName;
			otherPlayerName = other.NickName;
			if (PhotonNetwork.IsMasterClient)
			{
				Debug.LogFormat( "OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient ); // called before OnPlayerLeftRoom

			}
		}

		/// <summary>
		/// Called when a Photon Player got disconnected. We need to load a smaller scene.
		/// </summary>
		/// <param name="other">Other.</param>
		public override void OnPlayerLeftRoom( Player other  )
		{
			Debug.Log( "OnPlayerLeftRoom() " + other.NickName ); // seen when other disconnects

			if ( PhotonNetwork.IsMasterClient )
			{
				Debug.LogFormat( "OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient ); // called before OnPlayerLeftRoom
			}

			// just disconnect the user, OnDisconnected Will be called which will reaload the scene
			Disconnect();
		}

		/// <summary>
		/// Called when the local player left the room. We need to load the launcher scene.
		/// </summary>
		public override void OnLeftRoom()
		{
			//SceneManager.LoadScene("PunBasics-Launcher");
		}

		public override void OnCreateRoomFailed(short errro , string message){
			LogFeedback("OnCreateRoomFailed");
			Debug.Log("OnCreateRoomFailed "+message);
			ShowConnectionStatus("Failed to Create Room.." , true);

		}
		public override void OnJoinRoomFailed(short errro , string message){
			LogFeedback("OnJoinRoomFailed");
			Debug.Log("OnJoinRoomFailed "+message);
			ShowConnectionStatus("Failed to Join Room.." , true);

		}

		public override void OnCreatedRoom()
		{
			LogFeedback("OnCreatedRoom");
			Debug.Log("OnCreatedRoom " + roomName);
			if(roomName.Length == 6)
				gameStatusText.text = "Share This Room ID "+ roomName +" \nwith your friend to play\ntogether";
			else gameStatusText.text = "Waiting for other player\nto join";
			ShowConnectionStatus("Connecting.." , false);
		}

		#endregion

		#region Buttons Public Methods

		public void LeaveRoom()
		{
			PhotonNetwork.LeaveRoom();
		}

		public void QuitApplication()
		{
			Application.Quit();
		}

		public void Disconnect(bool justDisconnet = false){
			if(!justDisconnet)
				ShowConnectionStatus("Disconnecting.." , true);
			PhotonNetwork.Disconnect();
		}

		public void CustomRoom()
		{
			isCreatJoinRoom = true;
			Connect();
			ShowConnectionStatus("Connecting.." , true);
		}
		public void CreateRoom()
		{
			roomName = RandomRoomName(); // share this room name to a friend
			ShowConnectionStatus("Creating Room.." , true);
			PhotonNetwork.CreateRoom(roomName , new RoomOptions { MaxPlayers = this.maxPlayersPerRoom});
			
			
		}
		public void JoinRoom()
		{
			if(inputRoomName.text.Length != 6){
				Debug.LogError("Wrong Room Name");
			}
			ShowConnectionStatus("Joining Room.." , true);
			PhotonNetwork.JoinRoom(inputRoomName.text);
		}

		public void LocalPlayerDoneWithTheGame(int score , float timeTaken){
			base.photonView.RPC("PRC_GameFinished" , RpcTarget.Others , score , timeTaken);
		}

		public void OfflineSinglePlayer(){
			SceneManager.LoadScene(1);
		}

		public void RestartTheGame(){
			PhotonNetwork.Disconnect();
			isRestarting = true;

		}

		public void ShowStats(){
			matchesWonPlayedText.text =  PlayerPrefs.GetInt(UTIL.TOTAL_GAMES_PLAYED , 0).ToString();
           	matchesWonText.text =  PlayerPrefs.GetInt(UTIL.TOTAL_GAMES_WON , 0).ToString();
            matchesWonTimeText.text =  PlayerPrefs.GetInt(UTIL.TOTAL_GAMES_TIME , 0).ToString();
            
		}

		#endregion

		#region Private Methods

		void CreateQustionBank()
		{
			if ( ! PhotonNetwork.IsMasterClient )
			{
				Debug.LogError( "PhotonNetwork : Trying to Load a level but we are not the master Client" );
				return;
			}

			Debug.LogFormat( "PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount );
			//PhotonNetwork.LoadLevel("PunBasics-Room for "+PhotonNetwork.CurrentRoom.PlayerCount);

				// if we are master client then generate the quesiton bank here, generate totalQuestion from here
			gameManagerScript.GenerateQuestions(totalQuestion);
			LogFeedback("Question bank is created");
			// if we have generated the quesiton bank master can now just send this 
			// we have used bufferother as RPC that is why we are syncing here otherwise we may have to Sync the question from other places
			// like when we reach max player per room
			if(gameManagerScript.questionBank.Count == totalQuestion){
				SyncQuestionBank();
				LogFeedback("Question bank is Sent for Buffer");
			} else {
				LogFeedback("Question bank is not synced");
			}
		}

		// now sync the quesion bank to other palyer in the room
		void SyncQuestionBank()
		{
			// make sure we have the question bank
			if (gameManagerScript.questionBank.Count == totalQuestion){
					// add the logic to sync
					if(PhotonNetwork.IsMasterClient){
						
						for (int i=0;i<totalQuestion;++i)
						{
							base.photonView.RPC("RPC_SyncQuestionBank" , RpcTarget.OthersBuffered , gameManagerScript.questionBank[i].for_network);
							LogFeedback("Sent Questions " + gameManagerScript.questionBank[i].for_network);
						}
							
					}
			}
			else {
				Debug.LogError("Question Bank list is not equal to  totalQuestion " + gameManagerScript.questionBank.Count);
			}

		}

		void EndGameDueToDisconnect()
		{
			// just remove all the questions
			gameManagerScript.FlushQuestionBank();
		}

		void RunCountDown()
		{
			timer.ActivateTimer(true);
			//StartCoroutine(CountDown(countDownTime));
		}
		
		IEnumerator CountDown(float dealy)
		{
			yield return new WaitForSeconds(dealy);
			playButtonScript.OnClicked();
		}
		void Update()
		{
			if(timer.RunTimer()) 
			{
			//	Debug.Log(timer.TimeRemaining());
				countDownText.text = timer.TimeRemaining().ToString();
			}
			if(timer.IsTimeOut() && !timeOut)
			{
				timeOut = true;
				gamePanelGameObj.SetActive(true);
				netowrkingGameObj.SetActive(false);
				//playButtonScript.OnClicked();
			}
		}

		string RandomRoomName(){
			int roomName = Random.Range(100000 , 999999);
			return roomName.ToString();
		}

		void ShowConnectionStatus(string status , bool isActive){
			connectingStatusPanel.SetActive(isActive);
			connctingStatusText.text = status;
		}

		bool CheckInternetConnection()
		{
			switch(Application.internetReachability)
			{
				case NetworkReachability.ReachableViaCarrierDataNetwork:
				case NetworkReachability.ReachableViaLocalAreaNetwork:
					return true;
				case NetworkReachability.NotReachable:
					return false;
			}
			return false;
		}
		
		

		#endregion
		
		#region  RPC's
		[PunRPC]
		private void RPC_SyncQuestionBank(string question)
		{
			// populate the quetion bank for other player , this is called because master client has send us the question
			// get all the question one by one
			if(gameManagerScript.questionBank.Count < totalQuestion) {
				gameManagerScript.questionBank.Add(new QuestionBank(question));
				Debug.Log("Added Qeustion " + question + "   " + gameManagerScript.questionBank[gameManagerScript.questionBank.Count-1].for_network);
				LogFeedback("Added Qeustion " + question);
			} 
			else 
			{
				Debug.LogError("We already has the question bank, Reason for this can be many, Handle this issue one by one");
			}

			/// this means we have synced
			//let master client know We are ready to start the game
			if(gameManagerScript.questionBank.Count == totalQuestion)
			{
				base.photonView.RPC("RPC_ClientReady" , RpcTarget.MasterClient , true);
			}
		}

		[PunRPC]
		private void RPC_ClientReady(bool isReady)
		{
			// show a button for ready
			if(isReady)
			{
				Debug.Log("Client is ready for the Game");
				LogFeedback("Other Player is Ready To Play");
				base.photonView.RPC("RPC_StartTheGame" , RpcTarget.AllViaServer);
		
			}

		}

		[PunRPC]
		private void RPC_StartTheGame()
		{
			Debug.Log("Now the actual game can be played");
			LogFeedback("Final Call for the Game, Let's Play");
			//gamePanelGameObj.SetActive(true);
			countDownText.gameObject.SetActive(true);
			gameStatusText.text = "Starting In";
			loadinAnimObj.SetActive(false);
			gameManagerScript.isMultiplayerActive = true; // this will make sure we do not timeout while playing the game
			RunCountDown();
		}

		[PunRPC]
		private void PRC_GameFinished(int otherPlayerscore , float timeTaken)
		{
			gameManagerScript.otherPlayerScore = otherPlayerscore;
			gameManagerScript.otherPlayerTimeTaken = timeTaken;
			Debug.Log("Got score from other Player" + otherPlayerscore);
			Debug.Log("Got time taken from other Player" + timeTaken);
		}

		#endregion

		
	}
}