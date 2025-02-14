using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ClockUI : MonoBehaviour
{
    private TextMeshProUGUI uiText;

    private void Start()
    {
        uiText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        uiText.text = DateTime.Now.ToString("t").ToUpper();
    }
}
