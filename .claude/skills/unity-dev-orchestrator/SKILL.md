---
name: unity-dev-orchestrator
description: Unity 게임 개발 작업 전체 흐름을 묶는 입구 Skill. "기능 만들어줘", "이 시스템 구현해줘", "플레이어 이동 만들어줘", "버그 고쳐줘", "리팩토링해줘"와 재실행·기능추가·부분수정 요청에서 사용한다. 설계 → 구현 → 검증 → 리뷰를 Agent Team으로 엮고, 산출물을 이어받고, 4단계 검증과 사람 승인을 관리한다. 단순한 C# 문법 질문이나 하네스와 무관한 단발성 편집에는 쓰지 않는다.
---

# Unity Dev Orchestrator

## 역할
뱀서 MVP(및 이후 기능)의 개발을 **Agent Team · Pipeline**(설계 → 구현 → 에디터조작 → 리뷰)으로 진행한다. 산출물을 이어받고, 4단계 검증 게이트와 사람 승인 지점을 관리한다.

## 팀 구성 (기본 4명)
- `unity-architect` — 설계 (`artifacts/01-design.md`)
- `gameplay-engineer` — 구현 (`Assets/**`)
- `unity-ai-operator` — MCP 컴파일·씬·테스트 (MCP for Unity 연결됨)
- `code-reviewer` — 점진 리뷰 (`artifacts/03-review.md`)
- (버그 발생 시) `debugger`를 팀에 추가

## 실행 모드 확인 (먼저)
1. `artifacts/`를 확인한다.
   - `improvement-log.md`를 읽어 직전 회고·다음 테스트를 파악한다(있으면).
   - 산출물이 없으면 **초기 실행**, 일부만 있으면 **부분 재실행**, 새 요청이면 **새 실행**.
2. 후속 키워드 분기: "다시"(재실행), "기능 추가"(기존 구조 읽기 우선), "이 버그만"(debugger 중심), "이전 설계 기반"(01-design 재사용), "리팩토링"(code-reviewer+architect 중심, 테스트로 회귀 방지).

## 진행 (Pipeline)
1. 요청·게임 범위·제약·승인 지점을 `artifacts/00-input.md`에 정리(있으면 갱신).
2. `TeamCreate`로 위 4명 팀 구성. `TaskCreate`로 시스템별 설계/구현/검증/리뷰 Task 등록(담당자·입력 파일·출력 위치·의존·완료 기준 명시). 시스템 순서는 game-plan 개발 순서(이동 → 스폰+추격AI → 체력·피격 → 자동공격 → 경험치·레벨업 → 게임오버)를 따른다.
3. **설계**: unity-architect → `artifacts/01-design.md`. 여기에 **배치 위치 후보(공통/프로젝트)**만 적고 확정하지 않는다.
4. **[사람 승인 게이트 A — 배치 위치]** 구현 착수 직전, 이 시스템을 `00_CommonFramework`(공통 재사용)로 둘지 `10_ProjectA`(프로젝트 전용)로 둘지 **사용자에게 확인**한다. 답을 받기 전 구현 시작 금지.
5. **구현**: gameplay-engineer → `Assets/`의 확정된 위치. `csharp-convention-guide` 준수.
6. **씬·에셋** 필요 시: unity-ai-operator에 위임. **[사람 승인 게이트 B — 씬·에셋]** 변경 요약을 보여주고 승인받은 뒤 실행. 대규모 변경 전 커밋/백업 권고.
7. **4단계 검증**(각 게이트, 막히면 다음으로 넘어가지 않고 즉시 보고 → 수정 → 해당 단계 재검증). MCP 연결이므로 1~3은 Stop hook이 자동 수행:
   1. 컴파일 에러 (Gate 1, `unity-validate.sh`)
   2. Play 모드 콘솔 에러 (Gate 2, `unity-validate.sh`)
   3. 기능 테스트 (Gate 3, `gate3-test-runner.sh` + `.claude/hooks/.viewer-state/gate3-test.json`) — `.cs` 생성·수정 시 `gate3-test.json`도 갱신.
   4. 사용자 확인 (viewer 피드백 → `artifacts/04-user-feedback.md`)
   - 결과는 `artifacts/02-validation.md`에 게이트 표로 기록된다.
8. **리뷰**: code-reviewer가 시스템 단위로 점진 검토 → `artifacts/03-review.md`. blocker/major는 gameplay-engineer에 수정 요청.
9. 통합: orchestrator가 산출물을 읽어 누락·충돌·승인 대기 정리.
10. `TeamDelete`로 팀 정리.
11. **마무리**: `artifacts/improvement-log.md`를 이번 실행 기준으로 **덮어쓰기**(최신 1건). 덮어쓰기 전 이전 내용을 `artifacts/chain-log.md`에 축약본으로 append(없으면 헤더와 함께 생성).
12. Phase 8: Stop hook이 `artifacts/`를 viewer로 표시하고, 4단계 사용자 피드백을 제출받는다.

## 산출물 계약
| 단계 | 위치 | 만드는 역할 | 다음에 읽는 역할 |
| --- | --- | --- | --- |
| 입력 정리 | `artifacts/00-input.md` | orchestrator | 전원 |
| 설계 | `artifacts/01-design.md` | unity-architect | gameplay-engineer, code-reviewer |
| 구현 | `Assets/00_CommonFramework/**` 또는 `Assets/10_ProjectA/**` | gameplay-engineer | unity-ai-operator, code-reviewer |
| 검증 | `artifacts/02-validation.md` | Stop hook / unity-ai-operator | orchestrator, 사용자 |
| 리뷰 | `artifacts/03-review.md` | code-reviewer | orchestrator |
| 사용자 피드백 | `artifacts/04-user-feedback.md` | 사용자(viewer) | orchestrator |
| 개선 기록 | `artifacts/improvement-log.md` | orchestrator | 다음 실행 Phase 0-A |
| 체인 로그 | `artifacts/chain-log.md` | orchestrator | 필요 시 과거 참조 |

## 사람 승인 지점
- **게이트 A**: 각 시스템 배치 위치(공통/프로젝트) 확정.
- **게이트 B**: 씬·프리팹·에셋 생성/변경/삭제, 대규모 리팩토링. 대규모 변경 전 커밋/백업.

## 실패 처리
- 컴파일/런타임/기능 게이트 실패 → 해당 단계에서 멈추고 보고 → (필요 시 debugger 추가) 원인 분석 → gameplay-engineer 수정 → 같은 단계 1회 재검증.
- 팀원 멈춤 → `TaskGet` 확인 → `SendMessage`로 원인 확인 → 1회 재시도.
- 설계-구현 충돌 → 한쪽을 지우지 않고 근거 남긴 뒤 orchestrator 판단.
- 입력 부족 → 추측 금지, 질문 목록 작성 후 멈춤.

## 금지
- Agent 파일만 두고 `TeamCreate` 흐름 없이 진행하지 않는다.
- C# 코드를 `artifacts/`에 두지 않는다(Unity 미컴파일).
- 컴파일·플레이 검증 없이 완료 처리하지 않는다.
- 사람 승인 없이 씬·에셋을 변경하거나 배치 위치를 임의 확정하지 않는다.
