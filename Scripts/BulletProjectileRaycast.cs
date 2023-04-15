using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletProjectileRaycast : MonoBehaviour
{
    [SerializeField] private Transform vfxHitYes;
    [SerializeField] private Transform vfxHitNo;
    
    private Vector3 targetPosition;

    private void Setup(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
    }

    private void Update()
    {
        float distanceBefore = Vector3.Distance(transform.position, targetPosition);

        Vector3 moveDir = (targetPosition - transform.position).normalized;
        float moveSpeed = 200f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        float distanceAfter = Vector3.Distance(transform.position, targetPosition);

        if (distanceBefore < distanceAfter)
        {
            Instantiate(vfxHitNo, targetPosition, Quaternion.identity);
            transform.Find("Trail").SetParent(null);
            Destroy(gameObject);
        }
    }

}
