---

name: prd

description: Unity 게임 기능 하나에 대한 요구사항 문서(PRD)를 docs/requirements/<기능명>.md에 생성한다. 사용자가 "/prd <기능명>", "PRD 작성해줘", "요구사항 문서 만들어줘"처럼 요청하거나, 새 기능을 구현하기 전에 스펙을 정리하려 할 때 사용한다. 기능명을 명시하지 않아도 기능 요구사항을 문서화하려는 의도가 보이면 사용한다.

argument-hint: "[기능명] (예: inventory-system, 없으면 한 줄 질문)"

context: fork

---

**$ARGUMENTS 기능**의 요구사항 문서(PRD)를 작성해 `docs/requirements/<기능명>.md`에 저장한다.

## 동작 절차

1. **기능명 파악** — 인자로 명시됐으면 그대로 사용한다. 없으면 한 줄로 무슨 기능인지 질문한다.
2. **PRD 초안 작성** — 아래 표준 섹션을 순서대로 채운다.
3. **리뷰 요청** — 초안을 사용자에게 보여주고 수정 피드백을 받는다. 확정 전까지 파일을 저장하지 않는다.
4. **파일 저장** — 확정되면 `docs/requirements/<기능명>.md`에 저장한다.
5. **사용법 안내** — 저장 완료 후 안내 메시지를 출력한다.

## 표준 섹션

PRD는 아래 순서를 그대로 따른다.

| 섹션 | 내용 |
|------|------|
| Overview | 기능 한 줄 요약 + 게임 내 맥락 |
| Goals | 구현 완료 후 달성해야 할 상태 (동사+명사 bullet) |
| Out of Scope | 이번 구현에서 명시적으로 제외하는 항목 |
| Technical Requirements | Unity 컴포넌트·API·물리 설정 등 구체 스펙 |
| Acceptance Criteria | 완료 판단 기준 (체크리스트 형식) |
| Open Questions | 미결 사항 |

## 작성 규칙

- 파일은 **반드시** `docs/requirements/` 폴더에 저장한다. 다른 위치 금지.
- 기능명은 kebab-case로 파일명에 쓴다 (예: `inventory-system.md`).
- 마크다운만 사용한다. 코드 블록은 Technical Requirements 섹션에만 허용한다.
- Unity 맥락(컴포넌트명·API)을 모르면 추측하지 않고 질문한다. 잘못된 스펙은 잘못된 구현으로 이어지므로, 모호하면 Open Questions에 남기거나 사용자에게 확인한다.
- 이미 같은 이름의 파일이 존재하면 덮어쓰기 전에 사용자에게 확인한다.

## 테스트 관련 AC — 현재 검증 안 함

이 프로젝트는 본 코드(`00_`·`10_`·`20_`)에 asmdef가 없어 전부 `Assembly-CSharp`로 컴파일된다. Unity Test Framework는 NUnit을 참조하는 어셈블리에서만 테스트를 찾으므로 테스트에는 asmdef가 필수인데, asmdef는 predefined assembly(`Assembly-CSharp`)를 참조할 수 없다. **자동화 테스트를 작성할 수단이 현재 없다.**

따라서 PRD 작성 시:

- "순수 테스트로 검증한다", "Play 모드 없이 테스트 가능하다" 같은 항목을 **Acceptance Criteria에 넣지 않는다.** 충족될 수 없는 AC는 완료 판단을 흐린다.
- 그 의도(로직을 Unity API 비의존으로 분리할 것)는 **Technical Requirements에 설계 요구로** 쓴다. 분리 자체는 여전히 지켜야 한다.
- 검증 기준이 필요하면 **Play 모드에서 관찰 가능한 형태**로 AC를 쓴다. 예: "순수 테스트로 낭떠러지 정지를 검증한다" → "낭떠러지 앞에서 이동이 멈춘다".
- 테스트 도입이 전제인 항목은 Out of Scope에 `테스트 어셈블리 도입 전까지 검증 안 함`으로 남긴다.

asmdef가 도입되면 이 절을 삭제하고 테스트 AC를 되살린다.

## 저장 후 안내 메시지

저장이 끝나면 아래 형식으로 출력한다.

```
✅ docs/requirements/<기능명>.md 저장 완료
이제 Claude에게 이렇게 요청하세요:
"@docs/requirements/<기능명>.md 참고해서 구현해줘"
```
