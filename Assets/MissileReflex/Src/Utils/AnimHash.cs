using UnityEngine;

namespace MissileReflex.Src.Utils
{
    public class AnimHash
    {
        public string Name { get; }
        public int Code { get; }

        public AnimHash(string name)
        {
            Name = name;
            Code = Animator.StringToHash(name);
        }

    }
}