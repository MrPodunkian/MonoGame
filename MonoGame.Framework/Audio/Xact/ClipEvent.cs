// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
	public abstract class ClipEvent
    {
        protected XactClip _clip;

	    protected ClipEvent(XactClip clip, float timeStamp, float randomOffset)
        {
            _clip = clip;
            TimeStamp = timeStamp;
            RandomOffset = randomOffset;
        }

	    public float RandomOffset { get; private set; }

	    public float TimeStamp { get; private set; }

	    public abstract void Fire(Cue cue);
    }
}

