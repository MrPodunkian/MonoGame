using MonoGame.OpenAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Platform.Audio
{
    public static partial class DefaultDeviceChecker
    {
        public static void Update()
        {
            if (ReopenDeviceExtension.instance != null && ReopenDeviceExtension.instance.IsInitialized)
            {
                ReopenDeviceExtension.instance.Poll();
            }
        }
    }
}
