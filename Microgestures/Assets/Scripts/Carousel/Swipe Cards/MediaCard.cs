using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Loading;
using UnityEngine;
using UnityEngine.UI;

public class MediaCard : SwipeCard
{
    [SerializeField] private List<TextMeshProUGUI> _titleText;
    [SerializeField] private List<TextMeshProUGUI> _artistText;
    [SerializeField] private Image _img;
    [SerializeField] private FadeUI _fader;
    [SerializeField] private Animator _likeAnimator;

    private void Start()
    {
        for (int i = 0; i < _titleText.Count; i++)
            _titleText[i].text = _cardContent.Text;
        for (int i = 0; i < _artistText.Count; i++)
            _artistText[i].text = _cardContent.TextAlt;
        _img.sprite = _cardContent.Img;

        _likeAnimator?.SetBool("liked", false);
    }

    public override void Highlight(bool active)
    {
        if (active)
            _fader.FadeIn();
        else
            _fader.FadeOut();

        base.Highlight(active);
    }

    public override void Interact()
    {
        _likeAnimator?.SetBool("liked", !_likeAnimator.GetBool("liked"));
    }
}
