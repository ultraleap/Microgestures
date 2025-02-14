using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class SwipeObject : MonoBehaviour
{
    private CanvasGroup _canvasGroup;
    private float _progress = 0.0f;

    /* Move the swipeable object between two stages */
    public void MoveBetween(Carousel.Stage from, Carousel.Stage to = null, float progress = 0.0f, float opacityCutoff = 0.0f)
    {
        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();

        if (to == null)
            to = from;

        _progress = Mathf.Clamp(Mathf.Abs(progress), 0.0f, 1.0f);

        transform.localPosition = Vector3.Lerp(from.Transform.localPosition, to.Transform.localPosition, _progress);
        transform.localRotation = Quaternion.Lerp(from.Transform.localRotation, to.Transform.localRotation, _progress);
        transform.localScale = Vector3.Lerp(from.Transform.localScale, to.Transform.localScale, _progress);

        if (progress < opacityCutoff)
        {
            _canvasGroup.alpha = from.Opacity;
        }
        else if (progress > 1.0f - opacityCutoff)
        {
            _canvasGroup.alpha = to.Opacity;
        }
        else
        {
            _canvasGroup.alpha = Mathf.Lerp(from.Opacity, to.Opacity, _progress);
        }
    }
}
