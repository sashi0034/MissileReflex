#nullable enable

using UnityEngine;

namespace MissileReflex.Src.Battle
{
    public class TankSe : MonoBehaviour
    {
#nullable disable
        [SerializeField] private AudioSource audioSource;
        public AudioSource AudioSource => audioSource;

        [SerializeField] private AudioClip seExplosion;
        [SerializeField] private AudioClip seShot;
        [SerializeField] private AudioClip seTouchedMissile;
        
        
#nullable enable

        public void PlaySeShot()
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(seShot);
        }
        
        public void PlaySeExplosion()
        {
            audioSource.pitch = Random.Range(1.3f, 1.5f);
            audioSource.PlayOneShot(seExplosion);
        }

        public void PlaySeTouchedMissile()
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(seTouchedMissile);
        }
    }
}