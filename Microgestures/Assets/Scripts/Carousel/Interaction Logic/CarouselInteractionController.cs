using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using Leap.Unity;

public class CarouselInteractionController : MonoBehaviour
{
    [SerializeField] private List<Interaction> _interactions;

    [SerializeField] private DoubleTapDetector _doubleTapDetector;

    [SerializeField] private bool _forceType = false;
    [SerializeField] private InteractionType _forcedType;

    [Space]

    [SerializeField] private Chirality _chirality = Chirality.Right;

    public Action<Chirality> OnChiralityChanged;
    public Chirality Chirality => _chirality;

    private Carousel _carousel;
    public Action<Carousel> OnActiveCarouselChanged;

    public InteractionType InteractionType
    {
        get
        {
            return _interactionType;
        }
        set
        {
            SetCarousel(_carousel, value);
        }
    }
    private InteractionType _interactionType;

    private CarouselMgLogic _activeDetector = null;

    public float AcceptDistance
    {
        get
        {
            if (_activeDetector is CarouselMgLogicSlide)
                return ((CarouselMgLogicSlide)_activeDetector).AcceptAfterProgress;
            return 1.0f;
        }
    }

    public float RealValue
    {
        get
        {
            if (_activeDetector == null)
                return 0.0f;
            return _activeDetector.RealValue;
        }
    }

    public bool Interacting
    {
        get
        {
            if (_activeDetector == null)
                return false;
            return _activeDetector.Interacting;
        }
    }

    public void Awake()
    {
        if (_doubleTapDetector == null)
            _doubleTapDetector = FindObjectOfType<DoubleTapDetector>(true);

        _doubleTapDetector.OnDoubleTap.AddListener(HandleDoubleTap);
    }

    private void Start()
    {
        SetChirality(_chirality);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F4))
        {
            SetChirality(_chirality == Chirality.Left ? Chirality.Right : Chirality.Left);
        }
    }

    /* Set interaction chirality */
    public void SetChirality(Chirality chirality)
    {
        _chirality = chirality;
        _doubleTapDetector.chirality = _chirality;

        OnChiralityChanged?.Invoke(_chirality);
    }

    /* Set the active carousel and interaction method */
    public void SetCarousel(Carousel carousel, InteractionType type)
    {
        bool carouselChanged = _carousel != carousel;
        ResetAll();

        if (_forceType)
            type = _forcedType;

        if (carouselChanged && _carousel != null)
            _carousel.SetActiveState(false);

        _carousel = carousel;
        _interactionType = type;

        for (int i = 0; i < _interactions.Count; i++)
        {
            if (_interactions[i].Type == _interactionType)
            {
                _interactions[i].Detector.Advance += _carousel.AdvanceStage;
                _interactions[i].Detector.Progress += HandleProgress;

                _activeDetector = _interactions[i].Detector;
            }

            _interactions[i].Detector.enabled = _interactions[i].Type == _interactionType;
            _interactions[i].Detector.chirality = _chirality;
        }

        _carousel.SetProgress(_carousel.Progress, true);
        _carousel.UpdateHighlights();
        _carousel.SetActiveState(true);

        if (carouselChanged)
            OnActiveCarouselChanged?.Invoke(_carousel);
    }

    /* Reset stuff */
    public void ResetAll()
    {
        for (int i = 0; i < _interactions.Count; i++)
        {
            _interactions[i].Detector.enabled = false;

            if (_carousel != null)
            {
                _interactions[i].Detector.Advance -= _carousel.AdvanceStage;
                _interactions[i].Detector.Progress -= HandleProgress;
            }
        }
    }

    private void HandleProgress(float progress)
    {
        _carousel?.SetProgress(progress);
        _carousel?.UpdateHighlights(AcceptDistance);
    }

    private void HandleDoubleTap()
    {
        _carousel?.InteractWithHighlightedCard();
    }

    [Serializable]
    public class Interaction
    {
        public InteractionType Type;
        public CarouselMgLogic Detector;
    }
}

public enum InteractionType
{
    INERTIA_MULTIPLE_ITEMS = 2,
    SLIDE_MULTIPLE_ITEMS = 4,
    SLIDE_SINGLE_ITEM_PROGRESS_IMMEDIATELY = 5,
    SLIDE_SINGLE_ITEM_PROGRESS_IMMEDIATELY_NO_FEEDBACK = 6,
    NO_INTERACTION = 7,
}
