using System;
using System.Collections;
using System.Collections.Generic;
using Leap.Unity.InputModule;
using Ultraleap.XRTemplate;
using UnityEngine;
using UnityEngine.UI;

public class Taskbar : MonoSingleton<Taskbar>
{
    [SerializeField] private FadeUI _taskbarFader;
    [SerializeField] private FadeUI _allExamplesFader;
    [SerializeField] private FadeUI _collectionsFader;
    [SerializeField] private FadeUI _settingsFader;

    [Space]

    [SerializeField] private FadeUI _viewBlocker;

    [Space]

    [SerializeField] private GameObject _originDummy;
    [SerializeField] private GameObject _userInterface;

    [Space]

    [SerializeField] public float _uiForwardOffset = 0.45f;
    [SerializeField] public float _uiVerticalOffset = 0.31f;
    [SerializeField] private float _uiScale = 0.25f;
    [SerializeField] public float _lerpSpeed = 3.11f;

    [Space]

    [SerializeField] private CarouselInteractionController _interactionController;
    [SerializeField] private Button _leftChiralityButton;
    [SerializeField] private Button _rightChiralityButton;

    [Space]

    [SerializeField] private Button _arEnvButton;
    [SerializeField] private Button _vrEnvButton;
    [SerializeField] private GameObject _envOptGameObject;

    private PlatformFeatures _platformFeatures;

    private State _interfaceState;
    public State InterfaceState { get { return _interfaceState; } }

    public Action OnViewBlockerVisible;
    public Action<State> OnInterfaceStateChanged;

    private void RecenterPanel()
    {
        _userInterface.transform.localScale = new Vector3(_uiScale, _uiScale, _uiScale);

        Vector3 newPos = Camera.main.transform.position + (Camera.main.transform.forward * _uiForwardOffset);
        newPos.y = Camera.main.transform.position.y - _uiVerticalOffset;
        _userInterface.transform.position = newPos;
        _userInterface.transform.rotation = CalculateLookAt(true);

        _originDummy.transform.position = _userInterface.transform.position;
        _originDummy.transform.rotation = CalculateLookAt(false);
    }
    private Quaternion CalculateLookAt(bool clampToY = false)
    {
        if (!clampToY) return Quaternion.LookRotation(Camera.main.transform.forward);
        return Quaternion.AngleAxis(Quaternion.LookRotation(Camera.main.transform.forward).eulerAngles.y, Vector3.up);
    }

    protected override void Awake()
    {
        _platformFeatures = FindObjectOfType<PlatformFeatures>();
        _platformFeatures.OnPassthroughSettingChanged += delegate (bool enabled)
        {
            UpdateEnvButtons();
        };

        _envOptGameObject.SetActive(_platformFeatures.PassthroughSupported);
    }

    private IEnumerator Start()
    {
        _viewBlocker.FadeIn(true);
        yield return new WaitForSeconds(0.5f);
        OnRecenterButtonPress();
        UpdateChiralityButtons();
        UpdateEnvButtons();
        yield return new WaitForSeconds(0.75f);
        _envOptGameObject.SetActive(_platformFeatures.PassthroughSupported);
        UpdateEnvButtons();
    }

    /* Recenter button press */
    public void OnRecenterButtonPress()
    {
        StartCoroutine(OnRecenterButtonPressCoroutine());
    }
    private IEnumerator OnRecenterButtonPressCoroutine()
    {
        _viewBlocker.FadeIn();

        SetInterfaceState(State.HIDING_UI);
        yield return new WaitForSeconds(0.5f);

        RecenterPlayer();
        RecenterPanel();
        SceneController.Instance.ShowScene(SceneController.Instance.CurrentScene, false, true);

        _viewBlocker.FadeOut();
    }
    public void RecenterPlayer()
    {
        if (Camera.main.transform.parent == null)
        {
            GameObject parent = new GameObject("PlayerParent");
            Camera.main.transform.parent = parent.transform;
        }

        Quaternion p = Camera.main.transform.rotation * Quaternion.Euler(0, 180, 0);
        float q = (gameObject.transform.transform.rotation * Quaternion.Inverse(p) * Camera.main.transform.parent.rotation).eulerAngles.y;
        Camera.main.transform.parent.rotation = Quaternion.Euler(0, q, 0);
        Camera.main.transform.parent.position -= Camera.main.transform.position - gameObject.transform.transform.position;
    }

    /* Chirality change buttons */
    public void SetLeftChirality()
    {
        _interactionController.SetChirality(Leap.Unity.Chirality.Left);
        UpdateChiralityButtons();
    }
    public void SetRightChirality()
    {
        _interactionController.SetChirality(Leap.Unity.Chirality.Right);
        UpdateChiralityButtons();
    }
    private void UpdateChiralityButtons()
    {
        _leftChiralityButton.interactable = _interactionController.Chirality == Leap.Unity.Chirality.Right;
        _rightChiralityButton.interactable = _interactionController.Chirality == Leap.Unity.Chirality.Left;
    }

    /* Passthrough buttons */
    public void SetAR()
    {
        _platformFeatures.EnablePassthrough(true);
        UpdateEnvButtons();
    }
    public void SetVR()
    {
        _platformFeatures.EnablePassthrough(false);
        UpdateEnvButtons();
    }
    private void UpdateEnvButtons()
    {
        _arEnvButton.interactable = !_platformFeatures.PassthroughEnabled;
        _vrEnvButton.interactable = _platformFeatures.PassthroughEnabled;
    }

    /* Quit App */
    public void QuitApp()
    {
        StartCoroutine(QuitAppCoroutine());
    }
    private IEnumerator QuitAppCoroutine()
    {
        _viewBlocker.FadeIn();
        yield return new WaitForSeconds(0.5f);
        Application.Quit();
    }

    /* Set the state of the interface */
    public void SetInterfaceState(string state)
    {
        SetInterfaceState((State)Enum.Parse(typeof(State), state));
    }
    public void SetInterfaceState(State state, bool immediate = false)
    {
        if (state == State.SHOWING_UI && _interfaceState == State.HIDING_UI)
            RecenterPanel();

        if (state != State.FADED_UI)
        {
            _taskbarFader.FadeOut(immediate);
            _allExamplesFader.FadeOut(immediate);
            _settingsFader.FadeOut(immediate);
            _collectionsFader.FadeOut(immediate);
        }

        switch (state)
        {
            case State.SHOWING_UI:
                _taskbarFader.FadeIn(immediate);
                _collectionsFader.FadeIn(immediate);
                break;
            case State.FADED_UI:
                _taskbarFader.FadeOut(immediate, 0.4f);
                if (_settingsFader.IsVisible)
                    _settingsFader.FadeOut(immediate, 0.4f);
                if (_allExamplesFader.IsVisible)
                    _allExamplesFader.FadeOut(immediate, 0.4f);
                if (_collectionsFader.IsVisible)
                    _collectionsFader.FadeOut(immediate, 0.4f);
                break;
        }

        _interfaceState = state;
        OnInterfaceStateChanged?.Invoke(_interfaceState);
    }

    //new stuff for mg scene selector
    public void ShowAllExamples()
    {
        if (_interfaceState != State.SHOWING_UI)
            return;

        _allExamplesFader.FadeIn();
        _collectionsFader.FadeOut();
        _settingsFader.FadeOut();
    }
    public void ShowCollections()
    {
        if (_interfaceState != State.SHOWING_UI)
            return;

        _allExamplesFader.FadeOut();
        _collectionsFader.FadeIn();
        _settingsFader.FadeOut();
    }
    public void ShowSettings()
    {
        if (_interfaceState != State.SHOWING_UI)
            return;

        _allExamplesFader.FadeOut();
        _collectionsFader.FadeOut();
        _settingsFader.FadeIn();
    }

    /* This fades in the view blocker, calls OnViewBlockerVisible event, and then fades it out again. Useful for hiding transitions */
    public void DoViewBlocker()
    {
        StartCoroutine(DoViewBlockerCoroutine());
    }
    private IEnumerator DoViewBlockerCoroutine()
    {
        _viewBlocker.FadeIn();
        yield return new WaitForSeconds(_viewBlocker.TransitionTime + 0.05f);
        OnViewBlockerVisible?.Invoke();
        yield return new WaitForSeconds(0.05f);
        _viewBlocker.FadeOut();
    }

    public enum State
    {
        SHOWING_UI,
        FADED_UI,
        HIDING_UI,
    }
}
