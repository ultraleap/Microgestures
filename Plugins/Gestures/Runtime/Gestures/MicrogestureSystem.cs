

using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace Leap.Unity.GestureMachine
{
    public class MicrogestureSystem : MonoBehaviour
    {
        public enum MicrogestureState { IDLE, READY, CONTACTING }

        /// <summary>
        /// Current state of the Microgesture System
        /// IDLE = hand not in correct microgesture pose
        /// READY = hand in correct pose, but not contacting
        /// CONTACTING = hand in correct pose, and contacting
        /// </summary>
        public MicrogestureState State { get; private set; }


        public bool usePoseToActivate = true;

        public Chirality Chirality => _chirality;
        private Chirality _chirality = Chirality.Right;

        [SerializeField]
        private GestureMachine _poseGesture, _contactGesture, _slideGesture;
        [SerializeField]
        private ThumbFlickGesture thumbFlickGesture;

        [Tooltip("Called when the thumb touches the index finger, with the hand in a valid pose")]
        public UnityEvent<float> OnContactStart;

        [Tooltip("Called when the thumb stops touching the index finger, or if the hand leaves a valid pose")]
        public UnityEvent<float> OnContactEnd;

        [Tooltip("Called as the thumb slides along the index finger")]
        public UnityEvent<float> OnSlide;

        public float SlideValue { get { return Mathf.Clamp01(_slideGesture.CurrentValue); } }

        private bool _rawContact;

        public Action<MicrogestureState> OnStateChanged;

        private void Awake()
        {
            if (usePoseToActivate)
            {
                State = MicrogestureState.IDLE;
            }
            else
            {
                State = MicrogestureState.READY;
            }

            UpdateSystemChirality(_chirality);

            _poseGesture.processing = usePoseToActivate;
            _contactGesture.processing = !usePoseToActivate;
            _slideGesture.processing = !usePoseToActivate;
        }

        private void OnEnable()
        {
            _poseGesture.OnStateChange += PoseStateChange;
            _contactGesture.OnStateChange += ContactStateChange;
        }

        private void OnDisable()
        {
            _poseGesture.OnStateChange -= PoseStateChange;
            _contactGesture.OnStateChange -= ContactStateChange;
        }

        private void PoseStateChange(bool result, float val)
        {
            if (!usePoseToActivate)
            {
                result = true;
            }

            if (result)
            {
                SetState(MicrogestureState.READY);
                _contactGesture.processing = true;
                _slideGesture.processing = true;
            }
            else
            {
                SetState(MicrogestureState.IDLE);
                _contactGesture.processing = false;
                _slideGesture.processing = false;
            }
        }

        private void ContactStateChange(bool result, float val)
        {
            if (_contactGesture.GetComponent<ThumbFlickGesture>())
            {
                if (thumbFlickGesture.resultDirection == DirectionalGesture.Direction.Down)
                {
                    _rawContact = true;
                }
                else if (thumbFlickGesture.resultDirection == DirectionalGesture.Direction.Up)
                {
                    _rawContact = false;
                }
            }
            else
            {
                _rawContact = result;
            }
        }

        private void Update()
        {
            if (State == MicrogestureState.READY)
            {
                // On Contact Start
                if (_rawContact && _slideGesture.CurrentValue > 0 && _slideGesture.CurrentValue < 1)
                {
                    SetState(MicrogestureState.CONTACTING);
                    OnContactStart.Invoke(SlideValue);
                }
            }
            else if (State == MicrogestureState.CONTACTING)
            {
                // On Contact End
                if (!_rawContact || _slideGesture.CurrentValue < 0 || _slideGesture.CurrentValue > 1)
                {
                    SetState(MicrogestureState.READY);
                    OnContactEnd.Invoke(SlideValue);
                }
                else
                {
                    OnSlide.Invoke(SlideValue);
                }
            }
        }

        public void UpdateSystemChirality(Chirality chirality)
        {
            _poseGesture.chirality = chirality;
            _contactGesture.chirality = chirality;
            _slideGesture.chirality = chirality;
        }

        private void SetState(MicrogestureState newState)
        {
            if (newState != State)
            {
                OnStateChanged?.Invoke(newState);

                if (State == MicrogestureState.CONTACTING && newState == MicrogestureState.IDLE)
                {
                    OnContactEnd.Invoke(SlideValue);
                }
            }

            State = newState;
        }
    }
}