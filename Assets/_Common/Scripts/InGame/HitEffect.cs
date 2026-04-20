using System.Collections;
using UnityEngine;

/* ======================================================================
 * [Class]: HitEffect (Singleton)
 * [Role] : 게임 오버 시 원인이 된 충돌 오브젝트를 시각적으로 강조(Highlight)하는 연출 클래스
 * ====================================================================== */
public class HitEffect : Singleton<HitEffect>
{
    public void PlayHighlight(Transform target)
    {
        if (target == null) return;
        StartCoroutine(PulseRoutine(target));
    }

    IEnumerator PulseRoutine(Transform target)
    {
        Vector3 originalScale = target.localScale;
        Vector3 targetScale = originalScale * 0.8f;

        float speed = 4f;
        float time = 0;

        while (true)
        {
            if (target == null) yield break;

            // PingPong은 시간이 계속 늘어나도 알아서 0 -> 1 -> 0 -> 1 ... 왕복
            float t = Mathf.PingPong(time * speed, 1);

            target.localScale = Vector3.Lerp(originalScale, targetScale, t);

            // 게임오버라 TimeScale이 0일 테니 unscaledDeltaTime 사용
            time += Time.unscaledDeltaTime;
            yield return null;
        }
    }
}
