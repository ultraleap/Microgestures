using Leap.Unity.InputModule;
using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonFeedback : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private AudioClip buttonSFX;
    private AudioSource audioPlayer = null;

    private Button button;
    private Toggle toggle;

    private TextMeshProUGUI txt;
    private Image img;

    private CompressibleUI _compressible;

    private float _compressibleInteractable;
    private float _compressibleNonInteractable;

    private bool _manualDepress = false;

    /* Setup */
    private void Start()
    {
        audioPlayer = GetComponent<AudioSource>();
        if (audioPlayer == null) audioPlayer = gameObject.AddComponent<AudioSource>();

        _compressible = GetComponent<CompressibleUI>();
        _compressibleInteractable = _compressible.Layers[0].MaxFloatDistance;
        _compressibleNonInteractable = _compressibleInteractable / 2.0f;

        button = GetComponent<Button>();
        toggle = GetComponent<Toggle>();

        txt = gameObject.GetComponentInChildren<TextMeshProUGUI>(true);
        Transform childContainer = transform.GetChild(0);
        if (childContainer != null)
        {
            for (int i = 0; i < childContainer.childCount; i++)
            {
                img = childContainer.GetChild(i).GetComponent<Image>();
                if (img != null)
                    break;
            }
        }

        if (button == null && toggle == null)
        {
            Debug.LogWarning("ButtonSound applied to object with no Button or Toggle");
            return;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Interactable)
            audioPlayer.PlayOneShot(buttonSFX);
    }

    bool Interactable { 
        get
        {
            if (button != null && button.interactable)
                return true;
            if (toggle != null && toggle.interactable)
                return true;
            return false;
        }
    }

    private void Update()
    {
        if (button != null)
            _compressible.Layers[0].MaxFloatDistance = !Interactable || _manualDepress ? _compressibleNonInteractable : _compressibleInteractable;
    }

    public void ManualDepress(bool depress)
    {
        _manualDepress = depress;
    }
}
