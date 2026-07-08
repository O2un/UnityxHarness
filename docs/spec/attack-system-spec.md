# AttackSystem — /prd 입력 답변

> `/prd AttackSystem` 실행 후 이어지는 질문에 붙여넣을 답변 정리. 근거 설계: `outline.md`(4A-3c11 PRD 기준).

| 질문 | 답변 |
|---|---|
| 무엇을 만드는가 | 가까운 적을 쿨다운마다 자동 공격하고, 맞은 적이 피격되는 전투 루프. 공격 방식은 근접 스윙 / 투사체 / 오라·장판 3종이지만, 발동·판정·피격 흐름은 하나로 공유한다 |
| 구성요소 | `SkillModule`(쿨다운·발동, 순수 C#), `HitboxModule`(피격 판정·R3 발행, 순수 C#), `ISkillDefinition`(타깃 선정+공격 방식 계약등 데이터)과 그 구현 3종, `AttackHitboxView`(가해 히트박스), `DamageableView`(피격 히트박스) |
| 역할 분리 | SkillModule은 "언제 발동할지"만 안다 — 타깃 선정도 공격 방식도 스킬 정의가 정한다. 타깃 후보(`ActorManager`)는 발동 컨텍스트로 정의에 넘겨줄 뿐, SkillModule이 직접 고르지 않는다. 방식별·타깃별 if/switch 분기 금지 |
| 히트박스 구조 | 판정은 가해 히트박스(`AttackHitboxView`, 트리거 Collider — 스킬이 켜는 판정 영역)와 피해 히트박스(`DamageableView`, 몸체 Collider + `IDamageable`)가 겹칠 때 일어난다. Collider는 둘 다 View 계층. `HitboxModule`은 어느 쪽 Unity API에도 의존하지 않는다 |
| 스킬 3종 | 근접 스윙(전방 히트박스 즉시 판정) / 투사체 발사(스폰은 `PoolModule` 위임 — `Instantiate`/`Destroy` 직접 호출 금지, `Get`/`Release` 경유) / 오라·장판(지속 히트박스, 주기마다 재판정). 쿨다운·사거리·히트박스 모양·데미지·지속시간은 코드가 아니라 스킬 정의 값으로 |
| 데이터 | 몬스터 데이터(체력·이동속도 등)는 `ScriptableObject`로 만들어 `NpcContext`가 입력받는다. 스킬 3종의 정의(타깃 선정 전략,공격 방식, 대미지, 쿨타임 등)도 각각 `ScriptableObject`로 만들어 `PlayerContext`가 입력받아 조립한다 |
| 계약 | 피격 이벤트 payload는 `IDamageable` 대상 + 데미지만 담는다. 구체 적·Player 타입 참조 금지 — HitboxModule은 상대가 적인지 Player인지 모른다. R3 구독은 `AddTo`로 lifecycle에 바인딩 |
| 이번 범위 | 피격 이벤트를 구독해 체력 감소와 처치 판정까지 연결한다. 경험치 지급, 스킬 선택 UI, 레벨업 강화는 범위 밖 |
