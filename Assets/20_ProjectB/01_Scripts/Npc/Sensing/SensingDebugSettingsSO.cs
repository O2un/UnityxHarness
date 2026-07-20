using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [CreateAssetMenu(menuName = "ProjectB/Platformer/SensingDebugSettings")]
    public sealed class SensingDebugSettingsSO : ScriptableObject
    {
        [SerializeField] private bool _drawWhenSelectedOnly = true;

        [SerializeField] private bool _showSight = true;
        [SerializeField] private bool _showHearing = true;
        [SerializeField] private bool _showGroundAndWall = true;
        [SerializeField] private bool _showRanges = true;
        [SerializeField] private bool _showLabels = true;

        [SerializeField] private Color _sightColor = new(1f, 1f, 1f, 0.35f);
        [SerializeField] private Color _sightVisibleColor = new(1f, 0.35f, 0.2f, 0.6f);
        [SerializeField] private Color _hearingColor = new(0.3f, 0.7f, 1f, 0.35f);
        [SerializeField] private Color _hearingActiveColor = new(0.3f, 1f, 1f, 0.9f);
        [SerializeField] private Color _probeClearColor = new(0.4f, 1f, 0.4f, 0.9f);
        [SerializeField] private Color _probeHitColor = new(1f, 0.85f, 0.2f, 0.9f);
        [SerializeField] private Color _rangeColor = new(0.6f, 0.6f, 0.6f, 0.3f);
        [SerializeField] private Color _lastKnownColor = new(1f, 0.5f, 0.9f, 0.9f);
        [SerializeField] private Color _labelColor = Color.white;

        [SerializeField, Min(0f)] private float _lineThickness = 2f;
        [SerializeField, Min(0f)] private float _lastKnownMarkerSize = 0.25f;

        public bool DrawWhenSelectedOnly => _drawWhenSelectedOnly;

        public bool ShowSight => _showSight;
        public bool ShowHearing => _showHearing;
        public bool ShowGroundAndWall => _showGroundAndWall;
        public bool ShowRanges => _showRanges;
        public bool ShowLabels => _showLabels;

        public Color SightColor => _sightColor;
        public Color SightVisibleColor => _sightVisibleColor;
        public Color HearingColor => _hearingColor;
        public Color HearingActiveColor => _hearingActiveColor;
        public Color ProbeClearColor => _probeClearColor;
        public Color ProbeHitColor => _probeHitColor;
        public Color RangeColor => _rangeColor;
        public Color LastKnownColor => _lastKnownColor;
        public Color LabelColor => _labelColor;

        public float LineThickness => _lineThickness;
        public float LastKnownMarkerSize => _lastKnownMarkerSize;
    }
}
