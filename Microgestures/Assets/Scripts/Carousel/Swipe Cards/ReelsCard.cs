using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ReelsCard : SwipeCard
{
    [SerializeField] private RawImage _videoPlayerImg;
    [SerializeField] private VideoPlayer _videoPlayer;
    [SerializeField] private TextMeshProUGUI _userText;
    [SerializeField] private TextMeshProUGUI _descText;
    [SerializeField] private TextMeshProUGUI _musicText;
    [SerializeField] private TextMeshProUGUI _likeCountText;
    [SerializeField] private FadeUI _fader;
    [SerializeField] private Animator _likeAnimator;

    RenderTexture _tex;

    private void Start()
    {
        _userText.text = _cardContent.Text;
        _descText.text = _cardContent.TextExp;
        _musicText.text = _cardContent.TextAlt;
        _videoPlayer.clip = _cardContent.Vid;

        _likeCountText.text = (Mathf.Round(UnityEngine.Random.Range(1.5f, 85.9f) * 10.0f) * 0.1f) + "M";
        _likeAnimator.SetBool("liked", false);

        _tex = new RenderTexture(_videoPlayer.targetTexture);
        _videoPlayer.targetTexture = _tex;
        _videoPlayer.Stop();
        _videoPlayer.Prepare();
        _videoPlayer.Pause();

        _videoPlayerImg.texture = _tex;
    }

    public override void Highlight(bool active)
    {
        if (active)
        {
            _fader.FadeIn();
            _videoPlayer.Play();
        }
        else
        {
            _fader.FadeOut();
            _videoPlayer.Pause();
        }

        base.Highlight(active);
    }

    public override void Interact()
    {
        _likeAnimator.SetBool("liked", !_likeAnimator.GetBool("liked"));
    }
}
