﻿using System;

namespace GameStateManagementSample
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var factory = new Microsoft.Xna.Platform.GameFrameworkViewSource<GameStateManagementSampleGame>();
            Windows.ApplicationModel.Core.CoreApplication.Run(factory);
        }
    }
}
