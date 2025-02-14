using Riten.Windinator;
using Riten.Windinator.Shapes;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteAlways]
public class BlobProgress : CanvasDrawer
{
    [Tooltip("This is a lerp modifier applied to the visual to make it more squidgy in movement")]
    [SerializeField] float _dampening = 5.0f;

    [Tooltip("This is the blend modifier applied to the blob to make it stretch more")]
    [SerializeField] float _blending = 5.6f;

    [Tooltip("This is the radius of the blob")]
    [SerializeField] float _radius = 30.0f;

    [Tooltip("If using a square, this is the roundedness on the edges")]
    [SerializeField] float _squareEdgeRounding = 0.02f;

    [Tooltip("This is the distance that the blob needs to move in order for it to split, given the above parameters: " +
        "this is used to configure the affordance of a completed swipe - as a split is therefore a threshold for completeness")]
    [SerializeField] float _splitDist = 114.0f;

    [Space]

    [Tooltip("The colour applied when a weak pulse happens")]
    [SerializeField] private Color _pulseColourStrong = Color.green;

    [Tooltip("The colour applied when a strong pulse happens")]
    [SerializeField] private Color _pulseColourWeak = Color.green;

    [Tooltip("The shadow size applied when a strong pulse happens")]
    [SerializeField] private float _pulseShadowSizeStrong = 55.0f;

    [Tooltip("The shadow size applied when weak a pulse happens")]
    [SerializeField] private float _pulseShadowSizeWeak = 25.0f;

    [Tooltip("The modifier applied to the pulse's exit")]
    [SerializeField] private float _pulseModifier = 1.0f;

    [Space]

    [Tooltip("The colour applied when highlighted")]
    [SerializeField] private Color _highlightColour = Color.red;

    [Tooltip("The modifier applied to the blob radius when not highlighted")]
    [SerializeField] float _nonHighlightRadiusModifier = 1.25f;

    [Space]

    [SerializeField] bool _isSquare = true;


    private CanvasGraphic _graphic;
    private Color _startColour;
    private float _startShadowSize;

    private Vector2 _blobPos = Vector2.zero;
    private float _t = 0.0f;
    private float _tTarget = 1.0f;

    private bool _highlighted = false;
    private float _finalRadius;


    private void Awake()
    {
        _graphic = GetComponent<CanvasGraphic>();
        _startColour = _graphic.color;
        _startShadowSize = _graphic.ShadowSize;
    }

    /* Set the value at which the blob should split */
    public void SetSplitVal(float val)
    {
        _tTarget = val;
    }

    /* Set the current value */
    public void SetVal(float val)
    {
        _t = val;
    }

    /* Pulse the UI */
    public void Pulse(bool strong = false)
    {
        _graphic.color = strong ? _pulseColourStrong : _pulseColourWeak;
        _graphic.ShadowSize = strong ? _pulseShadowSizeStrong : _pulseShadowSizeWeak;
    }

    /* Set highlighted */
    public void Highlight(bool highlighted)
    {
        _highlighted = highlighted;
    }

    private void Update()
    {
        Vector2 targetP = new Vector2(this.transform.position.x, this.transform.position.y) + new Vector2((_t / _tTarget) * _splitDist, 0);
        if (_blobPos != targetP)
        {
            _blobPos = Vector2.Lerp(_blobPos, targetP, Time.deltaTime * _dampening);
            SetDirty();
        }

        Color targetC = _highlighted ? _highlightColour : _startColour;
        if (_graphic.color != targetC)
        {
            _graphic.color = Color.Lerp(_graphic.color, targetC, Time.deltaTime * _pulseModifier);
            _graphic.ShadowSize = Mathf.Lerp(_graphic.ShadowSize, _startShadowSize, Time.deltaTime * _pulseModifier);
            SetDirty();
        }

        float targetR = _highlighted ? _radius : _radius * _nonHighlightRadiusModifier;
        if (_finalRadius != targetR)
        {
            _finalRadius = Mathf.Lerp(_finalRadius, targetR, Time.deltaTime * _pulseModifier);
            SetDirty();
        }

        base.Update();
    }

    protected override void Draw(CanvasGraphic canvas, Vector2 size)
    {
#if !UNITY_ANDROID
        if (_isSquare)
        {
            canvas.RectBrush.Draw(this.transform.position, new Vector2(0, 0));
            canvas.RectBrush.Draw(_blobPos, new Vector2(_finalRadius, _finalRadius), new Vector4(_squareEdgeRounding, _squareEdgeRounding, _squareEdgeRounding, _squareEdgeRounding), _blending * _radius);
        }
        else
        {
#endif
            canvas.CircleBrush.Draw(this.transform.position, 0.0f);
            canvas.CircleBrush.Draw(_blobPos, _finalRadius, _blending * _radius);
#if !UNITY_ANDROID
        }
#endif
    }
}
