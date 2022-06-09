using UnityEngine;

namespace LittleBit.Modules.TouchInput
{
    [CreateAssetMenu(fileName = "TouchInputConfig", menuName = "Configs/Touch input config",
        order = 0)]
    public class TouchInputConfig : ScriptableObject
    {
        [SerializeField]
        [Tooltip(
            "When the finger is held on an item for at least this duration without moving, the gesture is recognized as a long tap.")]
        private float _clickDurationThreshold = 0.7f;

        [SerializeField]
        [Tooltip(
            "A double click gesture is recognized when the time between two consecutive taps is shorter than this duration.")]
        private float _doubleclickDurationThreshold = 0.5f;

        [SerializeField]
        [Tooltip(
            "This value controls how close to a vertical line the user has to perform a tilt gesture for it to be recognized as such.")]
        private float _tiltMoveDotTreshold = 0.7f;

        [SerializeField]
        [Tooltip(
            "Threshold value for detecting whether the fingers are horizontal enough for starting the tilt. Using this value you can prevent vertical finger placement to be counted as tilt gesture.")]
        private float _tiltHorizontalDotThreshold = 0.5f;

        [SerializeField]
        [Tooltip(
            "A drag is started as soon as the user moves his finger over a longer distance than this value. The value is defined as normalized value. Dragging the entire width of the screen equals 1. Dragging the entire height of the screen also equals 1.")]
        private float _dragStartDistanceThresholdRelative = 0.05f;

        [SerializeField]
        [Tooltip(
            "When this flag is enabled the drag started event is invoked immediately when the long tap time is succeeded.")]
        private bool _longTapStartsDrag = true;

        public bool LongTapStartsDrag => _longTapStartsDrag;
        public float ClickDurationThreshold => _clickDurationThreshold;
        public double DragStartDistanceThresholdRelative => _dragStartDistanceThresholdRelative;
        public double DoubleclickDurationThreshold => _doubleclickDurationThreshold;
        public double TiltMoveDotTreshold => _tiltMoveDotTreshold;
        public double TiltHorizontalDotThreshold => _tiltHorizontalDotThreshold;
    }
}

