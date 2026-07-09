# ExperienceSystem PRD

> 근거: `docs/spec/ExperienceSystem.md` · 경험치 루프의 가운데(PRD 2). 입력은 PRD 1(`ItemDrop`)의 "경험치 N 획득", 출력은 PRD 3(`LevelUpSelection`)의 입력이 되는 `LevelUpEvent`.

## Overview

"경험치 N 획득" 입력을 받아 누적하고, 레벨별 필요치를 넘으면 초과분을 이월하며 레벨업을 발행하는 순수 로직 계층. 처치 보상 루프의 가운데로, 아이템·Collider·UI·일시정지는 모르고 오직 amount 입력과 `LevelUpEvent` 출력만 다룬다.

## Goals

- `ExperienceModule.Gain(amount)`로 "경험치 N 획득" 입력 하나만 받아 경험치를 누적한다
- 누적치를 현재 레벨 필요치와 비교해, 초과 시 초과분을 이월하며 레벨을 올린다 (한 번에 여러 레벨 상승 허용)
- 레벨별 필요 경험치를 데이터로 주입받아 사용한다 (Module 내부 하드코딩 없음)
- 현재 경험치·현재 레벨을 `ReactiveProperty`로 노출해 HUD가 구독하게 한다
- 레벨이 오르면 새 레벨 등 결과 정보만 담은 `LevelUpEvent`를 R3로 단방향 발행한다

## Out of Scope

- 경험치 아이템 스폰·주움 감지 (PRD 1 `ItemDrop` 소관)
- 레벨업 선택 UI·게임 일시정지·능력 적용 (PRD 3 `LevelUpSelection` 소관)
- HUD 게이지·UI 표현 (Module은 상태만 노출, 표시는 구독 측 책임)
- 경험치 감소·리셋·저장/로드
- 최대 레벨(cap) — 이번 범위에서 제외 (레벨은 무한 상승 가정)

## Technical Requirements

### 배치

- **`10_ProjectA` 전용**에 배치한다 (프로젝트 A 전용 시스템).
- `ExperienceModule`·`LevelUpEvent`는 **순수 C# (Unity 비의존)**. `new`로 생성 가능해야 하며 Unity API·Collider·UI·`Time.timeScale`에 의존하지 않는다.

### 구성요소

| 구성요소 | 계층 | 책임 |
|---|---|---|
| `ExperienceModule` | Module (순수 C#) | 경험치 누적, 레벨 판정·이월, 상태 노출, `LevelUpEvent` 발행 |
| `LevelUpEvent` | Event payload | 레벨업 결과 정보(새 레벨 등) 전달 |
| 레벨 필요치 데이터 | Data | 레벨별 필요 경험치 정의 (`AnimationCurve`) |

### 획득 입력

- 공개 API는 `Gain(amount)` 하나. 아이템 종류·Collider·플레이어 타입을 인자로 받지 않는다.

### 레벨 판정·이월

- `Gain` 시: 누적치 += amount → 현재 레벨 필요치와 비교 → 초과분이 있으면 초과분을 이월하며 레벨 +1, 다음 필요치와 다시 비교. 한 호출로 여러 레벨이 오를 수 있다.
- **다중 레벨 상승 시 `LevelUpEvent`를 레벨마다 1회씩 발행한다** (2레벨 오르면 2번 발행).

### 데이터 (레벨별 필요 경험치)

- 레벨별 필요 경험치는 외부에서 **`AnimationCurve`로 주입**한다. 인스펙터에서 커브로 편집한다.
- 이후 정교한 조정이 필요해지면 값 테이블(`ScriptableObject` 등)로 확장할 수 있으나, 이번 범위는 `AnimationCurve`로 시작한다.
- Module은 이 데이터를 **읽기만** 하고, 값을 내부에 하드코딩하지 않는다.

### 상태 노출

- 현재 경험치·현재 레벨을 `ReactiveProperty<T>`로 노출한다. HUD 등 구독 측이 값 변화를 구독한다.
- R3 사용 범위는 `ReactiveProperty`·`Subject`·`Subscribe`·`AddTo`로 한정 (복잡한 Operator 체이닝 금지).

### 레벨업 발행 (하류 연결)

- `LevelUpEvent` payload는 **결과 정보만** 담는다 (새 레벨 등). 구독 측이 누구인지 모른다 — 단방향.
- PRD 3 `LevelUpSelection`의 구독 Context가 이 이벤트를 받는다. Module은 하류를 직접 참조하지 않는다.
- R3 구독은 `AddTo`로 lifecycle에 바인딩한다.

## Acceptance Criteria

- [ ] `Gain(amount)` 호출로 경험치가 누적된다
- [ ] 누적치가 현재 레벨 필요치를 넘으면 초과분이 이월되며 레벨이 오른다
- [ ] 한 번의 `Gain`으로 여러 레벨이 오르는 경우가 정상 처리되고, `LevelUpEvent`가 레벨마다 1회씩 발행된다
- [ ] 레벨별 필요 경험치가 `AnimationCurve`로 주입되고 Module에 하드코딩이 없다
- [ ] 현재 경험치·현재 레벨이 `ReactiveProperty`로 노출되어 구독 가능하다
- [ ] 레벨 상승 시 `LevelUpEvent`가 발행되고 payload에 결과 정보(새 레벨 등)만 담긴다
- [ ] `ExperienceModule`이 Unity API·Collider·UI·일시정지·구체 적/아이템 타입을 참조하지 않는다
- [ ] `ExperienceModule`이 `new`로 생성 가능한 순수 C# 클래스다
- [ ] R3 구독이 `AddTo`로 lifecycle에 바인딩된다

## Open Questions

- (해결됨) 배치 위치 → **`10_ProjectA` 전용**
- (해결됨) 다중 레벨 상승 시 발행 → **레벨마다 1회씩**
- (해결됨) 데이터 형식 → **`AnimationCurve`로 시작**
- (해결됨) 최대 레벨(cap) → **이번 범위 제외** (무한 상승 가정)
