using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Metadata;
using UnityEngine;

public class SceneController : MonoSingleton<SceneController>
{
    [SerializeField] private List<MgScene> _availableScenes;

    [SerializeField] private float _leftRightOffset = 3.0f;
    [SerializeField] private float _forwardsOffset = 1.83f;

    [Space]

    [SerializeField] private CarouselInteractionController _interactionController;
    [SerializeField] private GameObject _sceneParent;

    [Space]

    [SerializeField] private FadeUI _spinnyObject;

    private List<Carousel> _carousels;

    private Coroutine _showSceneCoroutine = null;

    public string CurrentScene => _currentScene;
    private string _currentScene = "";

    public CarouselInteractionController InteractionController => _interactionController;

    private Transform _lookAt = null;
    private int _closest = -1;

    private bool _hasPopulatedScenes = false;
    private bool _isShowingScene = false;
    private bool _sceneIsActive = false;
    private string _prevRequestedScene = "";

    private void Awake()
    {
        _lookAt = new GameObject("LookAt").transform;
        _lookAt.parent = Camera.main.transform;
        _lookAt.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    private IEnumerator Start()
    {
        for (int i = 0; i < _availableScenes.Count; i++)
        {
            MgScene scene = _availableScenes[i];
            scene.container = new GameObject(scene.id);
            scene.container.transform.parent = _sceneParent.transform;

            yield return new WaitForEndOfFrame();

            if (scene.demo_center?.prefab != null)
            {
                yield return new WaitForEndOfFrame();

                GameObject center = Instantiate(scene.demo_center.prefab, scene.container.transform);
                center.transform.localPosition = scene.demo_center.world_offset + new Vector3(0, 0, 0);
                center.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                scene.carousels.Add(center.GetComponentInChildren<Carousel>());
            }
            if (scene.demo_left?.prefab != null)
            {
                yield return new WaitForEndOfFrame();

                GameObject left = Instantiate(scene.demo_left.prefab, scene.container.transform);
                left.transform.localPosition = scene.demo_left.world_offset + new Vector3(-_leftRightOffset, 0, 0);
                left.transform.rotation = Quaternion.Euler(new Vector3(0, -50.0f, 0));
                scene.carousels.Add(left.GetComponentInChildren<Carousel>());
            }
            if (scene.demo_right?.prefab != null)
            {
                yield return new WaitForEndOfFrame();

                GameObject right = Instantiate(scene.demo_right.prefab, scene.container.transform);
                right.transform.localPosition = scene.demo_right.world_offset + new Vector3(_leftRightOffset, 0, 0);
                right.transform.rotation = Quaternion.Euler(new Vector3(0, 50.0f, 0));
                scene.carousels.Add(right.GetComponentInChildren<Carousel>());
            }

            yield return new WaitForEndOfFrame();
        }

        float[][] times = new float[_availableScenes.Count][];
        for (int i = 0; i < _availableScenes.Count; i++)
        {
            times[i] = new float[_availableScenes[i].carousels.Count];
            for (int x = 0; x < _availableScenes[i].carousels.Count; x++)
            {
                FadeUI fader = _availableScenes[i].carousels[x]?.Fader;
                if (fader == null) continue;
                times[i][x] = fader.TransitionTime;
                fader.TransitionTime = 0.001f;
                fader.FadeIn();
            }
        }

        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < _availableScenes.Count; i++)
        {
            for (int x = 0; x < _availableScenes[i].carousels.Count; x++)
            {
                FadeUI fader = _availableScenes[i].carousels[x]?.Fader;
                if (fader == null) continue;
                fader.FadeOut();
            }
        }

        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < _availableScenes.Count; i++)
        {
            for (int x = 0; x < _availableScenes[i].carousels.Count; x++)
            {
                FadeUI fader = _availableScenes[i].carousels[x]?.Fader;
                if (fader == null) continue;
                fader.TransitionTime = times[i][x];
            }
        }

        _hasPopulatedScenes = true;
        ShowScene(_prevRequestedScene);
    }

    void Update()
    {
        if (_carousels == null || _carousels.Count == 0 || _carousels[0].Fader == null || !_carousels[0].Fader.IsVisible)
            return;

        int closest = GetClosestLooked();
        if (closest != -1 && closest != _closest)
        {
            _interactionController.SetCarousel(_carousels[closest], _carousels[closest].DefaultInteractionType);
            _closest = closest;
        }
    }

    /* Figure out which carousel the user is looking at */
    private int GetClosestLooked()
    {
        if (!_sceneIsActive)
            return -1;

        float prevDist = float.MaxValue;
        int closest = -1;
        for (int i = 0; i < _carousels.Count; i++)
        {
            _lookAt.LookAt(_carousels[i].transform);
            float dist = Mathf.Abs(Quaternion.Angle(Camera.main.transform.rotation, _lookAt.rotation));

            if (prevDist >= dist)
            {
                prevDist = dist;
                closest = i;
            }
        }
        return closest;
    }

    /* Show the home scene */
    public void HomeScene() => ShowScene("");

    /* Shows a new microgesture demo scene, and closes the main menu, if it's open */
    public void ShowScene(string id, bool doFade = true, bool force = false)
    {
        _prevRequestedScene = id;
        if (!_hasPopulatedScenes)
            return;

        Taskbar.Instance.SetInterfaceState(Taskbar.State.HIDING_UI);

        if (!force && id != "" && _currentScene == id)
            return;
        _isShowingScene = true;
        _sceneIsActive = false;

        MgScene scene = _availableScenes.FirstOrDefault(o => o.id == id);
        if (_showSceneCoroutine != null)
            StopCoroutine(_showSceneCoroutine);
        StartCoroutine(ShowSceneCoroutine(scene, doFade));
    }
    private IEnumerator ShowSceneCoroutine(MgScene scene, bool doFade)
    {
        //Hide previously active content
        if (_carousels != null)
        {
            foreach (Carousel carousel in _carousels)
            {
                if (carousel?.Fader != null)
                {
                    carousel.Fader.FadeOut(doFade);
                    carousel?.TooltipFader?.FadeOut(doFade);
                    yield return new WaitForSeconds(doFade ? carousel.Fader.TransitionTime : 0.0f);
                }
                else if (carousel?.TooltipFader != null)
                {
                    carousel?.Fader?.FadeOut(doFade);
                    carousel.TooltipFader.FadeOut(doFade);
                    yield return new WaitForSeconds(doFade ? carousel.TooltipFader.TransitionTime : 0.0f);
                }
            }
        }
        _spinnyObject.FadeOut();
        yield return new WaitForSeconds(_spinnyObject.TransitionTime);

        //Reset stuff
        _closest = -1;
        _carousels = null;
        _currentScene = scene?.id;
        _interactionController.ResetAll();

        //Show the new content
        if (scene == null)
        {
            _spinnyObject.FadeIn();
            yield return new WaitForSeconds(_spinnyObject.TransitionTime);
        }
        else
        {
            Vector3 pos = Camera.main.transform.position + (Camera.main.transform.forward * _forwardsOffset);
            pos.y = Camera.main.transform.position.y;
            scene.container.transform.position = pos;
            scene.container.transform.rotation = Quaternion.AngleAxis(Quaternion.LookRotation((scene.container.transform.position - Camera.main.transform.position).normalized).eulerAngles.y, Vector3.up);

            _carousels = scene.carousels;
            if (_carousels != null)
            {
                foreach (Carousel carousel in _carousels)
                {
                    if (carousel?.Fader != null)
                    {
                        carousel.Fader.FadeIn(doFade);
                        yield return new WaitForSeconds(carousel.Fader.TransitionTime);
                    }
                    carousel?.SetActiveState(false);
                    yield return new WaitForSeconds(0.1f);
                }
                foreach (Carousel carousel in _carousels)
                {
                    carousel?.SetProgress(carousel.Progress, true);
                    carousel?.UpdateHighlights();
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        _showSceneCoroutine = null;
        _isShowingScene = false;
        _sceneIsActive = true;

        if (_prevRequestedScene != scene?.id)
            ShowScene(scene?.id);
    }

    [Serializable]
    public class MgScene
    {
        public string id;

        //To instance
        public MgDemo demo_left;
        public MgDemo demo_center;
        public MgDemo demo_right;

        //Instanced
        [HideInInspector] public GameObject container;
        [HideInInspector] public List<Carousel> carousels = new List<Carousel>();
    }

    [Serializable]
    public class MgDemo
    {
        public GameObject prefab;
        public Vector3 world_offset;
    }
}
