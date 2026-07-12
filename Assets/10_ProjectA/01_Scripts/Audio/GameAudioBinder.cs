using System;
using Cysharp.Threading.Tasks;
using O2un.Actors;
using O2un.Combat;
using O2un.Manager;
using O2un.Progression;
using R3;
using VContainer.Unity;

namespace O2un.Audio
{
    public sealed class GameAudioBinder : IInitializable, IDisposable
    {
        private const string BGM_BATTLE = "bgm/battle";
        private const string SFX_ENEMY_DEATH = "sfx/enemy_death";
        private const string SFX_LEVEL_UP = "sfx/level_up";
        private const string SFX_EXP_PICKUP = "sfx/exp_pickup";
        private const string SFX_GAME_OVER = "sfx/game_over";
        private const string SFX_PLAYER_HIT = "sfx/player_hit";

        private readonly IAudioService _audio;
        private readonly IGameManager _gameManager;
        private readonly IEnemyKillEvent _enemyKill;
        private readonly IExperienceReader _experience;
        private readonly IExpGainedSource _expGained;
        private readonly PlayerHealthAdapter _playerHealth;

        private readonly CompositeDisposable _disposables = new();

        private int _lastHp;
        private bool _bgmStarted;

        public GameAudioBinder(
            IAudioService audio,
            IGameManager gameManager,
            IEnemyKillEvent enemyKill,
            IExperienceReader experience,
            IExpGainedSource expGained,
            PlayerHealthAdapter playerHealth)
        {
            _audio = audio;
            _gameManager = gameManager;
            _enemyKill = enemyKill;
            _experience = experience;
            _expGained = expGained;
            _playerHealth = playerHealth;
        }

        public void Initialize()
        {
            _lastHp = _playerHealth.CurrentHP.CurrentValue;

            _enemyKill.OnKilled.Subscribe(_ => _audio.PlaySfx(SFX_ENEMY_DEATH)).AddTo(_disposables);
            _experience.OnLevelUp.Subscribe(_ => _audio.PlaySfx(SFX_LEVEL_UP)).AddTo(_disposables);
            _expGained.OnGained.Subscribe(_ => _audio.PlaySfx(SFX_EXP_PICKUP)).AddTo(_disposables);
            _playerHealth.OnDeath.Subscribe(_ => _audio.PlaySfx(SFX_GAME_OVER)).AddTo(_disposables);
            _playerHealth.CurrentHP.Subscribe(OnHpChanged).AddTo(_disposables);

            _gameManager.CurrentState.Subscribe(OnGameStateChanged).AddTo(_disposables);
        }

        private void OnGameStateChanged(GameState state)
        {
            if (GameState.Playing == state)
            {
                if (false == _bgmStarted)
                {
                    _bgmStarted = true;
                    _audio.PlayBgmAsync(BGM_BATTLE).Forget();
                }
                return;
            }

            if (GameState.Victory == state || GameState.Defeat == state)
            {
                _bgmStarted = false;
                _audio.StopBgm();
            }
        }

        private void OnHpChanged(int hp)
        {
            if (hp < _lastHp && 0 < hp)
            {
                _audio.PlaySfx(SFX_PLAYER_HIT);
            }

            _lastHp = hp;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
