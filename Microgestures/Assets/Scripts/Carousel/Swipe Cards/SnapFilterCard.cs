using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SnapFilterCard : SwipeCard
{
    [SerializeField] private Image _icon;

    private SnapFilterApplier _filterApplier = null;

    private void Start()
    {
        _filterApplier = this.transform.GetComponentInParent<SnapFilterApplier>();

        _icon.sprite = _cardContent.Img;
    }

    public override void Highlight(bool active)
    {
        if (active && _filterApplier?.ActiveLensName == _cardContent?.Text)
            return;

        if (active)
            _filterApplier?.Apply(_cardContent?.ImgAlt, _cardContent?.Img, _cardContent?.Text, _cardContent?.TextAlt);

        base.Highlight(active);
    }

    public override void Interact()
    {

    }
}
