# Reflex Arcade

> Unity 모바일 멀티태스킹 아케이드 게임 | 팀 프로젝트 | Google Play 출시작

2분할 화면에서 미니게임을 동시에 플레이하며 점수를 경쟁하는 멀티태스킹 아케이드.
미니게임 4종 수록, 향후 확장 가능한 구조로 설계.

🎮 [Google Play](https://play.google.com/store/apps/details?id=com.rgb.ReflexArcade&hl=ko)

## 담당 역할

**Color Twin 미니게임 구현**  
떨어지는 오브젝트와 하단 버튼의 색상을 일치시키는 아케이드 액션 게임.  
게임 로직 · 이벤트 시스템 · UI · 사운드 연동 담당

## 주요 구현

### 이벤트 기반 판정 시스템

성공·실패 판정을 `static Action` 이벤트로 분리해 `FallingCircle`이 판정 결과만 발행하고, `GameManager`와 `ScoreManager`가 독립적으로 구독하는 구조로 설계. 판정 로직과 점수 계산·게임 오버 처리가 결합되지 않는다.

```csharp
// FallingCircle.cs
public static Action onSpriteMatch;
public static Action<FallingCircle> onSpriteMismatch;

void CheckImageMatch()
{
    if (image.sprite == target.GetComponent<Image>().sprite)
        onSpriteMatch?.Invoke();           // ScoreManager가 구독 → 점수 증가
    else if (image.sprite != null)
        onSpriteMismatch?.Invoke(this);    // GameManager가 구독 → 게임 오버
}
```

### 고정 배열 순환 재사용 — 오브젝트 풀링 기반 최적화

오브젝트를 매번 생성·파괴하지 않고 미리 생성된 배열 내 인덱스를 순환하며 재사용. `ResetPosition()`에서 위치와 상태만 초기화해 GC 발생을 최소화한다.

```csharp
// ColorTwinGameManager.cs
public FallingCircle[] fallingCirclesL;
private int currentLIndex = 0;

// 다음 오브젝트 활성화 시
currentIndex = (currentIndex + 1) % fallingCircles.Length;  // 순환
```

### 코루틴 기반 오브젝트 간격 자동 조절

다음 오브젝트를 활성화하기 전에 이전 오브젝트와의 Y축 거리를 매 프레임 확인해, `distanceThreshold(350f)` 이상 벌어질 때까지 대기. 오브젝트 간 시각적 겹침을 방지한다.

```csharp
// ColorTwinGameManager.cs
IEnumerator SpawnLoop(FallingCircle[] fallingCircles, int currentIndex)
{
    while (true)
    {
        yield return new WaitForSeconds(Random.Range(minSpawnDelay, maxSpawnDelay));

        foreach (var circle in fallingCircles)
        {
            if (circle.gameObject != currentCircle && circle.isMoving)
            {
                while (Mathf.Abs(startY - circle.rectTransform.anchoredPosition.y) < distanceThreshold)
                    yield return null;  // 간격이 확보될 때까지 대기
            }
        }

        currentCircle.GetComponent<FallingCircle>().isMoving = true;
        currentIndex = (currentIndex + 1) % fallingCircles.Length;
    }
}
```

좌우 화면을 독립 코루틴으로 병렬 제어한다.

### 출현 확률 기반 빈 슬롯 설계 — 3단계 판정 분기

오브젝트가 `appearanceRate(30%)` 확률로만 스프라이트를 가지며, 나머지는 `null`(투명)로 처리된다. 판정이 세 가지로 자연스럽게 분기되어 빈 슬롯을 별도 타입 없이 처리할 수 있다.

```csharp
void CheckImageMatch()
{
    if (image.sprite == target.sprite)   // 일치 → 점수
        onSpriteMatch?.Invoke();
    else if (image.sprite != null)       // 불일치 → 게임 오버
        onSpriteMismatch?.Invoke(this);
    // else: null(빈 슬롯) → 무시
}
```

`appearanceRate` 수치 조정만으로 난이도를 제어할 수 있으며, 오브젝트 풀링 구조와도 자연스럽게 통합된다.

### 실시간 거리 판정 로직

낙하 중인 오브젝트와 타겟 사이의 Y축 거리를 매 프레임 계산해, `judgeDistance(250f)` 이하로 진입하는 순간 색상 일치 여부를 판별한다.

```csharp
// FallingCircle.cs
IEnumerator CheckDistanceAndMove()
{
    while (true)
    {
        if (isMoving)
            rectTransform.anchoredPosition += Vector2.down * fallSpeed * Time.deltaTime;

        float yDistance = Mathf.Abs(rectTransform.localPosition.y - target.localPosition.y);
        if (yDistance <= judgeDistance)
            CheckImageMatch();

        yield return null;
    }
}
```

## 기술 스택

Unity · C# · C# Action 이벤트 · Object Pooling · Coroutine · Mobile (Android)
