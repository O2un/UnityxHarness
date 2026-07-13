---
name: code-eval
description: 프로젝트 A의 누적 개선 기록과 CLAUDE.md 규칙을 근거로 전체 코드를 전수 점검할 때 사용합니다.
disable-model-invocation: true
context: fork
agent: Explore
---

프로젝트 A의 현재 코드 전체를 읽고, 수정 없이 코드 규칙 Eval 결과만 보고하세요.

## 입력

- `artifacts/chain-log.md`: 반복 이슈와 전수 점검 항목
- `CLAUDE.md`: 현재 프로젝트 규칙
- `docs/conventions/convention.md`: 코드·폴더 컨벤션
- `Assets/**`: Project A에서 사용한 모든 C# 코드와 Unity 구성의 전수 점검 대상

## 점검 범위

- 공통 코드와 Project A 코드의 배치, Context / Actor / View / Module 경계, 의존 방향
- 런타임 씬 전역 탐색, static Instance, Resources.Load 등 금지 패턴
- Addressables 등록·참조, Pool 사용, R3 lifecycle 바인딩, Console 오류와 Play가 필요한 항목
- 누적 개선 기록에 있는 재발 방지 규칙

## 출력 형식

항목마다 아래 중 하나로만 판정합니다.

- `Pass`: 규칙 준수 확인
- `Issue`: 위반 확인
- `Needs Play Check`: 코드만으로 확인할 수 없어 Play 검증 필요

각 항목에 점검 대상 파일·라인, 규칙 또는 개선 기록 근거, 현재 상태, 다음 확인 방법을 함께 적으세요.
`Issue`와 `Needs Play Check`는 `artifacts/project-a-eval-report.md`에 옮길 수 있게 독립된 항목으로 작성하세요.

코드를 수정하거나 수치를 조정하지 마세요.
