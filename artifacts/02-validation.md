# 02. 검증 (4단계 게이트)

작성: unity-validate hook (CoplayDev MCP for Unity 자동 검증)
일시: 2026-07-09 23:39

## 게이트 진행 요약
| 단계 | 결과 | 시간 |
| --- | --- | --- |
| 1. 컴파일 | ✅ 통과 | 23:39 |
| 2. 런타임 에러 | ✅ 통과 | 23:39 |
| 3. 기능 점검 (자동) | ✅ 통과 | 23:39 |
| 4. 기능 점검 (사용자) | ⏳ 대기 (hooks 뷰어에서 제출 예정) | - |

## 자동 검증 결과
- 1단계 컴파일: refresh_unity(force/compile) + editor/state 대기 통과. 컴파일 에러 0.
- 2단계 런타임 에러: Play 모드 진입 후 콘솔 에러·예외 없음.
- 3단계 상세: PASS: O2un.Actors.ActorView OK

---

## 게이트③ 수동 정량 실측 (orchestrator, `execute_code` 직접 조작) — LevelUpSelection(PRD 3)

Stop hook 자동 검증(위)과 별개로, 씬·에셋 배선(게이트B) 직후 orchestrator가 DI 컨테이너를 직접 조작해 기능을 실측했다.

### 1차 실측에서 버그 발견 → 원인 규명 → 수정 → 재실측
- `IExperienceWriter.Gain(100000)`으로 672레벨 강제 상승 → `LevelUpSelectionPanel`이 뜨지 않고 `Time.timeScale`도 1 그대로임을 확인.
- 원인: `LevelUpSelectionContext`가 씬에 배치돼 있었지만 `GameSceneScope.autoInjectGameObjects` 배열(VContainer가 씬 MonoBehaviour에 `[Inject]`를 자동 호출하는 목록)에 등록되지 않아 **`Init`이 한 번도 호출되지 않음** — `_pool`이 null인 상태로 방치됨(unity-ai-operator가 게이트B 배선 시 GameObject·컴포넌트 배치는 했지만 이 리스트 등록을 누락).
- `SerializedObject`/`SerializedProperty` API로 `autoInjectGameObjects`에 `LevelUpSelectionContext` GameObject(fileID 1535419201)를 추가하고 `EditorSceneManager.SaveScene`으로 저장.

### 재실측 (수정 후)
1. **단일 레벨업**: `Gain(20)` → `_pool` 정상 주입 확인 → `Time.timeScale=0`, `LevelUpSelectionPanel.activeSelf=true`, 후보 3개(`MeleeSwingSkill_Lv2, ProjectileSkill, MeleeSwingSkill`) 정상 표시.
2. **선택 적용**: `vm.ChooseCandidate(0)`(`MeleeSwingSkill_Lv2` 선택) → `Time.timeScale=1`, 패널 비활성화. `PlayerActor._skills`의 `SkillSlot` 목록 확인 → `melee_swing Lv2`로 정확히 **덮어쓰기**됨(스택 병합 아님).
3. **다중 레벨업 큐잉**: `Gain(500)` → `_pendingCount=8`(8레벨 동시 상승) → 정지 유지 확인 → `ChooseCandidate(0)`를 8회 반복 호출 → `pendingCount`가 `8→7→...→1→0`으로 정확히 감소하며 매번 새 후보 3개 갱신, 마지막 선택 후 `Time.timeScale=1`·패널 비활성화로 정상 재개.
4. Play 세션 전체에서 `read_console(types=error)` 결과 이번 변경 관련 에러 0건.

## 발견된 기존 버그 (이번 PRD 범위 밖, 별도 보고 필요)
- `Assets/00_CommonFramework/00_Scripts/Combat/Hitbox/AttackHitboxView.cs:71` — Play 모드 종료 시 다수의 `NullReferenceException`. `OnDespawned()`가 `_hitbox = null` 처리 후 `Update()`의 `_active` 체크와 `_hitbox.Tick(dt)` 호출 사이에 레이스가 있는 것으로 추정. 이번 세션의 변경과 무관 — LevelUpSelection이 플레이어의 기존 스킬을 자동 발사시키며 히트박스 풀링이 우연히 트리거된 것. **후속 조치 필요, 이번 PRD 완료 처리에는 영향 없음.**

## 게이트 B 배선 내역
- `Assets/10_ProjectA/03_Data/LevelUpSkillPool.asset`(신규) — `MeleeSwingSkill(Lv1)`, `MeleeSwingSkill_Lv2`, `ProjectileSkill(Lv1)`, `ProjectileSkill_Lv2`, `AuraFieldSkill(Lv1)` 5종 등록.
- 기존 3개 스킬 SO에 `_skillId`/`_level` 값 채움 + "동일 스킬 상위 레벨" 테스트용 복제본 2개(`MeleeSwingSkill_Lv2`, `ProjectileSkill_Lv2`) 신규 생성.
- `Assets/Scenes/GameScene.unity` — 기존 `Canvas` 하위 `LevelUpSelectionPanel`(+버튼 3개) 배치, `LevelUpSelectionContext` GameObject 배치 및 `GameSceneScope.autoInjectGameObjects` 등록(누락분 수정 포함), `_levelUpSkillPool` 인스펙터 배선.

## 게이트④ 리뷰 (code-reviewer)
- blocker 0 / major 0 / minor 2.
- M1: `LevelUpSkillPoolSO.Candidates`가 `_candidates` 배열이 null일 때 방어 없음(`?? Array.Empty<SkillDefinitionSO>()` 권장).
- M2: `LevelUpSelectionView`의 버튼·라벨 배열 길이 불일치 시 `IndexOutOfRangeException` 위험(길이 가드 권장).
- 설계(§0/§2/§7/§10)와 구현 1:1 일치, 컨벤션·의존 방향 위반 없음. 상세는 `03-review.md` 참고.
