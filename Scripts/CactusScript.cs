using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CactusScript : MonoBehaviour
{
    [SerializeField] public GameObject Player;
    private AudioSource hitSound;
    public int hits = 0;
    public int hitsToKill = 3;
    private GameObject cactus;
    private AudioClip clip;

    private void Awake()
    {
        hitSound = Player.GetComponent<ThirdPersonShooterController>().cactusBreak;
        cactus = this.gameObject;
        clip = hitSound.clip;
    }

    // Update is called once per frame
    void Update()
    {
        if (hits >= hitsToKill)
        {
            AudioSource.PlayClipAtPoint(hitSound.clip, transform.position);
            cactus.SetActive(false); // hide prop
        }
    }
}
