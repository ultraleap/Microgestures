using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SwitchGameCard : SwipeCard
{
    [SerializeField] private Image _icon;

    private SwitchHighlightGame _highlightGame = null;

    private void Start()
    {
        _highlightGame = this.transform.GetComponentInParent<SwitchHighlightGame>();

        _icon.sprite = _cardContent.Img;
    }

    public override void Highlight(bool active)
    {
        if (active)
            _highlightGame?.UpdateTitle(_cardContent?.Text);
        else if (_highlightGame?.GetTitle() == _cardContent?.Text)
            _highlightGame?.UpdateTitle("");

        base.Highlight(active);
    }

    public override void Interact()
    {

    }
}
