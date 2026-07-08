# 02. 검증 (4단계 게이트) · AttackSystem

작성: orchestrator + unity-ai-operator
일시: 2026-07-08

## 게이트 진행 요약
| 단계 | 결과 | 비고 |
| --- | --- | --- |
| 1. 컴파일 | ✅ 통과 | refresh_unity(scope=all,force) 후 read_console error 0건. CS 진단 0. |
| 2. 런타임 에러 (Play 콘솔) | ⏳ 대기 | 게이트 B(씬·에셋) 완료 후 수행 |
| 3. 기능 점검 (자동/라이브) | ⏳ 대기 | 게이트 B 후 |
| 4. 기능 점검 (사용자) | ⏳ 대기 | viewer 제출 |

## 게이트 1 상세
- 신규 `.cs`: `00_CommonFramework/00_Scripts/Combat/**`, `10_ProjectA/01_Scripts/Combat/**`, `MonsterDataSO.cs`.
- 수정 `.cs`: PlayerActor, PlayerContext, NpcActor, NpcContext, GameSceneScope.
- 컴파일 에러 0건 → 게이트 1 통과. 씬·에셋 무변경 확인.
