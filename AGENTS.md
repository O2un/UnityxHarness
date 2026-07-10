# CLAUDE.md — Unity 프로젝트 공통 컨벤션

> 이 파일은 Claude Code가 매 세션마다 자동으로 읽는 규칙집입니다.
> 코드 생성·수정·리팩토링 시 아래 규칙을 **반드시** 따르세요.
> 규칙을 벗어나는 판단이 필요하다고 느껴지면 코드 작성을 멈추고 사람에게 먼저 확인하세요.

---

## 1. 프로젝트 개요

- **엔진**: Unity 6 (6000.x)
- **언어**: C# (.NET Standard 2.1)
- **아키텍처**: Manager / Module / Service 3레이어 패턴
- **DI**: VContainer

---

## 2. 폴더 구조

폴더는 레이어(Manager/Module/Service)가 아니라 **기능 단위**로 분리합니다.

```
Assets/
├── 00_CommonFramework/        ← 두 프로젝트가 공유하는 공통 인프라
│   └── {대분류}/              ← 기능 그룹 (예: SystemManager, Gameplay)
│       └── {중분류}/          ← 구체 기능 (예: CameraManager, InputManager)
│           ├── {Feature}Manager.cs    ← 같은 기능의 모든 파일을 한 폴더에
│           ├── I{Feature}.cs
│           └── {Feature}Module.cs
├── 10_ProjectA/               ← 프로젝트 A 전용 (동일 구조)
└── 20_ProjectB/               ← 프로젝트 B 전용 (동일 구조)
```

**예시:**
```
00_CommonFramework/
└── SystemManager/
    └── CameraManager/
        ├── CameraManager.cs
        ├── IVirtualCamera.cs
        └── CameraSelectModule.cs
```

- 폴더 앞 숫자(`00_`, `10_`, `20_`)는 Unity Project 뷰 정렬 순서를 고정하기 위한 컨벤션입니다
- 같은 기능에 속하는 Manager · Module · Interface는 **같은 중분류 폴더**에 둡니다
- 공통 인프라는 반드시 `00_CommonFramework`에 작성하세요. `10_ProjectA` 또는 `20_ProjectB` 안에 인프라 코드를 직접 작성하지 마세요
- 어느 대분류·중분류에 넣어야 할지 판단이 서지 않으면 사람에게 확인하세요

---

## 3. 코드 컨벤션

### 네이밍

| 대상 | 규칙 | 예시 |
|---|---|---|
| 클래스 | PascalCase | `PlayerMover`, `EnemySpawner` |
| 인터페이스 | `I` 접두사 + PascalCase | `IInputReader`, `IDamageable` |
| 메서드·프로퍼티 | PascalCase | `GetHealth()`, `IsAlive` |
| private 필드 | `_` + camelCase | `_health`, `_speed` |
| 상수 | UPPER_SNAKE_CASE | `MAX_HEALTH`, `SPAWN_INTERVAL` |

### 클래스 역할

- **Manager**: 순수 C# 클래스. 두 가지 존재 방식이 있음:
  - 씬/전역 스코프 단독 존재 (예: GameManager) — VContainer 싱글턴으로 등록·주입
  - MonoBehaviour 단위 객체 소속 (예: 캐릭터·적처럼 개별 오브젝트가 있을 때) — MonoBehaviour가 필드로 소유하고 로직 위임
  - 가능하면 게임 로직을 직접 포함하지 않고 Module에 위임. 단, 기능이 1~2개로 단순하면 Module로 굳이 분리하지 않아도 됨
- **Module**: 순수 C# 클래스. Unity API에 의존하지 않음. `new`로 생성 가능해야 함
  - 여기서 "Unity API 의존"이란 `MonoBehaviour` 상속, `Transform`/`GameObject`/컴포넌트 조작, 씬 탐색 같은 **씬·오브젝트 조작**을 말한다. `Mathf`·`Random` 같은 순수 정적 유틸이나 `AnimationCurve` 같은 **데이터 타입**은 값으로 받아 쓰는 것이므로 Module에서 사용해도 된다 (위반 아님, 리뷰에서 지적하지 말 것)
- **Service**: 파일·씬·에셋 등 외부 시스템 접점. 반드시 인터페이스로 추상화

### Module 분리 판단 기준

Manager · Module · Interface로 나누는 것 자체보다 **레이어 간 의존 방향을 지키는 것**이 더 중요하다. 처음부터 모든 기능을 Module로 쪼갤 필요는 없다. 나중에 기능이 늘거나 재사용이 필요해질 때 분리해도 된다 (예: 사칙연산만 있는 계산기라면 처음부터 `ComputeModule`을 만들 필요 없음).

판단이 애매하면 코드를 작성하기 전에 먼저 질문한다: "이 Manager에는 A기능, B기능, C기능이 있습니다. Module로 분리할까요, 아니면 Manager에 직접 구현할까요?"

### 의존 방향

```
Manager → Module
Manager → (interface를 통해) Service
```

- 역방향 금지: Module이 Manager를 직접 참조하면 안 됩니다
- Module끼리 직접 참조 금지: 필요 시 Manager를 통해 간접 통신
- 구체 클래스 직접 참조 금지: 항상 인터페이스를 통해 의존

---

## 4. 라이브러리 사용 범위

| 라이브러리 | 허용 범위 | 금지 |
|---|---|---|
| **VContainer** | Singleton Manager 등록·주입 | LifetimeScope 밖에서 `Container.Resolve<>()` 직접 호출 |
| **R3** | `ReactiveProperty`, `Subject`, `Subscribe`, `AddTo` | `SelectMany`, `FlatMap`, `Zip` 등 복잡한 Operator 체이닝 |
| **UniTask** | 파일 I/O, 씬 로딩, `UniTask.Delay` 시간 대기 | `Task`, `Thread`, 코루틴 혼용 |

허용 범위 밖의 기능이 필요하다고 판단되면 코드 작성을 **멈추고** 사람에게 먼저 확인하세요.

### DI 등록 방식: RegisterInstance vs WithParameter

- 어떤 값(예: ScriptableObject)을 **여러 곳에서 타입으로 주입**받거나, **씬 autoInject 대상의 메서드 주입**처럼 컨테이너가 타입으로 자동 해소해야 한다면 `RegisterInstance`로 컨테이너에 등록한다.
- 반대로 **소비처가 하나뿐이고 다른 데서 쓰일 가능성이 없다면**, 전역 `RegisterInstance` 대신 그 등록의 `WithParameter(...)`로 값을 직접 넘기는 **파라미터 주입**을 쓴다. 의존 범위를 불필요하게 넓히지 않기 위함이다.
- 아무도 주입받지 않는 `RegisterInstance`(죽은 등록)는 제거한다. 특히 `WithParameter`로 필요한 값을 이미 다 넘기면서 동시에 원본 SO를 `RegisterInstance`하는 중복을 주의한다.

---

## 5. 금지 패턴

아래 패턴은 일반적으로 동작하더라도 이 프로젝트에서는 사용하지 마세요.

```csharp
// ❌ 씬 탐색 금지 — DI 주입으로 대체
FindObjectOfType<GameManager>();
GameObject.Find("Player");

// ❌ Singleton static Instance 직접 구현 금지 — VContainer 사용
public static T Instance { get; private set; }

// ❌ Resources.Load 금지 — AssetService(Addressables) 사용
Resources.Load<GameObject>("Prefabs/Enemy");

// ❌ PlayerPrefs 직접 접근 금지 — SaveService 사용
PlayerPrefs.SetInt("Score", 100);

// ❌ 비동기 혼용 금지 — UniTask async/await 단일 방식
StartCoroutine(SomeRoutine());
new Thread(() => { }).Start();

// ❌ 순수 로직을 MonoBehaviour에 직접 작성 금지 — Module로 분리
public class HealthLogic : MonoBehaviour { /* 순수 계산 로직 */ }
```

# 6. Memory
memory 파일을 작성할때 프로젝트 루트에 있는 Memory 폴더에도 똑같이 작성해서 git으로 파일을 관리 할 수 있어야 한다.

---

# 7. 개발 하네스

이 프로젝트는 Unity 게임 개발 하네스를 사용합니다. (3D 탑다운 뱀서 MVP · 신규 설계 · MCP for Unity 연결)

## 자연어 라우팅
게임 기능 구현·시스템 설계·버그 수정·리팩토링 요청이 오면, 개별 Agent를 직접 부르기 전에 **`unity-dev-orchestrator` Skill을 먼저 사용**하세요.

예: "플레이어 이동 만들어줘", "적 스폰 구현해줘", "이 버그 고쳐줘" → `unity-dev-orchestrator` 실행.

단순한 C# 문법 질문이나 특정 파일 한 줄 수정 같은 단발성 편집은 그냥 처리합니다.

## 흐름 요약
설계(unity-architect) → **[승인: 배치 위치 공통/프로젝트]** → 구현(gameplay-engineer) → 씬·검증(unity-ai-operator, **[승인: 씬·에셋]**) → 리뷰(code-reviewer). 검증은 4단계 게이트(①컴파일 ②Play 콘솔에러 ③기능테스트 ④사용자 확인)로, ①~③은 Stop hook이 수행합니다.
  - Stop hook의 자동 검증은 **수동 트리거 방식**입니다: `artifacts/.viewer-state/validate-requested` 마커 파일이 있을 때만 실행되고, 실행 후 스스로 지워집니다. unity-ai-operator가 검증할 준비가 된 시점에 이 마커를 생성합니다. 마커가 없으면 Stop마다 즉시 스킵되어, 긴 작업 중 매 턴 컴파일·Play 검증이 도는 것을 방지합니다.

- **MVP 코드 배치 위치는 오픈 퀘스천**입니다. 각 시스템을 `00_CommonFramework`(공통)로 둘지 `10_ProjectA`(프로젝트 전용)로 둘지 구현 직전 사용자에게 확인하고, 임의로 정하지 않습니다.

## 주요 위치
- Agent: `.claude/agents/` (unity-architect, gameplay-engineer, code-reviewer, unity-ai-operator)
- Skill: `.claude/skills/` (unity-dev-orchestrator, csharp-convention-guide, add-global-manager, add-module, prd, game-plan)
- 결과 확인 뷰어 hooks: `.claude/hooks/` (Stop hook, Node.js 필요)
- 설계·검토·기록: `artifacts/`
- 참조 문서: `docs/conventions/convention.md`, `docs/design/game-plan.md`

## 변경 이력
- 2026-07-02 초기 하네스 구성 (Agent Team · Pipeline)
---

## Imported from Memory

### 코드 주석 금지

C# 코드 작성 시 기능을 설명하는 주석을 추가하지 않는다. 잘 지어진 식별자 이름으로 의미를 전달하고, WHY가 비자명한 경우에만 한 줄 주석을 허용한다.

원본: `memory/MEMORY.md`, `memory/feedback_no_code_comments.md`
