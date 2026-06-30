---

name: prd

description: Unity 게임 기능 하나에 대한 요구사항 문서(PRD)를 docs/requirements/<기능명>.md에 생성한다. 사용자가 "/prd <기능명>", "PRD 작성해줘", "요구사항 문서 만들어줘"처럼 요청하거나, 새 기능을 구현하기 전에 스펙을 정리하려 할 때 사용한다. 기능명을 명시하지 않아도 기능 요구사항을 문서화하려는 의도가 보이면 사용한다.

argument-hint: "[기능명] (예: inventory-system, 없으면 한 줄 질문)"

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

## 저장 후 안내 메시지

저장이 끝나면 아래 형식으로 출력한다.

```
✅ docs/requirements/<기능명>.md 저장 완료
이제 Claude에게 이렇게 요청하세요:
"@docs/requirements/<기능명>.md 참고해서 구현해줘"
```
