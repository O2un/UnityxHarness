using R3;
using TMPro;
using UnityEngine;

namespace O2un.Manager
{
    public sealed class ScoreView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreText;

        public void Bind(ScoreVM vm)
        {
            vm.Score.Subscribe(x => _scoreText.SetText("{0}", x)).AddTo(this);
        }
    }
}
