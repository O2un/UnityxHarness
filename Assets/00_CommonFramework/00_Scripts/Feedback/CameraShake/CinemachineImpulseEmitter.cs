using Unity.Cinemachine;
using UnityEngine;

namespace O2un.Feedback
{
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public sealed class CinemachineImpulseEmitter : MonoBehaviour, IImpulseEmitter
    {
        private CinemachineImpulseSource _source;

        private void Awake()
        {
            _source = GetComponent<CinemachineImpulseSource>();
        }

        public void Emit(float force)
        {
            if (null == _source)
            {
                return;
            }

            // Awake에서 설정하면 이후 Instance가 재생성되며 값이 유실된다. 발행 시점에 보장한다.
            CinemachineImpulseManager.Instance.IgnoreTimeScale = true;
            _source.GenerateImpulseWithForce(force);
        }
    }
}
