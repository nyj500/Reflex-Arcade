using TMPro;
using UnityEngine;

/* ======================================================================
 * [Class]: MirrorDodgeManager
 * [Role] : 게임 상태(시작/종료), 점수, 튜토리얼, 광고를 총괄 관리
 *          BaseGameManager 상속 → IsGameRunning, StartGame() 공통 제공
 * * [Method Summary]
 * 1. AddScore : 점수 증가 및 UI 갱신
 * 2. GameOver : 게임 종료 처리 (중복 호출 방지 포함)
 * ====================================================================== */
namespace MirrorDodge
{
    public class MirrorDodgeManager : BaseGameManager
    {
        public static MirrorDodgeManager Instance { get; private set; }

        [SerializeField] private GameObject gameOverUI;
        [SerializeField] private GameoverUI scoreUI;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TutorialUI tutorialUI;

        private int currentScore = 0;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (DataManager.Instance != null && DataManager.Instance.IsFirstSeen(GameType.MirrorDodge))
            {
                tutorialUI.Open(GameType.MirrorDodge, StartGame);
            }
            else
            {
                tutorialUI.gameObject.SetActive(false);
                StartGame();
            }
        }

        public void AddScore(int amount)
        {
            if (!IsGameRunning) return;

            currentScore += amount;
            if (scoreText != null)
                scoreText.text = currentScore.ToString();

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayScoreSound();
        }

        public void GameOver()
        {
            // 두 공이 동시에 장애물에 닿았을 때 중복 호출 방지
            if (!IsGameRunning) return;

            IsGameRunning = false;
            Time.timeScale = 0f;

            int bestScore = 0;
            bool isNewRecord = false;

            if (DataManager.Instance != null)
            {
                isNewRecord = DataManager.Instance.CheckAndSaveBestScore(GameType.MirrorDodge, currentScore);
                bestScore = DataManager.Instance.GetBestScore(GameType.MirrorDodge);
            }

            if (gameOverUI != null)
            {
                gameOverUI.SetActive(true);
                scoreUI.ShowResult(currentScore, bestScore, isNewRecord);
            }

            if (AdManager.Instance != null)
                AdManager.Instance.ScheduleInterstitial();
        }
    }
}
