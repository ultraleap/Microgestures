using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MapLocationMarker : MonoBehaviour
{
    [SerializeField] private List<Location> _locations;
    [SerializeField] private GameObject _marker;

    [Space]

    [SerializeField] private FadeUI _previewFader;
    [SerializeField] private Image _previewImage;

    private int _currentTargetLocation = 0;

    public string HighlightedID => _highlightedID;
    private string _highlightedID = "";

    public void HighlightLocation(string id, Sprite icon)
    {
        _currentTargetLocation = _locations.IndexOf(_locations.FirstOrDefault(o => o.ID == id));
        _highlightedID = id;

        _previewImage.sprite = icon;
        _previewFader.FadeIn();
    }

    public void HidePreview()
    {
        _highlightedID = "";
        _previewFader.FadeOut();
    }

    private void Update()
    {
        _marker.transform.position = Vector3.Lerp(_marker.transform.position, _locations[_currentTargetLocation].Marker.position, Time.deltaTime * 10.0f);
    }

    [Serializable]
    class Location
    {
        public Transform Marker;
        public string ID;
    }
}
