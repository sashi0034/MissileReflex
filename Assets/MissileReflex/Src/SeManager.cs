#nullable enable

using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src
{
    public partial class SeManager : MonoBehaviour
    {
#nullable disable
        private static SeManager _instance;
        public static SeManager Instance => _instance;
        
        [SerializeField] private AudioSource audioSource;
        
        [SerializeField] private AudioClip seMatchingStart;
        public AudioClip SeMatchingStart => seMatchingStart;
        
#nullable enable

        [EventFunction]
        private void Awake()
        {
            if (Util.EnsureSingleton(this, ref _instance) == false) return;
            Util.AssertNotNullSerializeFieldsRecursive(this, nameof(MissileReflex),new ());
        }
        
        public void PlaySe(AudioClip clip)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}