---

name: add-module

description: Manager가 위임할 순수 로직 Module을 추가한다. Module은 Unity API에 의존하지 않고 new로 생성 가능한 순수 C# 클래스다. 특정 Manager 산하에 속하는 로직 단위(예: GameFlowModule, HealthModule)에 사용한다.

argument-hint: "[ModuleName] (소속 Manager도 함께 명시 권장, 예: GameFlowModule in GameManager)"

---

**$ARGUMENTS Module**을 프로젝트 컨벤션에 맞게 생성한다.

## 파일 위치

- 소속 Manager와 **같은 폴더**에 둔다
- `{프로젝트루트}/{대분류}/{ManagerName}/{ModuleName}Module.cs`
- 소속 Manager가 불분명하면 사람에게 먼저 확인한다

## 클래스 규칙

- 순수 C# 클래스. Unity API(`MonoBehaviour`, `GameObject`, `Transform` 등)에 의존하지 않음
- `new`로 생성 가능해야 함 — VContainer에 직접 등록하지 않음. Manager가 필드로 소유
- Manager를 직접 참조하지 않음 (역방향 의존 금지)
- 다른 Module을 직접 참조하지 않음 — 필요 시 Manager를 통해 간접 통신
- 외부에 알려야 할 상태 변화는 `ReactiveProperty<T>` 또는 `Subject<T>`로 노출 (C# event 금지)
- 생성자로만 의존성 주입. public setter 금지

## Manager 연동

- 소속 Manager는 Module을 private 필드로 소유하고 `new`로 직접 생성
- Manager는 외부에서 받은 호출을 Module 메서드에 위임
- Module이 노출하는 ReactiveProperty는 Manager가 Subscribe하거나 외부에 그대로 전달

## 생성 후 검증

- Unity API(`MonoBehaviour`, `FindObjectOfType`, `GameObject.Find` 등) 참조 없는지 확인
- Manager 또는 다른 Module을 직접 참조하지 않는지 확인
- `new` 없이 VContainer에 단독 등록되어 있지 않은지 확인
- CLAUDE.md 금지 패턴 위반 없는지 확인
