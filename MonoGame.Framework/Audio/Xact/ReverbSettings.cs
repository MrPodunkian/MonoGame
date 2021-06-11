// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
    public class ReverbSettings
    {
        private readonly DspParameter[] _parameters = new DspParameter[22];

        // ARTHUR 6/11/2021: Made ReverbSettings public and added default constructor based on values from FarmerSounds.xap
        public ReverbSettings()
        {
            _parameters[0] = new DspParameter(15, 0, 300); // ReflectionsDelayMs
            _parameters[1] = new DspParameter(22, 0, 85); // ReverbDelayMs
            _parameters[2] = new DspParameter(6, 0, 30); // PositionLeft
            _parameters[3] = new DspParameter(6, 0, 30); // PositionRight
            _parameters[4] = new DspParameter(27, 0, 30); // PositionLeftMatrix
            _parameters[5] = new DspParameter(27, 0, 30); // PositionRightMatrix
            _parameters[6] = new DspParameter(15, 0, 15); // EarlyDiffusion
            _parameters[7] = new DspParameter(15, 0, 15); // LateDiffusion
            _parameters[8] = new DspParameter(8, 0, 12); // LowEqGain
            _parameters[9] = new DspParameter(4, 0, 9); // LowEqCutoff
            _parameters[10] = new DspParameter(8, 0, 8); // HighEqGain
            _parameters[11] = new DspParameter(6, 0, 14); // HighEqCutoff
            _parameters[12] = new DspParameter(0, 5, 5); // RearDelayMs
            _parameters[13] = new DspParameter(6198.798828F, 20, 20000); // RoomFilterFrequencyHz
            _parameters[14] = new DspParameter(-10, -100, 0); // RoomFilterMainDb
            _parameters[15] = new DspParameter(0, -100, 0); // RoomFilterHighFrequencyDb
            _parameters[16] = new DspParameter(-6.02F, -100, 20); // ReflectionsGainDb
            _parameters[17] = new DspParameter(-3.02F, -100, 20); // ReverbGainDb
            _parameters[18] = new DspParameter(5.125499F, 0.1F, 30); // DecayTimeSec
            _parameters[19] = new DspParameter(100, 0, 100); // DensityPct
            _parameters[20] = new DspParameter(100, 1, 100); // RoomSizeFeet
            _parameters[21] = new DspParameter(100, 0, 100); // WetDryMixPct
        }

        public ReverbSettings(BinaryReader reader)
        {
            _parameters[0] = new DspParameter(reader); // ReflectionsDelayMs
            _parameters[1] = new DspParameter(reader); // ReverbDelayMs
            _parameters[2] = new DspParameter(reader); // PositionLeft
            _parameters[3] = new DspParameter(reader); // PositionRight
            _parameters[4] = new DspParameter(reader); // PositionLeftMatrix
            _parameters[5] = new DspParameter(reader); // PositionRightMatrix
            _parameters[6] = new DspParameter(reader); // EarlyDiffusion
            _parameters[7] = new DspParameter(reader); // LateDiffusion
            _parameters[8] = new DspParameter(reader); // LowEqGain
            _parameters[9] = new DspParameter(reader); // LowEqCutoff
            _parameters[10] = new DspParameter(reader); // HighEqGain
            _parameters[11] = new DspParameter(reader); // HighEqCutoff
            _parameters[12] = new DspParameter(reader); // RearDelayMs
            _parameters[13] = new DspParameter(reader); // RoomFilterFrequencyHz
            _parameters[14] = new DspParameter(reader); // RoomFilterMainDb
            _parameters[15] = new DspParameter(reader); // RoomFilterHighFrequencyDb
            _parameters[16] = new DspParameter(reader); // ReflectionsGainDb
            _parameters[17] = new DspParameter(reader); // ReverbGainDb
            _parameters[18] = new DspParameter(reader); // DecayTimeSec
            _parameters[19] = new DspParameter(reader); // DensityPct
            _parameters[20] = new DspParameter(reader); // RoomSizeFeet
            _parameters[21] = new DspParameter(reader); // WetDryMixPct
        }

        public float this[int index]
        {
            get { return _parameters[index].Value; }
            set { _parameters[index].SetValue(value); }
        }

        public float ReflectionsDelayMs { get { return _parameters[0].Value; } }
        public float ReverbDelayMs { get { return _parameters[1].Value; } }
        public float PositionLeft { get { return _parameters[2].Value; } }
        public float PositionRight { get { return _parameters[3].Value; } }
        public float PositionLeftMatrix { get { return _parameters[4].Value; } }
        public float PositionRightMatrix { get { return _parameters[5].Value; } }
        public float EarlyDiffusion { get { return _parameters[6].Value; } }
        public float LateDiffusion { get { return _parameters[7].Value; } }
        public float LowEqGain { get { return _parameters[8].Value; } }
        public float LowEqCutoff { get { return _parameters[9].Value; } }
        public float HighEqGain { get { return _parameters[10].Value; } }
        public float HighEqCutoff { get { return _parameters[11].Value; } }
        public float RearDelayMs { get { return _parameters[12].Value; } }
        public float RoomFilterFrequencyHz { get { return _parameters[13].Value; } }
        public float RoomFilterMainDb { get { return _parameters[14].Value; } }
        public float RoomFilterHighFrequencyDb { get { return _parameters[15].Value; } }
        public float ReflectionsGainDb { get { return _parameters[16].Value; } }
        public float ReverbGainDb { get { return _parameters[17].Value; } }
        public float DecayTimeSec { get { return _parameters[18].Value; } }
        public float DensityPct { get { return _parameters[19].Value; } }
        public float RoomSizeFeet { get { return _parameters[20].Value; } }
        public float WetDryMixPct { get { return _parameters[21].Value; } }
    }
}
