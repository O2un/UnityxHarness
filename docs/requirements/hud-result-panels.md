# HUD & Result Panels

playable-game-flow 분해 **4/5**. 상위: [playable-game-flow.md](playable-game-flow.md) · 선행: [game-state-flow.md](game-state-flow.md)

## 목표

시작·HUD·클리어·게임오버 패널이 `GameManager.CurrentState`를 구독해 자동 전환된다. 수동 호출 없음.

## 대상

- **패널 전환**: `CurrentState`(`ReadOnlyReactiveProperty<GameState>`) 구독으로 Idle/Playing/Victory/Defeat별 패널 표시.
- **시작 패널**: 시작 버튼 → `GameManager.StartGame()`.
- **HUD**: Playing 중 표시 (체력·레벨 등 기존 요소 연동).
- **결과 패널(Victory/Defeat)**: 도달 웨이브·레벨·처치 수 표시. "재시작"·"게임 선택 씬으로" 버튼.

## 범위 밖

- 상태·결과 데이터 산출 (3단계)
- 게임 선택 씬 자체 (5단계) — 버튼은 씬 전환 호출만

## 완료 기준

- [ ] 시작·HUD·클리어·게임오버 패널이 `GameState` 구독만으로 전환 (수동 호출 없음)
- [ ] 시작 버튼 → `Playing`
- [ ] 결과 패널에 웨이브·레벨·킬 표시
- [ ] 재시작·게임 선택 씬 복귀 버튼 동작
- [ ] 컴파일 0

## Open

- HUD에 표시할 기존 요소 범위 → 구현 시 확정
