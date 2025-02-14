using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class SwipeableAlert : CarouselMgLogicSlide
{
    [SerializeField] private List<Alert> _alerts;

    [Space]

    [SerializeField] private SwipeUI _swiper;
    [SerializeField] private TextMeshProUGUI _headerText;
    [SerializeField] private FadeUI _headerTextFader;
    [SerializeField] private TextMeshProUGUI _subtitleText;
    [SerializeField] private FadeUI _subtitleTextFader;

    public Action OnHide;

    private FadeUI _fader;
    private Alert _activeAlert;
    private Coroutine _displayCoroutine = null;

    public void Show(AlertType type, string alertText)
    {
        if (!gameObject.activeInHierarchy)
            return;

        _headerTextFader.FadeOut(true);
        _subtitleTextFader.FadeOut(true);
        _swiper.Hide(true);

        _completed = false;

        _activeAlert = _alerts.FirstOrDefault(o => o.AlertType == type);
        if (_activeAlert == null)
        {
            Debug.LogWarning("SwipeableAlert failed to find alert of type: " + type);
            return;
        }

        _headerText.text = type.ToString().Replace('_', ' ').FirstLetterToUpper();
        _subtitleText.text = alertText;

        if (_displayCoroutine != null)
        {
            StopCoroutine(_displayCoroutine);
            _displayCoroutine = null;
        }
        _displayCoroutine = StartCoroutine(ShowAlertCoroutine());
    }
    private IEnumerator ShowAlertCoroutine()
    {
        _fader.FadeIn();
        yield return new WaitForSeconds(0.05f);
        _headerTextFader.FadeIn();
        yield return new WaitForSeconds(0.15f);
        _subtitleTextFader.FadeIn();
        yield return new WaitForSeconds(0.25f);
        _swiper.Show(_activeAlert.SwipeType, _activeAlert.AlertIcon, _activeAlert.CompletionIcon);

        _displayCoroutine = null;
    }

    protected void Awake()
    {
        _fader = GetComponent<FadeUI>();
        if (_fader == null)
            _fader = gameObject.AddComponent<FadeUI>();
        _fader.disableOnFade = false;

        if (AcceptAfterProgress != 1.0f)
            _swiper.ShowCompletionIconAfter = AcceptAfterProgress;

        OnCompletedToThreshold += Hide;

        base.Awake();
    }

    private void Start()
    {
        Hide();
    }

    private void Update()
    {
        _swiper.SetProgress(_swipeProgressLerped);
    }

    protected void Hide(SwipeDirection direction = SwipeDirection.NONE)
    {
        if (direction != SwipeDirection.RIGHT || !_fader.IsVisible)
            return;

        _fader.FadeOut();
        OnHide?.Invoke();
    }

    [System.Serializable]
    public class Alert
    {
        public AlertType AlertType;
        public SwipeType SwipeType;

        public Sprite AlertIcon;
        public Sprite CompletionIcon;
    }
}

public enum AlertType
{
    ALARM,
    REMINDER,
    NOTIFICATION,
    MESSAGE,
}

public static class TextUtils
{
    public static string FirstLetterToUpper(this string str)
    {
        if (str == null)
            return null;

        if (str.Length > 1)
            return char.ToUpper(str[0]) + str.Substring(1);

        return str.ToUpper();
    }
}