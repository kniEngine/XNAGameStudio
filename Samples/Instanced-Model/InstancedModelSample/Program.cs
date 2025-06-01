#region File Description
//-----------------------------------------------------------------------------
// InstancedModelSampleGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;

namespace InstancedModelSample
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (InstancedModelSampleGame game = new InstancedModelSampleGame())
            {
                game.Run();
            }
        }
    }
#endif
}

