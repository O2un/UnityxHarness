# Game State Flow

playable-game-flow 분해 **3/5**. 상위: [playable-game-flow.md](playable-game-flow.md) · 선행: [end-trigger-events.md](end-trigger-events.md)

## 목표

`GameManager`가 2단계 이벤트를 구독해 상태 전환·웨이브 순차 진행·킬 집계·재시작을 배선한다. `GameState`(Idle/Playing/Paused/Victory/Defeat)는 공통 스코프 유지.

## 대상

- **상태 전환**: 시작 → `Playing`. 마지막 웨이브 완료 → `Victory`. `PlayerHealthAdapter.OnDeath` → `EndGame(false)`(`Defeat`).
- **웨이브 순차 진행**: 각 웨이브 완료 조건 충족 시 다음 웨이브 진행, 최소 5웨이브.
- **킬 집계**: 스폰된 `EnemyHealth.OnDeath` 구독해 처치 수 누적. 별도 서비스/이벤트 버스 추가 안 함.
- **결과 데이터**: 도달 웨이브·레벨·처치 수를 4단계 UI가 읽을 수 있게 노출.
- **재시작**: 웨이브·스킬 슬롯·단계·경험치·레벨·UI 리셋 후 `Idle` 복귀. 각 시스템에 리셋 메서드 추가.

## 범위 밖

- UI 패널 구현 (4단계)
- 게임 선택 씬 (5단계)

## 완료 기준

- [ ] 시작 → `Playing`, 첫 웨이브 시작
- [ ] 마지막 웨이브 완료 → `Victory`, 플레이어 HP 0 → `Defeat`
- [ ] 각 웨이브 완료 조건으로 순차 진행 (5웨이브)
- [ ] 킬 카운트 누적, 결과 데이터(웨이브·레벨·킬) 노출
- [ ] 재시작 시 전 시스템 리셋 + `Idle` 복귀
- [ ] 컴파일 0

## Open

- 각 시스템 리셋 메서드 시그니처 → 구현 시 확정
