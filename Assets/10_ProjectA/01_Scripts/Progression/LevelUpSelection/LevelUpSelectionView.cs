using System.Collections.Generic;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace O2un.Progression
{
    public sealed class LevelUpSelectionView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button[] _candidateButtons;
        [SerializeField] private TMP_Text[] _candidateLabels;

        public void Bind(LevelUpSelectionViewModel vm)
        {
            vm.IsVisible.Subscribe(x => _panel.SetActive(x)).AddTo(this);

            vm.CandidateLabels.Subscribe(labels => ApplyLabels(vm, labels)).AddTo(this);
        }

        private void ApplyLabels(LevelUpSelectionViewModel vm, IReadOnlyList<string> labels)
        {
            int slotCount = Mathf.Min(_candidateButtons.Length, _candidateLabels.Length);
            for (int i = 0; i < slotCount; i++)
            {
                Button button = _candidateButtons[i];
                if (i >= labels.Count)
                {
                    button.gameObject.SetActive(false);
                    continue;
                }

                int index = i;
                _candidateLabels[i].SetText(labels[i]);
                button.gameObject.SetActive(true);
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => vm.ChooseCandidate(index));
            }
        }
    }
}
