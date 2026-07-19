# ItemUpgradeSystem — /prd 입력 답변

> `/prd 아이템 획득 & 능력치 강화` 실행 후 붙여넣을 답변 정리입니다. 근거는 현재 공통 InventoryManager 코드와 룸 클리어·문 상호작용 구현입니다.

| 질문 | 답변 |
|---|---|
| 무엇을 만드는가 | 룸을 클리어하면 문 앞에 카드 2개가 스폰되고, 플레이어가 하나를 골라 능력치 강화 또는 스킬 습득 효과를 얻는 보상 흐름 |
| 획득 확정 규칙 | 후보 범위에서 상호작용하면 `IInventoryWriter.Add`를 호출한다. `Added`면 선택을 확정하고 다른 후보를 회수한다. `SlotsFull`이면 후보를 유지하고 슬롯 안내를 표시한다. 슬롯 교체 UI는 이번 범위에서 제외한다 |
| 카드 데이터 | 카드 SO는 `IItemData`를 구현하고 `Category = Passive`로 둔다. modifier·대상 스탯·해금 스킬은 `IItemData`를 확장한 Project B 전용 카드 효과 계약에 둔다 |
| 공통 인벤토리 재사용 | 픽업은 `IInventoryWriter`에 의존해 카드를 담고, StatModule은 `IInventoryReader.Slots`에 의존해 획득 목록을 읽는다. 공통 InventoryManager와 `IItemData`는 Project B 구체 타입을 참조하지 않는다 |
| 능력치 집계 | 순수 C# StatModule이 base값 + 획득 Passive 카드 modifier 합으로 최종 스탯을 계산한다. 카드 없음·한 장·동일 계열 두 장 누적을 Play 없이 테스트한다 |
| 스킬 습득 | 수치 modifier가 아니라 SkillModule의 해금 플래그를 갱신한다 |
| 카드 4종 연결 | 스킬 습득 → SkillModule, 공격 강화 → SkillModule·HitboxModule, 생존 강화 → HealthModule, 이동 강화 → PlayerMover. 수치·대상·해금 스킬은 카드 SO 데이터로 노출한다 |
| 역할과 배치 | 진행 로직은 후보 추첨·Add 결과·선택 확정, View는 문 앞 배치·프롬프트·선택 표현을 담당한다. 카드 데이터·효과 계약·진행 로직·View·StatModule은 `Assets/20_ProjectB/` 아래에 둔다 |
| 검증 범위 | 요구사항 문서의 Constraints에 선택 확정 조건, 공통 인벤토리 재사용, StatModule 순수성, 수치 집계·스킬 해금 구분, `20_ProjectB` 배치를 명시한다. 실제 구현과 중첩·해금 Play 검증은 다음 시간에 진행한다 |
