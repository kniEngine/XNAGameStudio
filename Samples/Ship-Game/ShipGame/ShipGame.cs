#region File Description
//-----------------------------------------------------------------------------
// ShipGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ShipGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ShipGameGame : Microsoft.Xna.Framework.Game
    {
         static public ShipGameGame Instance; // DONT DO THAT

        GraphicsDeviceManager graphics;
        ScreenManager screen;
        GameManager game;
        FontManager font;

        AudioEngine audioEngine;
        WaveBank waveBank;
        SoundBank soundBank;

        bool renderVsync = true;

        public ShipGameGame()
        {
            ShipGameGame.Instance = this;

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.GraphicsProfile = GraphicsProfile.HiDef;

#if XNA // Xact
            audioEngine = new AudioEngine("Content/sounds/sounds.xgs");
            waveBank = new WaveBank(audioEngine, "Content/sounds/Wave Bank.xwb");
            soundBank = new SoundBank(audioEngine, "Content/sounds/Sound Bank.xsb");
#endif

            game = new GameManager(soundBank);

            graphics.PreferredBackBufferWidth = GameOptions.ScreenWidth;
            graphics.PreferredBackBufferHeight = GameOptions.ScreenHeight;

            IsFixedTimeStep = renderVsync;
            graphics.SynchronizeWithVerticalRetrace = renderVsync;
        }


        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to 
        /// run. This is where it can query for any required services and load any 
        /// non-graphic related content. Calling base.Initialize will enumerate through 
        /// any components and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            font = new FontManager(graphics.GraphicsDevice);
            screen = new ScreenManager(this, font, game);

            font.LoadContent(Content);
            game.LoadContent(graphics.GraphicsDevice, Content);
            screen.LoadContent(graphics.GraphicsDevice, Content);
        }


        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        protected override void UnloadContent()
        {
            screen.UnloadContent();
            game.UnloadContent();
            font.UnloadContent();

            screen = null;
            font = null;
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            float ElapsedTimeFloat = (float)gameTime.ElapsedGameTime.TotalSeconds;

            screen.ProcessInput(ElapsedTimeFloat);
            screen.Update(ElapsedTimeFloat);

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            screen.Draw(graphics.GraphicsDevice);

            base.Draw(gameTime);
        }

        /// <summary>
        /// This is called to switch full screen mode.
        /// </summary>
        public void ToggleFullScreen()
        {
            graphics.ToggleFullScreen();
        }

    }
}
