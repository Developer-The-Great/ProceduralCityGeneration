using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshVersions : MonoBehaviour
{
    [SerializeField] public GameObject[] versions;


    public int GetCount()
    {
        return versions.Length;
    }
}
