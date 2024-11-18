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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.XR;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.XR;

#endregion

namespace Primitives3D
{
    /// <summary>
    /// This sample shows how to draw 3D geometric primitives
    /// such as cubes, spheres, and cylinders.
    /// </summary>
    public class Primitives3DVRGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;

        XRDevice xrDevice;
        BasicEffect spriteBatchEffect;

        SpriteBatch spriteBatch;
        SpriteFont spriteFont;

        KeyboardState currentKeyboardState;
        KeyboardState lastKeyboardState;
        GamePadState currentGamePadState;
        GamePadState lastGamePadState;
        GamePadState currentTouchControllerState;
        GamePadState lastTouchControllerState;

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


        public Primitives3DVRGame()
        {
            Content.RootDirectory = "Content";
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferFormat = SurfaceFormat.Color;
            graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            graphics.PreferMultiSampling = false;

            // 90Hz Frame rate for oculus
            TargetElapsedTime = TimeSpan.FromTicks(111111);
            IsFixedTimeStep = true;

            // we don't care is the main window is Focuses or not
            // because we render on the Oculus surface.
            InactiveSleepTime = TimeSpan.FromSeconds(0);

            graphics.GraphicsProfile = GraphicsProfile.HiDef;

            // create xr device
            xrDevice = new XRDevice("Primitives3DXR", this);
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("hudFont");
            spriteBatchEffect = new BasicEffect(GraphicsDevice);
            spriteBatchEffect.TextureEnabled = true;
            spriteBatchEffect.VertexColorEnabled = true;

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

        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            var ms = Mouse.GetState();
            var ts = TouchPanel.GetState();
            TouchLocation tl = default;
            if (ts.Count > 0)
                tl = ts[0];

            if (ms.LeftButton == ButtonState.Pressed
            ||  tl.State == TouchLocationState.Pressed)
            {
                if (xrDevice.DeviceState == XRDeviceState.Disabled
                ||  xrDevice.DeviceState == XRDeviceState.NoPermissions)
                {
                    try
                    {
                        // Initialize Oculus VR
                        int ovrCreateResult = xrDevice.BeginSessionAsync();

                    }
                    catch (Exception ovre)
                    {
                        System.Diagnostics.Debug.WriteLine(ovre.Message);
                    }
                }
            }

            HandleInput();

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            Vector3 cameraPosition = new Vector3(0f, 0f, 0f);
            float aspect = GraphicsDevice.Viewport.AspectRatio;
            Matrix view = Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(1, aspect, 1, 10);

            if (xrDevice.DeviceState == XRDeviceState.Enabled)
            {
                // draw on VR headset
                int ovrResult = xrDevice.BeginFrame();
                if (ovrResult >= 0)
                {
                    HeadsetState headsetState = xrDevice.GetHeadsetState();

                    // draw each eye on a rendertarget
                    foreach (XREye eye in xrDevice.GetEyes())
                    {
                        RenderTarget2D rt = xrDevice.GetEyeRenderTarget(eye);
                        if (rt == null)
                            continue;

                        GraphicsDevice.SetRenderTarget(rt);

                        // VR eye view and projection
                        view = headsetState.GetEyeView(eye);
                        projection = xrDevice.CreateProjection(eye, 0.05f, 10);

                        Matrix globalWorld = Matrix.CreateWorld(cameraPosition, Vector3.Forward, Vector3.Up);
                        view = Matrix.Invert(globalWorld) * view;

                        DrawScene(gameTime, view, projection);
                        DrawGroundAndControllers(gameTime, view, projection);

                        // Resolve eye rendertarget
                        GraphicsDevice.SetRenderTarget(null);
                        // submit eye rendertarget
                        xrDevice.CommitRenderTarget(eye, rt);
                    }

                    // submit frame
                    int result = xrDevice.EndFrame();

                    return;
                }
            }

            // draw on PC screen
            GraphicsDevice.SetRenderTarget(null);

            DrawScene(gameTime, view, projection);
        }

        private void DrawScene(GameTime gameTime, Matrix view, Matrix projection)
        {
            if (xrDevice.SessionMode == XRSessionMode.AR)
                GraphicsDevice.Clear(Color.Transparent);
            else
                GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.RasterizerState = (isWireframe) 
                                           ? wireFrameState 
                                           : RasterizerState.CullCounterClockwise;

            // Create camera matrices, making the object spin.
            float time = (float)gameTime.TotalGameTime.TotalSeconds;

            float yaw = time * 0.4f;
            float pitch = time * 0.7f;
            float roll = time * 1.1f;

            Matrix world = Matrix.Identity;
            world *= Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
            world *= Matrix.CreateTranslation(Vector3.Forward*2.5f);
            //world *= Matrix.CreateTranslation(Vector3.Up * 0.8f);

            // Draw the current primitive.
            GeometricPrimitive currentPrimitive = primitives[currentPrimitiveIndex];
            Color color = colors[currentColorIndex];

            currentPrimitive.Draw(world, view, projection, color);

            // Reset the fill mode renderstate.
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;


            // Draw billboard text.
            string text = "A = Change primitive\n" +
                          "B = Change color\n" +
                          "Y = Toggle wireframe";

            Matrix cameraMtx = Matrix.Invert(view);
            Vector3 objectPosition = world.Translation;
            spriteBatchEffect.World = Matrix.CreateConstrainedBillboard(
                    objectPosition, cameraMtx.Translation, Vector3.UnitY, cameraMtx.Forward, Vector3.Forward);
            spriteBatchEffect.View = view;
            spriteBatchEffect.Projection = projection;
            spriteBatch.Begin(SpriteSortMode.Deferred,
                effect: spriteBatchEffect);
            spriteBatch.DrawString(spriteFont, text, new Vector2(-0.40f, 1.0f),
            Color.White, 0, Vector2.Zero, 0.005f,
            SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally, 0);
            spriteBatch.End();

            // draw any drawable components
            base.Draw(gameTime);
        }

        private void DrawGroundAndControllers(GameTime gameTime, Matrix view, Matrix projection)
        {
            // draw ground
            GeometricPrimitive currentPrimitive = primitives[0];

            Matrix world = Matrix.Identity;
            world *= Matrix.CreateScale(2f, 0f,4f);

            Color color = Color.DarkGray;

            //currentPrimitive.Draw(world, view, projection, color);

            // draw controllers
            HandsState handsState = xrDevice.GetHandsState();

            GamePadState ltc = TouchController.GetState(TouchControllerType.LTouch);
            Matrix lp = handsState.GetHandTransform(0);
            Matrix lg = handsState.GetGripTransform(0);

            if (ltc.IsConnected)
            {
                color = Color.Gainsboro;
                DrawController(view, projection, color, lp, lg);
            }

            GamePadState rtc = TouchController.GetState(TouchControllerType.RTouch);
            Matrix rp = handsState.GetHandTransform(1);
            Matrix rg = handsState.GetGripTransform(1);

            if (rtc.IsConnected)
            {
                color = Color.Yellow;
                DrawController(view, projection, color, rp, rg);
            }

            GamePadCapabilities lcap = TouchController.GetCapabilities(TouchControllerType.LTouch);
            GamePadCapabilities rcap = TouchController.GetCapabilities(TouchControllerType.RTouch);
            GamePadCapabilities  cap = TouchController.GetCapabilities(TouchControllerType.Touch);

            return;
        }

        private void DrawController(Matrix view, Matrix projection, Color color, Matrix pworld, Matrix gworld)
        {
            GeometricPrimitive currentPrimitive = primitives[2];

            Matrix world;

            world = Matrix.Identity;
            world *= Matrix.CreateRotationX(MathHelper.Tau / 4);
            world *= Matrix.CreateScale(new Vector3(1f, 1f, 8f) * 0.01f);
            world *= pworld;
            currentPrimitive.Draw(world, view, projection, color);

            world = Matrix.Identity;
            world *= Matrix.CreateRotationX(MathHelper.Tau / 4);
            world *= Matrix.CreateScale(new Vector3(1f, 1f, 2f) * 0.03f);
            world *= gworld;
            currentPrimitive.Draw(world, view, projection, color);
        }

        #endregion

        #region Handle Input

        float lvibe = 0;
        float rvibe = 0;

        /// <summary>
        /// Handles input for quitting or changing settings.
        /// </summary>
        void HandleInput()
        {
            lastKeyboardState = currentKeyboardState;
            lastGamePadState = currentGamePadState;
            lastTouchControllerState = currentTouchControllerState;

            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);
            currentTouchControllerState = TouchController.GetState(TouchControllerType.Touch);

            // Check for exit.
            if (IsPressed(Keys.Escape, Buttons.Back))
            {
                try { Exit(); }
                catch (PlatformNotSupportedException) { /* ignore exit */ }
            }

            lvibe *= 0.85f;
            rvibe *= 0.85f;
            if (lvibe <= 0.1f) lvibe = 0;
            if (rvibe <= 0.1f) rvibe = 0;

            // Change primitive?
            if (IsPressed(Keys.A, Buttons.A))
            {
                currentPrimitiveIndex = (currentPrimitiveIndex + 1) % primitives.Count;
                rvibe = 1f;
            }

            // Change color?
            if (IsPressed(Keys.B, Buttons.B))
            {
                currentColorIndex = (currentColorIndex + 1) % colors.Count;
                rvibe = 1f;
            }

            if (IsPressed(Keys.X, Buttons.X))
            {
                lvibe = 1f;
            }

            // Toggle wireframe?
            if (IsPressed(Keys.Y, Buttons.Y))
            {
                isWireframe = !isWireframe;
                lvibe = 1f;
            }

            TouchController.SetVibration(TouchControllerType.LTouch, lvibe);
            TouchController.SetVibration(TouchControllerType.RTouch, rvibe);
        }

        /// <summary>
        /// Checks whether the specified key or button has been pressed.
        /// </summary>
        bool IsPressed(Keys key, Buttons button)
        {
            return (currentKeyboardState.IsKeyDown(key) &&
                    lastKeyboardState.IsKeyUp(key)) ||
                   (currentGamePadState.IsButtonDown(button) &&
                    lastGamePadState.IsButtonUp(button)) ||
                   (currentTouchControllerState.IsButtonDown(button) &&
                    !lastTouchControllerState.IsButtonDown(button));
        }

        #endregion
    }


}
