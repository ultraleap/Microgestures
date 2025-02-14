using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Loading;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class CafeCard : SwipeCard
{
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _starText;
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField] private Image _img;

    private MapLocationMarker _mapMarker;

    private void Start()
    {
        _mapMarker = this.transform.GetComponentInParent<MapLocationMarker>();

        _titleText.text = _cardContent.Text;
        _starText.text = UnityEngine.Random.Range(1.2f, 5.0f).ToString("0.0");
        _timeText.text = "Open | Closes " + _cardContent.TextAlt;
    }

    public override void Highlight(bool active)
    {
        if (active)
            _mapMarker?.HighlightLocation(_cardContent?.TextExp, _cardContent?.Img);

        if (!active && _mapMarker?.HighlightedID == _cardContent?.TextExp)
            _mapMarker.HidePreview();

        base.Highlight(active);
    }

    public override void Interact()
    {

    }
}
