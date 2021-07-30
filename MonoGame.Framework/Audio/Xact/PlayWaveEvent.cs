// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
    enum VariationType
    {
        Ordered,
        OrderedFromRandom,
        Random,
        RandomNoImmediateRepeats,
        Shuffle
    };

    class PlayWaveEvent : ClipEvent
    {
        private readonly SoundBank _soundBank;

        private readonly VariationType _variation;

        private readonly int _loopCount;

        private readonly bool _newWaveOnLoop;

        protected List<PlayWaveVariant> _variants;
        
        protected int _totalWeights;

        protected float _trackVolume;
        protected float _trackPitch;
        protected float _trackFilterFrequency;
        protected float _trackFilterQFactor;

        protected float _clipReverbMix;

        private readonly Vector4? _filterVar;
        private readonly Vector2? _volumeVar;
        private readonly Vector2? _pitchVar;

        private bool _streaming;

        public PlayWaveEvent(   XactClip clip, float timeStamp, float randomOffset, SoundBank soundBank,
                                int[] waveBanks, int[] tracks, byte[] weights, int totalWeights,
                                VariationType variation, Vector2? volumeVar, Vector2? pitchVar, Vector4? filterVar,
                                int loopCount, bool newWaveOnLoop)
            : base(clip, timeStamp, randomOffset)
        {
            _soundBank = soundBank;

            _variants = new List<PlayWaveVariant>();
            _totalWeights = 0;

            for (int i = 0; i < tracks.Length; i++)
            {
                PlayWaveVariant variant = new PlayWaveVariant();
                variant.soundBank = _soundBank;
                variant.waveBank = waveBanks[i];
                variant.track = tracks[i];

                if (weights != null)
                {
                    variant.weight = weights[i];
                }
                else
                {
                    variant.weight = 1;
                }

                _totalWeights += variant.weight;

                _variants.Add(variant);
            }

            _volumeVar = volumeVar;
            _pitchVar = pitchVar;
            _filterVar = filterVar;

            _trackVolume = 1.0f;
            _trackPitch = 0;
            _trackFilterFrequency = 0;
            _trackFilterQFactor = 0;

            if (_clip.UseReverb)
            {
                _clipReverbMix = 1.0F;
            }
            else
            {
                _clipReverbMix = 0;
            }

            _variation = variation;
            _loopCount = loopCount;
            _newWaveOnLoop = newWaveOnLoop;
        }

        public override void Fire(Cue cue)
        {
            Play(true, cue);
        }

        private void Play(bool pickNewWav, Cue cue)
        {
            var trackCount = _variants.Count;

            int variant_index = cue.VariantIndex;

            if (variant_index < 0)
            {
                variant_index = 0;
            }

            // Do we need to pick a new wav to play first?
            if (pickNewWav)
            {
                switch (_variation)
                {
                    case VariationType.Ordered:
                        variant_index = (variant_index + 1) % trackCount;
                        break;

                    case VariationType.OrderedFromRandom:
                        variant_index = (variant_index + 1) % trackCount;
                        break;

                    case VariationType.Random:
                        {
                            var sum = XactHelpers.Random.Next(_totalWeights);
                            for (var i=0; i < trackCount; i++)
                            {
                                sum -= _variants[i].weight;
                                if (sum <= 0)
                                {
                                    variant_index = i;
                                    break;
                                }
                            }
                        }
                        break;

                    case VariationType.RandomNoImmediateRepeats:
                    {
                        var last = variant_index;
                        var sum = XactHelpers.Random.Next(_totalWeights);
                        for (var i=0; i < trackCount; i++)
                        {
                            sum -= _variants[i].weight;
                            if (sum <= 0)
                            {
                                variant_index = i;
                                break;
                            }
                        }

                        if (variant_index == last)
                            variant_index = (variant_index + 1) % trackCount;
                        break;
                    }

                    case VariationType.Shuffle:
                        // TODO: Need some sort of deck implementation.
                        variant_index = XactHelpers.Random.Next() % trackCount;
                        break;
                };
            }

            SoundEffectInstance new_wave = _variants[variant_index].GetSoundEffectInstance();

            if (new_wave == null)
            {
                // We couldn't create a sound effect instance, most likely
                // because we've reached the sound pool limits.
                return;
            }

            _trackVolume = _clip.DefaultVolume;

            // Do all the randoms before we play.
            if (_volumeVar.HasValue)
                _trackVolume = _volumeVar.Value.X + ((float)XactHelpers.Random.NextDouble() * _volumeVar.Value.Y);
            if (_pitchVar.HasValue)
                _trackPitch = _pitchVar.Value.X + ((float)XactHelpers.Random.NextDouble() * _pitchVar.Value.Y);
            if (_clip.FilterEnabled)
            {
                if (_filterVar.HasValue)
                {
                    _trackFilterFrequency = _filterVar.Value.X + ((float)XactHelpers.Random.NextDouble() * _filterVar.Value.Y);
                    _trackFilterQFactor = _filterVar.Value.Z + ((float)XactHelpers.Random.NextDouble() * _filterVar.Value.W);
                }
                else
                {
                    _trackFilterFrequency = _clip.FilterFrequency;
                    _trackFilterQFactor = _clip.FilterQ;                
                }
            }
 
            // This is a shortcut for infinite looping of a single track.
            // NOTE 7/29/2021: Non-infinite loops are not supported currently.
            new_wave.IsLooped = _loopCount == 255 && trackCount == 1;

            new_wave.Volume = _trackVolume;
            new_wave.Pitch = _trackPitch;

            if (_clip.UseReverb)
            {
                new_wave.PlatformSetReverbMix(_clipReverbMix);
            }

            if (_clip.FilterEnabled)
            {
                new_wave.PlatformSetFilter(_clip.FilterMode, _trackFilterQFactor, _trackFilterFrequency);
            }

            cue.Volume = _trackVolume;
            cue.Pitch = _trackPitch;
            cue.PlaySoundInstance(new_wave, variant_index);
        }

        public void SetSoundEffects(List<SoundEffect> sounds)
        {
            _variants.Clear();
            _totalWeights = 0;

            foreach (var sound in sounds)
            {
                PlayWaveVariant variant = new PlayWaveVariant();
                variant.overrideSoundEffect = sound;
                _totalWeights += variant.weight;

                _variants.Add(variant);
            }
        }
    }

    public class PlayWaveVariant
    {
        public SoundEffect overrideSoundEffect = null;
        public SoundBank soundBank = null;
        public int waveBank = -1;
        public int track = -1;

        public byte weight = 1;

        public SoundEffect GetSoundEffect()
        {
            if (overrideSoundEffect != null)
            {
                return overrideSoundEffect;
            }

            if (soundBank != null)
            {
                return soundBank.GetSoundEffect(waveBank, track);
            }

            return null;
        }

        public SoundEffectInstance GetSoundEffectInstance()
        {
            if (overrideSoundEffect != null)
            {
                return overrideSoundEffect.GetPooledInstance(true);
            }

            if (soundBank != null)
            {
                bool streaming;
                return soundBank.GetSoundEffectInstance(waveBank, track, out streaming);
            }

            return null;
        }
    }
}

