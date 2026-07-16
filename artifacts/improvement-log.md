# 개선 기록 (improvement-log)

## 2026-07-17 · Project B 근접 3단 콤보 (melee-combo PRD)

### 구현

- 신규 5파일 (`Assets/20_ProejctB/01_Scripts/Player/Combat/`): `MeleeComboData`(SO — 버퍼 시간·단계별 데미지/히트박스 크기·오프셋), `MeleeComboModule`(순수 C# — 단계·윈도우·버퍼 판단, Tick 외부 주입), `MeleeAttackView`(Collider On/Off·TryHit), `MeleeAnimationEventBridge`(AnimEvent 5종 → C# event), `MeleeComboRefs`(직렬화 묶음).
- 기존 수정: `Player2DActor`(발동 어댑터 — 단계별 HitboxModule 3개 소유, OnHit 구독, 점프 캔슬), `Player2DContext`(SerializeField → 생성자 전달).
- 공통 Input 확장 (사용자 승인): `IInputReader.IsAttackPressed`, `PlayerInputModule.OnAttack` 구현, `InputManager` 노출.
- 공통 Combat 코어(HitboxModule/IDamageable/DamageEvent) 무수정 재사용.

### 사용자 확정 결정

- Animation Event 5종 (PRD 4종 + `AttackEnd` 복귀 판정용).
- SO에 윈도우 시간 필드 제외 (진실은 클립 이벤트 배치).
- 대시 캔슬은 추후 (`Cancel()` 공용 API로 확장 지점 확보), 점프 캔슬만 실장.
- 씬·프리팹·Animator·Animation Event 설정은 사용자 직접 수행 (게이트 B — 코드만 승인).

### 검증 및 리뷰

- Gate 1 컴파일: 통과 (리뷰 수정 후 재확인 포함).
- Gate 2~3: 사용자 씬 설정 후 진행 대기.
- 리뷰: blocker 0 / major 1(빈 스테이지 IndexOutOfRange → `PressAttack` 방어 수정 완료) / minor 3(기록만).

### 다음 확인

- 사용자 씬 설정 완료 후: SO 에셋 생성 → 프리팹(MeleeHitbox 자식 + View, Animator 오브젝트에 Bridge) → Animator 파라미터(Attack/AttackStage/AttackCancel) → 클립 3개에 AnimEvent 5종 배치 → Play 검증(Gate 2~3).
- 기능테스트 시나리오에 "빠른 연속 전이 중 히트박스 켜짐 유지"(리뷰 m1) 포함.
- 대시 구현 시 점프와 동일 지점에 `Cancel()` 연결 한 줄 추가.
