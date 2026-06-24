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
│       ├── Actor/             PlayerContext · PlayerActor · PlayerMover · PlayerView
│       ├── DI/                LifetimeScope · Bootstrap
│       ├── Manager/
│       │   ├── GameManager/
│       │   ├── Input/         InputManager · PlayerInputModule
│       │   ├── Camera/        CameraManager
│       │   ├── ScoreManager/  ScoreManager · ScoreVM · ScoreView · ScoreContext
│       │   ├── OptionManager/
│       │   └── SceneManager/
│       ├── UI/                UIStore
│       └── Data/              PlayerData · OptionsData
├── 10_ProjectA/               3D 탑다운 서바이버 (Part 4A에서 구현)
└── 20_ProjectB/               2D 횡스크롤 액션 (Part 4B에서 구현)
```

---

## 진행 상황

### ✅ STEP 1 — AI 게임 개발의 기준 잡기
LLM 특성 이해 · 도구 선택 · 개발 환경 세팅

### ✅ STEP 2 — Unity 프로젝트 구조 설계
컨벤션 · MonoBehaviour vs 순수 C# 분리 기준 · Manager/Module/Service · VContainer DI  
Input · Observer(R3) · Camera · UI(MVVM) · Save · UniTask · 씬 로딩 · 씬 연결 실습

### 🔨 STEP 3 — 하네스 엔지니어링 *(3-4c2 Compaction까지 완료)*

| | 항목 |
|---|---|
| ✅ | CLAUDE.md (Instruction) |
| ✅ | Memory |
| ✅ | Context Control |
| ✅ | Skills — add-global-manager · add-module · code-review |
| ✅ | Commands |
| ✅ | Plan Mode |
| ✅ | Compaction |
| ⬜ | Hooks · MCP · Config · Subagents · Evals · Observability |

### ⬜ STEP 4 — AI 결과물 검증·리팩토링 루프
### ⬜ STEP 5 — 실전 게임 프로젝트 (3D 뱀서류 + 2D 액션)

---

## 하네스 구성 요소

| 파일·폴더 | 역할 |
|---|---|
| `CLAUDE.md` | 매 세션 자동 주입되는 규칙집 (컨벤션·금지 패턴·라이브러리 범위) |
| `.claude/skills/add-global-manager/` | 씬·전역 스코프 싱글턴 Manager 추가 절차 |
| `.claude/skills/add-module/` | Manager 산하 순수 로직 Module 추가 절차 |
| `.claude/skills/code-review/` | 코드 리뷰 수행 절차 |
| `Memory/` | 세션 간 지속되는 자동 학습 기록 (git 추적) |

---
