---
name: balance-eval
description: 완성된 프로젝트 A 한 판의 Play 실측값을 웨이브 밸런스 기준값과 대조해 난이도 흐름을 점검할 때 사용합니다.
disable-model-invocation: true
context: fork
agent: Explore
---

완성된 프로젝트 A 한 판의 Play 실측 데이터를 웨이브 밸런스 기준값과 대조하고, 수정 없이 밸런스 Eval 결과만 보고하세요.

## 입력

- `docs/evals/wave-balance-eval.md`: 웨이브·몬스터 기준 데이터와 Play 실측 필드
- `docs/evals/wave-data.md`: 웨이브 기준 수치 원본
- 사용자가 넘기는 Play 실측 기록: 레벨·업그레이드 경로별 DPS, 공격 범위, 동시 타격 수, 생존 지표, peak alive

## 점검 범위

- 각 웨이브 구간에서 플레이어 실측 성능이 몬스터 HP·이동 속도·spawnRate를 감당하는지
- 판 전체의 클리어 가능성과, 초반→중반→후반 난이도 변화가 기준 흐름을 따르는지
- 업그레이드 선택 경로별 전투 차이
- pool prewarm 대비 실측 peak alive
- Play 중 발생한 오류

## 출력 형식

항목마다 아래 중 하나로만 판정합니다.

- `Pass`: 기준 흐름과 일치 확인
- `Issue`: 기준과의 불일치 확인
- `Needs Measurement`: 실측 데이터가 부족해 판단 불가

각 항목에 대조한 웨이브 구간, 기준값과 실측값, 근거, 다음 확인 방법을 함께 적으세요.
`Issue`와 `Needs Measurement`는 `artifacts/project-a-eval-report.md`에 옮길 수 있게 독립된 항목으로 작성하세요.

실측값이 부족하거나 애매하면 억지로 `Pass`나 `Issue`를 만들지 말고 `Needs Measurement`로 남기세요. Eval 중 수치를 변경하지 마세요.
