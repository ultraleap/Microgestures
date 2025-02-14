using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToLastInHierarchy : MonoBehaviour
{
    // Start is called before the first frame update
    IEnumerator Start()
    {
        //wait for setup
        for (int i = 0; i < 5; i++)
        {
            yield return null;
        }

        transform.SetAsLastSibling();
    }
}
