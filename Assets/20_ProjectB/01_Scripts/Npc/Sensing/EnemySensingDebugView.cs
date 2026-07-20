using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace O2un.ProjectB.Platformer
{
    public sealed class EnemySensingDebugView : MonoBehaviour
    {
        [SerializeField] private SensingDebugSettingsSO _settings;
        [SerializeField] private Enemy2DSensorView _sensor;
        [SerializeField] private Npc2DContext _context;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (null == _settings || true == _settings.DrawWhenSelectedOnly)
            {
                return;
            }

            Draw();
        }

        private void OnDrawGizmosSelected()
        {
            if (null != _settings && false == _settings.DrawWhenSelectedOnly)
            {
                return;
            }

            Draw();
        }

        private void Draw()
        {
            if (null == _settings || null == _sensor || null == _sensor.Data)
            {
                return;
            }

            EnemySensingData data = _sensor.Data;
            IEnemy2DBlackboard blackboard = null != _context ? _context.Blackboard : null;

            Vector2 eye = _sensor.EyeOrigin.position;
            float facing = null != blackboard ? blackboard.Facing : (0f <= transform.localScale.x ? 1f : -1f);

            if (true == _settings.ShowRanges)
            {
                DrawRanges(eye, data);
            }

            if (true == _settings.ShowSight)
            {
                DrawSight(eye, facing, data, blackboard);
            }

            if (true == _settings.ShowHearing)
            {
                DrawHearing(eye, data, blackboard);
            }

            if (true == _settings.ShowGroundAndWall)
            {
                DrawProbes(eye, facing, data, blackboard);
            }

            if (true == _settings.ShowLabels)
            {
                DrawLabels(eye, blackboard);
            }
        }

        private void DrawRanges(Vector2 eye, EnemySensingData data)
        {
            Handles.color = _settings.RangeColor;
            Handles.DrawWireDisc(eye, Vector3.forward, data.AggroRange, _settings.LineThickness);
        }

        private void DrawSight(Vector2 eye, float facing, EnemySensingData data, IEnemy2DBlackboard blackboard)
        {
            bool isVisible = null != blackboard && true == blackboard.IsPlayerVisible;

            Vector3 forward = new(facing, 0f, 0f);
            Vector3 arcStart = Quaternion.AngleAxis(-data.SightAngle * 0.5f, Vector3.forward) * forward;

            Handles.color = isVisible ? _settings.SightVisibleColor : _settings.SightColor;
            Handles.DrawSolidArc(eye, Vector3.forward, arcStart, data.SightAngle, data.SightDistance);

            if (null == blackboard || false == blackboard.HasTarget)
            {
                return;
            }

            Handles.color = isVisible ? _settings.SightVisibleColor : _settings.ProbeHitColor;
            Handles.DrawLine(eye, blackboard.TargetPosition, _settings.LineThickness);
        }

        private void DrawHearing(Vector2 eye, EnemySensingData data, IEnemy2DBlackboard blackboard)
        {
            bool heard = null != blackboard && true == blackboard.HeardSoundThisTick;

            Handles.color = heard ? _settings.HearingActiveColor : _settings.HearingColor;
            Handles.DrawWireDisc(transform.position, Vector3.forward, data.HearingRadius, _settings.LineThickness);
        }

        private void DrawProbes(Vector2 eye, float facing, EnemySensingData data, IEnemy2DBlackboard blackboard)
        {
            Vector2 groundOrigin = _sensor.GroundProbe.position;
            groundOrigin.x += facing * data.WallProbeDistance;

            bool groundAhead = null == blackboard || true == blackboard.GroundAhead;
            Handles.color = groundAhead ? _settings.ProbeClearColor : _settings.ProbeHitColor;
            Handles.DrawLine(groundOrigin, groundOrigin + Vector2.down * data.GroundProbeDistance, _settings.LineThickness);

            bool wallAhead = null != blackboard && true == blackboard.WallAhead;
            Handles.color = wallAhead ? _settings.ProbeHitColor : _settings.ProbeClearColor;
            Handles.DrawLine(eye, eye + new Vector2(facing, 0f) * data.WallProbeDistance, _settings.LineThickness);
        }

        private void DrawLabels(Vector2 eye, IEnemy2DBlackboard blackboard)
        {
            if (null == blackboard)
            {
                Handles.color = _settings.LabelColor;
                Handles.Label(eye + Vector2.up, $"{name}\n(not playing)");
                return;
            }

            if (true == blackboard.HasLastKnownTargetPosition)
            {
                Handles.color = _settings.LastKnownColor;
                Handles.DrawWireCube(blackboard.LastKnownTargetPosition, Vector3.one * _settings.LastKnownMarkerSize);
                Handles.DrawDottedLine(eye, blackboard.LastKnownTargetPosition, 4f);
            }

            Handles.color = _settings.LabelColor;
            Handles.Label(
                eye + Vector2.up,
                $"visible={blackboard.IsPlayerVisible}  heard={blackboard.HeardSoundThisTick}  hit={blackboard.WasHitThisTick}\n" +
                $"ground={blackboard.GroundAhead}  wall={blackboard.WallAhead}\n" +
                $"sincePerceived={blackboard.TimeSincePerceived:F2}s");
        }
#endif
    }
}
