// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
	class VolumeEvent : ClipEvent
	{
	    private readonly float _volume;

        public VolumeEvent(XactClip clip, float timeStamp, float randomOffset, float volume)
            : base(clip, timeStamp, randomOffset)
        {
            _volume = volume;
        }

		public override void Fire(Cue cue) 
        {
            cue.Volume = _volume;
        }
    }
}

