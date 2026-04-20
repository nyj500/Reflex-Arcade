using UnityEngine;

/* ======================================================================
 * [Class]: Singleton<T>
 * [Role] : 씬 안에서만 유지되는 로컬 싱글톤 베이스 클래스
 *          StagePopupManager, HitEffect 등 씬 전용 컴포넌트에서 사용
 * ====================================================================== */
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
