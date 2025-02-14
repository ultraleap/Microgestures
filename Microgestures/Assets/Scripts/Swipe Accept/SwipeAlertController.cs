using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeAlertController : MonoBehaviour
{
    [SerializeField] private SwipeableAlert _alert;
    [SerializeField] private List<AlertContent> _alerts;

    private int _alertIndex = 0;

    private void Start()
    {
        _alert.OnHide += GenerateNewAlertAfterTime;
        GenerateNewAlert();
    }

    private void GenerateNewAlertAfterTime()
    {
        StartCoroutine(GenerateNewAlertAfterTimeCoroutine());
    }
    private IEnumerator GenerateNewAlertAfterTimeCoroutine()
    {
        yield return new WaitForSeconds(Random.Range(1.5f, 5.0f));
        GenerateNewAlert();
    }

    private void GenerateNewAlert()
    {
        _alert.Show(_alerts[_alertIndex].Type, _alerts[_alertIndex].Text);

        _alertIndex++;
        if (_alertIndex >= _alerts.Count)
            _alertIndex = 0;
    }

    [System.Serializable]
    private class AlertContent
    {
        public AlertType Type;
        public string Text;
    }
}
