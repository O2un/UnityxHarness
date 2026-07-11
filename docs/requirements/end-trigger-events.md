# End Trigger Events

playable-game-flow 분해 **2/5**. 상위: [playable-game-flow.md](playable-game-flow.md) · 선행: [wave-logic-relocation.md](wave-logic-relocation.md)

## 목표

`GameManager`가 구독할 종료·집계 이벤트를 노출한다. 상태 전환 배선은 3단계에서 처리.

## 대상

- **플레이어 사망**: `PlayerHealthAdapter`에 `OnDeath`(`Observable<Unit>`) 추가. `CurrentHP`가 0에 도달하는 시점을 어댑터 내부에서 구독·판정해 발행.
- **웨이브 완료 판정**: 스폰 타임라인 소진(`_nextIndex >= _timeline.Count` 등) + 활성 적 수 0을 완료로 보는 판정을 `EnemySpawnManager`/`WaveModule` 흐름 위에 추가하고 완료를 이벤트/프로퍼티로 노출.
- **킬 카운트 소스**: `EnemyHealth.OnDeath`는 이미 존재. 스폰/풀링 시점에 구독 가능하도록, 스폰 콜백과 `Dispose` 시 해제 지점을 확정 (실제 집계는 3단계 `GameManager`).

## 범위 밖

- `GameManager`의 상태 전환·킬 누적 로직 (3단계)
- 웨이브 데이터 구조 변경

## 완료 기준

- [ ] `PlayerHealthAdapter.OnDeath`가 HP 0 도달 시 발행
- [ ] 웨이브 완료(스폰 소진 + 활성 적 0)가 이벤트/프로퍼티로 노출
- [ ] 스폰된 `EnemyHealth`의 `OnDeath` 구독·해제 지점 확정
- [ ] 컴파일 0

## Open

- 웨이브 "마지막 웨이브 완료"와 "각 웨이브 완료" 구분 노출 방식 → 구현 시 확정
