using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SwitchHighlightGame : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _gameTitleText;

    public void UpdateTitle(string title)
    {
        _gameTitleText.text = title;
    }

    public string GetTitle()
    {
        return _gameTitleText.text;
    }
}
