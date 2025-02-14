using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SnapFilterApplier : MonoBehaviour
{
    [SerializeField] private FadeUI _loadingOverlay;

    [SerializeField] private Image _lensPreview;
    [SerializeField] private Image _lensIcon;
    [SerializeField] private TextMeshProUGUI _lensName;
    [SerializeField] private TextMeshProUGUI _lensAuthor;

    [Space]

    [SerializeField] private float _minWaitTime = 0.9f;
    [SerializeField] private float _maxWaitTime = 2.2f;

    private Coroutine _loadingCoroutine = null;

    public string ActiveLensName => _lensName.text;

    public void Apply(Sprite lensPreview, Sprite lensIcon, string lensName, string lensAuthor = "")
    {
        if (_loadingCoroutine != null)
            StopCoroutine(_loadingCoroutine);

        _loadingCoroutine = StartCoroutine(LoadingCoroutine(lensPreview, lensIcon, lensName, lensAuthor));
    }

    private IEnumerator LoadingCoroutine(Sprite lensPreview, Sprite lensIcon, string lensName, string lensAuthor)
    {
        _loadingOverlay.FadeIn();

        _lensIcon.sprite = lensIcon;
        _lensName.text = lensName;
        
        if (_lensAuthor != null)
            _lensAuthor.text = lensAuthor;

        yield return new WaitForSeconds(Random.Range(_minWaitTime, _maxWaitTime));
        _lensPreview.sprite = lensPreview;
        yield return new WaitForEndOfFrame();

        _loadingOverlay.FadeOut();
    }
}
