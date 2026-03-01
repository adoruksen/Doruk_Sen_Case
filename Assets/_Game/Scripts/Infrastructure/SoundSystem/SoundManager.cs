using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace RubyCase.Core.Audio
{
    public sealed class SoundManager : ISoundManager
    {
        private readonly SoundSettings settings;

        private AudioSource[] sources;
        private bool[] sourceBusy;
        private int nextSearchStartIndex;

        private readonly GameObject audioRootObject;
        private readonly CancellationToken destroyToken;

        public SoundManager(SoundSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (settings.library == null)
                throw new ArgumentException("SoundSettings.library is null.");

            this.settings = settings;

            audioRootObject = new GameObject("SoundSystem");
            UnityEngine.Object.DontDestroyOnLoad(audioRootObject);

            destroyToken = audioRootObject.GetCancellationTokenOnDestroy();

            CreatePool();
        }

        private void CreatePool()
        {
            int poolSize = Mathf.Max(1, settings.poolSize);

            sources = new AudioSource[poolSize];
            sourceBusy = new bool[poolSize];

            for (int i = 0; i < poolSize; i++)
            {
                var child = new GameObject($"SFX_{i:00}");
                child.transform.SetParent(audioRootObject.transform, false);

                var source = child.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.spatialBlend = 0f;

                sources[i] = source;
                sourceBusy[i] = false;
            }

            nextSearchStartIndex = 0;
        }

        public AudioSource Play(SoundType type)
        {
            if (!settings.library.TryGet(type, out var entry) || entry == null)
                return null;

            if (entry.clip == null)
                return null;

            int index = GetAvailableSourceIndex();

            var source = sources[index];
            sourceBusy[index] = true;

            source.Stop();
            source.clip = entry.clip;
            source.loop = entry.isLoop;

            source.pitch = entry.isRandomPitch
                ? UnityEngine.Random.Range(entry.pitchMin, entry.pitchMax)
                : 1f;

            source.Play();

            if (!entry.isLoop)
            {
                ReleaseAfterFinish(index, entry.clip.length, source.pitch).Forget();
            }

            return source;
        }

        public void StopLoop(AudioSource source)
        {
            if (source == null)
                return;

            for (int i = 0; i < sources.Length; i++)
            {
                if (sources[i] != source)
                    continue;

                source.Stop();
                source.clip = null;
                source.loop = false;
                source.pitch = 1f;

                sourceBusy[i] = false;
                return;
            }
        }

        private int GetAvailableSourceIndex()
        {
            for (int i = 0; i < sources.Length; i++)
            {
                int index = (nextSearchStartIndex + i) % sources.Length;

                if (!sourceBusy[index])
                {
                    nextSearchStartIndex = (index + 1) % sources.Length;
                    return index;
                }
            }

            int reusedIndex = nextSearchStartIndex;
            nextSearchStartIndex = (nextSearchStartIndex + 1) % sources.Length;

            var source = sources[reusedIndex];
            source.Stop();
            source.clip = null;
            source.loop = false;
            source.pitch = 1f;

            sourceBusy[reusedIndex] = false;

            return reusedIndex;
        }

        private async UniTaskVoid ReleaseAfterFinish(int index, float clipLength, float pitch)
        {
            float duration = clipLength / Mathf.Max(0.01f, Mathf.Abs(pitch));

            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: destroyToken);

            var source = sources[index];
            if (source == null)
                return;

            if (!source.loop && !source.isPlaying)
            {
                source.clip = null;
                source.pitch = 1f;
                sourceBusy[index] = false;
            }
        }
    }
}