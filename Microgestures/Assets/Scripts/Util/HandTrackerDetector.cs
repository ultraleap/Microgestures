using Leap;
using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class HandTrackerDetector : MonoBehaviour
{
    public LeapProvider leapProvider;
    public Image leftHand, rightHand;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Hand left = leapProvider.CurrentFrame?.GetHand(Chirality.Left);
        Hand right = leapProvider.CurrentFrame?.GetHand(Chirality.Right);

        if (left!= null)
        {
            leftHand.color = Color.green;
        }
        else
        {
            leftHand.color = Color.white;
        }

        if (right!= null)
        {
            rightHand.color = Color.green;
        }
        else
        {
            rightHand.color = Color.white;
        }
    }
}
