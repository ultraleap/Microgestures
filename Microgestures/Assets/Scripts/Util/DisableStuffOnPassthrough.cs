using System.Collections;
using System.Collections.Generic;
using Ultraleap.XRTemplate;
using UnityEngine;

public class DisableStuffOnPassthrough : MonoBehaviour
{
    [SerializeField] private PlatformFeatures _platformFeatures;
    [SerializeField] private List<GameObject> _stuff;

    void Awake()
    {
        _platformFeatures.OnPassthroughSettingChanged += OnPassthroughChanged;
        OnPassthroughChanged(_platformFeatures.PassthroughEnabled);
    }

    private void OnPassthroughChanged(bool enabled)
    {
        for (int i = 0; i < _stuff.Count; i++)
            _stuff[i].SetActive(!enabled);
    }
}
