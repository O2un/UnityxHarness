# Unity × Harness

> **하네스 엔지니어링 Unity 게임 개발**  
> 장르불문 평생 써먹는 AI Agent 활용 바이블 w. Claude Code & Codex  
> [패스트캠퍼스 강의 페이지](https://fastcampus.co.kr/biz_online_unityclaude)

AI를 코드 작성기로 쓰는 것이 아니라, AI Agent가 프로젝트의 규칙을 이해하고 안정적인 개발 파트너로 동작하도록 하는 **하네스(작업 환경)** 를 설계하는 방법을 다룹니다.

---

## 기술 스택

| 분류 | 내용 |
|---|---|
| 엔진 | Unity 6 (6000.x) |
| 언어 | C# (.NET Standard 2.1) |
| DI | VContainer |
| Reactive | R3 |
| Async | UniTask |
| AI 도구 | Claude Code · Codex |

---

## 아키텍처

Manager / Module / Service 3레이어 패턴 + VContainer DI

```
Manager   흐름 조율자. 순수 C# 클래스. VContainer Singleton으로 등록·주입
Module    기능 단위 순수 C# 클래스. Unity API 의존 없음. new로 생성
Service   파일·씬·에셋 등 외부 시스템 접점. 인터페이스로 추상화
```

폴더는 레이어가 아닌 **기능 단위**로 분리합니다.

```
Assets/
├── 00_CommonFramework/        두 프로젝트가 공유하는 공통 인프라
│   └── 00_Scripts/
│       ├── Actor/             Player · Enemy · ActorManager
│       ├── AI/                공용 AI 인프라
│       ├── Combat/            Health · Hitbox · Skill
│       ├── DI/                LifetimeScope · Bootstrap
│       ├── Manager/           GameManager · Input · Camera · Scene · Score · Option
│       │                      Asset · DataProvider · Pool · Inventory
│       ├── UI/                GameSelect · Hud · Loading
│       └── Data/              PlayerData · OptionsData
├── 10_ProjectA/               3D 탑다운 서바이버 (플레이 가능 MVP)
│   └── 01_Scripts/
│       ├── Actor/Npc/AI/      Chase · Dash · ArmoredMelee 적 AI (State/Condition SO)
│       ├── Actor/Item/        경험치·아이템 드롭
│       ├── Combat/Skill/      Projectile · MeleeSwing · AuraField 스킬
│       ├── Manager/           EnemySpawner(Wave) · GameManager(상태·킬카운트)
│       ├── Progression/       Experience · LevelUpSelection
│       └── UI/GameFlow/       Start · HUD · Victory · Defeat 패널
└── 20_ProjectB/               2D 횡스크롤 액션 (Part 4B에서 구현)
```

씬: `Bootstrap` → `GameSelect`(진입점) → `Loading` → `GameScene`

---

## 진행 상황

### ✅ STEP 1 — AI 게임 개발의 기준 잡기
LLM 특성 이해 · 도구 선택 · 개발 환경 세팅

### ✅ STEP 2 — Unity 프로젝트 구조 설계
컨벤션 · MonoBehaviour vs 순수 C# 분리 기준 · Manager/Module/Service · VContainer DI  
Input · Observer(R3) · Camera · UI(MVVM) · Save · UniTask · 씬 로딩 · 씬 연결 실습

### ✅ STEP 3 — 하네스 엔지니어링

| | 항목 |
|---|---|
| ✅ | CLAUDE.md (Instruction) |
| ✅ | Memory |
| ✅ | Context Control · Compaction |
| ✅ | Skills · Commands · Plan Mode |
| ✅ | Hooks — Stop hook 4단계 검증 게이트 · agent-router · 결과 뷰어 |
| ✅ | MCP — MCP for Unity(CoplayDev) 연결 (에디터 조작·컴파일·플레이 테스트) |
| ✅ | Subagents — unity-architect · gameplay-engineer · unity-ai-operator · code-reviewer |
| ✅ | Claude ↔ Codex 에이전트 라우팅 |
| 🔨 | Evals · Observability |

### 🔨 STEP 4 — AI 결과물 검증·리팩토링 루프
설계 → 구현 → 씬·검증 → 리뷰 파이프라인을 Agent Team으로 구동.  
4단계 검증 게이트(①컴파일 ②Play 콘솔에러 ③기능테스트 ④사용자 확인) 운영 중.

### 🔨 STEP 5 — 실전 게임 프로젝트
**ProjectA (3D 뱀서류) — 플레이 가능 MVP 완성**

| 시스템 | 상태 |
|---|---|
| 플레이어 이동 (Topdown3D) | ✅ |
| 적 스폰 & 웨이브 스케일링 | ✅ |
| 적 AI — Chase · Dash · ArmoredMelee | ✅ |
| 자동 공격 스킬 — Projectile · MeleeSwing · AuraField | ✅ |
| 체력 · 사망 처리 | ✅ |
| 경험치 · 레벨업 능력 선택 | ✅ |
| 게임 흐름 — Start / HUD / Victory / Defeat · 재시작 | ✅ |
| 게임 선택 씬 (진입점) | ✅ |

**ProjectB (2D 횡스크롤 액션)** — ⬜ 예정

---

## 하네스 구성 요소

| 파일·폴더 | 역할 |
|---|---|
| `CLAUDE.md` | 매 세션 자동 주입되는 규칙집 (컨벤션·금지 패턴·라이브러리 범위) |
| `.claude/agents/` | 서브에이전트 — unity-architect · gameplay-engineer · unity-ai-operator · code-reviewer |
| `.claude/skills/` | unity-dev-orchestrator · scope-gate · prd · game-plan · add-global-manager · add-module · csharp-convention-guide · code-review · claude-to-codex-migration |
| `.claude/hooks/` | Stop hook(4단계 검증) · agent-router · 결과 뷰어 (Node.js) |
| `artifacts/` | 파이프라인 산출물 (설계·검증·리뷰·체인 로그) |
| `docs/` | conventions · design(game-plan · task-breakdown) · evals · requirements |
| `Memory/` | 세션 간 지속되는 자동 학습 기록 (git 추적) |

### 개발 흐름

```
설계(unity-architect) → [승인: 배치 위치] → 구현(gameplay-engineer)
  → 씬·검증(unity-ai-operator) [승인: 씬·에셋] → 리뷰(code-reviewer)
```

게임 기능 요청은 `unity-dev-orchestrator` Skill이 위 파이프라인으로 라우팅합니다.

---
