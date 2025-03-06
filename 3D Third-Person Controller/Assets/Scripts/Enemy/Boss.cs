using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class Boss : EnemyBase
{
    void Start()
    {
        
    }
    
    void Update()
    {
        View();
    }

    protected override void View()
    {
        base.View();
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(detectionCenter.position, detectionRadius);

        if (targets[0] != null)
        {
            Gizmos.DrawRay(detectionCenter.position, ((targets[0].transform.root.position + targets[0].transform.root.up * 0f) - detectionCenter.position).normalized);
            Gizmos.DrawRay(detectionCenter.position, ((targets[0].transform.root.position + targets[0].transform.root.up * 0.5f) - detectionCenter.position).normalized);
            Gizmos.DrawRay(detectionCenter.position, ((targets[0].transform.root.position + targets[0].transform.root.up * 1f) - detectionCenter.position).normalized);
            Gizmos.DrawRay(detectionCenter.position, ((targets[0].transform.root.position + targets[0].transform.root.up * 1.5f) - detectionCenter.position).normalized);   
        }
    }
}
