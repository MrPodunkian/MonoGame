// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
    public class XactSoundBankSound
    {
        public bool complexSound;
        public XactClip[] soundClips;
        public readonly int waveBankIndex;
        public readonly int trackIndex;
        public float volume = 1.0F;
        public float pitch;
        public uint categoryID;
        public SoundBank soundBank;
        public bool useReverb;

        public int[] rpcCurves;

        public XactSoundBankSound(SoundEffect[] sound_effects, int category_id, bool loop = false, bool use_reverb = false)
        {
            List<PlayWaveVariant> variants = new List<PlayWaveVariant>();

            foreach (var sound_effect in sound_effects)
            {
                var variant = new PlayWaveVariant();
                variant.overrideSoundEffect = sound_effect;
                variants.Add(variant);
            }

            complexSound = true;
            soundClips = new XactClip[1];
            rpcCurves = new int[0];
            categoryID = (uint)category_id;
            useReverb = use_reverb;

            soundClips[0] = new XactClip(variants, loop, useReverb);
        }

        public XactSoundBankSound(List<PlayWaveVariant> variants, int category_id, bool loop = false, bool use_reverb = false)
        {
            complexSound = true;
            soundClips = new XactClip[1];
            rpcCurves = new int[0];
            categoryID = (uint)category_id;
            useReverb = use_reverb;

            soundClips[0] = new XactClip(variants, loop, useReverb);
        }

        public XactSoundBankSound(SoundBank soundBank, int waveBankIndex, int trackIndex)
        {
            complexSound = false;

            this.soundBank = soundBank;
            this.waveBankIndex = waveBankIndex;
            this.trackIndex = trackIndex;
            rpcCurves = new int[0];
        }

        public XactSoundBankSound(AudioEngine engine, SoundBank soundBank, BinaryReader soundReader)
        {
            this.soundBank = soundBank;

            var flags = soundReader.ReadByte();
            complexSound = (flags & 0x1) != 0;
            var hasRPCs = (flags & 0x0E) != 0;
            var hasDSPs = (flags & 0x10) != 0;

            categoryID = soundReader.ReadUInt16();
            volume = XactHelpers.ParseVolumeFromDecibels(soundReader.ReadByte());
            pitch = soundReader.ReadInt16() / 1200.0f;
            soundReader.ReadByte(); //priority
            soundReader.ReadUInt16(); // filter stuff?
            
            var numClips = 0;
            if (complexSound)
                numClips = soundReader.ReadByte();
            else 
            {
                trackIndex = soundReader.ReadUInt16();
                waveBankIndex = soundReader.ReadByte();
            }

            if (!hasRPCs)
                rpcCurves = new int[0];
            else
            {
                var current = soundReader.BaseStream.Position;

                // This doesn't seem to be used... might have been there
                // to allow for some future file format expansion.
                var dataLength = soundReader.ReadUInt16();

                var numPresets = soundReader.ReadByte();
                rpcCurves = new int[numPresets];
                for (var i = 0; i < numPresets; i++)
                    rpcCurves[i] = engine.GetRpcIndex(soundReader.ReadUInt32());

                // Just in case seek to the right spot.
                soundReader.BaseStream.Seek(current + dataLength, SeekOrigin.Begin);
            }

            if (!hasDSPs)
                useReverb = false;
            else
            {
                // The file format for this seems to follow the pattern for 
                // the RPC curves above, but in this case XACT only supports
                // a single effect...  Microsoft Reverb... so just set it.
                useReverb = true;
                soundReader.BaseStream.Seek(7, SeekOrigin.Current);
            }

            if (complexSound)
            {
                soundClips = new XactClip[numClips];
                for (int i = 0; i < numClips; i++)
                    soundClips[i] = new XactClip(soundBank, soundReader, useReverb);
            }
        }

        public SoundEffectInstance GetSimpleSoundInstance()
        {
            if (complexSound)
            {
                return null;
            }

            bool streaming;

            SoundEffectInstance instance = soundBank.GetSoundEffectInstance(waveBankIndex, trackIndex, out streaming);

            return instance;
        }
    }
}
