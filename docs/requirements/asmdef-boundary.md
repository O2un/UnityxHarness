# 공통·프로젝트 asmdef 경계 설정

## Overview

현재 UnityxHarness는 `00_CommonFramework`, `10_ProjectA`, `20_ProjectB` 폴더로 코드 위치와 참조 방향을 구분하고 있지만, 이 경계를 컴파일 단위로 나누는 asmdef가 없습니다. 공통 코어와 각 프로젝트를 별도 어셈블리로 분리하고, 공통이 프로젝트를 참조하지 않는 방향을 Unity 컴파일 단계에서 검증할 수 있게 만듭니다.

## Goals

- `Assets/00_CommonFramework/`에 `O2un.CommonFramework.asmdef`를 만든다.
- `Assets/10_ProjectA/`에 `O2un.ProjectA.asmdef`를 만들고 `O2un.CommonFramework`만 참조한다.
- `Assets/20_ProjectB/`에 `O2un.ProjectB.asmdef`를 만들고 `O2un.CommonFramework`만 참조한다.
- `Assets/00_CommonFramework/Tests/EditMode/`에 `O2un.CommonFramework.Tests.asmdef`를 만들고 EditMode 테스트 어셈블리로 구성한다.
- 기존 코드가 컴파일되는지 확인하고, 이전에 건너뛴 검증을 다시 실행한다.

## Out of Scope

- 코드의 네임스페이스 이름 변경.
- 공통·Project A·Project B 사이의 책임을 임의로 이동하는 리팩터링.
- PlayMode 테스트 또는 기능 자동화 테스트의 신규 구현.
- 서드 파티 에셋·패키지 폴더에 asmdef를 추가하거나 수정하는 작업.

## Technical Requirements

### asmdef 배치와 이름

| 위치 | 파일명 | Assembly name | 참조 |
|---|---|---|---|
| `Assets/00_CommonFramework/` | `O2un.CommonFramework.asmdef` | `O2un.CommonFramework` | 없음 |
| `Assets/10_ProjectA/` | `O2un.ProjectA.asmdef` | `O2un.ProjectA` | `O2un.CommonFramework` |
| `Assets/20_ProjectB/` | `O2un.ProjectB.asmdef` | `O2un.ProjectB` | `O2un.CommonFramework` |
| `Assets/00_CommonFramework/Tests/EditMode/` | `O2un.CommonFramework.Tests.asmdef` | `O2un.CommonFramework.Tests` | `O2un.CommonFramework`, Test Assemblies |

### 참조 방향

```text
O2un.ProjectA ─┐
               ├──> O2un.CommonFramework
O2un.ProjectB ─┘

O2un.CommonFramework.Tests ───> O2un.CommonFramework
```

- `O2un.CommonFramework`는 `O2un.ProjectA`, `O2un.ProjectB`를 참조하지 않는다.
- Project A와 Project B는 서로 참조하지 않는다.
- 테스트 어셈블리는 `includePlatforms`를 `Editor`로 제한하고 `optionalUnityReferences`에 `TestAssemblies`를 사용한다.
- asmdef 참조는 Unity Inspector의 Assembly Definition References에서 설정한다. GUID를 수동으로 작성하지 않는다.

### 코드·에셋 배치 규칙

- `Assets/00_CommonFramework/00_Scripts/` 아래의 공통 코드와 `99_Dev/` 개발 지원 코드는 `O2un.CommonFramework`에 포함한다.
- `Assets/10_ProjectA/`, `Assets/20_ProjectB/` 아래의 게임 전용 코드·프리팹·데이터는 각각의 프로젝트 어셈블리에 포함한다.
- `Assets/10_ProjectA/_3rd/`, `Assets/20_ProjectB/_3rd/`와 Packages의 서드 파티 코드에는 asmdef를 추가하지 않는다.
- 컴파일 오류가 발생하면 먼저 누락된 공통 참조, 프로젝트 간 직접 참조, asmdef 밖에 남은 코드가 있는지 확인한다. 문제를 피하려고 공통 어셈블리에 Project A 또는 Project B 참조를 추가하지 않는다.

### 검증

1. Unity Editor가 asmdef 생성 후 스크립트를 다시 컴파일하는지 확인한다.
2. Console에 컴파일 에러가 없는지 확인한다.
3. EditMode Test Runner에서 `O2un.CommonFramework.Tests`가 검색되는지 확인한다.
4. 기존 valid 파이프라인의 컴파일 검증을 다시 실행하고, 실패 시 어느 어셈블리에서 발생했는지 기록한다.
5. Project A와 Project B 씬을 각각 Play 모드로 실행해 기존 진입 흐름에 오류가 없는지 확인한다.

## Acceptance Criteria

- [ ] 공통·Project A·Project B·공통 EditMode 테스트용 asmdef 파일이 지정된 경로에 있다.
- [ ] `O2un.CommonFramework`의 References에 Project A·Project B 어셈블리가 없다.
- [ ] `O2un.ProjectA`와 `O2un.ProjectB`가 모두 `O2un.CommonFramework`를 참조한다.
- [ ] Project A와 Project B가 서로를 참조하지 않는다.
- [ ] Unity Console에 asmdef 도입으로 인한 컴파일 에러가 없다.
- [ ] EditMode Test Runner가 공통 테스트 어셈블리를 인식한다.
- [ ] 기존에 건너뛴 컴파일·테스트 검증을 다시 실행한 결과가 기록돼 있다.

