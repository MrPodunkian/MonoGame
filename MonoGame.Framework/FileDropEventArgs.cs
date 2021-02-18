// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// This class is used in the <see cref="SdlGameWindow.FileDrop"/> event as <see cref="EventArgs"/>.
    /// </summary>
    public struct FileDropEventArgs
    {
        public FileDropEventArgs(string file_path)
        {
            FilePath = file_path;
        }

        /// <summary>
        /// The path to the file that was dropped onto the window.
        /// </summary>
        public readonly string FilePath;
    }
}
