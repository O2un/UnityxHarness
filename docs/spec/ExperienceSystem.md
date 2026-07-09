# ExperienceSystem — /prd 입력 답변 (PRD 2 · ExperienceModule & LevelUpEvent)

> `/prd ExperienceSystem` 실행 후 이어지는 질문에 붙여넣을 답변 정리. 근거 설계: `outline.md`.
> 경험치 루프의 가운데. 입력("경험치 N 획득")은 PRD 1(`ItemDrop`)에서, 출력(`LevelUpEvent`)은 PRD 3(`LevelUpSelection`)으로 이어진다.

| 질문 | 답변 |
|---|---|
| 무엇을 만드는가 | 처치 보상 루프의 가운데. "경험치 N 획득"을 받아 누적하고, 임계값을 넘으면 레벨업을 발행한다 |
| 구성요소 | `ExperienceModule`(누적·레벨 판정·이월), `LevelUpEvent`(R3 발행) |
| 획득 입력 | `ExperienceModule.Gain(amount)`로 "경험치 N 획득" 입력 하나만 받는다. 아이템 종류·Collider는 모른다 |
| 레벨 판정 | 누적 → 현재 레벨 필요치 비교 → 초과분 이월하며 레벨 상승. 한 번에 여러 레벨 상승 허용 |
| 데이터 | 레벨별 필요 경험치는 데이터로 주입 — 인스펙터에서 AnimationCurve로 편집하고, 정교한 조정이 필요하면 값 테이블로 뺀다. Module 안에 하드코딩 X |
| 상태 노출 | 현재 경험치·현재 레벨은 `ReactiveProperty`로 노출 (HUD가 구독) |
| 레벨업 발행 | 레벨이 오르면 `LevelUpEvent` 발행. payload는 새 레벨 등 결과 정보만. 구독 측이 누구인지 모른다(단방향) |
| 배치 | `ExperienceModule`·`LevelUpEvent`는 순수 C# (Unity 비의존) |
| 입력(Depends On) | "경험치 N 획득"(PRD 1) |
| 출력(Provides) | `LevelUpEvent`(PRD 3의 입력) |
| 금지 | `ExperienceModule`이 Unity API·Collider·UI·일시정지에 의존 금지. 레벨별 필요 경험치 하드코딩 금지. 구체 적·아이템 타입 참조 금지 |
