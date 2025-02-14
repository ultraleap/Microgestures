using Leap.Unity;
using Leap.Unity.Interaction;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HandUI : MonoBehaviour
{
    [SerializeField] private GameObject _ui;

    [Space]

    [SerializeField] private InteractionButton handButton;
    [SerializeField] private Image handButton_BG;
    [SerializeField] private Color handButton_Hover;
    [SerializeField] private Color handButton_Press;
    [SerializeField] private Color handButton_Default;

    [Space]

    [SerializeField] private Sprite _openIcon;
    [SerializeField] private Sprite _closeIcon;
    [SerializeField] private Image _icon;

    [Space]

    [SerializeField] private LeapProvider _provider = null;
    [SerializeField] private IsIndexInTrigger _triggerCheck;

    /* Setup */
    private void Start()
    {
        if (_provider == null)
            _provider = FindObjectOfType<LeapProvider>();

        Taskbar.Instance.OnInterfaceStateChanged += UpdateHandIcons;
        UpdateHandIcons(Taskbar.Instance.InterfaceState);
    }

    private void UpdateHandIcons(Taskbar.State state)
    {
        _icon.sprite = state == Taskbar.State.HIDING_UI ? _openIcon : _closeIcon;
    }

    /* Show/hide the actual UI based on hand visibility - stops floating UI in worldspace when hands are null */
    void Update()
    {
        if (_provider != null)
        {
            bool leftHandVisible = _provider.CurrentFrame?.Hands.FirstOrDefault(o => o.IsLeft) != null;
            if (_ui.activeInHierarchy != leftHandVisible)
            {
                _ui.SetActive(leftHandVisible);
                UpdateHandIcons(Taskbar.Instance.InterfaceState);
            }
        }
        UpdateButtonHover();
    }

    /* Change sprite of hand button on hover & press */
    private void UpdateButtonHover()
    {
        if (handButton.isPressed)
        {
            handButton_BG.color = handButton_Press;
        }
        else if (_triggerCheck.IsIndexInside)
        {
            handButton_BG.color = handButton_Hover;
        }
        else
        {
            handButton_BG.color = handButton_Default;
        }
    }

    /* Hand button press */
    public void OnHandButtonPress()
    {
        switch (Taskbar.Instance.InterfaceState)
        {
            case Taskbar.State.FADED_UI:
            case Taskbar.State.SHOWING_UI:
                Taskbar.Instance.SetInterfaceState(Taskbar.State.HIDING_UI);
                break;
            case Taskbar.State.HIDING_UI:
                Taskbar.Instance.SetInterfaceState(Taskbar.State.SHOWING_UI);
                break;
        }
    }
}
