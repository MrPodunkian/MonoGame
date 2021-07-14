#if FAUDIO
// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MonoGame.Framework.Utilities;

#if OPENAL
using MonoGame.OpenAL;
#if IOS || MONOMAC
using AudioToolbox;
using AudioUnit;
using AVFoundation;
#endif
#endif

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>
    /// Provides microphones capture features.  
    /// </summary>
    public sealed partial class Microphone
    {
        internal static void PopulateCaptureDevices()
        {

        }

        internal void PlatformStart()
        {

        }

        internal void PlatformStop()
        {

        }

        internal int GetQueuedSampleCount()
        {
            return 0;
        }

        internal void Update()
        {

        }

        internal int PlatformGetData(byte[] buffer, int offset, int count)
        {
            return 0;
        }
    }
}
#endif
