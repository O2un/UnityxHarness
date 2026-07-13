---
name: balance-eval
description: 프로젝트 A의 애셋 값으로 웨이브 밸런스 기댓값을 계산식으로 산출하고 임계값과 대조해 난이도 흐름을 평가할 때 사용합니다.
disable-model-invocation: true
context: fork
agent: Explore
---

프로젝트 A의 애셋·코드 값으로 웨이브 밸런스 기댓값을 계산식으로 산출하고, 임계값과 대조해 수정 없이 밸런스 Eval 결과만 보고하세요. 동적 플레이 실측은 이 평가 범위 밖입니다.

## 입력

- `docs/evals/wave-balance-prep.md`: 몬스터(`MonsterDataSO`)·웨이브(`WaveDataSO`)·플레이어(`SkillStats`+`SkillUpgradeData`) 기준값과 파생 지표 계산식, 임계값
- 애셋에서 읽은 실제 수치(추측 금지)

## 산출·점검 범위

- 레벨·업그레이드별 기대 DPS(단발=Damage/Cooldown, 지속=Damage/reHit)를 델타 누적으로 계산
- 각 웨이브의 초당 유입 HP(도착 HP 합 / 구간 길이)와 기대 DPS로 `pressure`를 계산해 임계값(안정<0.8 / 경계 0.8–1.2 / 위험>1.2)과 대조
- 초반→중반→후반으로 pressure가 의도한 대로 상승하는지, 업그레이드 경로별 pressure 차이
- `expectedPeakAlive`(유입−처치율 누적 추정)와 그로부터 역산한 pool prewarm

## 계산 가정

- 명중률·범위 유지·동시 타격 수는 조작에 따라 달라지므로 단일값으로 단정하지 말고 계수 `k`로 스윕해 **밴드**로 낸다.
- 가정이 결과를 좌우하는 항목은 그 가정을 함께 적는다.

## 출력 형식

항목마다 아래 중 하나로만 판정합니다.

- `Pass`: 임계값 안, 의도한 난이도 흐름과 일치
- `Issue`: 임계값을 벗어나거나 흐름이 어긋남
- `Needs Assumption`: 계산 가정이 불확실해 밴드로만 판단 가능

각 항목에 대조한 웨이브 구간, 기댓값과 임계값, 계산 근거(입력 애셋 값), 사용한 가정을 함께 적으세요.
`Issue`와 `Needs Assumption`은 `docs/evals/project-a-eval-report.md`에 옮길 수 있게 독립된 항목으로 작성하세요.

동적 플레이 실측이 필요하다고 판단되면 그 항목을 범위 밖으로 표시하고 별도 작업으로 남기세요. Eval 중 수치를 변경하지 마세요.
