using System.Collections;
using TMPro;
using UnityEngine;
namespace GravitySplit
{
    public class GravityGameManager : BaseGameManager
    {
        public static GravityGameManager Instance;
        [SerializeField] private GameObject gameOverUI;
        [SerializeField] private GameoverUI scoreUI;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TutorialUI tutorialUI;

        private int currentScore = 0;


        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            // 해당 게임 타입의 튜토리얼을 처음 보는지 확인 후 열기
            if (DataManager.Instance != null && DataManager.Instance.IsFirstSeen(GameType.GravitySplit))
            {
                tutorialUI.Open(GameType.GravitySplit, StartGame);
            }

            // 튜토리얼을 봤다면 UI 열지않고 바로 게임 시작
            else
            {
                tutorialUI.gameObject.SetActive(false);

                StartGame();
            }
        }
        public void AddScore(int amount)
        {
            currentScore += amount;
            if (scoreText != null)
            {
                scoreText.text = currentScore.ToString();
            }
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayScoreSound();
            }
        }


        public void GameOver()
        {
            IsGameRunning = false;
            Time.timeScale = 0f;
            int bestScore = 0;
            bool isNewRecord = false;

            if (DataManager.Instance != null)
            {
                isNewRecord = DataManager.Instance.CheckAndSaveBestScore(GameType.GravitySplit, currentScore);
            }

            if (DataManager.Instance != null)
            {
                bestScore = DataManager.Instance.GetBestScore(GameType.GravitySplit);
            }

            if (gameOverUI != null)
            {
                gameOverUI.SetActive(true);
                scoreUI.ShowResult(currentScore, bestScore, isNewRecord);
            }

            if (AdManager.Instance != null)
            {
                AdManager.Instance.ScheduleInterstitial();
            }
        }
    }

}
