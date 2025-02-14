using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class ValuePlot : MonoBehaviour
{
    public int plotWidth = 100;
    public int plotHeight = 10;
    public float maxValue = 1;
    public float minValue = 0;

    public Color zeroLineColour = Color.grey; 
    public Color plotColour = Color.cyan;
    public Color constLineColour = Color.red;

    private RawImage plotImage;
    private Texture2D plotTex;
    private int plotIndex = 0;

    public bool drawConstLine = true;
    public bool drawZeroLine = true;
    public float constLineVal = 0;
    private Coroutine updateRoutine;

    public TextMeshProUGUI value;
    public TextMeshProUGUI title;

    // Start is called before the first frame update
    void Awake()
    {
        plotImage = GetComponent<RawImage>();
        plotTex = new Texture2D(plotWidth, plotHeight);
        plotTex.filterMode = FilterMode.Point;
        ClearPlot();

        plotImage.texture = plotTex;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // public IEnumerator UpdateValue()
    // {
    //     while (Application.isPlaying)
    //     {
    //         yield return new WaitForSeconds(1);
    //         string valueString = valueMetric.GetLastValues()[metricIndex];
    //         valueString = valueString.Replace("%", "");
            
    //         float value = 0;
    //         if (float.TryParse(valueString, out value))
    //         {
    //             UpdatePlot(value);
    //         }
    //     }
        
    // }

    public void UpdateTitle(string newTitle)
    {
        title.text = newTitle;
    }

    public void UpdatePlot(float value)
    {
        if(this.value != null) { this.value.text = value.ToString("0.000"); }
        float minValAbs = Mathf.Abs(minValue);
        float maxMinValue = Mathf.Abs(maxValue) + minValAbs;
        float remappedValue = value + minValAbs;

        remappedValue /= maxMinValue;

        float constLineHeight = constLineVal / maxMinValue;
        float zeroLineHeight = minValAbs / maxMinValue;
        for(int i = 0; i < plotHeight; i++)
        {
            Color colour = i > (remappedValue * plotHeight) ? new Color(0,0,0,0) : plotColour;

            if (i == (int) (constLineHeight * plotHeight) && drawConstLine)
            {
                plotTex.SetPixel(plotIndex, i, constLineColour);
            }
            else if (i == (int)(zeroLineHeight * plotHeight) && drawZeroLine)
            {
                plotTex.SetPixel(plotIndex, i, zeroLineColour);
            }
            else
            {
                plotTex.SetPixel(plotIndex, i, colour);
            }
        }
        plotIndex = (plotIndex + 1) % plotWidth;
        plotTex.Apply();
    }

    public void SetPlotColour(Color newColour)
    {
        for (int y = 0; y < plotHeight; y++)
        {
            for (int x = 0; x < plotWidth; x++)
            {
                Color color = plotTex.GetPixel(x, y);
                if(color == plotColour)
                {
                    plotTex.SetPixel(x, y, newColour);
                }
            }
        }
        plotColour = newColour;
        plotTex.Apply();
    }

    public void ClearPlot()
    {
        Color[] clearColour = new Color[plotHeight];
        for(int i = 0; i < plotHeight; i++)
        {
            clearColour[i] = new Color(0,0,0,0);
        }
        for(int i = 0; i < plotWidth; i++)
        {
            plotTex.SetPixels(i, 0, 1, plotHeight, clearColour);
        }
        plotTex.Apply(false);
    }
}
