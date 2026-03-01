using System;
using UnityEngine;

namespace RubyCase.Core.Audio
{
    [CreateAssetMenu(fileName = "SoundLibrary", menuName = "RubyCase/Audio/Sound Library")]
    public sealed class SoundLibrary : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            public SoundType type;
            public AudioClip clip;

            [Header("Options")]
            public bool isLoop;
            public bool isRandomPitch = true;
            [Min(0.01f)] public float pitchMin = 0.95f;
            [Min(0.01f)] public float pitchMax = 1.05f;
        }

        [SerializeField] private Entry[] entries;

        public bool TryGet(SoundType type, out Entry entry)
        {
            if (entries != null)
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    var e = entries[i];
                    if (e != null && e.type == type)
                    {
                        entry = e;
                        return true;
                    }
                }
            }

            entry = null;
            return false;
        }
    }
}