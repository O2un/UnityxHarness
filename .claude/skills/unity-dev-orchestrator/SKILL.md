---
name: unity-dev-orchestrator
description: 새 Unity 게임 시스템, 여러 시스템·레이어를 가로지르는 변경, 다수 파일 신설, 씬·프리팹·에셋 작업처럼 설계와 단계별 승인이 필요한 큰 개발 작업에서 사용한다. 기존 구조 안의 1~2개 파일 국소 버그 수정·리팩토링·부분 기능 추가, 질문·문서·설정·하네스 수정에는 사용하지 않는다. 설계 → 구현 → 검증 → 리뷰를 필요한 Agent만 선택해 엮고 산출물과 승인 지점을 관리한다.
---

# Unity Dev Orchestrator

## 역할
큰 Unity 개발 작업을 **Agent Team · Pipeline**(설계 → 구현 → 에디터조작 → 리뷰)으로 진행한다. 산출물을 이어받고, 4단계 검증 게이트와 사람 승인 지점을 관리한다.

## 진입 조건
- 다음 중 하나가 명확할 때 진입한다: 새 독립 시스템 구축, 여러 시스템/레이어 변경, 다수 파일 신설, 씬·프리팹·에셋 변경, 사용자 요청에 의한 전체 파이프라인 실행.
- 기존 구조 안의 국소 작업은 호출 문구가 "버그 수정", "리팩토링", "기능 추가"여도 진입하지 않는다.
- 진입 후 조사에서 실제 범위가 1~2개 파일의 국소 수정으로 확인되면 전체 파이프라인을 만들지 말고 현재 대화의 직접 처리로 되돌린다.

## 팀 구성 (필요한 역할만 선택)
- `unity-architect` — 새 구조나 시스템 경계 설계가 필요할 때 (`artifacts/01-design.md`)
- `gameplay-engineer` — C# 구현이 필요할 때 (`Assets/**`)
- `unity-ai-operator` — MCP 컴파일·씬·에셋·Play 테스트가 필요할 때
- `code-reviewer` — 여러 시스템에 영향이 있거나 회귀 위험이 큰 변경을 검토할 때 (`artifacts/03-review.md`)
- (버그 발생 시) `debugger`를 팀에 추가

## 실행 모드 확인 (먼저)
1. 현재 요청을 `진입 조건`과 먼저 대조한다. 진입 조건에 맞지 않는 국소 작업이면 파일이나 `artifacts/`를 광범위하게 탐색하지 말고 여기서 종료해 직접 처리한다.
2. 전체 오케스트레이션 대상일 때만 `artifacts/`를 확인한다.
   - `improvement-log.md`를 읽어 직전 회고·다음 테스트를 파악한다(있으면).
   - 산출물이 없으면 **초기 실행**, 일부만 있으면 **부분 재실행**, 새 요청이면 **새 실행**.
3. 키워드만으로 모드를 정하지 않는다. "다시"는 필요한 단계만 재실행하고, "이전 설계 기반"은 01-design을 재사용한다.

## 진행 (Pipeline)
0. **규모 게이트**: 설계 착수 전, `scope-gate` 스킬로 요청 규모를 판단한다. `분리 필요` 판정이면 분해안을 사용자에게 보여주고 승인받기 전 아래 단계로 진행하지 않는다. `한 번에 진행`이면 그대로 이어간다.
1. 요청·게임 범위·제약·승인 지점을 `artifacts/00-input.md`에 정리(있으면 갱신).
2. `TeamCreate`로 이번 작업에 필요한 역할만 구성한다. `TaskCreate`로 필요한 설계/구현/검증/리뷰 Task만 등록한다(담당자·입력 파일·출력 위치·의존·완료 기준 명시). game-plan 개발 순서는 해당 MVP 전체를 구현하는 요청일 때만 따른다.
3. **설계가 필요할 때**: unity-architect → `artifacts/01-design.md`. 새 시스템이면 여기에 **배치 위치 후보(공통/프로젝트)**만 적고 확정하지 않는다. 기존 설계가 충분하면 이 단계를 생략한다.
4. **[사람 승인 게이트 A — 새 시스템 배치 위치]** 새 시스템의 배치 위치가 미확정일 때만 구현 착수 직전에 `00_CommonFramework`(공통 재사용) 또는 프로젝트 전용 영역 중 어디에 둘지 **사용자에게 확인**한다. 기존 파일 수정에는 적용하지 않는다.
5. **구현**: gameplay-engineer → `Assets/`의 확정된 위치. `csharp-convention-guide` 준수.
6. **씬·에셋** 필요 시: unity-ai-operator에 위임. **[사람 승인 게이트 B — 씬·에셋]** 변경 요약을 보여주고 승인받은 뒤 실행. 대규모 변경 전 커밋/백업 권고.
7. **4단계 검증**(각 게이트, 막히면 다음으로 넘어가지 않고 즉시 보고 → 수정 → 해당 단계 재검증). MCP 연결이므로 1~3은 Stop hook이 수행하되, **수동 트리거**로 동작한다:
   1. 컴파일 에러 (Gate 1, `unity-validate.sh`)
   2. Play 모드 콘솔 에러 (Gate 2, `unity-validate.sh`)
   3. 기능 테스트 (Gate 3, `unity-validate.sh`가 `.claude/hooks/.viewer-state/gate3-test.json`을 읽어 `gate3_run_test` 호출) — `.cs` 생성·수정 시 `gate3-test.json`도 갱신.
   4. 사용자 확인 (viewer 피드백 → `artifacts/04-user-feedback.md`)
   - `unity-validate.sh`는 `artifacts/.viewer-state/validate-requested` 마커 파일이 있을 때만 1~3을 실행하고, 실행 후 마커를 스스로 지운다. 마커가 없는 Stop은 즉시 스킵된다(긴 구현 작업 중 매 턴 검증이 도는 것 방지). unity-ai-operator가 검증 준비가 된 시점에 이 마커를 생성한다.
   - 결과는 `artifacts/02-validation.md`에 게이트 표로 기록된다.
8. **리뷰가 필요할 때**: 여러 시스템에 영향이 있거나 회귀 위험이 큰 변경은 code-reviewer가 시스템 단위로 점진 검토 → `artifacts/03-review.md`. blocker/major는 gameplay-engineer에 수정 요청.
9. 통합: orchestrator가 산출물을 읽어 누락·충돌·승인 대기 정리.
10. 팀을 만들었으면 `TeamDelete`로 정리.
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
- **게이트 A**: 새 시스템의 배치 위치(공통/프로젝트)가 미확정일 때만 확정.
- **게이트 B**: 씬·프리팹·에셋 생성/변경/삭제, 대규모 리팩토링. 대규모 변경 전 커밋/백업.

## 실패 처리
- 컴파일/런타임/기능 게이트 실패 → 해당 단계에서 멈추고 보고 → (필요 시 debugger 추가) 원인 분석 → gameplay-engineer 수정 → 같은 단계 1회 재검증.
- 팀원 멈춤 → `TaskGet` 확인 → `SendMessage`로 원인 확인 → 1회 재시도.
- 설계-구현 충돌 → 한쪽을 지우지 않고 근거 남긴 뒤 orchestrator 판단.
- 입력 부족 → 추측 금지, 질문 목록 작성 후 멈춤.

## 금지
- 전체 오케스트레이션에서 Agent를 사용한다면 `TeamCreate` 흐름 없이 진행하지 않는다.
- 국소 작업을 전체 오케스트레이션으로 확장하지 않는다.
- C# 코드를 `artifacts/`에 두지 않는다(Unity 미컴파일).
- 컴파일·플레이 검증 없이 완료 처리하지 않는다.
- 사람 승인 없이 씬·에셋을 변경하거나 배치 위치를 임의 확정하지 않는다.
