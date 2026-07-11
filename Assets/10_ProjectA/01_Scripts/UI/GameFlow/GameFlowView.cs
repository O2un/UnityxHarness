using System;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace O2un.UI
{
    public sealed class GameFlowView : MonoBehaviour
    {
        [SerializeField] private GameObject _startPanel;
        [SerializeField] private GameObject _hudPanel;
        [SerializeField] private GameObject _victoryPanel;
        [SerializeField] private GameObject _defeatPanel;

        [SerializeField] private Button _startButton;
        [SerializeField] private Button _victoryRestartButton;
        [SerializeField] private Button _defeatRestartButton;
        [SerializeField] private Button _victoryGameSelectButton;
        [SerializeField] private Button _defeatGameSelectButton;

        [SerializeField] private TMP_Text _victoryResultText;
        [SerializeField] private TMP_Text _defeatResultText;

        public event Action GameSelectRequested;

        public void Bind(GameFlowVM vm)
        {
            vm.ShowStart.Subscribe(x => SetActive(_startPanel, x)).AddTo(this);
            vm.ShowHud.Subscribe(x => SetActive(_hudPanel, x)).AddTo(this);
            vm.ShowVictory.Subscribe(x => SetActive(_victoryPanel, x)).AddTo(this);
            vm.ShowDefeat.Subscribe(x => SetActive(_defeatPanel, x)).AddTo(this);

            vm.ResultText.Subscribe(text =>
            {
                if (null != _victoryResultText) _victoryResultText.SetText(text);
                if (null != _defeatResultText) _defeatResultText.SetText(text);
            }).AddTo(this);

            if (null != _startButton)
                _startButton.OnClickAsObservable().Subscribe(_ => vm.OnStartClicked()).AddTo(this);
            if (null != _victoryRestartButton)
                _victoryRestartButton.OnClickAsObservable().Subscribe(_ => vm.OnRestartClicked()).AddTo(this);
            if (null != _defeatRestartButton)
                _defeatRestartButton.OnClickAsObservable().Subscribe(_ => vm.OnRestartClicked()).AddTo(this);
            if (null != _victoryGameSelectButton)
                _victoryGameSelectButton.OnClickAsObservable().Subscribe(_ => GameSelectRequested?.Invoke()).AddTo(this);
            if (null != _defeatGameSelectButton)
                _defeatGameSelectButton.OnClickAsObservable().Subscribe(_ => GameSelectRequested?.Invoke()).AddTo(this);
        }

        private static void SetActive(GameObject panel, bool active)
        {
            if (null != panel)
            {
                panel.SetActive(active);
            }
        }
    }
}
