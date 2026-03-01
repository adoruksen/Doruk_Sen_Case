using UnityEngine;

namespace RubyCase.Core.Audio
{
    public interface ISoundManager
    {
        AudioSource Play(SoundType type);
        void StopLoop(AudioSource source);
    }
}