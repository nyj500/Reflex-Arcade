using UnityEngine;
using UnityEngine.UI;

/* ==================================================================================
 * [Class]: StageLockView
 * [Role] : 개별 스테이지의 잠금 상태를 관리하고, 클릭 시 해금 조건을 팝업 매니저에게 전달
 *
 * [변경 이유]
 * 기존: prerequisiteGames 리스트를 직접 인스펙터에 입력 → Config와 데이터 이중 관리
 * 변경: targetGame 하나만 설정하면, 조건 목록은 Config에서 자동으로 읽어옴
 *       → 인스펙터 설정이 단순해지고, Config 수정 시 자동 반영
 *
 * [Flow]
 * 1. UpdateUI    : config.IsUnlocked(targetGame)으로 잠금 여부 판단 → UI 갱신
 * 2. OnLockClick : config.GetConditions(targetGame)으로 조건 목록 가져와 팝업 요청
 * ================================================================================== */
public class StageLockView : MonoBehaviour
{
    [SerializeField] private StageUnLockConfig config;

    // 이 뷰가 담당하는 게임 (열릴 게임, 예: GravitySplit)
    // 조건 목록은 config에서 자동으로 읽어오므로 별도 입력 불필요
    [SerializeField] private GameType targetGame;

    [SerializeField] private GameObject lockIcon;
    [SerializeField] private Button playButton;

    void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        // config에서 targetGame의 모든 조건 달성 여부를 한 번에 확인
        bool isUnlocked = config != null && config.IsUnlocked(targetGame);

        if (lockIcon != null)
        {
            lockIcon.SetActive(!isUnlocked);

            // 잠긴 상태일 때만 자물쇠 버튼 클릭 이벤트 등록
            Button lockBtn = lockIcon.GetComponent<Button>();
            if (lockBtn != null)
            {
                lockBtn.onClick.RemoveAllListeners();
                lockBtn.onClick.AddListener(OnLockClick);
            }
        }

        if (playButton != null) playButton.gameObject.SetActive(isUnlocked);
    }

    private void OnLockClick()
    {
        if (StagePopupManager.Instance == null || config == null) return;

        // config에서 조건 목록을 가져와 팝업에 전달
        var conditions = config.GetConditions(targetGame);
        if (conditions != null)
            StagePopupManager.Instance.Show(lockIcon.transform.position, conditions);
    }
}
