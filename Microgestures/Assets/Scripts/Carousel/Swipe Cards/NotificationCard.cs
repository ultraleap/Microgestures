using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class NotificationCard : SwipeCard
{
    [SerializeField] private TextMeshProUGUI _usernameText;
    [SerializeField] private TextMeshProUGUI _contentText;
    [SerializeField] private Image _socialIcon;
    [SerializeField] private TextMeshProUGUI _interactText;

    [SerializeField] private FadeUI _highlightFader;
    [SerializeField] private Animator _highlightAnimator;

    private void Start()
    {
        _usernameText.text = _cardContent.Text;
        _contentText.text = _cardContent.TextExp;
        _socialIcon.sprite = _cardContent.Img;
        _interactText.text = "Select to respond";
    }

    public override void Highlight(bool active)
    {
        if (active)
        {
            _highlightFader.FadeIn();
            _interactText.text = "Select to respond";
        }
        else
        {
            _highlightFader.FadeOut();
        }

        _highlightAnimator.SetBool("highlighted", active);

        base.Highlight(active);
    }

    public override void Interact()
    {
        _interactText.text = _cardContent.TextAlt;
    }
}
