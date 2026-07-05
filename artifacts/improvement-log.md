# 개선 기록 (improvement-log)

이 파일은 **가장 최근 실행 1건**만 담는 단일 스냅샷이다. 다음 실행의 Phase 0-A가 그대로 읽어 연속 작업 기준으로 삼는다. 새 실행이 끝나면 통째로 덮어쓴다(이전 내용은 덮어쓰기 전에 chain-log.md로 축약 누적).

---

## 2026-07-05 · 캐릭터 이동(CharacterMover) 구현

### 무엇을 했나
- `docs/requirements/character-movement.md` 구현. 설계(unity-architect) → 게이트 A(배치) → 구현(gameplay-engineer) → 리뷰(code-reviewer) → 게이트 B(씬/프리팹) → 4단계 검증까지 완주.
- 신규(모두 `00_CommonFramework/00_Scripts/`): `Actor/{MoveStats, IMoveDirectionProvider, CharacterMover, CameraRelativeMoveModule, ActorView}.cs`, `Manager/Camera/ICameraBasisProvider.cs`. 수정: `CameraManager`(ICameraBasisProvider 구현), `PlayerActor`, `PlayerContext`, `GameSceneScope`(DI 등록). 삭제: `PlayerMover.cs`, `PlayerView.cs`(+meta).
- 게이트 A: **전부 00_CommonFramework**(사용자 확정). 게이트 B: PlayerActor.prefab에 ActorView+CharacterController(r0.3/h2/center y1), _stats=속도5/회전720, autoInject에 PlayerContext 보정.
- 4단계: ①컴파일 ✅ ②Play 콘솔 ✅ ③기능(라이브 execute_code + gate3 스모크) ✅ / ④사용자 육안 대기.

### 아쉬웠던 점 / 원인
- **MCP 도구가 세션에 안 떴다**: `.mcp.json` 서버키 `UnityMCP` ↔ `settings.local.json`의 `enabledMcpjsonServers` `unity-mcp` 불일치. 서버는 떠 있어도 CC 세션 도구목록에 미등록. 초기에 unity-ai-operator가 MCP 도구 접근 실패로 두 번 헛돌았다.
- **프리팹 저장 거부**: PlayerView.cs 삭제로 프리팹 루트에 missing script가 남아 "Prefab with a missing script" 에러로 저장 실패. 사용자가 원인을 먼저 지적.
- **주입 누락 위험**: autoInjectGameObjects가 [NULL, NULL, HUD]로 PlayerContext가 빠져 있었다(그대로면 이동 안 됨).

### 반영
- `enabledMcpjsonServers`를 `UnityMCP`로 수정(세션 재시작 후 도구 로드됨).
- 스크립트 삭제→프리팹 참조가 남는 경우 `GameObjectUtility.RemoveMonoBehavioursWithMissingScript`를 프리팹 스테이지에서 선행 실행 후 저장.
- autoInjectGameObjects의 NULL 제거 + 대상 Context 추가를 게이트 B 체크리스트에 포함.

### 다음 테스트 (다음 실행 입력)
- **game-plan 개발 순서 2번: 적 스폰 + 추격 AI**.
  - 이동 재사용 검증 포인트: `IMoveDirectionProvider`를 `ChaseDirectionProvider`(자기→타깃 방향)로 교체, `CharacterMover`/`ActorView`는 무수정 재사용, 적 `MoveStats`만 다르게 주입.
  - 몬스터 스탯 스키마 설계 시 `MoveStats`를 값으로 임베드(설계 §5 결정 반영).

### 하네스 자체 개선 메모
- MCP 서버명 표기 일치는 필수(이전 로그의 "표기 차이 문제없음"은 틀렸음 — 도구 미로딩 원인이었다).
- Unity 에디터가 포커스를 잃으면 MCP ping 무응답으로 멈출 수 있다(메인 스레드 틱 정지). 배선 중 무응답 시 에디터 창 포커스 요청.
- `gate3-test.json`은 `O2un.Actors.ActorView` 인스턴스화 스모크로 설정(주입/필드체크 없음 — Bind가 런타임이라 필드 non-null 체크는 false-fail).
