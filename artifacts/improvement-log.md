# 개선 기록 (improvement-log)

이 파일은 **가장 최근 실행 1건**만 담는 단일 스냅샷이다. 다음 실행의 Phase 0-A가 그대로 읽어 연속 작업 기준으로 삼는다. 새 실행이 끝나면 통째로 덮어쓴다(이전 내용은 덮어쓰기 전에 chain-log.md로 축약 누적).

---

## 2026-07-02 · 하네스 초기 구성

### 무엇을 했나
- `/unity-dev-harness`로 3D 뱀서 MVP용 개발 하네스를 신규 구성.
- Agent 4종(unity-architect, gameplay-engineer, code-reviewer, unity-ai-operator), Skill 2종(unity-dev-orchestrator, csharp-convention-guide) 생성.
- hooks 자산 복사(.claude/hooks), settings.local.json에 Stop hook 등록(unity-validate → gate3-test-runner → open-viewer), CLAUDE.md에 하네스 포인터 추가.

### 아쉬웠던 점 / 원인
- 아직 게임플레이 코드는 0줄. 하네스만 구성한 상태라 4단계 검증을 실제로 돌려본 적 없음.

### 반영
- 코드 배치 위치(공통/프로젝트)는 오픈 퀘스천으로 두고 orchestrator 게이트 A에서 매번 사용자 확인하도록 설계.

### 다음 테스트 (다음 실행 입력)
- **첫 orchestrator 실행: 플레이어 이동(Topdown3D)** 구현.
  - game-plan 개발 순서 1번, 나머지의 기반.
  - 기존 PlayerActor/PlayerMover/PlayerContext 재사용 여부를 unity-architect가 설계에서 판단.
  - 이때 4단계 게이트(Stop hook 자동 ①~③ + 사용자 ④)와 배치 위치 게이트 A를 처음으로 실검증.

### 하네스 자체 개선 메모
- open-viewer.sh는 bash 전용(process substitution)이라 settings에서 `bash`로 호출. Unity 실행·뷰어 오픈 확인 완료.
- `Gate3TestTool.cs`를 `Assets/00_CommonFramework/99_Dev/Editor/`로 이동 → 컴파일 성공, MCP에 `gate3_run_test` 툴 등록 확인. Gate 3 동작 가능. 남은 것은 `.cs` 작성 시 `.claude/hooks/.viewer-state/gate3-test.json` 갱신뿐.
- `enabledMcpjsonServers`는 `unity-mcp`(소문자)지만 실제 MCP 서버는 `UnityMCP`로 정상 연결·동작 확인됨(도구 호출·컴파일 성공). 이름 표기 차이는 실사용에 문제 없음.
