using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEngine;

namespace ColorTwin
{
    public class ColorTwinGameManager : BaseGameManager
    {
        public static ColorTwinGameManager Instance;
        
        public FallingCircle[] fallingCirclesL;
        public FallingCircle[] fallingCirclesR;
        private int currentLIndex = 0;
        private int currentRIndex = 0;

        private float distanceThreshold = 350f;
        
        public float minSpawnDelay = 0f;
        public float maxSpawnDelay = 1f;

        [SerializeField] private TutorialUI tutorialUI;
        
        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
        void Start()
        {
            // that game type's tutorial first time check
            if (DataManager.Instance != null && DataManager.Instance.IsFirstSeen(GameType.ColorTwin))
            {
                tutorialUI.Open(GameType.ColorTwin, StartGame);
            }
            else // If tutorial has been seen, start game without showing UI
            {
                tutorialUI.gameObject.SetActive(false);

                StartGame();
            }
            
            StartCoroutine(SpawnLoop(fallingCirclesL, currentLIndex));
            StartCoroutine(SpawnLoop(fallingCirclesR, currentRIndex));
        }

        private void OnEnable()
        {
            FallingCircle.onSpriteMismatch += HandleSpriteMismatch;
        }

        private void OnDisable()
        {
            FallingCircle.onSpriteMismatch -= HandleSpriteMismatch;

            StopAllCoroutines();
        }

        IEnumerator SpawnLoop(FallingCircle[] fallingCircles, int currentIndex)
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(minSpawnDelay, maxSpawnDelay));

                // 1. Activate the current circle
                GameObject currentCircle = fallingCircles[currentIndex].gameObject;
        
                foreach (var circle in fallingCircles)
                {
                    // Wait until other circles have moved a certain distance away
                    if (circle.gameObject != currentCircle && circle.isMoving)
                    {
                        float startY = currentCircle.gameObject.GetComponent<RectTransform>().anchoredPosition.y;
                        while (Mathf.Abs(startY - circle.gameObject.GetComponent<RectTransform>().anchoredPosition.y) < distanceThreshold)
                        {
                            yield return null;
                        }
                    }
                }

                currentCircle.GetComponent<FallingCircle>().isMoving = true;
                currentIndex = (currentIndex + 1) % fallingCircles.Length;
            }
        }

        private void GameOver()
        {
            Debug.Log("Game Over!");
            // Stop game play
            Time.timeScale = 0f;
            StopAllCoroutines();
            if (AdManager.Instance != null)
            {
                AdManager.Instance.ScheduleInterstitial();
            }
        }

        private void HandleSpriteMismatch(FallingCircle targetCircle)
        {
            if (targetCircle != null)
            {
                GameOver();
            }
        }
    }
}