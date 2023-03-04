#nullable enable

using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src
{
    public class SeManager : MonoBehaviour
    {
#nullable disable
        private static SeManager _instance;
        public static SeManager Instance => _instance;
        
        [SerializeField] private AudioSource audioSource;
        
        [SerializeField] private AudioClip seMatchingStart;
        public AudioClip SeMatchingStart => seMatchingStart;

        [SerializeField] private AudioClip seMatchingEnd;
        public AudioClip SeMatchingEnd => seMatchingEnd;

        [SerializeField] private AudioClip seBattleStart;
        public AudioClip SeBattleStart => seBattleStart;

        [SerializeField] private AudioClip seBattleFinish;
        public AudioClip SeBattleFinish => seBattleFinish;
        
        [SerializeField] private AudioClip sePlayerScored;
        public AudioClip SePlayerScored => sePlayerScored;

        [SerializeField] private AudioClip sePlayerRespawn;
        public AudioClip SePlayerRespawn => sePlayerRespawn;

        [SerializeField] private AudioClip seSectionSwitch;
        public AudioClip SeSectionSwitch => seSectionSwitch;

        [SerializeField] private AudioClip seSectionTap;
        public AudioClip SeSectionTap => seSectionTap;

        [SerializeField] private AudioClip seSectionConfirm;
        public AudioClip SeSectionConfirm => seSectionConfirm;

        [SerializeField] private AudioClip seSectionFront;
        public AudioClip SeSectionFront => seSectionFront;

        [SerializeField] private AudioClip seResultClap;
        public AudioClip SeResultClap => seResultClap;

        [SerializeField] private AudioClip seResultShow;
        public AudioClip SeResultShow => seResultShow;

        [SerializeField] private AudioClip seChatPost;
        public AudioClip SeChatPost => seChatPost;

        [SerializeField] private AudioClip seRatingChange;
        public AudioClip SeRatingChange => seRatingChange;
        
#nullable enable

        [EventFunction]
        private void Awake()
        {
            if (Util.EnsureSingleton(this, ref _instance) == false) return;
            Util.AssertNotNullSerializeFieldsRecursive(this, nameof(MissileReflex),new ());
        }
        
        public void PlaySe(AudioClip clip)
        {
            PlaySePitch(clip, 1f);
        }
        
        public void PlaySePitch(AudioClip clip, float pitch)
        {
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(clip);
        }
    }
}