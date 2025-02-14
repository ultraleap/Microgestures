using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering.UI;
using UnityEngine.UI;

public class SwipeUI : MonoBehaviour
{
    [SerializeField] private Image _swipeDot;
    [SerializeField] private Image _startMarker;
    [SerializeField] private Image _endMarker;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private RectTransform _container;
    [SerializeField] private Image _alertIcon;
    [SerializeField] private FadeUI _alertIconFader;
    [SerializeField] private Image _completionIcon;
    [SerializeField] private FadeUI _completionIconFader;

    [Space]

    [Range(0, 1)]
    [SerializeField] private float _progress;
    [SerializeField] private bool _showOnStartup = false;
    [SerializeField] private SwipeType _defaultSwipeType = SwipeType.BRING_LEFT_WITH_DOT;

    [Space]

    [SerializeField] private float _textHighlightDistance = -0.4f;
    [SerializeField] private float _textHighlightSpeed = 0.5f;
    [SerializeField] private Color _textDefaultColour = Color.black;
    [SerializeField] private Color _textHighlightColour = Color.grey;

    [Space]

    [SerializeField] private Color _dotInitialColour = Color.grey;
    [SerializeField] private Color _dotCompletedColour = Color.black;

    public Action<float> OnSwipeProgress;
    public Action OnSwipeCompleted;

    public bool IsVisible => _visible;
    private bool _visible = false;

    public float Progress => _progress;

    [HideInInspector]
    public float ShowCompletionIconAfter = 0.05f;
    [HideInInspector]
    public float HideTextAfter = 0.1f;
    [HideInInspector]
    public float ConsiderCompletedAfter = 1.0f;

    private RectTransform _rect;
    private FadeUI _textFader;
    private FadeUI _fader;

    private SwipeType _swipeType;
    private float _lastCalculatedProgress = float.MaxValue;

    public void Show(SwipeType type, Sprite alertIcon, Sprite completionIcon)
    {
        _progress = 0.0f;
        CalculateProgress(); //Reset position on current swipe type
        _swipeType = type;
        CalculateProgress(); //Configure new swipe type

        if (alertIcon != null) 
            _alertIcon.sprite = alertIcon;
        if (completionIcon != null) 
            _completionIcon.sprite = completionIcon;

        _alertIconFader.FadeIn(true);
        _completionIconFader.FadeOut(true);

        _fader.FadeIn();
        _visible = true;
    }

    public void Hide(bool immediate = false)
    {
        _fader.FadeOut(immediate);
        _visible = false;
    }

    public void SetProgress(float progress)
    {
        if (!_visible)
            return;

        _progress = progress;

        if (_progress < 0.0f)
            _progress = 0.0f;
        if (_progress > 1.0f)
            _progress = 1.0f;
    }

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _fader = GetComponent<FadeUI>();
        if (_fader == null)
            _fader = gameObject.AddComponent<FadeUI>();
        _textFader = _text.GetComponent<FadeUI>();
        if (_textFader == null)
            _textFader = _text.gameObject.AddComponent<FadeUI>();

        //Fix the text in place
        _text.transform.parent = this.transform;
        _text.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        _text.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        _text.rectTransform.pivot = new Vector2(0.5f, 0.5f);

        //Make sure markers are disabled (TODO: should probably just generate these automatically)
        _startMarker.enabled = false;
        _endMarker.enabled = false;

        //Sanity check stuff
        if (_swipeDot.rectTransform.rect.width != _swipeDot.rectTransform.rect.height)
            Debug.LogWarning("Warning: swipe dot is not square! Some transforms may not work as expected.");
        if (_startMarker.rectTransform.anchoredPosition.x != _endMarker.rectTransform.anchoredPosition.x * -1.0f)
            Debug.LogWarning("Warning: start/end markers are not same distance from ends!");
        
        if (_showOnStartup)
            Show(_defaultSwipeType, null, null);
        else
            Hide(true);
    }

    private void CalculateProgress() 
    {
        switch (_swipeType)
        {
            case SwipeType.BRING_LEFT_WITH_DOT:
                //Shift the left stretch anchor of the container, which brings everthing to the right
                _container.offsetMin = new Vector2(
                    Mathf.Lerp(0.0f, _rect.rect.width - (_swipeDot.rectTransform.anchoredPosition.x / 4.0f) - _swipeDot.rectTransform.rect.width, _progress), 
                    0.0f
                );
                break;
            case SwipeType.DOT_MOVES_ON_ITS_OWN:
                //Shift the dot's positition from left marker to right marker within the container
                _swipeDot.rectTransform.position = new Vector3(
                    Mathf.Lerp(_startMarker.rectTransform.position.x, _endMarker.rectTransform.position.x, _progress),
                    _swipeDot.rectTransform.position.y,
                    _swipeDot.rectTransform.position.z
                );
                break;
            case SwipeType.DOT_EXTENDS_TO_END:
                //Stretch the dot from the left to the right within the container 
                _swipeDot.rectTransform.sizeDelta = new Vector2(
                    Mathf.Lerp(_swipeDot.rectTransform.sizeDelta.y, _rect.rect.width - (_startMarker.rectTransform.anchoredPosition.x / 4.0f), _progress),
                    _swipeDot.rectTransform.sizeDelta.y
                );
                _swipeDot.rectTransform.anchoredPosition = new Vector2(
                    Mathf.Lerp(_startMarker.rectTransform.anchoredPosition.x, _rect.rect.width / 2.0f, _progress),
                    _swipeDot.rectTransform.anchoredPosition.y
                );
                break;
        }

        //Show/hide text based on progress
        if (_progress > HideTextAfter && _textFader.IsVisible)
        {
            _textFader.FadeOut();
        }
        else if (_progress < HideTextAfter && !_textFader.IsVisible)
        {
            _textFader.FadeIn();
        }

        //Show/hide icons based on progress
        if (_progress > ShowCompletionIconAfter && _alertIconFader.IsVisible)
        {
            _alertIconFader.FadeOut();
            _completionIconFader.FadeIn();
        }
        else if (_progress < ShowCompletionIconAfter && !_alertIconFader.IsVisible)
        {
            _alertIconFader.FadeIn();
            _completionIconFader.FadeOut();
        }

        //Update dot colour
        _swipeDot.color = Color.Lerp(_dotInitialColour, _dotCompletedColour, _progress);

        //Fire events
        OnSwipeProgress?.Invoke(_progress);
        if (_progress >= ConsiderCompletedAfter)
            OnSwipeCompleted?.Invoke();
    }

    private void FixedUpdate()
    {
        if (!_visible)
            return;

        if (_progress != _lastCalculatedProgress)
        {
            CalculateProgress();
            _lastCalculatedProgress = _progress;
        }
    }

    private void Update()
    {
        if (!_visible)
            return;

        //Animate text shimmer effect
        for (int i = 0; i < _text.textInfo.characterCount; ++i)
        {
            Color newColour = Color.Lerp(_textDefaultColour, _textHighlightColour, Mathf.PingPong((Time.time + (i * _textHighlightDistance)) * _textHighlightSpeed, 1.0f));
            int meshIndex = _text.textInfo.characterInfo[i].materialReferenceIndex;
            int vertexIndex = _text.textInfo.characterInfo[i].vertexIndex;
            Color32[] vertexColors = _text.textInfo.meshInfo[meshIndex].colors32;
            vertexColors[vertexIndex + 0] = newColour;
            vertexColors[vertexIndex + 1] = newColour;
            vertexColors[vertexIndex + 2] = newColour;
            vertexColors[vertexIndex + 3] = newColour;
        }
        _text.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
    }
}

public enum SwipeType
{
    BRING_LEFT_WITH_DOT,
    DOT_MOVES_ON_ITS_OWN,
    DOT_EXTENDS_TO_END,
}
