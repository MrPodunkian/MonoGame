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
        protected bool _complexSound;
        public XactClip[] _soundClips;
        private readonly int _waveBankIndex;
        private readonly int _trackIndex;
        public readonly float _volume;
        public readonly float _pitch;
        public uint _categoryID;
        private readonly SoundBank _soundBank;
        public readonly bool _useReverb;

        internal readonly int[] RpcCurves;

        public List<Cue> activeCues;
        
        public XactSoundBankSound(SoundBank soundBank, int waveBankIndex, int trackIndex)
        {
            _complexSound = false;

            _soundBank = soundBank;
            _waveBankIndex = waveBankIndex;
            _trackIndex = trackIndex;
            RpcCurves = new int[0];

            activeCues = new List<Cue>();
        }

        public XactSoundBankSound(AudioEngine engine, SoundBank soundBank, BinaryReader soundReader)
        {
            _soundBank = soundBank;

            activeCues = new List<Cue>();

            var flags = soundReader.ReadByte();
            _complexSound = (flags & 0x1) != 0;
            var hasRPCs = (flags & 0x0E) != 0;
            var hasDSPs = (flags & 0x10) != 0;

            _categoryID = soundReader.ReadUInt16();
            _volume = XactHelpers.ParseVolumeFromDecibels(soundReader.ReadByte());
            _pitch = soundReader.ReadInt16() / 1200.0f;
            soundReader.ReadByte(); //priority
            soundReader.ReadUInt16(); // filter stuff?
            
            var numClips = 0;
            if (_complexSound)
                numClips = soundReader.ReadByte();
            else 
            {
                _trackIndex = soundReader.ReadUInt16();
                _waveBankIndex = soundReader.ReadByte();
            }

            if (!hasRPCs)
                RpcCurves = new int[0];
            else
            {
                var current = soundReader.BaseStream.Position;

                // This doesn't seem to be used... might have been there
                // to allow for some future file format expansion.
                var dataLength = soundReader.ReadUInt16();

                var numPresets = soundReader.ReadByte();
                RpcCurves = new int[numPresets];
                for (var i = 0; i < numPresets; i++)
                    RpcCurves[i] = engine.GetRpcIndex(soundReader.ReadUInt32());

                // Just in case seek to the right spot.
                soundReader.BaseStream.Seek(current + dataLength, SeekOrigin.Begin);
            }

            if (!hasDSPs)
                _useReverb = false;
            else
            {
                // The file format for this seems to follow the pattern for 
                // the RPC curves above, but in this case XACT only supports
                // a single effect...  Microsoft Reverb... so just set it.
                _useReverb = true;
                soundReader.BaseStream.Seek(7, SeekOrigin.Current);
            }

            if (_complexSound)
            {
                _soundClips = new XactClip[numClips];
                for (int i = 0; i < numClips; i++)
                    _soundClips[i] = new XactClip(soundBank, soundReader, _useReverb);
            }
        }

        public SoundEffectInstance GetSimpleSoundInstance()
        {
            if (_complexSound)
            {
                return null;
            }

            bool streaming;

            SoundEffectInstance instance = _soundBank.GetSoundEffectInstance(_waveBankIndex, _trackIndex, out streaming);

            return instance;
        }
    }
}
