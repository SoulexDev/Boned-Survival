using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LODManager : MonoBehaviour
{
    public static LODManager Instance;

    public delegate void UpdateLOD();
    public static event UpdateLOD OnUpdateLOD;

    public int LODRadiusInterval;
    private Vector3 lastPos;

    private void Awake()
    {
        Instance = this;
        lastPos = transform.position;
    }
    private void Update()
    {
        if (Vector3.Distance(lastPos, transform.position) >= LODRadiusInterval/2)
        {
            lastPos = transform.position;
            OnUpdateLOD?.Invoke();
        }
    }
}