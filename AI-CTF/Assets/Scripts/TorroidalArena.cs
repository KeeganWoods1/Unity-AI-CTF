using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorroidalArena : MonoBehaviour
{
    float axisOutOfBounds = 18.5f;

    private void OnTriggerExit(Collider other)
    {
        Relocate();
    }

    private void Relocate()
    {
        
        if (transform.position.x >= axisOutOfBounds)
        {
            transform.position = new Vector3(1.5f, transform.position.y, transform.position.z);
        }
        else if (transform.position.x < 1.5f)
        {
            transform.position = new Vector3(18.5f, transform.position.y, transform.position.z);
        }
        else if (transform.position.z >= axisOutOfBounds)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 1.5f);
        }
        else
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 18.5f);
        }
    }
}
