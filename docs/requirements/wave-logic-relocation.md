# Wave Logic Relocation

playable-game-flow 분해 **1/5**. 상위: [playable-game-flow.md](playable-game-flow.md)

## 목표

`Assets/00_CommonFramework/00_Scripts/Manager/EnemySpawner/`의 웨이브 로직을 `10_ProjectA`로 이동. 구조·동작 변경 없이 위치와 DI 등록만 옮긴다.

## 대상

- `WaveModule`, `EnemySpawnManager`, `WaveDataSO`, `SpawnRequest`, `AnnulusSampler`, `GaussianSampler`
- DI 등록(`IAsyncStartable`/`ITickable`, `WaveDataSO`)을 프로젝트 A LifetimeScope로 이동
- `.meta` 함께 이동해 GUID 유지 (`WaveData_Test.asset` 참조 안 깨지게)

## 범위 밖

- 웨이브 완료 판정·킬 카운트·상태 전환 (2~5단계)
- 내부 로직·데이터 구조 변경

## 완료 기준

- [ ] 대상 파일이 `10_ProjectA`로 이동됨
- [ ] DI 등록이 프로젝트 A 스코프로 이동됨
- [ ] 컴파일 0, 기존 스폰 동작 유지
- [ ] `WaveData_Test.asset` 참조 유지

## Open

- `AnnulusSampler`/`GaussianSampler`가 공통에서 참조되면 잔류 (구현 시 grep 확인)
