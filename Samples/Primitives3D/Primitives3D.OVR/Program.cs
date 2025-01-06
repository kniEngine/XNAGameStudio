﻿using System;

namespace Primitives3D
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Uncomment this line to enable VR with the nkast.Kni.Platform.WinForms.DX11.OculusOVR package.
            Microsoft.Xna.Platform.XR.XRFactory.RegisterXRFactory(new Microsoft.Xna.Platform.XR.LibOVR.ConcreteXRFactory());

            using (var game = new Primitives3DVRGame())
                game.Run();
        }
    }
}
