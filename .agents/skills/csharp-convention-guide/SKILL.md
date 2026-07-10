---
name: csharp-convention-guide
description: 이 프로젝트의 C# 코드를 작성·리뷰할 때 따르는 컨벤션 가이드. 네이밍, if 평가값 앞, 캐싱, 빈 함수 표기, VContainer DI 계층·클래스 작성 순서, R3/UniTask 허용 범위를 규정한다. gameplay-engineer가 코드를 쓸 때와 code-reviewer가 점검할 때 사용한다. 원본은 docs/conventions/convention.md와 CLAUDE.md이며 이 Skill은 그 요약이다. 충돌 시 원본 문서를 따른다.
---

# C# 컨벤션 가이드

원본: `docs/conventions/convention.md`, `CLAUDE.md`. 이 Skill은 실무 요약이며, 애매하면 원본과 사람 확인을 우선한다.

## 사용 조건
- 사용: 게임플레이 C# 스크립트를 작성·수정·리뷰할 때.
- 미사용: 문서 작성, 씬·에셋 조작(그건 unity-ai-operator), 설계 자체(unity-architect).

## 입력
- 대상 스크립트, `artifacts/01-design.md`, 원본 컨벤션 문서.

## 절차 (작성/리뷰 시 확인 항목)

### 1. 네이밍
| 대상 | 규칙 |
| --- | --- |
| 네임스페이스·클래스·메서드·프로퍼티 | PascalCase |
| 인터페이스 | `I` + PascalCase |
| private 필드 | `_camelCase` (언더바 + 소문자 시작) |
| 지역변수·파라미터 | camelCase |
| 전역 상수 | ALL_UPPER_SNAKE_CASE |

- 네임스페이스는 `프로젝트명.대분류.모듈` 형식.

### 2. 코드 습관
- **if 평가값 앞**: `if (false == something)`, `if (null == gameObject)` — 대입 오타 방지.
- **캐싱**: `FindObject`류 사용 금지(에디터 툴 예외). 꼭 필요하면 상의. `RequireComponent` 캐싱은 `_cache ??= GetComponent<T>()` 패턴. UnityEngine.Object가 아닌 순수 C# 클래스는 `??=` 사용 가능.
- **빈 함수 표기**: abstract/override로 강제됐지만 미사용이면 본문에 `// NULL` 표기. 미구현 필수 항목은 `throw new NotImplementedException()` 상태로 남긴다(빈 채로 두지 않음). 빈 override가 많아지면 virtual 전환·클래스 분리 고려.
- **주석**: 기능 설명 주석은 달지 않는다. WHY가 비자명할 때만 한 줄.

### 3. 클래스 역할·의존 방향
- Manager(순수 C#) → Module(순수 C#, Unity API 비의존, `new` 가능) → Service(외부 접점, 인터페이스 추상화).
- 의존 방향: `Manager → Module`, `Manager →(interface)→ Service`. 역참조·Module 간 직접 참조·구체 클래스 직접 참조 금지.
- Module 분리는 과하게 하지 않는다. 기능 1~2개면 Manager 직접 구현. 애매하면 질문.

### 4. VContainer DI
- 계층: `ProjectLifetimeScope`(전 구간 유지 SystemManager) → SceneScope(`CommonLifetimeScope` 상속, 씬 한정).
- Mono 클래스는 생성자 `[Inject]` 누락 실수를 피하려 **필드에 `[Inject]`**. 주입 후 초기화가 필요하면 `IInitializable` 상속해 `Initialize`.
- `LifetimeScope` 밖에서 `Container.Resolve<>()` 직접 호출 금지.
- 파라미터가 늘면 별도 파라미터 묶음 클래스로 분리(부모에 넘길 때 특히). 파라미터 과다는 SOLID 위반 신호.

### 5. 클래스 작성 순서
```csharp
public sealed class ClassName : Parent, Interface
{
    private readonly InjectedClass _injectedClass;   // 생성자 위 = DI 받은 것만
    public ClassName(InjectedClass inject) { _injectedClass = inject; }

    private readonly ReactiveProperty<int> _value = new();  // 생성자 아래 = Reactive
    public ReadOnlyReactiveProperty<int> Value => _value;

    protected override async UniTask InitAsync()  // Init에서 가공·구독
    {
        await base.InitAsync();
        _injectedClass.Reactive.Subscribe(x => _value.Value = x).AddTo(DisposableR3);
    }
}
```

### 6. 라이브러리 허용 범위
- **VContainer**: 싱글턴 Manager 등록·주입만.
- **R3**: `ReactiveProperty`, `Subject`, `Subscribe`, `AddTo`. `SelectMany`/`FlatMap`/`Zip` 등 복잡한 체이닝 금지.
- **UniTask**: 파일 I/O, 씬 로딩, `UniTask.Delay`. `Task`/`Thread`/코루틴 혼용 금지.
- 범위 밖 기능이 필요하면 코드 작성을 멈추고 사람에게 확인.

### 7. 금지 패턴
`FindObjectOfType`·`GameObject.Find`, static `Instance`, `Resources.Load`, `PlayerPrefs` 직접 접근, `StartCoroutine`/`new Thread`, MonoBehaviour에 순수 로직 작성 — 전부 금지.

## 출력
- 이 Skill은 문서를 만들지 않는다. 코드는 gameplay-engineer가 `Assets/`에, 리뷰 결과는 code-reviewer가 `artifacts/03-review.md`에 남긴다.

## 품질 기준
- 위 1~7을 모두 만족해야 통과. 위반 시: 금지 패턴·의존 방향 위반은 blocker, 네이밍·순서 이탈은 major, 스타일은 minor.
- 범위 밖·모호하면 원본 문서와 사람 확인을 우선한다.
