using System;
using R3;

namespace O2un.ProjectB.Platformer
{
    public sealed class MeleeComboModule : IDisposable
    {
        private readonly int _stageCount;
        private readonly float _inputBufferTime;

        private int _currentStage;
        private bool _isComboWindowOpen;
        private float _bufferTimer;
        private bool _transitioned;

        public bool IsAttacking => _currentStage > 0;
        public int CurrentStage => _currentStage;
        public bool IsComboWindowOpen => _isComboWindowOpen;
        public bool HasBufferedInput => _bufferTimer > 0f;

        private readonly Subject<int> _onAttackTriggered = new();
        private readonly Subject<Unit> _onComboReset = new();

        public Observable<int> OnAttackTriggered => _onAttackTriggered;
        public Observable<Unit> OnComboReset => _onComboReset;

        public MeleeComboModule(int stageCount, float inputBufferTime)
        {
            _stageCount = stageCount;
            _inputBufferTime = inputBufferTime;
        }

        public void PressAttack()
        {
            if (0 >= _stageCount)
            {
                return;
            }

            if (0 == _currentStage)
            {
                TriggerStage(1);
                return;
            }

            if (true == _isComboWindowOpen && _currentStage < _stageCount)
            {
                TriggerStage(_currentStage + 1);
                return;
            }

            _bufferTimer = _inputBufferTime;
        }

        public void OpenComboWindow()
        {
            if (0 == _currentStage)
            {
                return;
            }

            // 새 클립의 윈도우가 열렸다면 이전 클립의 잔여 AttackEnd 보호는 더 이상 필요 없음
            _transitioned = false;
            _isComboWindowOpen = true;

            if (true == HasBufferedInput && _currentStage < _stageCount)
            {
                _bufferTimer = 0f;
                TriggerStage(_currentStage + 1);
            }
        }

        public void CloseComboWindow()
        {
            _isComboWindowOpen = false;
        }

        public void NotifyStageEnd()
        {
            if (0 == _currentStage)
            {
                return;
            }

            if (true == _transitioned)
            {
                _transitioned = false;
                return;
            }

            ResetCombo();
        }

        public void Cancel()
        {
            if (0 == _currentStage)
            {
                return;
            }

            ResetCombo();
        }

        public void Tick(float dt)
        {
            if (_bufferTimer > 0f)
            {
                _bufferTimer -= dt;
                if (_bufferTimer <= 0f)
                {
                    _bufferTimer = 0f;
                }
            }
        }

        private void TriggerStage(int stage)
        {
            // 전 단계 클립의 AttackEnd가 새 단계 재생 중 도착해도 리셋되지 않도록 전이를 표시
            _transitioned = _currentStage > 0;
            _currentStage = stage;
            _isComboWindowOpen = false;
            _onAttackTriggered.OnNext(stage);
        }

        private void ResetCombo()
        {
            _currentStage = 0;
            _isComboWindowOpen = false;
            _bufferTimer = 0f;
            _transitioned = false;
            _onComboReset.OnNext(Unit.Default);
        }

        public void Dispose()
        {
            _onAttackTriggered.Dispose();
            _onComboReset.Dispose();
        }
    }
}
