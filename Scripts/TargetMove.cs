using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMove : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] public AudioSource Hooray;
    private int hits;
    private Vector3 startPosition;

    private void Awake()
    {
        hits = 0;
        startPosition = transform.position;
    }

    public void Update()
    {
        
    }

    public void Move()
    {
        hits += 1;
        AudioSource.PlayClipAtPoint(Hooray.clip, transform.position);

        if (hits <= 30)
        {
            transform.position = new Vector3(Random.Range(-90, -82), 0.5f, Random.Range(-130, -100));
        }
        else
        {
            hits = 0;
            transform.position = startPosition;
        }
    }
}
