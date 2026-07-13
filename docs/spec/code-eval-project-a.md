# `/code-eval` Project A 코드 규칙 Eval 프롬프트

아래 파일을 먼저 읽고 Project A의 코드 규칙 Eval을 수행해 주세요. 밸런스 Eval은 `/balance-eval`로 따로 돌립니다.

- `@CLAUDE.md`
- `@artifacts/chain-log.md`
- `@docs/conventions/convention.md`
- `@Assets/**`

## 요청

누적 개선 기록과 규칙을 체크리스트로 삼아 전체 코드를 전수 점검합니다. 분석 중 자동 수정은 하지 말고, 결과만 보고서로 남겨 주세요.

다음 항목을 전수 확인합니다.

- `Assets/00_CommonFramework/`, `Assets/10_ProjectA/` 배치
- Context / Actor / View / Module 책임과 참조 방향
- DI 등록 흐름
- 런타임 씬 전역 탐색 계열 호출
- Addressables와 Pool 사용
- R3 lifecycle 바인딩
- Console 오류와 컴파일 오류
- `chain-log.md`에 기록된 반복 이슈

각 항목은 `Pass`, `Issue`, `Needs Play Check` 중 하나로 판정하고 파일 경로·라인·근거·심각도·후속 조치를 기록합니다.

## 결과 파일

`@artifacts/project-a-eval-report.md`의 코드·하네스 Eval 섹션에 옮길 수 있게, `Issue`와 `Needs Play Check`를 독립된 항목으로 작성해 주세요. 마지막에 자동 수정하지 않은 이유를 적습니다.
