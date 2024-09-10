#region File Description
//-----------------------------------------------------------------------------
// Primitives3DGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
#endregion

namespace Primitives3D
{
    /// <summary>
    /// This sample shows how to draw 3D geometric primitives
    /// such as cubes, spheres, and cylinders.
    /// </summary>
    public class Primitives3DGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;

        SpriteBatch spriteBatch;
        SpriteFont spriteFont;

        KeyboardState currentKeyboardState;
        KeyboardState lastKeyboardState;
        GamePadState currentGamePadState;
        GamePadState lastGamePadState;
        MouseState currentMouseState;
        MouseState lastMouseState;
        TouchCollection currentTouchState;

        // Store a list of primitive models, plus which one is currently selected.
        List<GeometricPrimitive> primitives = new List<GeometricPrimitive>();

        int currentPrimitiveIndex = 0;

        // store a wireframe rasterize state
        RasterizerState wireFrameState;

        // Store a list of tint colors, plus which one is currently selected.
        List<Color> colors = new List<Color>
        {
            Color.Red,
            Color.Green,
            Color.Blue,
            Color.White,
            Color.Black,
        };

        int currentColorIndex = 0;

        // Are we rendering in wireframe mode?
        bool isWireframe;


        #endregion

        #region Initialization


        public Primitives3DGame()
        {
            Content.RootDirectory = "Content";
            graphics = new GraphicsDeviceManager(this);

            graphics.GraphicsProfile = GraphicsProfile.HiDef;

#if WINDOWS_PHONE
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 800;
            graphics.IsFullScreen = true;
#endif
        }

        BasicEffect basicEffect;

        const int VertexCount = 128;
        VertexPositionColor[] verts = new VertexPositionColor[VertexCount];
        short[] indices   = new short[VertexCount];
        int[]   indices32 = new int[VertexCount];

        VertexBuffer vb;
        IndexBuffer ib;
        IndexBuffer ib32;

        SoundEffect sfx;
        SoundEffect sfx2;

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("hudFont");

            //Load2();
            //return;

            primitives.Add(new CubePrimitive(GraphicsDevice));
            primitives.Add(new SpherePrimitive(GraphicsDevice));
            primitives.Add(new CylinderPrimitive(GraphicsDevice));
            primitives.Add(new TorusPrimitive(GraphicsDevice));
            primitives.Add(new TeapotPrimitive(GraphicsDevice));

            wireFrameState = new RasterizerState()
            {
                FillMode = FillMode.WireFrame,
                CullMode = CullMode.None,
            };


            sfx = Content.Load<SoundEffect>("sfx");
            using (System.IO.Stream sfxstream = TitleContainer.OpenStream(Content.RootDirectory+"\\sfx2.wav"))
            {
                sfx2 = SoundEffect.FromStream(sfxstream);
            }

            // Draw tests
            basicEffect = new BasicEffect(this.GraphicsDevice);
            basicEffect.VertexColorEnabled = true;
            basicEffect.LightingEnabled = false;


            for (int i = 0; i < VertexCount; i++)
            {
                var radianAngle = i * MathF.PI * 2 / VertexCount;

                var x = MathF.Sin(radianAngle);
                var y = MathF.Cos(radianAngle);
                verts[i] = new VertexPositionColor(new Vector3(x, y, 0) * 0.90f, Color.White);
                indices[i] = (short)i;
                indices32[i] = i;
            }

            vb = new VertexBuffer(this.GraphicsDevice, VertexPositionColor.VertexDeclaration,
                VertexCount, BufferUsage.WriteOnly);

            int vertstride = VertexPositionColor.VertexDeclaration.VertexStride;
            //vb.SetData(verts, 0, VertexCount);
            for (int i = 0; i < VertexCount; i++)
            {
                vb.SetData(i * vertstride,
                    verts, i, 1,
                    vertstride);
            }

            for (int i = 0; i < VertexCount; i++)
            {
                Color[] colors = new Color[] { new Color(i/128f, 0, 0, 1f) };
                vb.SetData( (i * vertstride) + sizeof(float)*3,
                    colors, 0, 1,
                    vertstride);
            }


            ib = new IndexBuffer(this.GraphicsDevice, IndexElementSize.SixteenBits, 
                VertexCount, BufferUsage.WriteOnly);

            int idxstride = sizeof(short);
            //ib.SetData(indices, 0, VertexCount);
            for (int i = 0; i < VertexCount; i++)
            {
                ib.SetData(i * idxstride,
                    indices, i, 1);
            }


            ib32 = new IndexBuffer(this.GraphicsDevice, IndexElementSize.ThirtyTwoBits,
                VertexCount, BufferUsage.WriteOnly);

            int idx32stride = sizeof(int);
            //ib32.SetData(indices32, 0, VertexCount);
            for (int i = 0; i < VertexCount; i++)
            {
                ib32.SetData(i * idx32stride,
                    indices, i, 1);
            }

            return;

        }

        #endregion

        #region Update and Draw


        bool is1Down = false;
        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            HandleInput();

            KeyboardState keyboardState = Keyboard.GetState();
            is1Down = keyboardState.IsKeyDown(Keys.D1);

            base.Update(gameTime);
        }

        RenderTarget2D rt;
        Texture2D tx1;
        Texture2D tx2;

        protected void Load2()
        {
            rt = new RenderTarget2D(this.GraphicsDevice, 256,256, false, SurfaceFormat.Color,
                //DepthFormat.Depth24
                DepthFormat.Depth24Stencil8
                );

            tx1 = new Texture2D(this.GraphicsDevice, 32, 32, false, SurfaceFormat.Color);
            tx2 = new Texture2D(this.GraphicsDevice, 32, 32, false, SurfaceFormat.Color);

            Color[] colordata1 = new Color[32 * 32];
            for (int i = 0; i < colordata1.Length; i++)
                colordata1[i] = Color.Red;
            Color[] colordata2 = new Color[32 * 32];
            for (int i = 0; i < colordata2.Length; i++)
                colordata2[i] = Color.Green;

            tx1.SetData(colordata1);
            tx2.SetData(colordata2);
        }

        protected void Draw2(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(rt);
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.Clear(
                ClearOptions.DepthBuffer | ClearOptions.Stencil | ClearOptions.Target,
                Color.CornflowerBlue, 1, 0);

            DepthStencilState depthStencilState = new DepthStencilState
            {
                ReferenceStencil = 1,
                StencilEnable = true,
                StencilFunction = CompareFunction.Always,
                StencilPass = StencilOperation.Replace,
                DepthBufferEnable = false
            };

            spriteBatch.Begin(SpriteSortMode.Deferred, depthStencilState: depthStencilState);
            spriteBatch.Draw(tx1, Vector2.One * (0 + 0), null, Color.White, 0, Vector2.Zero, 1.0f, SpriteEffects.None,
                layerDepth: 0.6f
                );
            spriteBatch.End();

            depthStencilState.Dispose();
            depthStencilState = new DepthStencilState
            {
                ReferenceStencil = 0,
                StencilEnable = true,
                StencilFunction = CompareFunction.Equal,
                StencilPass = StencilOperation.Keep,
                DepthBufferEnable = false
            };
            GraphicsDevice.DepthStencilState = depthStencilState;

            spriteBatch.Begin(SpriteSortMode.Deferred, depthStencilState: depthStencilState);
            spriteBatch.Draw(tx2, Vector2.One * (0 + 16), null, Color.White, 0, Vector2.Zero, 1.0f, SpriteEffects.None,
                layerDepth: 0.5f
                );
            spriteBatch.End();




            spriteBatch.Begin(SpriteSortMode.Deferred, depthStencilState: DepthStencilState.Default);
            spriteBatch.Draw(tx1, Vector2.One*(64+0), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None,
                layerDepth: 0.6f
                );
            spriteBatch.Draw(tx2, Vector2.One*(64+16), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None,
                layerDepth: 0.5f
                );
            spriteBatch.End();




            // -----

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            spriteBatch.Draw(rt, Vector2.Zero, Color.White);
            spriteBatch.End();

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            //Draw2(gameTime);
            //base.Draw(gameTime);
            //return;

            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (is1Down)
                GraphicsDevice.Clear(Color.Blue);
            else
                GraphicsDevice.Clear(Color.CornflowerBlue);


            if (isWireframe)
            {
                GraphicsDevice.RasterizerState = wireFrameState;
            }
            else
            {
                GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            }

            // Create camera matrices, making the object spin.
            float time = (float)gameTime.TotalGameTime.TotalSeconds;

            float yaw = time * 0.4f;
            float pitch = time * 0.7f;
            float roll = time * 1.1f;

            Vector3 cameraPosition = new Vector3(0, 0, 2.5f);

            float aspect = GraphicsDevice.Viewport.AspectRatio;

            Matrix world = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
            Matrix view = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(1, aspect, 1, 10);

            // Draw the current primitive.
            GeometricPrimitive currentPrimitive = primitives[currentPrimitiveIndex];
            Color color = colors[currentColorIndex];

            currentPrimitive.Draw(world, view, projection, color);

            // Reset the fill mode renderstate.
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            // Draw overlay text.
            string text = "A or tap top of screen = Change primitive\n" +
                          "B or tap bottom left of screen = Change color\n" +
                          "Y or tap bottom right of screen = Toggle wireframe";

            spriteBatch.Begin();
            //spriteBatch.DrawString(spriteFont, text, new Vector2(48, 48), Color.White);

            var gamePadState = GamePad.GetState(playerIndex);
            var cap = GamePad.GetCapabilities(playerIndex);
            var isbdown = gamePadState.IsButtonDown(Buttons.A);

            spriteBatch.DrawString(spriteFont, "gamePadState.IsConnected " + gamePadState.IsConnected.ToString(), new Vector2(10, 70), Color.White);
            spriteBatch.DrawString(spriteFont,"cap.IsConnected " + cap.IsConnected, new Vector2(10, 90), Color.White);
            spriteBatch.DrawString(spriteFont, "cap.DisplayName " + cap.DisplayName, new Vector2(10, 120), Color.White);
            spriteBatch.DrawString(spriteFont, "isbdown " + isbdown.ToString(), new Vector2(10, 150), Color.White);
            spriteBatch.DrawString(spriteFont, "gamePadState.Buttons " + gamePadState.Buttons.ToString(), new Vector2(10, 180), Color.White);
            spriteBatch.DrawString(spriteFont, "PacketNumber " + gamePadState.PacketNumber, new Vector2(10, 210), Color.White);
            spriteBatch.DrawString(spriteFont, "gamePadState.DPad " + gamePadState.DPad.ToString(), new Vector2(10, 240), Color.White);
            spriteBatch.DrawString(spriteFont, "gamePadState.Triggers " + gamePadState.Triggers.ToString(), new Vector2(10, 270), Color.White);

            spriteBatch.DrawString(spriteFont, "gamePadState.ThumbSticks.Left  " + gamePadState.ThumbSticks.Left.ToString(),  new Vector2(10, 230), Color.White);
            spriteBatch.DrawString(spriteFont, "gamePadState.ThumbSticks.Right " + gamePadState.ThumbSticks.Right.ToString(), new Vector2(10, 260), Color.White);


            bool leftTrigger = gamePadState.IsButtonDown(Buttons.LeftTrigger);
            bool rightTrigger = gamePadState.IsButtonDown(Buttons.RightTrigger);

            spriteBatch.DrawString(spriteFont, "gamePadState.Triggers " + leftTrigger +" "+rightTrigger, new Vector2(10, 290), Color.White);


            spriteBatch.End();


            // If this is 0, it works perfectly.
            // If it's changed to 1, then the white
            // arc doesn't draw and I only see a blue
            // background
            int vertexOffset = 1;

            vertexOffset = 1;

            vertexOffset = (int)(((gameTime.TotalGameTime.TotalSeconds * 64)) % 64);

            EffectTechnique currentTechnique = basicEffect.CurrentTechnique;
            foreach (EffectPass pass in currentTechnique.Passes)
            {
                pass.Apply();

                //{
                //    GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(
                //        PrimitiveType.LineStrip,
                //        verts,
                //        vertexOffset,
                //        64);
                //}

                //{
                //    GraphicsDevice.SetVertexBuffer(vb);
                //    GraphicsDevice.DrawPrimitives(
                //        PrimitiveType.LineStrip,
                //        vertexOffset,
                //        64);
                //}

                {
                    GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                        PrimitiveType.LineStrip, //PrimitiveType.LineList,
                        verts,
                        vertexOffset,
                        64 + 1,
                        indices,
                        0,
                        64
                        );
                }

                //{
                //    GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                //        PrimitiveType.LineStrip, //PrimitiveType.LineList,
                //        verts,
                //        vertexOffset,
                //        64 + 1,
                //        indices32,
                //        0,
                //        64
                //        );
                //}

                //{
                //    GraphicsDevice.SetVertexBuffer(vb);
                //    //GraphicsDevice.Indices = ib;
                //    GraphicsDevice.Indices = ib32;
                //    GraphicsDevice.DrawIndexedPrimitives(
                //        PrimitiveType.LineStrip,
                //        vertexOffset,
                //        0,
                //        64);
                //}
            }

            base.Draw(gameTime);
        }

        PlayerIndex playerIndex = PlayerIndex.One;

        #endregion

        #region Handle Input


        /// <summary>
        /// Handles input for quitting or changing settings.
        /// </summary>
        void HandleInput()
        {
            lastKeyboardState = currentKeyboardState;
            lastGamePadState = currentGamePadState;
            lastMouseState = currentMouseState;

#if WINDOWS_PHONE
            currentKeyboardState = new KeyboardState();
#else
            currentKeyboardState = Keyboard.GetState();
#endif
            currentGamePadState = GamePad.GetState(playerIndex);
            currentMouseState = Mouse.GetState();
            currentTouchState = TouchPanel.GetState();

            // Check for exit.
            if (IsPressed(Keys.Escape, Buttons.Back))
            {
                try { this.Exit(); }
                catch (PlatformNotSupportedException) { }
            }

            // Change primitive?
            Viewport viewport = GraphicsDevice.Viewport;
            int halfWidth = viewport.Width / 2;
            int halfHeight = viewport.Height / 2;
            Rectangle topOfScreen = new Rectangle(0, 0, viewport.Width, halfHeight);
            if (IsPressed(Keys.A, Buttons.A) || LeftMouseIsPressed(topOfScreen) || TouchIsPressed(topOfScreen))
            {
                currentPrimitiveIndex = (currentPrimitiveIndex + 1) % primitives.Count;
                GamePad.SetVibration(PlayerIndex.One, 1, 0, 0, 1);
            }


            // Change color?
            Rectangle botLeftOfScreen = new Rectangle(0, halfHeight, halfWidth, halfHeight);
            if (IsPressed(Keys.B, Buttons.B) || LeftMouseIsPressed(botLeftOfScreen) || TouchIsPressed(botLeftOfScreen))
            {
                currentColorIndex = (currentColorIndex + 1) % colors.Count;
            }


            // Toggle wireframe?
            Rectangle botRightOfScreen = new Rectangle(halfWidth, halfHeight, halfWidth, halfHeight);
            if (IsPressed(Keys.Y, Buttons.Y) || LeftMouseIsPressed(botRightOfScreen) || TouchIsPressed(botRightOfScreen))
            {
                isWireframe = !isWireframe;
            }

            if (IsPressed(Keys.S, Buttons.DPadDown))
            {
                sfx2.Play();
            }
        }


        /// <summary>
        /// Checks whether the specified key or button has been pressed.
        /// </summary>
        bool IsPressed(Keys key, Buttons button)
        {
            return (currentKeyboardState.IsKeyDown(key) &&
                    lastKeyboardState.IsKeyUp(key)) ||
                   (currentGamePadState.IsButtonDown(button) &&
                    lastGamePadState.IsButtonUp(button));
        }

        bool LeftMouseIsPressed(Rectangle rect)
        {
            return (currentMouseState.LeftButton == ButtonState.Pressed &&
                    lastMouseState.LeftButton != ButtonState.Pressed &&
                    rect.Contains(currentMouseState.X, currentMouseState.Y));
        }

        bool TouchIsPressed(Rectangle rect)
        {
            foreach (TouchLocation touch in currentTouchState)
            {
                if (touch.State == TouchLocationState.Pressed && rect.Contains(touch.Position))
                    return true;
            }

            return false;
        }

        #endregion
    }


}
