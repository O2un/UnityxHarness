# 개선 기록 (improvement-log)

## 2026-07-12 · audio-system 구현

### 구현 내용
- BGM 1채널 + SFX PlayOneShot + 볼륨 반영의 최소 오디오 시스템.
- 재생 코어(`00_CommonFramework/00_Scripts/Manager/AudioManager/`): `IAudioService`, `AudioManager`(순수 C# 싱글턴, IAssetService+AudioPlayerView 주입), `AudioPlayerView`(MonoBehaviour, BGM/SFX AudioSource 2개).
- 이벤트→SFX 매핑(`10_ProjectA/01_Scripts/Audio/GameAudioBinder.cs`): 처치·레벨업·경험치·사망 + 피격(HP 감소 단일 지점) 5종 구독, 진입 시 `bgm/battle` 재생.
- DI: `GameSceneScope`에 `RegisterComponentInHierarchy<AudioPlayerView>` + `AudioManager` 싱글턴 + `RegisterEntryPoint<GameAudioBinder>` 3줄.
- 리뷰 후 SFX 로드 dedupe(`_sfxLoading`) + try/finally 예외 처리 적용(M1·M2).
- 결정: 볼륨 저장 안 함(반영만), CommonFramework는 ProjectA 이벤트 타입 미참조.

### 검증
- Gate 1 컴파일: 통과(에러 0, 도메인 리로드 완료). 리뷰 후 재수정분도 재확인.
- Gate B 씬: `GameScene`에 `AudioPlayer` GameObject + `AudioPlayerView` 부착, AudioSource 2개 Reset 자동 구성, 씬 저장.
- Gate 2 Play: **DI 배선 에러 0(통과)**. Addressables 키 미존재 에러는 예상됨 — 플레이스홀더 AudioClip을 MCP로 생성 불가해 6개 주소가 아직 비어 있음.

### 후속 변경 (2026-07-12 · "게임 시작 시 BGM")
- BGM 재생 시점을 씬 로드(Initialize) → **`IGameManager.CurrentState == Playing`** 전이로 변경. Victory/Defeat 시 StopBgm, Pause→Resume 재시작 방지 `_bgmStarted` 가드. `GameAudioBinder` 생성자에 `IGameManager` 추가(GameSceneScope에서 `.As<IGameManager>()` 기등록).
- **`bgm/battle` Addressable 등록 완료**: 기존 파일 `Assets/10_ProjectA/53_Sound_Resources/bgm/battle.mp3`(guid bd071d987c6924ab0ab421685026fadd)를 `Default Local Group`에 주소 `bgm/battle`로 등록.
- `AudioPlayer`의 AudioSource 2개 `spatialBlend=0`(2D) 설정 — 거리 감쇠 방지.
- 검증: 컴파일 0, DI 해소 정상, Idle 진입 시 BGM 미재생(설계대로), 이전 로드 실패 에러 소멸 확인.

### 다음 실행 규칙 (미완 항목)
- **SFX 클립 5개는 아직 Addressables 미등록**: `sfx/enemy_death`, `sfx/level_up`, `sfx/exp_pickup`, `sfx/game_over`, `sfx/player_hit`. 파일이 준비되면 등록. 등록 전까지 해당 SFX 로드 에러는 정상.
- **Gate 4(사용자 확인)**: GameSelect → Start 버튼으로 Playing 진입 후 BGM이 실제로 들리는지 확인. (MCP는 UI 클릭 불가라 여기까지만 자동 검증됨)
- 볼륨 슬라이더 옵션 UI는 범위 밖 — 붙일 때 `SetBgmVolume`/`SetSfxVolume` 배선.
