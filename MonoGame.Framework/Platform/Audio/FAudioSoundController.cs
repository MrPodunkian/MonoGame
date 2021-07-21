using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Audio
{
    internal class FAudioContext
    {
        public static FAudioContext Context = null;

        public readonly IntPtr Handle;
        public readonly byte[] Handle3D;
        public readonly IntPtr MasterVoice;
        public readonly FAudio.FAudioDeviceDetails DeviceDetails;

        public float CurveDistanceScaler;
        public float DopplerScale;
        public float SpeedOfSound;

        public IntPtr ReverbVoice;
        private FAudio.FAudioVoiceSends reverbSends;

        private FAudioContext(IntPtr ctx, uint devices)
        {
            Handle = ctx;

            uint i;
            for (i = 0; i < devices; i += 1)
            {
                FAudio.FAudio_GetDeviceDetails(
                    Handle,
                    i,
                    out DeviceDetails
                );
                if ((DeviceDetails.Role & FAudio.FAudioDeviceRole.FAudioDefaultGameDevice) == FAudio.FAudioDeviceRole.FAudioDefaultGameDevice)
                {
                    break;
                }
            }
            if (i == devices)
            {
                i = 0; /* Oh well. */
                FAudio.FAudio_GetDeviceDetails(
                    Handle,
                    i,
                    out DeviceDetails
                );
            }
            if (FAudio.FAudio_CreateMasteringVoice(
                Handle,
                out MasterVoice,
                FAudio.FAUDIO_DEFAULT_CHANNELS,
                FAudio.FAUDIO_DEFAULT_SAMPLERATE,
                0,
                i,
                IntPtr.Zero
            ) != 0)
            {
                FAudio.FAudio_Release(ctx);
                Handle = IntPtr.Zero;
                Debug.WriteLine(
                    "Failed to create mastering voice!"
                );
                return;
            }

            CurveDistanceScaler = 1.0f;
            DopplerScale = 1.0f;
            SpeedOfSound = 343.5f;
            Handle3D = new byte[FAudio.F3DAUDIO_HANDLE_BYTESIZE];
            FAudio.F3DAudioInitialize(
                DeviceDetails.OutputFormat.dwChannelMask,
                SpeedOfSound,
                Handle3D
            );

            Context = this;
        }

        public void Dispose()
        {
            if (ReverbVoice != IntPtr.Zero)
            {
                FAudio.FAudioVoice_DestroyVoice(ReverbVoice);
                ReverbVoice = IntPtr.Zero;
                Marshal.FreeHGlobal(reverbSends.pSends);
            }
            if (MasterVoice != IntPtr.Zero)
            {
                FAudio.FAudioVoice_DestroyVoice(MasterVoice);
            }
            if (Handle != IntPtr.Zero)
            {
                FAudio.FAudio_Release(Handle);
            }
            Context = null;
        }

        public unsafe void AttachReverb(IntPtr voice)
        {
            // Only create a reverb voice if they ask for it!
            if (ReverbVoice == IntPtr.Zero)
            {
                IntPtr reverb;
                FAudio.FAudioCreateReverb(out reverb, 0);

                IntPtr chainPtr;
                chainPtr = Marshal.AllocHGlobal(
                    Marshal.SizeOf(typeof(FAudio.FAudioEffectChain))
                );
                FAudio.FAudioEffectChain* reverbChain = (FAudio.FAudioEffectChain*)chainPtr;
                reverbChain->EffectCount = 1;
                reverbChain->pEffectDescriptors = Marshal.AllocHGlobal(
                    Marshal.SizeOf(typeof(FAudio.FAudioEffectDescriptor))
                );

                FAudio.FAudioEffectDescriptor* reverbDesc =
                    (FAudio.FAudioEffectDescriptor*)reverbChain->pEffectDescriptors;
                reverbDesc->InitialState = 1;
                reverbDesc->OutputChannels = (uint)(
                    (DeviceDetails.OutputFormat.Format.nChannels == 6) ? 6 : 1
                );
                reverbDesc->pEffect = reverb;

                FAudio.FAudio_CreateSubmixVoice(
                    Handle,
                    out ReverbVoice,
                    1, /* Reverb will be omnidirectional */
                    DeviceDetails.OutputFormat.Format.nSamplesPerSec,
                    0,
                    0,
                    IntPtr.Zero,
                    chainPtr
                );
                FAudio.FAPOBase_Release(reverb);

                Marshal.FreeHGlobal(reverbChain->pEffectDescriptors);
                Marshal.FreeHGlobal(chainPtr);

                // Defaults based on FAUDIOFX_I3DL2_PRESET_GENERIC
                IntPtr rvbParamsPtr = Marshal.AllocHGlobal(
                    Marshal.SizeOf(typeof(FAudio.FAudioFXReverbParameters))
                );
                FAudio.FAudioFXReverbParameters* rvbParams = (FAudio.FAudioFXReverbParameters*)rvbParamsPtr;
                rvbParams->WetDryMix = 100.0f;
                rvbParams->ReflectionsDelay = 7;
                rvbParams->ReverbDelay = 11;
                rvbParams->RearDelay = FAudio.FAUDIOFX_REVERB_DEFAULT_REAR_DELAY;
                rvbParams->PositionLeft = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION;
                rvbParams->PositionRight = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION;
                rvbParams->PositionMatrixLeft = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION_MATRIX;
                rvbParams->PositionMatrixRight = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION_MATRIX;
                rvbParams->EarlyDiffusion = 15;
                rvbParams->LateDiffusion = 15;
                rvbParams->LowEQGain = 8;
                rvbParams->LowEQCutoff = 4;
                rvbParams->HighEQGain = 8;
                rvbParams->HighEQCutoff = 6;
                rvbParams->RoomFilterFreq = 5000f;
                rvbParams->RoomFilterMain = -10f;
                rvbParams->RoomFilterHF = -1f;
                rvbParams->ReflectionsGain = -26.0200005f;
                rvbParams->ReverbGain = 10.0f;
                rvbParams->DecayTime = 1.49000001f;
                rvbParams->Density = 100.0f;
                rvbParams->RoomSize = FAudio.FAUDIOFX_REVERB_DEFAULT_ROOM_SIZE;
                FAudio.FAudioVoice_SetEffectParameters(
                    ReverbVoice,
                    0,
                    rvbParamsPtr,
                    (uint)Marshal.SizeOf(typeof(FAudio.FAudioFXReverbParameters)),
                    0
                );
                Marshal.FreeHGlobal(rvbParamsPtr);

                reverbSends = new FAudio.FAudioVoiceSends();
                reverbSends.SendCount = 2;
                reverbSends.pSends = Marshal.AllocHGlobal(
                    2 * Marshal.SizeOf(typeof(FAudio.FAudioSendDescriptor))
                );
                FAudio.FAudioSendDescriptor* sendDesc = (FAudio.FAudioSendDescriptor*)reverbSends.pSends;
                sendDesc[0].Flags = 0;
                sendDesc[0].pOutputVoice = MasterVoice;
                sendDesc[1].Flags = 0;
                sendDesc[1].pOutputVoice = ReverbVoice;
            }

            // Oh hey here's where we actually attach it
            FAudio.FAudioVoice_SetOutputVoices(
                voice,
                ref reverbSends
            );
        }

        public static void Create()
        {
            IntPtr ctx;
            try
            {
                FAudio.FAudioCreate(
                    out ctx,
                    0,
                    FAudio.FAUDIO_DEFAULT_PROCESSOR
                );
            }
            catch
            {
                Console.WriteLine("FAudio: Failed to initialize.");
                /* FAudio is missing, bail! */
                return;
            }

            uint devices;
            FAudio.FAudio_GetDeviceCount(
                ctx,
                out devices
            );
            if (devices == 0)
            {
                Console.WriteLine("FAudio: No sound devices.");
                /* No sound cards, bail! */
                FAudio.FAudio_Release(ctx);
                return;
            }

            FAudioContext context = new FAudioContext(ctx, devices);

            if (context.Handle == IntPtr.Zero)
            {
                Console.WriteLine("FAudio: Failed to configure sound device..");
                /* Soundcard failed to configure, bail! */
                context.Dispose();
                return;
            }

            Context = context;
        }
    }
}
