using UnityEngine;

namespace RubyCase.Core.Audio
{
    [CreateAssetMenu(fileName = "SoundSettings", menuName = "RubyCase/Audio/Sound Settings")]
    public sealed class SoundSettings : ScriptableObject
    {
        [Min(1)]
        public int poolSize = 10;

        public SoundLibrary library;
    }
}