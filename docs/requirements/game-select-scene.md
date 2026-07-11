# Game Select Scene

playable-game-flow 분해 **5/5**. 상위: [playable-game-flow.md](playable-game-flow.md) · 선행: [hud-result-panels.md](hud-result-panels.md)

## 목표

`00_CommonFramework` 공통 씬 UI로 게임 선택 진입점을 만든다. 두 프로젝트가 공유하는 진입점이므로 공통 스코프.

## 대상

- **게임 목록**: 프로젝트 A(선택 가능), 프로젝트 B(미구현 → 버튼 비활성화 표시).
- **씬 전환**: A 선택 → 프로젝트 A 게임 씬으로 전환.
- **복귀 연동**: 결과 패널(4단계)의 "게임 선택 씬으로" 버튼이 이 씬으로 돌아온다.

## 범위 밖

- 프로젝트 B 콘텐츠
- 세이브/로드

## 완료 기준

- [ ] 게임 선택 씬이 `00_CommonFramework`에 위치
- [ ] A 선택 → 프로젝트 A 게임 씬 전환
- [ ] B 버튼 비활성화 표시
- [ ] 결과 패널에서 이 씬으로 복귀 가능
- [ ] 컴파일 0

## Open

- 구체 폴더 위치·씬 로딩 방식(AssetService/씬 이름) → 구현 시 확정
