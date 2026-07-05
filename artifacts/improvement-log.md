# 개선 기록 (improvement-log)

이 파일은 **가장 최근 실행 1건**만 담는 단일 스냅샷이다. 다음 실행의 Phase 0-A가 그대로 읽어 연속 작업 기준으로 삼는다. 새 실행이 끝나면 통째로 덮어쓴다(이전 내용은 덮어쓰기 전에 chain-log.md로 축약 누적).

---

## 2026-07-05 · PoolManager(오브젝트 풀링 인프라) 구현

### 무엇을 했나
- `docs/requirements/pool-manager.md`(PRD) 구현. 설계(unity-architect) → 게이트 A(배치, PRD 확정으로 skip) → 구현(gameplay-engineer) → 검증(unity-ai-operator, 게이트 1~3) → 리뷰(code-reviewer)까지 완주. 게이트 4(사용자 육안)·실적용 스모크(게이트 B)는 대기.
- 신규(모두 `00_CommonFramework/00_Scripts/Manager/PoolManager/`): `IPoolHandle.cs`, `IPoolService.cs`, `PoolModule.cs`, `PoolManager.cs`. 수정: `DI/GameSceneScope.cs`에 `builder.Register<PoolManager>(Lifetime.Singleton).As<IPoolService>();` 1줄.
- 구조: `PoolManager`(Manager, 순수 C#, `IPoolService`+`IDisposable`) → `PoolModule<T>`(Module, `UnityEngine.Pool.ObjectPool<T>` 래핑, `IPoolHandle<T>`+`IDisposable`). string키 `Dictionary<string,object>` 박싱 보관. create 콜백은 `resolver.Instantiate(prefab)`로 DI 유지. get/release는 SetActive 토글, destroy는 `Object.Destroy`.
- 결정: 등록은 명시적 `Register(key,prefab)`(eager 등록, 인스턴스는 ObjectPool 기본 lazy). 중복 Register 경고 후 무시, 미등록 키 `KeyNotFoundException`, 타입 불일치 `InvalidCastException`. `As<IPoolService>()`만 노출(`AsImplementedInterfaces` 금지 — `IPoolHandle`/`PoolModule` 컨테이너 미노출).

### 4단계 게이트
- ①컴파일 ✅ (read_console 에러 0, 신규 4파일 validate_script standard diagnostics 0)
- ②런타임 ✅ (라이브 스모크 중 예외 0)
- ③기능 ✅ (execute_code 라이브: Get→Release→re-Get 동일 InstanceID **재사용 확인**, activeSelf True→False→True, 타입불일치/미등록 예외 OK, 중복Register 무시 OK)
- ④사용자 ⏳ + PRD AC 마지막(Enemy/Projectile/Effect 실적용)은 프리팹 필요 → **게이트 B 승인 대기**

### 리뷰
- blocker 0 / major 0 / minor 2(비차단, 코드 변경 불요). `PoolModule`의 Unity API 사용은 PRD 명시 예외로 확인.

### 아쉬웠던 점 / 원인 → 반영
- **MCP 도구는 세션 시작 시점에만 로드된다.** 브리지(8080)를 세션 중간에 켜도 `mcp__UnityMCP__*`가 안 뜸 → 세션 재시작으로 해제. (다음엔 시작 전에 Unity+브리지 Connected 먼저 확인.)
- **execute_code는 Roslyn 미설치 → codedom(C# 6) 폴백.** 폴백 시 (1) 코드 최상단 `using` 금지(메서드 바디로 실행) → 전부 FQN 사용, (2) 확장메서드(VContainer `Register<T>(Lifetime)`/`Resolve<T>()`)가 `using` 없이 미해석 → 컨테이너 확장 API 대신 대상(`new PoolManager(resolver)`) 직접 생성으로 스모크. `var`는 사용 가능.
- **에디트 모드 스모크에서 `Destroy may not be called from edit mode` 콘솔 1건**은 코드 결함 아님(런타임 Play에선 `Object.Destroy`가 정확, `DestroyImmediate`가 오히려 부적절). 검증 문서에 비이슈로 명기.

### 다음 테스트 (다음 실행 입력)
- **game-plan 개발 순서 2번: 적 스폰 + 추격 AI**(이전 실행에서 이월).
  - 재사용: `IMoveDirectionProvider`를 `ChaseDirectionProvider`(자기→타깃)로 교체, `CharacterMover`/`ActorView` 무수정 재사용, 적 `MoveStats`만 다르게 주입.
  - **PoolManager 실적용 기회**: 적 스폰을 `IPoolService`로 풀링 → PRD AC 마지막 항목(Enemy 실적용 스모크) 충족. Enemy 프리팹 필요 → 게이트 B.

### 하네스 자체 개선 메모
- PRD에 배치 위치가 명시 확정돼 있으면 게이트 A는 skip 가능(사용자에게 재확인 불필요). 이번엔 PRD Technical Requirements가 `00_CommonFramework` 확정 → 바로 진행.
- 서브에이전트(code-reviewer)는 Read/Grep/Glob만 있어 파일 쓰기 불가 → 산출물(`03-review.md`)은 orchestrator가 저장. gameplay-engineer(Write 보유)와 역할 구분.
