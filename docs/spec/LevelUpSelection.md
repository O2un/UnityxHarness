# LevelUpSelection — /prd 입력 답변 (PRD 3 · 선택 UI & 일시정지)

> `/prd LevelUpSelection` 실행 후 이어지는 질문에 붙여넣을 답변 정리. 근거 설계: `outline.md`.
> 경험치 루프의 끝. 입력(`LevelUpEvent`)은 PRD 2(`ExperienceSystem`)에서 이어진다.

| 질문 | 답변 |
|---|---|
| 무엇을 만드는가 | 처치 보상 루프의 끝. `LevelUpEvent`를 받아 게임을 멈추고 능력 세 개 중 하나를 고르게 한 뒤 재개한다 |
| 구성요소 | 레벨업 선택 UI(일시정지·세 버튼), 구독 Context(`LevelUpEvent` 수신·일시정지·능력 적용) |
| 입력 | `LevelUpEvent` 구독. payload의 새 레벨 등 결과 정보만 사용 |
| 일시정지 | 게임 멈춤은 표현 계층 결정(`Time.timeScale` 또는 상태 전환). `ExperienceModule`은 관여하지 않음 |
| 능력 선택 | 능력 세 후보를 능력 정의 목록(데이터)에서 뽑아 버튼으로 표시. 하나 선택 시 능력 적용 후 재개 |
| 데이터 | 능력 후보 목록은 데이터로 구성 (4A-3c12에서 만든 스킬 데이터 활용 — 대미지·쿨타임·범위·투사체 수 등 여러 방향) |
| 배치 | 구독·일시정지·선택 UI는 View/Context |
| 입력(Depends On) | `LevelUpEvent`(PRD 2) |
| 출력(Provides) | 능력 적용 후 게임 재개 — 루프 재시작 |
| 금지 | 능력 후보 하드코딩 금지. `ExperienceModule` 내부 참조 금지. 구체 적·아이템 타입 참조 금지 |
