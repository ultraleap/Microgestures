using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MgSceneTile : MonoBehaviour
{
    [Header("Scene Info")]
    [SerializeField] private Sprite _scenePreviewImage;
    [SerializeField] private string _scenePreviewName;
    [SerializeField] private string _sceneID;

    [Header("Components")]
    [SerializeField] private Image _tileImage;
    [SerializeField] private TextMeshProUGUI _tileText;
    [SerializeField] private Button _tileBtn;

    void OnValidate()
    {
        if (_tileImage == null || _tileText == null)
            return;

        _tileImage.sprite = _scenePreviewImage;
        _tileText.text = _scenePreviewName;
    }

    public void LoadScene()
    {
        Taskbar.Instance.SetInterfaceState(Taskbar.State.HIDING_UI);
        SceneController.Instance.ShowScene(_sceneID);
    }
}
