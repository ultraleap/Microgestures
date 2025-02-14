using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

public abstract class SwipeCard : SwipeObject
{
    public bool Highlighted => _highlighted;
    private bool _highlighted = false;

    [SerializeField] private FadeUI _blobFader;
    [SerializeField] private BlobProgress _blobProgress;

    [SerializeField] private List<CardContent> _content;
    private static int _index = 0;

    public int Variants => _content.Count;

    protected CardContent _cardContent = null;
    private bool _carouselIsActive = false;

    void Awake()
    {
        _index++;
        if (_index >= _content.Count)
            _index = 0;

        _cardContent = _content[_index];
    }

    /* The card is currently in a slot that highlights it for interaction, might want to do some nice UI */
    public virtual void Highlight(bool active)
    {
        _highlighted = active;

        if (_highlighted)
        {
            _blobFader?.FadeIn();
            _blobProgress?.SetSplitVal(SceneController.Instance.InteractionController.AcceptDistance + 0.15f);
        }
        else
        {
            _blobFader?.FadeOut();
        }
    }

    /* The card has been interacted with, might want to do *something* */
    public abstract void Interact();

    /* Let the card know if the carousel is currently active/inactive */
    public void SetCarouselActive(bool active)
    {
        _carouselIsActive = active;
    }

    /* Update our progress indicator if we have one, while the carousel is active */
    private void Update()
    {
        if (!_carouselIsActive || _blobProgress == null)
            return;

        _blobProgress.SetVal(SceneController.Instance.InteractionController.RealValue);
        _blobProgress.Highlight(SceneController.Instance.InteractionController.Interacting);
    }

    [Serializable]
    protected class CardContent
    {
        public Sprite Img;
        public Sprite ImgAlt;
        public string Text;
        public string TextAlt;

        public VideoClip Vid;

        [TextArea(4, 4)]
        public string TextExp;
    }
}
