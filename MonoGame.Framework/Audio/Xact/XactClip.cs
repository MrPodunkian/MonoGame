// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
    public class XactClip
    {
        public float DefaultVolume;

        public ClipEvent[] clipEvents;

        public bool FilterEnabled;
        public FilterMode FilterMode;
        public float FilterQ;
        public ushort FilterFrequency;

        internal readonly bool UseReverb;

        public XactClip(List<PlayWaveVariant> variants, bool loop, bool use_reverb)
        {
            clipEvents = new ClipEvent[1];
            var play_wave_event = new PlayWaveEvent(this, variants, 0.0F, 0.0F, loop);
            play_wave_event.variationType = VariationType.Random;
            clipEvents[0] = play_wave_event;

            DefaultVolume = 1.0F;
            UseReverb = use_reverb;
        }

        public XactClip (SoundBank soundBank, BinaryReader clipReader, bool use_reverb)
        {
#pragma warning disable 0219
            UseReverb = use_reverb;

            var volume_db = XactHelpers.ParseDecibels(clipReader.ReadByte());
            DefaultVolume = XactHelpers.ParseVolumeFromDecibels(volume_db);

            var clip_offset = clipReader.ReadUInt32();

            // Read the filter info.
            var filter_q_and_flags = clipReader.ReadUInt16();
            FilterEnabled = (filter_q_and_flags & 1) == 1;
            FilterMode = (FilterMode)((filter_q_and_flags >> 1) & 3);
            FilterQ = (filter_q_and_flags >> 3) * 0.01f;
            FilterFrequency = clipReader.ReadUInt16();

            var old_position = clipReader.BaseStream.Position;
            clipReader.BaseStream.Seek(clip_offset, SeekOrigin.Begin);
            
            var num_events = clipReader.ReadByte();
            clipEvents = new ClipEvent[num_events];
            
            for (var i=0; i<num_events; i++) 
            {
                var eventInfo = clipReader.ReadUInt32();
                var randomOffset = clipReader.ReadUInt16() * 0.001f;

                // TODO: eventInfo still has 11 bits that are unknown!
                var eventId = eventInfo & 0x1F;
                var timeStamp = ((eventInfo >> 5) & 0xFFFF) * 0.001f;
                var unknown = eventInfo >> 21;

                switch (eventId) {
                case 0:
                    // Stop Event
                    throw new NotImplementedException("Stop event");

                case 1:
                {
                    // Unknown!
                    clipReader.ReadByte();

                    // Event flags
                    var eventFlags = clipReader.ReadByte();
                    var playRelease = (eventFlags & 0x01) == 0x01;
                    var panEnabled = (eventFlags & 0x02) == 0x02;
                    var useCenterSpeaker = (eventFlags & 0x04) == 0x04;

                    int trackIndex = clipReader.ReadUInt16();
                    int waveBankIndex = clipReader.ReadByte();					
                    var loopCount = clipReader.ReadByte();
                    var panAngle = clipReader.ReadUInt16() / 100.0f;
                    var panArc = clipReader.ReadUInt16() / 100.0f;
                    
                    clipEvents[i] = new PlayWaveEvent(
                        this,
                        timeStamp, 
                        randomOffset,
                        soundBank, 
                        new[] { waveBankIndex }, 
                        new[] { trackIndex },
                        null,
                        0,
                        VariationType.Ordered, 
                        null,
                        null,
                        null,
                        loopCount,
                        false);

                    break;
                }

                case 3:
                {
                    // Unknown!
                    clipReader.ReadByte();

                    // Event flags
                    var eventFlags = clipReader.ReadByte();
                    var playRelease = (eventFlags & 0x01) == 0x01;
                    var panEnabled = (eventFlags & 0x02) == 0x02;
                    var useCenterSpeaker = (eventFlags & 0x04) == 0x04;

                    var loopCount = clipReader.ReadByte();
                    var panAngle = clipReader.ReadUInt16() / 100.0f;
                    var panArc = clipReader.ReadUInt16() / 100.0f;

                    // The number of tracks for the variations.
                    var numTracks = clipReader.ReadUInt16();

                    // Not sure what most of this is.
                    var moreFlags = clipReader.ReadByte();
                    var newWaveOnLoop = (moreFlags & 0x40) == 0x40;
                    
                    // The variation playlist type seems to be 
                    // stored in the bottom 4bits only.
                    var variationType = (VariationType)(moreFlags & 0x0F);

                    // Unknown!
                    clipReader.ReadBytes(5);

                    // Read in the variation playlist.
                    var waveBanks = new int[numTracks];
                    var tracks = new int[numTracks];
                    var weights = new byte[numTracks];
                    var totalWeights = 0;
                    for (var j = 0; j < numTracks; j++)
                    {
                        tracks[j] = clipReader.ReadUInt16();
                        waveBanks[j] = clipReader.ReadByte();
                        var minWeight = clipReader.ReadByte();
                        var maxWeight = clipReader.ReadByte();
                        weights[j] = (byte)(maxWeight - minWeight);
                        totalWeights += weights[j];
                    }

                    clipEvents[i] = new PlayWaveEvent(
                        this,
                        timeStamp,
                        randomOffset,
                        soundBank, 
                        waveBanks, 
                        tracks,
                        weights,
                        totalWeights,
                        variationType,
                        null,
                        null,
                        null,
                        loopCount,
                        newWaveOnLoop);

                    break;
                }

                case 4:
                {
                    // Unknown!
                    clipReader.ReadByte();

                    // Event flags
                    var eventFlags = clipReader.ReadByte();
                    var playRelease = (eventFlags & 0x01) == 0x01;
                    var panEnabled = (eventFlags & 0x02) == 0x02;
                    var useCenterSpeaker = (eventFlags & 0x04) == 0x04;

                    int trackIndex = clipReader.ReadUInt16();
                    int waveBankIndex = clipReader.ReadByte();
                    var loopCount = clipReader.ReadByte();
                    var panAngle = clipReader.ReadUInt16() / 100.0f;
                    var panArc = clipReader.ReadUInt16() / 100.0f;

                    // Pitch variation range
                    var minPitch = clipReader.ReadInt16() / 1200.0f;
                    var maxPitch = clipReader.ReadInt16() / 1200.0f;

                    // Volume variation range
                    var minVolume = XactHelpers.ParseVolumeFromDecibels(clipReader.ReadByte());
                    var maxVolume = XactHelpers.ParseVolumeFromDecibels(clipReader.ReadByte());

                    // Filter variation
                    var minFrequency = clipReader.ReadSingle();
                    var maxFrequency = clipReader.ReadSingle();
                    var minQ = clipReader.ReadSingle();
                    var maxQ = clipReader.ReadSingle();

                    // Unknown!
                    clipReader.ReadByte();

                    var variationFlags = clipReader.ReadByte();

                    // Enable pitch variation
                    Vector2? pitchVar = null;
                    if ((variationFlags & 0x10) == 0x10)
                        pitchVar = new Vector2(minPitch, maxPitch - minPitch);

                    // Enable volume variation
                    Vector2? volumeVar = null;
                    if ((variationFlags & 0x20) == 0x20)
                        volumeVar = new Vector2(minVolume, maxVolume - minVolume);

                    // Enable filter variation
                    Vector4? filterVar = null;
                    if ((variationFlags & 0x40) == 0x40)
                        filterVar = new Vector4(minFrequency, maxFrequency - minFrequency, minQ, maxQ - minQ);

                    clipEvents[i] = new PlayWaveEvent(
                        this,
                        timeStamp,
                        randomOffset,
                        soundBank,
                        new[] { waveBankIndex },
                        new[] { trackIndex }, 
                        null,
                        0,
                        VariationType.Ordered,
                        volumeVar,
                        pitchVar, 
                        filterVar,
                        loopCount,
                        false);

                    break;
                }

                case 6:
                {
                    // Unknown!
                    clipReader.ReadByte();

                    // Event flags
                    var eventFlags = clipReader.ReadByte();
                    var playRelease = (eventFlags & 0x01) == 0x01;
                    var panEnabled = (eventFlags & 0x02) == 0x02;
                    var useCenterSpeaker = (eventFlags & 0x04) == 0x04;

                    var loopCount = clipReader.ReadByte();
                    var panAngle = clipReader.ReadUInt16() / 100.0f;
                    var panArc = clipReader.ReadUInt16() / 100.0f;

                    // Pitch variation range
                    var minPitch = clipReader.ReadInt16() / 1200.0f;
                    var maxPitch = clipReader.ReadInt16() / 1200.0f;

                    // Volume variation range
                    var minVolume = XactHelpers.ParseVolumeFromDecibels(clipReader.ReadByte());
                    var maxVolume = XactHelpers.ParseVolumeFromDecibels(clipReader.ReadByte());

                    // Filter variation range
                    var minFrequency = clipReader.ReadSingle();
                    var maxFrequency = clipReader.ReadSingle();
                    var minQ = clipReader.ReadSingle();
                    var maxQ = clipReader.ReadSingle();

                    // Unknown!
                    clipReader.ReadByte();

                    // TODO: Still has unknown bits!
                    var variationFlags = clipReader.ReadByte();

                    // Enable pitch variation
                    Vector2? pitchVar = null;
                    if ((variationFlags & 0x10) == 0x10)
                        pitchVar = new Vector2(minPitch, maxPitch - minPitch);

                    // Enable volume variation
                    Vector2? volumeVar = null;
                    if ((variationFlags & 0x20) == 0x20)
                        volumeVar = new Vector2(minVolume, maxVolume - minVolume);

                    // Enable filter variation
                    Vector4? filterVar = null;
                    if ((variationFlags & 0x40) == 0x40)
                        filterVar = new Vector4(minFrequency, maxFrequency - minFrequency, minQ, maxQ - minQ);

                    // The number of tracks for the variations.
                    var numTracks = clipReader.ReadUInt16();

                    // Not sure what most of this is.
                    var moreFlags = clipReader.ReadByte();
                    var newWaveOnLoop = (moreFlags & 0x40) == 0x40;

                    // The variation playlist type seems to be 
                    // stored in the bottom 4bits only.
                    var variationType = (VariationType)(moreFlags & 0x0F);

                    // Unknown!
                    clipReader.ReadBytes(5);

                    // Read in the variation playlist.
                    var waveBanks = new int[numTracks];
                    var tracks = new int[numTracks];
                    var weights = new byte[numTracks];
                    var totalWeights = 0;
                    for (var j = 0; j < numTracks; j++)
                    {
                        tracks[j] = clipReader.ReadUInt16();
                        waveBanks[j] = clipReader.ReadByte();
                        var minWeight = clipReader.ReadByte();
                        var maxWeight = clipReader.ReadByte();
                        weights[j] = (byte)(maxWeight - minWeight);
                        totalWeights += weights[j];
                    }

                    clipEvents[i] = new PlayWaveEvent(
                        this,
                        timeStamp,
                        randomOffset,
                        soundBank,
                        waveBanks,
                        tracks,
                        weights,
                        totalWeights,
                        variationType,
                        volumeVar,
                        pitchVar, 
                        filterVar,
                        loopCount,
                        newWaveOnLoop);

                    break;
                }

                case 7:
                    // Pitch Event
                    throw new NotImplementedException("Pitch event");

                case 8:
                {
                    // Unknown!
                    clipReader.ReadBytes(2);

                    // Event flags
                    var eventFlags = clipReader.ReadByte();
                    var isAdd = (eventFlags & 0x01) == 0x01;

                    // The replacement or additive volume.
                    var decibles = clipReader.ReadSingle() / 100.0f;
                    var volume = XactHelpers.ParseVolumeFromDecibels(decibles + (isAdd ? volume_db : 0));

                    // Unknown!
                    clipReader.ReadBytes(9);

                    clipEvents[i] = new VolumeEvent(   this, 
                                                    timeStamp, 
                                                    randomOffset, 
                                                    volume);
                    break;
                }

                case 17:
                    // Volume Repeat Event
                    throw new NotImplementedException("Volume repeat event");

                case 9:
                    // Marker Event
                    throw new NotImplementedException("Marker event");

                default:
                    throw new NotSupportedException("Unknown event " + eventId);
                }
            }
            
            clipReader.BaseStream.Seek (old_position, SeekOrigin.Begin);
#pragma warning restore 0219
        }

        internal void Update(Cue cue, float old_time, float new_time)
        {
            // Scan the events and trigger any we have just passed.
            for (int i = 0; i < clipEvents.Length; i++)
            {
                var current_event = clipEvents[i];

                // We just passed a new event.

                if (new_time >= current_event.TimeStamp)
                {
                    if (old_time < current_event.TimeStamp) // This is a freshly passed event -- fire it.
                    {
                        current_event.Fire(cue);
                    }
                    else
                    {

                    }
                }
            }
        }
    }
}

