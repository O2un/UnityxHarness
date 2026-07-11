# 개선 기록 (improvement-log)

## 2026-07-11 · skill-upgrade-content 구현

### 구현 내용

- 기존 레벨업 UI 흐름은 유지하고 `LevelUpSelectionModule`의 후보 산출만 단계별 카드셋 기반으로 확장했다.
- `LevelUpSkillCardSetSO` 4개와 배열형 `LevelUpSkillCard` 36개를 `Assets/10_ProjectA/03_Data/SkillUpgradeCards/`에 구성했다.
- 카드에는 카드 ID, 분기 ID, 부모 카드 ID, 스킬 참조 및 모든 스탯 델타를 저장한다.
- 선택 결과의 카드 ID를 `SkillStats`에 보관하고, 다음 레벨 카드셋에서 부모 ID가 같은 카드만 후보로 반환한다.
- 투사체 다발 발사·관통, 오라 SphereCollider 반경, 근접 최대 타격 수를 기존 `AttackRequest`/`HitboxModule` 경로에 배선했다.

### 검증

- 컴파일: Unity MCP에서 컴파일·도메인 리로드 완료 상태와 콘솔 오류 0건을 확인했다.
- 데이터 점검: 레벨별 9장 × 4세트 = 36장, 분기 ID 36개 입력, 풀 에셋의 카드셋 참조 4개 확인.
- `git diff --check` 통과.
- Unity Play·런타임 콘솔·사용자 기능 확인은 대기 상태다.

### 다음 실행 규칙

- Unity 에디터에서 스크립트 재임포트 후 콘솔 오류 0을 확인한다.
- Lv2에서 각 스킬의 세 분기가 표시되는지, 선택 후 Lv3~Lv5에서 같은 분기 한 장만 이어지는지 확인한다.
- 투사체 다발/관통, 오라 반경, 근접 다중 타격을 Play 모드에서 실측한다.
