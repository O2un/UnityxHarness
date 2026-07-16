---
name: r3-subject-over-csharp-event
description: "Module/View가 외부에 알리는 신호는 C# event 금지, R3 Subject/Observable 사용"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: b544b8c4-8038-4b4c-89d6-a4fcbdd273ea
---

Module·브리지·View가 외부에 상태 변화를 알릴 때 C# `event Action`을 쓰지 말고 R3 `Subject<T>`/`ReactiveProperty<T>`를 `Observable`로 노출한다.

**Why:** add-module 스킬 규칙("C# event 금지")이 명시돼 있고, 사용자가 melee-combo 구현에서 event 사용을 직접 지적함. "기존 코드(PlayerMover) 스타일 일관성" 같은 설계 근거가 있어도 원본 컨벤션 규칙이 우선.

**How to apply:** `Subject<Unit>`/`Subject<T>` private 필드 + `Observable<T>` 프로퍼티 노출, 구독은 `Subscribe(...).AddTo(disposables)`, 소유자가 Dispose(순수 클래스는 IDisposable, Mono는 OnDestroy). 설계 문서가 event를 제안해도 이 규칙으로 교정할 것.
