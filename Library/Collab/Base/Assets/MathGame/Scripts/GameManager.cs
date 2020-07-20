
/***********************************************************************************************************
 * Produced by App Advisory - http://app-advisory.com													   *
 * Facebook: https://facebook.com/appadvisory															   *
 * Contact us: https://appadvisory.zendesk.com/hc/en-us/requests/new									   *
 * App Advisory Unity Asset Store catalog: http://u3d.as/9cs											   *
 * Developed by Gilbert Anthony Barouch - https://www.linkedin.com/in/ganbarouch                           *
 ***********************************************************************************************************/




using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using MenuBarouch;
using UnityEngine.SceneManagement;
#if APPADVISORY_LEADERBOARD
using AppAdvisory.social;
#endif
#if APPADVISORY_ADS
using AppAdvisory.Ads;
#endif

namespace AppAdvisory.MathGame
{
    public class QuestionBank
    {
        public int number1;
        public int number2;
        public int operateur;
        public int result;

        public String for_network; // this we will send to the network

        public QuestionBank(int number1, int number2, int operateur, int result)
        {
            this.number1 = number1;
            this.number2 = number2;
            this.operateur = operateur;
            this.result = result;
            for_network = number1 + "|" + number2 + "|" + operateur + "|" + result;
        }

        // conver s
        public QuestionBank(String question)
        {

            String[] temp = question.Split('|');
            number1 = int.Parse(temp[0]);
            number2 = int.Parse(temp[1]);
            operateur = int.Parse(temp[2]);
            result = int.Parse(temp[3]);
            for_network = question;

        }
    }

    public class GameManager : MonoBehaviour
    {
        public int numberOfPlayToShowInterstitial = 5;

        public string VerySimpleAdsURL = "http://u3d.as/oWD";

        static System.Random _random = new System.Random();

        public AudioClip musicBackground;
        public AudioClip goodAnswerSound;
        public AudioClip falsedAnswerSound;

        public int timeTotalGame;
        public int timeMalus;
        public int timeBonus;

        public Color NORMAL_COLOR;
        public Color GOOD_COLOR;
        public Color FAIL_COLOR;
        public Image BACKGROUND_BACK;

        public ParticleSystem ParticleSystemSuccessPrefab;

        public Text point;

        public GameObject QUESTIONS_GO;

        public GameObject BUTTONS_GO;

        public GameObject POINTS;
        public Text pointsText;

        public Text questionNumber1;
        public Text questionOperator; //+=0 -=1 *=2 /=3
        public Text questionNumber2;
        public Text questionResult;

        public Text[] reponses;

        public int level; //responsible to change the speed of the fill out => so the difficulty

        public int _score;

        public int GOODANSWER; //count the number of good answer, ie. the score

        public Slider slider; //the slider in the top of the game screen

        public Text levelText; //the text to see the level during the game

        private Vector2 pivotPoint;

        int _result = 0;
        int _number1 = 0;
        int _number2 = 0;
        int _operateur = 0;

        public delegate void _GameOver();
        public static event _GameOver OnGameOver;

        public bool isMultiplayerActive = false;


        #region FourtyFourty Variables
        public List<QuestionBank> questionBank = new List<QuestionBank>(); // only master client will create question bank and send it to other player
        public FourtyFourty.Launcher photonLauncherScript; // this script contains all the connection realated problems

        public Text feedBackText;
        public Text p1Score;
        public Text p2Score;
        public Text p1Name;
        public Text p2Name;
        public Text p1TimeText;
        public Text p2TimeText;
        public GameObject gameOverPanel;
        public GameObject[] disableMe;
        public int otherPlayerScore = -1;
        public float otherPlayerTimeTaken = 0f;
        private int questionIndex = 0;
        private float localPlayerTimeTaken = 0;
        private float waitTime = 60f;
        private float curTime = 0;

        private bool gameover = false;
        #endregion

        #region FourtyFourty Tweaked/Created Functions


        // call this function to GenerateQuestions from launcher script
        public void GenerateQuestions(int number)
        {
            if (questionBank.Count == photonLauncherScript.totalQuestion)
            {
                return;
            }
            for (int i = 0; i < number; ++i)
            {
                QuestionBank temp = ChooseOperator(true);
                if (temp != null)
                {
                    questionBank.Add(temp);
                    Debug.Log(i + 1 + ". " + questionBank[i]);
                }
            }
        }
        
        public void SaveGameStats(bool won = false)
        {
            PlayerPrefs.SetInt(UTIL.TOTAL_GAMES_PLAYED , PlayerPrefs.GetInt(UTIL.TOTAL_GAMES_PLAYED , 0)+1);
            if(won){
                PlayerPrefs.SetInt(UTIL.TOTAL_GAMES_WON , PlayerPrefs.GetInt(UTIL.TOTAL_GAMES_WON , 0)+1);
            }
            else {
                PlayerPrefs.SetInt(UTIL.TOTAL_GAMES_LOST , PlayerPrefs.GetInt(UTIL.TOTAL_GAMES_LOST , 0)+1);
            }
            
            PlayerPrefs.Save();
            Debug.Log("Saved PlayerPrefs "+PlayerPrefs.GetInt(UTIL.TOTAL_GAMES_PLAYED , 0) +" "+PlayerPrefs.GetInt(UTIL.TOTAL_GAMES_WON , 0) +" "+PlayerPrefs.GetInt(UTIL.TOTAL_GAMES_LOST , 0)+" "+PlayerPrefs.GetFloat(UTIL.TOTAL_GAMES_TIME , 0) );
        }

        void WiatForResultFromOtherPlayer()
        {
            StopMusic();
            for (int i = 0; i < disableMe.Length; ++i)
            {
                disableMe[i].SetActive(false);
            }
            SetQuestionMarks();
            Debug.Log("MYSCORE  " + _score);
            gameOverPanel.SetActive(true);
            StartCoroutine(WaitForOtherPlayerResult());
        }

        void SetQuestionMarks()
        {
            feedBackText.text = "Waiting..";
            Debug.Log("MYSCORE  " + _score);
            p1Score.text ="Score :"+ _score.ToString();
            Debug.Log("MYSCORE  " + p1Score.text);
            p2Score.text = "Waiting";
			p1TimeText.text = localPlayerTimeTaken.ToString("n2");
			p2TimeText.text = "";
            p1Name.text = photonLauncherScript.localPlayerName;
            p2Name.text = photonLauncherScript.otherPlayerName;

            p1Score.text ="Score :"+ _score.ToString();
            p1Score.text = "Score : " + _score.ToString();
            float time = PlayerPrefs.GetFloat(UTIL.TOTAL_GAMES_TIME , 0);
            if(time>localPlayerTimeTaken || time == 0)
            {
                PlayerPrefs.SetFloat(UTIL.TOTAL_GAMES_TIME ,localPlayerTimeTaken);
            }
            p1Score.text = "Score : " + _score.ToString();
        }

        public void FlushQuestionBank(){
            questionBank.Clear();
            questionIndex = 0;
        }

        IEnumerator WaitForOtherPlayerResult()
        {

            while (otherPlayerScore == -1)
            {

                curTime += 0.01f;
                //if the slider == 0 ===> game over
                if (curTime > waitTime)
                {
                    break;
                }
                yield return new WaitForSeconds(0.01f);
            }
            DisplayFinalResult();
        }

        void DisplayFinalResult()
        {
            if (otherPlayerScore == -1)
            {
                Debug.Log("We Timed Out");
                return;
            }

            p2Score.text ="Score : "+ otherPlayerScore.ToString();
			p2TimeText.text = otherPlayerTimeTaken.ToString("n2");

            if (otherPlayerScore > _score)
            {
                feedBackText.color = Color.red;
                feedBackText.text = "You Loose!!";
                SaveGameStats(false);
                Debug.LogError("YOU LOOOOOS BRO..");
            }
            else if (otherPlayerScore < _score)
            {
                feedBackText.color = Color.green;
                feedBackText.text = "You Win!!";
                SaveGameStats(true);
                Debug.LogError("YOU WINNNINNNNINN BRO..");
            }
            else
            {
                if (localPlayerTimeTaken < otherPlayerTimeTaken)
                {
                    feedBackText.color = Color.green;
                    feedBackText.text = "You Win!!";
                    Debug.LogError("YOU WINNNINNNNINN BRO..");
                     SaveGameStats(true);
                }
                else
                {
                    feedBackText.color = Color.red;
                    feedBackText.text = "You Loose!!";
                    Debug.LogError("YOU LOOOOOS BRO..");
                     SaveGameStats(false);

                }
            }
            photonLauncherScript.LeaveRoom();
           // StartCoroutine(Restart(30));
        }
        IEnumerator Restart(float delay)
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene(0);
        }
        //choose operateur for the question : + = 0    - = 1     * = 2      / = 3
        QuestionBank ChooseOperator(bool generateQuesBank = false)
        {

            print(questionBank.Count + "   " + questionIndex);

            // here comes the nice part which is called leveraging, we will leverate the game itself for now
            // we will use question bank to generate the qeustion now
            if (photonLauncherScript != null && questionBank.Count == photonLauncherScript.totalQuestion)
            {
                // this means we are done with question now show the results
                if (questionIndex == questionBank.Count)
                {
                    WiatForResultFromOtherPlayer();
                    photonLauncherScript.LocalPlayerDoneWithTheGame(_score, localPlayerTimeTaken);
                    return null;
                }
                else
                {
                    if (questionIndex < questionBank.Count)
                    {
                        SetText(questionBank[questionIndex].number1, questionBank[questionIndex].number2, questionBank[questionIndex].operateur, questionBank[questionIndex].result);
                        questionIndex++;
                    }
                }
                return null;
            }

            int operateur = 0;
            // this means we are trying to play in multiplayer mode
            if (generateQuesBank)
            {
                level = UnityEngine.Random.Range(1, 4);
            }
            if (level == 1)
            {
                operateur = UnityEngine.Random.Range(0, 2);
            }
            else if (level <= 3)
            {
                operateur = UnityEngine.Random.Range(0, 3);
            }
            else
            {
                operateur = UnityEngine.Random.Range(0, 4);
            }

            return CreateQuestion(operateur, generateQuesBank);
        }
        QuestionBank CreateQuestion(int operateur, bool generateQuesBank = false)
        {

            int result = 0;
            int number1 = 0;
            int number2 = 0;

            int essai = 0;

            while (true)
            {
                essai++;

                bool isOK = true;

                if (operateur == 3)
                {
                    int mult = UnityEngine.Random.Range(2 + (int)Mathf.Log(level), 2 + 2 * (int)Mathf.Log(level));

                    number2 = UnityEngine.Random.Range(2 + level / 2, 3 + level);

                    number1 = mult * number2;
                }
                else if (operateur == 1)
                {
                    number2 = UnityEngine.Random.Range(1 + (int)Mathf.Log(level) / 2, 5 + (int)Mathf.Log(level));

                    number1 = UnityEngine.Random.Range(number2 + (int)Mathf.Log(level) / 2, number2 + 5 + 2 * (int)Mathf.Log(level));

                }
                else
                {
                    number1 = UnityEngine.Random.Range(1 + (int)Mathf.Log(level) / 2, 5 + 2 * (int)Mathf.Log(level));
                    number2 = UnityEngine.Random.Range(1 + (int)Mathf.Log(level) / 2, 5 + 2 * (int)Mathf.Log(level));
                }

                result = GetResult(number1, number2, operateur);

                if (operateur == 1 || operateur == 3)
                {

                    int resultDIV = 0;

                    int resultMINUS = 0;

                    resultDIV = number1 / number2;

                    resultMINUS = number1 - number2;

                    if (resultDIV == resultMINUS)
                    {
                        isOK = false;
                    }

                }


                if (operateur == 0 || operateur == 2)
                {
                    int resultMULT = 0;

                    int resultPLUS = 0;

                    resultMULT = number1 * number2;

                    resultPLUS = number1 + number2;

                    if (resultMULT == resultPLUS)
                    {
                        isOK = false;
                    }
                }

                if (_result == result && _number1 == number1 && _number2 == number2 && _operateur == operateur)
                {
                    isOK = false;
                }

                if (result <= 0)
                {
                    isOK = false;
                }


                if (operateur == 3)
                {
                    if (number1 % number2 != 0)
                    {
                        isOK = false;
                    }

                    if (number1 / number2 == 0)
                    {
                        isOK = false;
                    }

                    if (number1 / number2 == 1)
                    {
                        isOK = false;
                    }
                }
                else
                {
                    if (operateur == 2)
                    {
                        if (number1 == 0 || number1 == 1 || number2 == 0 || number2 == 1 || result == 0 || result == 1)
                        {
                            isOK = false;
                        }
                    }
                }

                if (level <= 2)
                {
                    if (result > 9)
                    {
                        isOK = false;
                    }
                    if (result <= 0 || number1 <= 0 || number2 <= 0)
                    {
                        isOK = false;
                    }
                }
                else if (level <= 4)
                {
                    if (result > 50)
                    {
                        isOK = false;
                    }
                    if (result <= 0 || number1 <= 0 || number2 <= 0)
                    {
                        isOK = false;
                    }
                }
                else if (level <= 6)
                {
                    if (result > 99)
                    {
                        isOK = false;
                    }
                }


                if (result > 99)
                {
                    isOK = false;
                }


                //CHECK!!!
                if (isOK)
                {
                    if (operateur == 0)
                    {
                        int resultTest = number1 + number2;
                        if (resultTest != result)
                        {
                            isOK = false;
                        }
                    }
                    if (operateur == 1)
                    {
                        int resultTest = number1 - number2;
                        if (resultTest != result)
                        {
                            isOK = false;
                        }
                    }
                    if (operateur == 2)
                    {
                        int resultTest = number1 * number2;
                        if (resultTest != result)
                        {
                            isOK = false;
                        }
                    }
                    if (operateur == 4)
                    {
                        int resultTest = number1 / number2;
                        if (resultTest != result)
                        {
                            isOK = false;
                        }
                    }

                }
                // _ to avoid duplicate questions
                if (isOK)
                {
                    _result = result;
                    _number1 = number1;
                    _number2 = number2;
                    _operateur = operateur;

                    break;
                }
            }

            // if we are just generating question then don't even bother setting the text
            if (!generateQuesBank)
            {
                SetText(number1, number2, operateur, result);
                return null;
            }

            // question are in formate 1|2|+|3, we just need to sync these strings
            return new QuestionBank(number1, number2, operateur, result);
        }

        private void ResetThePanelForNewQuestion()
        {
            QUESTIONS_GO.GetComponent<Animator>().Play("AnimQuestionChange");

            slider.value += timeTotalGame;

            AnimColorBACKGROUND_BACK(true);

            BUTTONS_GO.GetComponent<Animator>().Play("AnimButtonGame");

            PlaySoundGood();

        }

        #endregion

        #region Original Code
        //play fx when answer is wrong
        void PlaySoundFalse()
        {
            GetComponent<AudioSource>().PlayOneShot(falsedAnswerSound);
        }

        //play fx when answer is good
        void PlaySoundGood()
        {
            GetComponent<AudioSource>().PlayOneShot(goodAnswerSound);
        }

        //play the music
        void PlayMusic()
        {
            GetComponent<AudioSource>().Play();
        }

        //stop the music
        void StopMusic()
        {
            GetComponent<AudioSource>().Stop();
        }

        void OnEnable()
        {
            Application.targetFrameRate = 60;
            StartGame();
        }

        void OnDisable()
        {
            StopMusic();
        }

        //method to start the game
        private void StartGame()
        {
            // it means we are not letting things get automatically controlled
            // We will manage displaying quesion from question back that is stored in question bank list
            if (isMultiplayerActive)
            {
                //return;
            }
            Init();
        }

        private void Init()
        {

            PlayMusic();

            //reset the score
            _score = 0;

            //reset the level
            level = 1;

            point.text = _score.ToString();

            levelText.text = "Level " + level.ToString();

            //create the first question
            ChooseOperator();

            //start the game
            StartCoroutine(TimerStart());

        }

        //decrease continiously the timer (= the slider), and if = 0 ==> gameover
        IEnumerator TimerStart()
        {
            slider.maxValue = timeTotalGame;
            slider.value = timeTotalGame;

            while (true)
            {

                float timer = 0.01f + ((float)Mathf.Sqrt(level)) / 100f;

                slider.value -= timer;

                localPlayerTimeTaken += 0.01f;
                //if the slider == 0 ===> game over
                if (slider.value == 0)
                {
                    break;
                }


                yield return new WaitForSeconds(0.01f);
            }

            if (isMultiplayerActive)
            {
                Debug.Log("NextQuestion from isMultiplayerActive");
                // just go to another question, don't do game over
                if (questionIndex < questionBank.Count)
                {
                    ResetThePanelForNewQuestion();
                    StartCoroutine(TimerStart());
					ChooseOperator();
                }
            }
            else
            {
                Debug.Log("NextQuestion from GameOver");
                GameOver();
            }
        }


        private void GameOver()
        {
            Debug.Log("GameOver Called");
            ScoreManager.SaveScore(_score, level);

            FindObjectOfType<MenuBarouch.MenuManager>().GoToMenu();

            ReportScoreToLeaderboard(_score);

            ShowAds();

            if (OnGameOver != null)
                OnGameOver();
        }


        //set the question text
        private void SetText(int n1, int n2, int oper, int result)
        {
            int TYPE_QUESTION = UnityEngine.Random.Range(0, 4);

            if (TYPE_QUESTION == 0)
            {
                questionNumber1.text = "?";

                questionNumber2.text = n2.ToString();

                questionOperator.text = GetOperator(oper);

                questionResult.text = result.ToString();

                GOODANSWER = n1;
            }

            if (TYPE_QUESTION == 1)
            {
                questionNumber1.text = n1.ToString();

                questionNumber2.text = n2.ToString();

                questionOperator.text = "?";

                questionResult.text = result.ToString();

                GOODANSWER = oper;
            }

            if (TYPE_QUESTION == 2)
            {
                questionNumber1.text = n1.ToString();

                questionNumber2.text = "?";

                questionOperator.text = GetOperator(oper);

                questionResult.text = result.ToString();

                GOODANSWER = n2;
            }

            if (TYPE_QUESTION == 3)
            {
                questionNumber1.text = n1.ToString();

                questionNumber2.text = n2.ToString();

                questionOperator.text = GetOperator(oper);

                questionResult.text = "?";

                GOODANSWER = result;
            }

            questionNumber1.transform.parent.Find("Selected").gameObject.SetActive(TYPE_QUESTION == 0);
            questionOperator.transform.parent.Find("Selected").gameObject.SetActive(TYPE_QUESTION == 1);
            questionNumber2.transform.parent.Find("Selected").gameObject.SetActive(TYPE_QUESTION == 2);
            questionResult.transform.parent.Find("Selected").gameObject.SetActive(TYPE_QUESTION == 3);


            if (TYPE_QUESTION != 1)
            {
                int[] answers = new int[4];

                List<int> l = new List<int>();

                l.Add(GOODANSWER);

                while (true)
                {
                    int ans = 0;

                    int addRange = 0;

                    while (true)
                    {
                        bool isOk = true;

                        ans = UnityEngine.Random.Range(1, GOODANSWER * 2 + 3);

                        if (ans <= 0)
                            isOk = false;

                        if (isOk)
                            break;

                        addRange++;
                    }

                    if (!l.Contains(ans))
                        l.Add(ans);

                    if (l.Count == 4)
                        break;
                }

                l.Sort();

                answers = l.ToArray();

                Array.Sort(answers);

                for (int i = 0; i < 4; i++)
                {
                    reponses[i].fontSize = 90;
                    reponses[i].text = answers[i].ToString();
                }
            }
            else
            {
                reponses[0].text = "+";
                reponses[0].fontSize = 130;
                reponses[1].text = "-";
                reponses[1].fontSize = 130;
                reponses[2].text = "x";
                reponses[3].text = "÷";
            }
        }


        public void OnClicked(Text text)
        {
            int myAnswer;

            bool isMaybeOperator = text.text.Length <= 1;

            if (text.text.Contains("+") && isMaybeOperator)
                myAnswer = 0;
            else if (text.text.Contains("-") && isMaybeOperator)
                myAnswer = 1;
            else if (text.text.Contains("x") && isMaybeOperator)
                myAnswer = 2;
            else if (text.text.Contains("÷") && isMaybeOperator)
                myAnswer = 3;
            else
                myAnswer = int.Parse(text.text);

            if (GOODANSWER == myAnswer)
            {
                _score++;

                if (_score % 5 == 0)
                {
                    level++;
                    levelText.text = "Level " + level.ToString();
                }

                pointsText.text = _score.ToString();

                StartCoroutine(GoodAnswerAnim());

                slider.value += timeTotalGame;

                AnimColorBACKGROUND_BACK(true);

                BUTTONS_GO.GetComponent<Animator>().Play("AnimButtonGame");

                PlaySoundGood();
            }
            else
            {
                slider.value -= timeTotalGame / 5;

                PlaySoundFalse();

                AnimColorBACKGROUND_BACK(false);
            }
        }



        private int GetResult(int n1, int n2, int oper)
        {
            if (oper == 0)
                return n1 + n2;
            else if (oper == 1)
                return n1 - n2;

            else if (oper == 2)
                return n1 * n2;

            else if (oper == 3)
                return n1 / n2;
            else
                return 0;
        }

        private string GetOperator(int oper)
        {
            if (oper == 0)
                return "+";
            else if (oper == 1)
                return "-";
            else if (oper == 2)
                return "x";
            else if (oper == 3)
                return "÷";
            else
                return "";
        }

        IEnumerator GoodAnswerAnim()
        {
            float time = 0.2f;

            QUESTIONS_GO.GetComponent<Animator>().Play("AnimQuestionChange");

            var goParticleSystemSystem = Instantiate(ParticleSystemSuccessPrefab.gameObject) as GameObject;
            var tParticleSystemSystem = goParticleSystemSystem.transform;
            tParticleSystemSystem.position = new Vector3(point.transform.position.x, point.transform.position.y, point.transform.position.z + 2);
            tParticleSystemSystem.rotation = Quaternion.identity;
            goParticleSystemSystem.SetActive(true);
            goParticleSystemSystem.GetComponent<ParticleSystem>().Emit(50);
            yield return new WaitForSeconds(time + 0.01f);

            ChooseOperator();
        }

        public static string[] RandomizeStrings(string[] arr)
        {
            List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
            // Add all strings from array
            // Add new random int each time
            foreach (string s in arr)
            {
                list.Add(new KeyValuePair<int, string>(_random.Next(), s));
            }
            // Sort the list by the random number
            var sorted = from item in list
                         orderby item.Key
                         select item;
            // Allocate new string array
            string[] result = new string[arr.Length];
            // Copy values to array
            int index = 0;
            foreach (KeyValuePair<int, string> pair in sorted)
            {
                result[index] = pair.Value;
                index++;
            }
            // Return copied array
            return result;
        }

        public void AnimColorBACKGROUND_BACK(bool isGoodAnswer)
        {
            StartCoroutine(AnimColorBACKGROUND_BACK_Corout(isGoodAnswer));
        }

        IEnumerator AnimColorBACKGROUND_BACK_Corout(bool isGoodAnswer)
        {
            Color c = FAIL_COLOR;

            var time = 0.3f;
            var originalTime = time;

            if (isGoodAnswer)
                c = GOOD_COLOR;

            while (time > 0.0f)
            {
                time -= Time.deltaTime;
                BACKGROUND_BACK.color = Color.Lerp(NORMAL_COLOR, c, time / originalTime);
                yield return 0;
            }

        }

        // <summary>
        /// If using Very Simple Leaderboard by App Advisory, report the score : http://u3d.as/qxf
        /// </summary>
        void ReportScoreToLeaderboard(int p)
        {
#if APPADVISORY_LEADERBOARD
			//LeaderboardManager.ReportScore(p);
#else
            print("Get very simple leaderboard to use it : http://u3d.as/qxf");
#endif
        }

        /// <summary>
        /// If using Very Simple Ads by App Advisory, show an interstitial if number of play > numberOfPlayToShowInterstitial: http://u3d.as/oWD
        /// </summary>
        public void ShowAds()
        {
            int count = PlayerPrefs.GetInt("GAMEOVER_COUNT", 0);
            count++;
            PlayerPrefs.SetInt("GAMEOVER_COUNT", count);
            PlayerPrefs.Save();

#if APPADVISORY_ADS
			if(count > numberOfPlayToShowInterstitial)
			{
#if UNITY_EDITOR
			print("count = " + count + " > numberOfPlayToShowINterstitial = " + numberOfPlayToShowInterstitial);
#endif
			if(AdsManager.instance.IsReadyInterstitial())
			{
#if UNITY_EDITOR
				print("AdsManager.instance.IsReadyInterstitial() == true ----> SO ====> set count = 0 AND show interstial");
#endif
				PlayerPrefs.SetInt("GAMEOVER_COUNT",0);
				AdsManager.instance.ShowInterstitial();
			}
			else
			{
#if UNITY_EDITOR
				print("AdsManager.instance.IsReadyInterstitial() == false");
#endif
			}

		}
		else
		{
			PlayerPrefs.SetInt("GAMEOVER_COUNT", count);
		}
		PlayerPrefs.Save();
#else
            if (count >= numberOfPlayToShowInterstitial)
            {
                Debug.LogWarning("To show ads, please have a look to Very Simple Ad on the Asset Store, or go to this link: " + VerySimpleAdsURL);
                Debug.LogWarning("Very Simple Ad is already implemented in this asset");
                Debug.LogWarning("Just import the package and you are ready to use it and monetize your game!");
                Debug.LogWarning("Very Simple Ad : " + VerySimpleAdsURL);
                PlayerPrefs.SetInt("GAMEOVER_COUNT", 0);
            }
            else
            {
                PlayerPrefs.SetInt("GAMEOVER_COUNT", count);
            }
            PlayerPrefs.Save();
#endif
        }

        #endregion
    }

}