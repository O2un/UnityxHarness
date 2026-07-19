# Memory Index

- [코드 주석 금지](feedback_no_code_comments.md) — 기능 설명 주석 달지 않기. WHY가 비자명한 경우만 한 줄 허용.
- [R3 Subject 우선](feedback_r3_subject_over_event.md) — 외부 알림 신호는 C# event 금지, R3 Subject/Observable 노출.
- [ISceneInitializable Init 타이밍](project_sceneinitializable_init_timing.md) — Init()은 Awake 시점. 다른 컴포넌트의 OnEnable 산출물은 Start()에서 잡을 것.
