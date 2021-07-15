#if FAUDIO
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using MonoGame.OpenAL;

namespace Microsoft.Xna.Framework.Audio
{
    public sealed partial class SoundEffect : IDisposable
    {
        internal const int MAX_PLAYING_INSTANCES = int.MaxValue;

        internal FAudio.FAudioBuffer handle;
        internal IntPtr formatPtr;
        internal ushort channels;
        internal uint sampleRate;
        internal uint loopStart;
        internal uint loopLength;

        internal static void PlatformInitialize()
        {
            Device();
        }

        internal static void PlatformShutdown()
        {
            Device().Dispose();
        }

        private void PlatformLoadAudioStream(Stream stream, out TimeSpan duration)
        {
            byte[] buffer;

            ALFormat format;
            int freq;
            int channels;
            int blockAlignment;
            int bitsPerSample;
            int samplesPerBlock;
            int sampleCount;
            buffer = AudioLoader.Load(stream, out format, out freq, out channels, out blockAlignment, out bitsPerSample, out samplesPerBlock, out sampleCount);

            duration = TimeSpan.FromSeconds((float)sampleCount / (float)freq);

            PlatformInitializeBuffer(buffer, buffer.Length, format, channels, freq, blockAlignment, bitsPerSample, 0, 0);
        }

        private void PlatformInitializePcm(byte[] buffer, int offset, int count, int sampleBits, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
        {
            if (sampleBits == 24)
            {
                // Convert 24-bit signed PCM to 16-bit signed PCM
                buffer = AudioLoader.Convert24To16(buffer, offset, count);
                offset = 0;
                count = buffer.Length;
                sampleBits = 16;
            }

            var format = AudioLoader.GetSoundFormat(AudioLoader.FormatPcm, (int)channels, sampleBits);
            CreateHandle(buffer, offset, count, null, 1, (ushort)channels, (uint)sampleRate, (uint)(sampleRate * (int)channels * 2), (ushort)((int)channels * 2), 16, loopStart, loopLength);
        }

        private void PlatformInitializeIeeeFloat(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
        {
            var format = AudioLoader.GetSoundFormat(AudioLoader.FormatIeee, (int)channels, 32);

            CreateHandle(buffer, offset, count, null, 3, (ushort)channels, (uint)sampleRate, (uint)(sampleRate * (int)channels * 2), (ushort)((int)channels * 2), 32, loopStart, loopLength);
        }

        private void PlatformInitializeAdpcm(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int blockAlignment, int loopStart, int loopLength)
        {
            /*
            if (!OpenALSoundController.Instance.SupportsAdpcm)
            {
                // If MS-ADPCM is not supported, convert to 16-bit signed PCM
                buffer = AudioLoader.ConvertMsAdpcmToPcm(buffer, offset, count, (int)channels, blockAlignment);
                PlatformInitializePcm(buffer, 0, buffer.Length, 16, sampleRate, channels, loopStart, loopLength);
                return;
            }*/

            var format = AudioLoader.GetSoundFormat(AudioLoader.FormatMsAdpcm, (int)channels, 0);
            int sampleAlignment = AudioLoader.SampleAlignment(format, blockAlignment);

            // bind buffer

            // Buffer length must be aligned with the block alignment
            int alignedCount = count - (count % blockAlignment);
            CreateHandle(buffer, offset, alignedCount, null, 2, (ushort)channels, (uint)sampleRate, (uint)(sampleRate * (int)channels * 2), (ushort)blockAlignment, 16, loopStart, loopLength);

            /*
            SoundBuffer = new OALSoundBuffer();
            // Buffer length must be aligned with the block alignment
            
            int alignedCount = count - (count % blockAlignment);
            SoundBuffer.BindDataBuffer(buffer, format, alignedCount, sampleRate, sampleAlignment);
            */
            //BindDataBuffer(buffer, format, alignedCount, sampleRate, sampleAlignment);
        }

        private void PlatformInitializeIma4(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int blockAlignment, int loopStart, int loopLength)
        {
            // NOT USED IN SDV.
            return;
            if (true/*!OpenALSoundController.Instance.SupportsIma4*/)
            {
                // If IMA/ADPCM is not supported, convert to 16-bit signed PCM
                buffer = AudioLoader.ConvertIma4ToPcm(buffer, offset, count, (int)channels, blockAlignment);
                PlatformInitializePcm(buffer, 0, buffer.Length, 16, sampleRate, channels, loopStart, loopLength);
                return;
            }

            var format = AudioLoader.GetSoundFormat(AudioLoader.FormatIma4, (int)channels, 0);
            int sampleAlignment = AudioLoader.SampleAlignment(format, blockAlignment);

            // bind buffer

            CreateHandle(buffer, offset, count, null, 2, (ushort)channels, (uint)sampleRate, (uint)(sampleRate * (int)channels * 2), (ushort)((int)channels * 2), 16, loopStart, loopLength);

            //SoundBuffer = new OALSoundBuffer();
            //SoundBuffer.BindDataBuffer(buffer, format, count, sampleRate, sampleAlignment);
        }

        public void CreateHandle(byte[] buffer, int offset, int count, byte[] extra_data, ushort format_tag, ushort channels, uint samples_per_sec, uint avg_bytes_per_sec, ushort block_align, ushort bits_per_sample, int loop_start, int loop_length)
        {
            Device();
            this.channels = channels;
            this.sampleRate = samples_per_sec;
            this.loopStart = (uint)loop_start;
            this.loopLength = (uint)loop_length;

            unsafe
            {
                // Buffer Format
                if (extra_data == null)
                {
                    formatPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(FAudio.FAudioWaveFormatEx)));
                }
                else
                {
                    formatPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(FAudio.FAudioWaveFormatEx)) + extra_data.Length);

                    Marshal.Copy(extra_data, 0, formatPtr + Marshal.SizeOf(typeof(FAudio.FAudioWaveFormatEx)), extra_data.Length);
                }

                FAudio.FAudioWaveFormatEx* pcm = (FAudio.FAudioWaveFormatEx*)formatPtr;
                pcm->wFormatTag = format_tag;
                pcm->nChannels = channels;
                pcm->nSamplesPerSec = samples_per_sec;
                pcm->nAvgBytesPerSec = avg_bytes_per_sec;
                pcm->nBlockAlign = block_align;
                pcm->wBitsPerSample = bits_per_sample;
                pcm->cbSize = (ushort)((extra_data == null) ? 0 : extra_data.Length);

                /* Easy stuff */
                handle = new FAudio.FAudioBuffer();
                handle.Flags = FAudio.FAUDIO_END_OF_STREAM;
                handle.pContext = IntPtr.Zero;

                /* Buffer data */
                handle.AudioBytes = (uint)count;
                handle.pAudioData = Marshal.AllocHGlobal(count);
                Marshal.Copy(
                    buffer,
                    offset,
                    handle.pAudioData,
                    count
                );

                /* Play regions */
                handle.PlayBegin = 0;
                if (format_tag == 1)
                {
                    handle.PlayLength = (uint)(
                        count /
                        channels /
                        (bits_per_sample / 8)
                    );
                }
                else if (format_tag == 2)
                {
                    handle.PlayLength = (uint)(
                        count /
                        block_align *
                        (((block_align / channels) - 6) * 2)
                    );
                }
                else if (format_tag == 0x166)
                {
                    FAudio.FAudioXMA2WaveFormatEx* xma2 = (FAudio.FAudioXMA2WaveFormatEx*)formatPtr;
                    // dwSamplesEncoded / nChannels / (wBitsPerSample / 8) doesn't always (if ever?) match up.
                    handle.PlayLength = xma2->dwPlayLength;
                }

                /* Set by Instances! */
                handle.LoopBegin = 0;
                handle.LoopLength = 0;
                handle.LoopCount = 0;
            }
        }

        private void BindDataBuffer(byte[] buffer, ALFormat format, int count, int sample_rate, int sample_alignment = 0)
        {
            Device();
        }

        private void PlatformInitializeFormat(byte[] header, byte[] buffer, int bufferSize, int loopStart, int loopLength)
        {
            var wavFormat = BitConverter.ToInt16(header, 0);
            var channels = BitConverter.ToInt16(header, 2);
            var sampleRate = BitConverter.ToInt32(header, 4);
            var blockAlignment = BitConverter.ToInt16(header, 12);
            var bitsPerSample = BitConverter.ToInt16(header, 14);

            var format = AudioLoader.GetSoundFormat(wavFormat, channels, bitsPerSample);
            PlatformInitializeBuffer(buffer, bufferSize, format, channels, sampleRate, blockAlignment, bitsPerSample, loopStart, loopLength);
        }

        private void PlatformInitializeBuffer(byte[] buffer, int bufferSize, ALFormat format, int channels, int sampleRate, int blockAlignment, int bitsPerSample, int loopStart, int loopLength)
        {
            switch (format)
            {
                case ALFormat.Mono8:
                case ALFormat.Mono16:
                case ALFormat.Stereo8:
                case ALFormat.Stereo16:
                    PlatformInitializePcm(buffer, 0, bufferSize, bitsPerSample, sampleRate, (AudioChannels)channels, loopStart, loopLength);
                    break;
                case ALFormat.MonoMSAdpcm:
                case ALFormat.StereoMSAdpcm:
                    PlatformInitializeAdpcm(buffer, 0, bufferSize, sampleRate, (AudioChannels)channels, blockAlignment, loopStart, loopLength);
                    break;
                case ALFormat.MonoFloat32:
                case ALFormat.StereoFloat32:
                    PlatformInitializeIeeeFloat(buffer, 0, bufferSize, sampleRate, (AudioChannels)channels, loopStart, loopLength);
                    break;
                case ALFormat.MonoIma4:
                case ALFormat.StereoIma4:
                    PlatformInitializeIma4(buffer, 0, bufferSize, sampleRate, (AudioChannels)channels, blockAlignment, loopStart, loopLength);
                    break;
                default:
                    throw new NotSupportedException("Unsupported wave format!");
            }
        }

        private void PlatformInitializeXact(MiniFormatTag codec, byte[] buffer, int channels, int sampleRate, int blockAlignment, int loopStart, int loopLength, out TimeSpan duration)
        {
            if (codec == MiniFormatTag.Adpcm)
            {
                PlatformInitializeAdpcm(buffer, 0, buffer.Length, sampleRate, (AudioChannels)channels, (blockAlignment + 22) * channels, loopStart, loopLength);
                duration = TimeSpan.FromSeconds((double)handle.PlayLength / (double) sampleRate);
                return;
            }

            throw new NotSupportedException("Unsupported sound format!");
        }

        internal static void PlatformSetReverbSettings(ReverbSettings reverbSettings)
        {
            unsafe
            {
                if (Device().ReverbVoice == IntPtr.Zero)
                {
                    return;
                }

                IntPtr rvbParamsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(FAudio.FAudioFXReverbParameters)));
                FAudio.FAudioFXReverbParameters* rvbParams = (FAudio.FAudioFXReverbParameters*)rvbParamsPtr;

                rvbParams->DecayTime = reverbSettings.DecayTimeSec;
                rvbParams->Density = reverbSettings.DensityPct;
                rvbParams->EarlyDiffusion = (byte)reverbSettings.EarlyDiffusion;
                rvbParams->HighEQCutoff = (byte)reverbSettings.HighEqCutoff;
                rvbParams->HighEQGain = (byte)reverbSettings.HighEqGain;
                rvbParams->LateDiffusion = (byte)reverbSettings.LateDiffusion;
                rvbParams->LowEQCutoff = (byte)reverbSettings.LowEqCutoff;
                rvbParams->LowEQGain = (byte)reverbSettings.LowEqGain;
                rvbParams->PositionLeft = (byte)reverbSettings.PositionLeft;
                rvbParams->PositionMatrixLeft = (byte)reverbSettings.PositionLeftMatrix;
                rvbParams->PositionMatrixRight = (byte)reverbSettings.PositionRightMatrix;
                rvbParams->PositionRight = (byte)reverbSettings.PositionRight;
                rvbParams->RearDelay = (byte)reverbSettings.RearDelayMs;
                rvbParams->ReflectionsDelay = (byte)reverbSettings.ReflectionsDelayMs;
                rvbParams->ReflectionsGain = reverbSettings.ReflectionsGainDb;
                rvbParams->ReverbDelay = (byte)reverbSettings.ReverbDelayMs;
                rvbParams->ReverbGain = reverbSettings.ReverbGainDb;
                rvbParams->RoomFilterFreq = reverbSettings.RoomFilterFrequencyHz;
                rvbParams->RoomFilterHF = reverbSettings.RoomFilterHighFrequencyDb;
                rvbParams->RoomFilterMain = reverbSettings.RoomFilterMainDb;
                rvbParams->RoomSize = reverbSettings.RoomSizeFeet;
                rvbParams->WetDryMix = reverbSettings.WetDryMixPct;

                FAudio.FAudioVoice_SetEffectParameters(
                        Device().ReverbVoice,
                        0,
                        rvbParamsPtr,
                        (uint)Marshal.SizeOf(typeof(FAudio.FAudioFXReverbParameters)),
                        0
                    );

                Marshal.FreeHGlobal(rvbParamsPtr);
            }
        }

        private void PlatformSetupInstance(SoundEffectInstance inst)
        {
            //inst.InitializeSound();
        }

        private void PlatformDispose(bool disposing)
        {
            Marshal.FreeHGlobal(formatPtr);
			Marshal.FreeHGlobal(handle.pAudioData);
        }

        internal static FAudioContext Device()
        {
            if (FAudioContext.Context != null)
            {
                return FAudioContext.Context;
            }
            FAudioContext.Create();
            if (FAudioContext.Context == null)
            {
                throw new NoAudioHardwareException("");
            }
            return FAudioContext.Context;
        }
    }
}
#endif
