using Leap.Unity.GestureMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Carousel : MonoBehaviour
{
    [Tooltip("The stages in the stack, which cards and/or the highlighter move between")]
    [SerializeField] private List<Stage> _stages;

    [Tooltip("The prefab to use when spawning cards into the stack")]
    [SerializeField] private SwipeCard _cardPrefab;

    [Tooltip("OPTIONAL: This object will be placed over the highlighted stage to act as a visual indicator for stacks with multiple highlightable stages")]
    [SerializeField] private SwipeObject _highlighter;

    [Tooltip("Enable this to make left swipes go right, and vice versa")]
    [SerializeField] private bool _invertDirection = false;

    [Range(0, 0.5f)]
    [Tooltip("This is a cutoff applied to the start/end of the opacity ramp for cards: leave on zero to ramp across the full transition")]
    [SerializeField] private float _opacityCutoff = 0.0f;

    [Space]

    [SerializeField] private FadeUI _tooltip = null;

    [Space]

    [SerializeField] private InteractionType _defaultInteractionType = InteractionType.SLIDE_SINGLE_ITEM_PROGRESS_IMMEDIATELY_NO_FEEDBACK;
    public InteractionType DefaultInteractionType => _defaultInteractionType;

    public Action<SwipeDirection> OnAdvanced;

    public float Progress => _progress;
    private float _progress = -1;

    private FadeUI _fader = null;
    public FadeUI Fader
    {
        get
        {
            if (_fader == null)
                _fader = gameObject?.GetComponent<FadeUI>();
            return _fader;
        }
    }

    public FadeUI TooltipFader
    {
        get
        {
            return _tooltip;
        }
    }

    private int FirstStage => 0;
    private int LastStage => _stages.Count - 1;

    private Transform _cardHolder; 
    private int _cardCount;

    //Cards that have been swiped off the end, ready to pull back round (basically an invisible bit of the carousel)
    //Cards at the lowest index should swipe back to the end, cards at the highest index should swipe forward to the start
    private List<SwipeCard> _cardsOffEnd = new List<SwipeCard>();

    void Start()
    {
        if (_defaultInteractionType == InteractionType.NO_INTERACTION) 
            return;

        //We forcibly add a "fake" first/last stage, with opacity zero, that we use for queued cards
        _stages.Insert(0, (Stage)_stages[FirstStage].Clone());
        _stages.Add((Stage)_stages[LastStage].Clone());
        _stages[FirstStage].Opacity = 0; 
        _stages[FirstStage].Highlightable = false;
        _stages[LastStage].Opacity = 0; 
        _stages[LastStage].Highlightable = false;

        //Make sure the card count is no lower than the number of stages
        _cardCount = _cardPrefab.Variants;
        if (_cardCount < _stages.Count)
        {
            Debug.LogWarning("WARNING: There are less card variants than positions in this carousel - some repitition will occur");
            _cardCount = _stages.Count;
        }

        //Disable gameobjects associated with the transforms for the stages (we might have in-scene markers to hide)
        for (int i = 0; i < _stages.Count; i++)
            _stages[i].Transform.gameObject.SetActive(false);

        //Highlight the first highlightable marker we come across
        for (int i = 0; i < _stages.Count; i++)
        {
            if (_stages[i].Highlightable)
            {
                _stages[i].Highlighted = true;
                _highlighter?.MoveBetween(_stages[i]);
                break;
            }
        }

        //We spawn our cards in separate zero'd transforms so that we can sort out render order
        _cardHolder = new GameObject("Cards").transform;
        _cardHolder.SetParent(this.transform);
        _cardHolder.localScale = Vector3.one;
        _cardHolder.SetLocalPositionAndRotation(Vector3.one, Quaternion.identity);
        for (int i = 0; i < _stages.Count; i++)
        {
            _stages[i].SpawnedCardParent = new GameObject("Stage " + i).transform;
            _stages[i].SpawnedCardParent.SetParent(_cardHolder);
            _stages[i].SpawnedCardParent.localScale = Vector3.one;
            _stages[i].SpawnedCardParent.SetLocalPositionAndRotation(Vector3.one, Quaternion.identity);
        }
        for (int i = 0; i < _stages.Count; i++)
        {
            _stages[i].SpawnedCardParent.SetSiblingIndex(_stages[i].Transform.GetSiblingIndex());
        }

        //Spawn a card at each marker
        for (int i = 0; i < _stages.Count; i++)
            SpawnCard(i);
        //Now spawn the remaining cards at the "off end" stack ready to pull through
        for (int i = _stages.Count; i < _cardCount; i++)
            SpawnCard(LastStage);

        SetProgress(0.0f, true);
        UpdateHighlights();
    }

    /* Spawns a card at a specified stage */
    private void SpawnCard(int stage)
    {
        if (_defaultInteractionType == InteractionType.NO_INTERACTION) 
            return;

        //Don't spawn cards in any invalid slot range
        if (stage > LastStage || stage < FirstStage)
            return;

        //Don't spawn cards multiple times for slots that are visible
        if ((stage != FirstStage && stage != LastStage) && _stages[stage].Card != null)
        {
            Debug.LogWarning("Can't spawn a card at " + stage + " - there's already one there.");
            return;
        }

        SwipeCard card = Instantiate(_cardPrefab, _stages[stage].SpawnedCardParent);
        card.MoveBetween(_stages[stage]);

        //If spawning at first/last, if a card is already there we throw them back to the invisible queue
        if (stage == FirstStage || stage == LastStage)
        {
            if (_stages[stage].Card == null)
                _stages[stage].Card = card;
            else
                _cardsOffEnd.Add(card);
        }
        else
        {
            _stages[stage].Card = card;
        }
    }

    /* Sets progress on a value between -1 and 1 (progress being distance to next stage in the stack, up or down) */
    public void SetProgress(float progress, bool forceUpdate = false)
    {
        if (_defaultInteractionType == InteractionType.NO_INTERACTION) 
            return;

        float progressClamped = Mathf.Clamp(progress, -1.0f, 1.0f);
        if (!forceUpdate && progressClamped == _progress)
            return;
        _progress = progressClamped;

        //For sanity: reset all SwipeObjects to be at progress zero
        int highlightedIndex = _stages.IndexOf(_stages.FirstOrDefault(o => o.Highlighted));
        _highlighter?.MoveBetween(_stages[highlightedIndex]);
        for (int i = 1; i < _stages.Count - 1; i++)
            _stages[i].Card?.MoveBetween(_stages[i]);

        SwipeDirection direction = _progress > 0.0f ? SwipeDirection.RIGHT : SwipeDirection.LEFT;
        if (_invertDirection) direction = direction.Invert();

        // If there is another highlightable stage in the direction we're swiping,
        // move the highlight stage towards that spot, rather than shifting the whole stack
        if (HasHighlightableMarker(direction))
        {
            if (direction == SwipeDirection.RIGHT)
                _highlighter?.MoveBetween(_stages[highlightedIndex], _stages[highlightedIndex + 1], Mathf.Abs(progress), _opacityCutoff);
            else
                _highlighter?.MoveBetween(_stages[highlightedIndex], _stages[highlightedIndex - 1], Mathf.Abs(progress), _opacityCutoff);
        }
        //Otherwise, shift the entire stack in the swipe direction
        else
        {
            for (int i = 1; i < _stages.Count - 1; i++)
            {
                if (direction == SwipeDirection.RIGHT)
                    _stages[i].Card?.MoveBetween(_stages[i], _stages[i - 1], Mathf.Abs(progress), _opacityCutoff);
                else
                    _stages[i].Card?.MoveBetween(_stages[i], _stages[i + 1], Mathf.Abs(progress), _opacityCutoff);
            }
        }
    }

    /* Update the highlight status for the cards */
    public void UpdateHighlights(float highlightUntil = 0.5f)
    {
        if (_defaultInteractionType == InteractionType.NO_INTERACTION) 
            return;

        for (int i = 1; i < _stages.Count - 1; i++)
            _stages[i].Card?.Highlight(_stages[i].Highlighted && Mathf.Abs(_progress) < highlightUntil);
    }

    /* Show/hide the tooltip, if we have one, and let cards know we're active */
    public void SetActiveState(bool active)
    {
        if (active)
        {
            if (_tooltip != null && !_tooltip.IsVisible)
                _tooltip.FadeIn();
        }
        else
        {
            if (_tooltip != null && _tooltip.IsVisible)
                _tooltip.FadeOut();
        }

        _stages.ForEach(o => o?.Card?.SetCarouselActive(active));
        _cardsOffEnd.ForEach(o => o?.SetCarouselActive(active));
    }

    /* Performs the logic for a completed cycle */
    public void AdvanceStage(SwipeDirection direction)
    {
        if (_defaultInteractionType == InteractionType.NO_INTERACTION) 
            return;

        if (_invertDirection)
            direction = direction.Invert();

        switch (direction)
        {
            case SwipeDirection.RIGHT:
                //See if we have a highlightable marker to the right of the currently highlighted one
                //If we do, we should shift the highlight to that one, before shifting the stack along
                for (int i = 1; i < _stages.Count; i++)
                {
                    if (_stages[i - 1].Highlighted && _stages[i].Highlightable)
                    {
                        _stages[i - 1].Highlighted = false;
                        _stages[i].Highlighted = true;
                        OnAdvanced?.Invoke(direction);
                        return;
                    }
                }

                //Remember the one at the start we're about to overwrite
                _cardsOffEnd.Insert(0, _stages[FirstStage].Card);

                //Move all cards down a marker
                for (int i = 0; i < _stages.Count - 1; i++)
                {
                    _stages[i].Card = _stages[i + 1].Card;
                    _stages[i].Card.transform.SetParent(_stages[i].SpawnedCardParent);
                }

                //Sort the end out
                _stages[LastStage].Card = _cardsOffEnd[_cardsOffEnd.Count - 1];
                _cardsOffEnd.RemoveAt(_cardsOffEnd.Count - 1);
                break;

            case SwipeDirection.LEFT:
                //See if we have a highlightable marker to the left of the currently highlighted one
                //If we do, we should shift the highlight to that one, before shifting the stack along
                for (int i = _stages.Count - 2; i > 0; i--)
                {
                    if (_stages[i + 1].Highlighted && _stages[i].Highlightable)
                    {
                        _stages[i + 1].Highlighted = false;
                        _stages[i].Highlighted = true;
                        OnAdvanced?.Invoke(direction);
                        return;
                    }
                }

                //Remember the one at the end we're about to overwrite
                _cardsOffEnd.Add(_stages[LastStage].Card);

                //Move all cards up a marker
                for (int i = _stages.Count - 1; i > 0; i--)
                {
                    _stages[i].Card = _stages[i - 1].Card;
                    _stages[i].Card.transform.SetParent(_stages[i].SpawnedCardParent);
                }

                //Sort the start out
                _stages[FirstStage].Card = _cardsOffEnd[0];
                _cardsOffEnd.RemoveAt(0);
                break;
        }

        SetProgress(_progress, true);
        OnAdvanced?.Invoke(direction);
    }

    /* See if there is another highlightable marker to the left/right of the currently highlighted one */
    private bool HasHighlightableMarker(SwipeDirection direction)
    {
        if (_defaultInteractionType == InteractionType.NO_INTERACTION) 
            return false;

        bool lastHighlighted = false;
        switch (direction)
        {
            case SwipeDirection.RIGHT:
                for (int i = 0; i < _stages.Count; i++)
                {
                    if (lastHighlighted && _stages[i].Highlightable)
                    {
                        return true;
                    }
                    if (_stages[i].Highlighted)
                    {
                        lastHighlighted = true;
                    }
                }
                break;

            case SwipeDirection.LEFT:
                for (int i = _stages.Count - 1; i > 0; i--)
                {
                    if (lastHighlighted && _stages[i].Highlightable)
                    {
                        return true;
                    }
                    if (_stages[i].Highlighted)
                    {
                        lastHighlighted = true;
                    }
                }
                break;
        }
        return false;
    }

    /* User interacts with card - let our highlighted card know (if it exists) */
    public void InteractWithHighlightedCard()
    {
        if (_defaultInteractionType == InteractionType.NO_INTERACTION) 
            return;

        for (int i = 1; i < _stages.Count - 1; i++)
        {
            //NOTE: We check to see if the *CARD* is highlighted, not the marker
            //      This is because the marker may be highlighted, but the card may not due to the level of progression between markers.
            if (_stages[i].Card == null || !_stages[i].Card.Highlighted)
                continue;

            _stages[i].Card.Interact();
        }
    }

    [System.Serializable]
    public class Stage : ICloneable
    {
        public Transform Transform;

        [Range(0, 1)]
        [Tooltip("The opacity to render the card at when in this stage")]
        public float Opacity;

        [Tooltip("Enable this to have the card in this stage highlight when at progress zero")]
        public bool Highlightable;

        [HideInInspector] public SwipeCard Card;
        [HideInInspector] public Transform SpawnedCardParent;
        [HideInInspector] public bool Highlighted;

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}

public static class SwipeStackUtils
{
    public static SwipeDirection Invert(this SwipeDirection direction)
    {
        return direction == SwipeDirection.LEFT ? SwipeDirection.RIGHT : SwipeDirection.LEFT;
    }
}

public enum TapType
{
    SINGLE_TAP,
    DOUBLE_TAP,
}