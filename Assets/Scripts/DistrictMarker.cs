using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DistrictMarker : MonoBehaviour
{

    [SerializeField]private District district;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    public District GetDistrict()
    {
        return district;
    }

    void OnValidate()
    {
        gameObject.name = district.ToString();
    }
    private void OnDrawGizmos()
    {
        Handles.Label(transform.position,district.ToString());

    }
}
