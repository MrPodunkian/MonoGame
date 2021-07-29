// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>
    /// Provides functionality for manipulating multiple sounds at a time.
    /// </summary>
    public class AudioCategory
    {
        readonly string _name;
        readonly AudioEngine _engine;
        readonly List<Cue> _sounds;

        // This is a bit gross, but we use an array here
        // instead of a field since AudioCategory is a struct
        // This allows us to save _volume when the user
        // holds onto a reference of AudioCategory, or when a cue
        // is created/loaded after the volume's already been set.
        internal float _volume;

        internal bool isBackgroundMusic;
        internal bool isPublic;

        internal bool instanceLimit;
        internal int maxInstances;
        internal MaxInstanceBehavior InstanceBehavior;

        internal CrossfadeType fadeType;
        internal float fadeIn;
        internal float fadeOut;

        
        internal AudioCategory (AudioEngine audioengine, string name, BinaryReader reader)
        {
            Debug.Assert(audioengine != null);
            Debug.Assert(!string.IsNullOrEmpty(name));

            _sounds = new List<Cue>();
            _name = name;
            _engine = audioengine;
            _sounds = new List<Cue>();

            maxInstances = reader.ReadByte ();
            instanceLimit = maxInstances != 0xff;

            fadeIn = (reader.ReadUInt16 () / 1000f);
            fadeOut = (reader.ReadUInt16 () / 1000f);

            byte instanceFlags = reader.ReadByte ();
            fadeType = (CrossfadeType)(instanceFlags & 0x7);
            InstanceBehavior = (MaxInstanceBehavior)(instanceFlags >> 3);

            reader.ReadUInt16 (); //unkn

            var volume = XactHelpers.ParseVolumeFromDecibels(reader.ReadByte());
            _volume = volume;

            byte visibilityFlags = reader.ReadByte ();
            isBackgroundMusic = (visibilityFlags & 0x1) != 0;
            isPublic = (visibilityFlags & 0x2) != 0;
        }

        internal void AddSound(Cue sound)
        {
            _sounds.Add(sound);
        }

        internal void RemoveSound(Cue sound)
        {
            _sounds.Remove(sound);
        }

        internal int GetPlayingInstanceCount()
        {
            var sum = 0;
            for (var i = 0; i < _sounds.Count; i++)
            {
                if (_sounds[i].IsPlaying)
                    sum++;
            }
            return sum;
        }

        internal Cue GetOldestInstance()
        {
            for (var i = 0; i < _sounds.Count; i++)
            {
                if (_sounds[i].IsPlaying)
                    return _sounds[i];
            }
            return null;
        }

        /// <summary>
        /// Gets the category's friendly name.
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Pauses all associated sounds.
        /// </summary>
        public void Pause ()
        {
            foreach (var sound in _sounds)
                sound.Pause();
        }

        /// <summary>
        /// Resumes all associated paused sounds.
        /// </summary>
        public void Resume ()
        {
            foreach (var sound in _sounds)
                sound.Resume();
        }

        /// <summary>
        /// Stops all associated sounds.
        /// </summary>
        public void Stop(AudioStopOptions options)
        {
            foreach (var sound in _sounds)
            {
                sound.Stop(options);
            }
        }

        /// <summary>
        /// Set the volume for this <see cref="AudioCategory"/>.
        /// </summary>
        /// <param name="volume">The new volume of the category.</param>
        /// <exception cref="ArgumentException">If the volume is less than zero.</exception>
        public void SetVolume(float volume)
        {
            if (volume < 0)
                throw new ArgumentException("The volume must be positive.");

            // Updating all the sounds in a category can be
            // very expensive... so avoid it if we can.
            if (_volume == volume)
                return;

            _volume = volume;

            foreach (var sound in _sounds)
            {
                sound._UpdateSoundParameters();
            }
        }

        /// <summary>
        /// Returns the name of this AudioCategory
        /// </summary>
        /// <returns>Friendly name of the AudioCategory</returns>
        public override string ToString()
        {
            return _name;
        }
    }
}

