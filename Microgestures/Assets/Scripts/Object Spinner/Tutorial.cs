using Leap.Unity.GestureMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    [Tooltip("Time between user interacting and tutorial showing")]
    [SerializeField] private float _timeToShowTut = 15.0f;

    [SerializeField] private bool _showInFrontOfUser = false;

    private Animator _animator;
    private MicrogestureSystem _slider;
    private float _lastContactTime = 0;

    void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _slider = FindObjectOfType<MicrogestureSystem>();
        _slider?.OnSlide.AddListener(OnSliderSlide);
    }

    private void OnSliderSlide(float val)
    {
        _lastContactTime = Time.realtimeSinceStartup;
    }

    void Update()
    {
        if (_slider == null) return;

        if (Time.realtimeSinceStartup - _lastContactTime >= _timeToShowTut)
        {
            if (_showInFrontOfUser)
                Center();

            _animator.SetTrigger("do_anim");
            _lastContactTime = Time.realtimeSinceStartup;
        }
    }

    private void Center()
    {
        this.transform.rotation = Quaternion.Euler(new Vector3(0, Camera.main.transform.rotation.eulerAngles.y, 0));
        this.transform.position = Camera.main.transform.position;
    }
}
