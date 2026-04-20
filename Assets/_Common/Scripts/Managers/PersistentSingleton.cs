using UnityEngine;

/* ======================================================================
 * [Class]: PersistentSingleton<T>
 * [Role] : 씬이 전환되어도 유지되는 전역 싱글톤 베이스 클래스
 *          AppManager, DataManager, SoundManager, AdManager 등에서 사용
 * ====================================================================== */
public abstract class PersistentSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
