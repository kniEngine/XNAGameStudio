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
using Microsoft.Xna.Framework.Input.XR;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
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
        SoundEffect sfxXPlowed;
        Song songChristmasTime;

        KeyboardState currentKeyboardState;
        KeyboardState lastKeyboardState;
        GamePadState currentGamePadState;
        GamePadState lastGamePadState;
        MouseState currentMouseState;
        MouseState lastMouseState;
        TouchCollection currentTouchState;
        GamePadState currentTouchControllerState;
        GamePadState lastTouchControllerState;

        private CubePrimitive _cubePrimitive;
        private SpherePrimitive _spherePrimitive;
        private CylinderPrimitive _cylinderPrimitive;
        private TorusPrimitive _torusPrimitive;
        private TeapotPrimitive _teapotPrimitive;

#if DEBUG
        const int _ballCount = 4;
#else
        const int _ballCount = 20;
#endif
        List<Ball> _balls;

        BoundingBox bounds;
        Vector3 boundsExt;

        List<Xplosion> _xplosions;

        const int initTries = 4;
        int _tries;

        string txtMotivation = "";


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

            xrDevice = new XRDevice("Primitives3DXR", this);
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("hudFont");
            sfxXPlowed = Content.Load<SoundEffect>("GemCollected");
            songChristmasTime = Content.Load<Song>("ChristmasTime");

            spriteBatchEffect = new BasicEffect(GraphicsDevice);
            spriteBatchEffect.TextureEnabled = true;
            spriteBatchEffect.VertexColorEnabled = true;

            _cubePrimitive = new CubePrimitive(GraphicsDevice);
            _spherePrimitive = new SpherePrimitive(GraphicsDevice);
            _cylinderPrimitive = new CylinderPrimitive(GraphicsDevice);
            _torusPrimitive = new TorusPrimitive(GraphicsDevice);
            _teapotPrimitive = new TeapotPrimitive(GraphicsDevice);

        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            HandleInput();

            TouchLocation tl = default;
            if (currentTouchState.Count > 0)
                tl = currentTouchState[0];

            // Create XR Device
            if (xrDevice.DeviceState == XRDeviceState.Disabled
            ||  xrDevice.DeviceState == XRDeviceState.NoPermissions)
            {
                Viewport vp = GraphicsDevice.Viewport;
                float vw = vp.Width;
                float vh = vp.Height;
                float hvw = vw / 2f;

                // select mode
                XRSessionMode mode = default(XRSessionMode);
                if (currentMouseState.LeftButton == ButtonState.Pressed)
                {
                    if (currentMouseState.X < hvw)
                        mode = XRSessionMode.VR;
                    else
                        mode = XRSessionMode.AR;
                }
                if (tl.State == TouchLocationState.Pressed
                ||  tl.State == TouchLocationState.Moved)
                {
                    if (tl.Position.X < hvw)
                        mode = XRSessionMode.VR;
                    else
                        mode = XRSessionMode.AR;
                }

                if (mode != default(XRSessionMode))
                {
                    try
                    {
                        xrDevice.BeginSessionAsync(mode);
                    }
                    catch (Exception ovre)
                    {
                        System.Diagnostics.Debug.WriteLine(ovre.Message);
                    }
                }
            }

            // create balls
            if (_balls == null)
                InitStage();

            TimeSpan scaledElapsedGameTime = gameTime.ElapsedGameTime;
            scaledElapsedGameTime = scaledElapsedGameTime * timescale;

            // move balls
            for (int b = _balls.Count - 1; b >= 0; b--)
            {
                _balls[b].Update(scaledElapsedGameTime, ref this.bounds);
            }

            // update _xplosions factor
            for (int x = _xplosions.Count - 1; x >= 0; x--)
            {
                _xplosions[x].Update(scaledElapsedGameTime);
                if (_xplosions[x]._factor >= 1f)
                    _xplosions.RemoveAt(x);
            }

            // chain _xplosions
            int initialXplosionsCount = _xplosions.Count;
            for (int x = _xplosions.Count - 1; x >= 0; x--)
            {
                for (int b = _balls.Count - 1; b >= 0; b--)
                {
                    bool intersects = _xplosions[x]._boundingSphere.Intersects(_balls[b]._boundingSphere);
                    if (intersects)
                    {
                        XplodeBall(b);
                    }
                }
            }

            if (_tries > 0)
            {
                if (_tries >= _balls.Count)
                    txtMotivation = "You got this!          ";
                else if (_tries == _balls.Count - 1)
                    txtMotivation = "Make it happen!        ";
                else if (_tries == 1 && _balls.Count == 3)
                    txtMotivation = "Focus!                 ";
            }

            if (_xplosions.Count == 0) // wait for explosions
            {
                if (_balls.Count == 0)
                {
                    txtMotivation = "You Won!               ";
                        InitStage();
                }
                else
                {
                    if (_tries == 0)
                    {
                        txtMotivation = "You Lost!              ";
                        InitStage();
                    }
                }
            }

            base.Update(gameTime);
        }

        private void InitStage()
        {
            _tries = initTries;
            txtMotivation = DaysUntilChristmas() + " days until Christmas!";

            bounds = new BoundingBox(
                new Vector3(-0.5f, -0.5f, -1.5f),
                new Vector3(+0.5f, +0.5f, -0.5f)
                );
            boundsExt = bounds.Max - bounds.Min;

            Random rnd = new Random();

            _xplosions = new List<Xplosion>();
            _balls = new List<Ball>(_ballCount);
            for (int b = 0; b < _ballCount; b++)
            {
                Vector3 pos = bounds.Min
                    + new Vector3(
                           boundsExt.X * rnd.NextSingle(),
                           boundsExt.Y * rnd.NextSingle(),
                           boundsExt.Z * rnd.NextSingle()
                    );

                Vector3 dir = new Vector3(
                           (rnd.NextSingle() - 0.5f),
                           (rnd.NextSingle() - 0.5f),
                           (rnd.NextSingle() - 0.5f)
                           );
                if (dir == Vector3.Zero) // no luck!
                    dir = Vector3.One;
                dir.Normalize();
                float vel = MathHelper.Lerp(0.125f, 0.275f, rnd.NextSingle());

                Ball ball = new Ball(
                    _spherePrimitive, _cylinderPrimitive,
                    pos
                    );
                ball.dir = dir;
                ball.vel = vel;
                
                ball._inityaw= rnd.NextSingle();
                ball._initpitch= rnd.NextSingle();
                ball._initroll= rnd.NextSingle();

                _balls.Add(ball);
            }
        }

        private void XplodeBall(int ballIndex)
        {
            sfxXPlowed.Play();

            Ball ball = _balls[ballIndex];
            _balls.RemoveAt(ballIndex);

            Vector3 pos = ball._boundingSphere.Center;
            float radius = ball._boundingSphere.Radius;
            Xplosion xplosion = new Xplosion(
                _spherePrimitive,
                pos, radius
            );

            _xplosions.Add(xplosion);
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
                if (MediaPlayer.State != MediaState.Playing)
                {
                    MediaPlayer.Volume = 0.3f; // background loop
                    MediaPlayer.IsRepeating = true;
                    MediaPlayer.Play(songChristmasTime);
                }

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
                        DrawGroundAndControllers(view, projection);

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
            else
            {
                // draw on backbuffer
                GraphicsDevice.SetRenderTarget(null);
                DrawXREntry();
                //DrawScene(gameTime, view, projection);
            }
        }

        private void DrawXREntry()
        {
            Viewport vp = GraphicsDevice.Viewport;
            float vw = vp.Width;
            float vh = vp.Height;
            float hvw = vw / 2f;

            var vrtxt = "Hold to enter VR";
            var artxt = "Hold to enter AR";

            Vector2 vrsize = spriteFont.MeasureString(vrtxt);
            Vector2 arsize = spriteFont.MeasureString(artxt);

            spriteBatch.Begin();
            spriteBatch.DrawString(spriteFont, vrtxt,
                new Vector2(hvw / 2f - vrsize.X / 2f, vh / 2f - vrsize.Y / 2f),
                Color.White);
            spriteBatch.DrawString(spriteFont, artxt,
                new Vector2(hvw + hvw / 2f -arsize.X / 2f, vh / 2f - arsize.Y / 2f),
                Color.White);
            spriteBatch.End();

        }

        private void DrawScene(GameTime gameTime, Matrix view, Matrix projection)
        {
            if (xrDevice.SessionMode == XRSessionMode.AR)
                GraphicsDevice.Clear(Color.Transparent);
            else
                GraphicsDevice.Clear(Color.Black);


            DrawText(gameTime, view, projection);


            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;
            for (int b = 0; b < _balls.Count; b++)
            {
                _balls[b].Draw(gameTime, view, projection);
            }

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            for (int x = 0; x < _xplosions.Count; x++)
            {
                _xplosions[x].Draw(view, projection);
            }


            // draw any drawable components
            base.Draw(gameTime);
        }

        private void DrawText(GameTime gameTime, Matrix view, Matrix projection)
        {
            // Reset the fill mode renderstate.
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            Matrix world = Matrix.Identity;
            world *= Matrix.CreateTranslation(Vector3.Forward * 2.5f);

            // Draw billboard text.
            string text = String.Empty;
            text += "Balls: "+ _balls.Count;
            text += "\n";
            text += "Tries: " + _tries;
            text += "\n";
            text += "\n";
            text += txtMotivation;
            text += "\n";

            Matrix cameraMtx = Matrix.Invert(view);
            Vector3 objectPosition = world.Translation;
            spriteBatchEffect.World = Matrix.CreateConstrainedBillboard(
                    objectPosition, cameraMtx.Translation, Vector3.UnitY, cameraMtx.Forward, Vector3.Forward);
            spriteBatchEffect.View = view;
            spriteBatchEffect.Projection = projection;
            spriteBatch.Begin(SpriteSortMode.Deferred, 
                depthStencilState: DepthStencilState.Default,
                effect: spriteBatchEffect);
            spriteBatch.DrawString(spriteFont, text, new Vector2(-1.00f, -0.40f),
            Color.White, 0, Vector2.Zero, 0.005f,
            SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally, 0);
            spriteBatch.End();
        }

        static int DaysUntilChristmas()
        {
            DateTime today = DateTime.Today;
            DateTime christmas = new DateTime(today.Year, 12, 25);
            if (today > christmas)
                christmas = new DateTime(today.Year + 1, 12, 25);

            return (christmas - today).Days;
        }

        private void DrawGroundAndControllers(Matrix view, Matrix projection)
        {
            // draw ground

            Matrix world = Matrix.Identity;
            world *= Matrix.CreateScale(2f, 0f,4f);

            Color color = Color.DarkGray;

            //_cubePrimitive.Draw(world, view, projection, color);

            // draw controllers
            HandsState handsState = xrDevice.GetHandsState();

            int explodeNum = -1;

            GamePadState ltc = TouchController.GetState(TouchControllerType.LTouch);
            if (ltc.IsConnected)
            {
                Matrix lp = handsState.GetHandTransform(0);
                DrawController(view, projection, lp, lvibe);
                HandleControllerTrigger(ltc, lp, ref explodeNum);
            }

            GamePadState rtc = TouchController.GetState(TouchControllerType.RTouch);
            if (rtc.IsConnected)
            {
                Matrix rp = handsState.GetHandTransform(1);
                DrawController(view, projection, rp, rvibe);
                HandleControllerTrigger(rtc, rp, ref explodeNum);
            }

            if (explodeNum != -1)
            {
                _tries--;
                XplodeBall(explodeNum);
            }

            return;
        }

        private void HandleControllerTrigger(GamePadState tc, Matrix rp, ref int explodeNum)
        {
            Matrix bb = Matrix.Identity;
            bb *= Matrix.CreateTranslation(0f, 0f, -0.2f);
            bb *= rp;
            BoundingSphere bs = new BoundingSphere(
                 bb.Translation,
                 0.01f // 0.025f
                );

            // test
            //_balls[_ballCount - 1]._boundingSphere = bs;
            //_balls[_ballCount - 1].Draw(new GameTime(), view, projection);

            int intersectNum = -1;
            for (int b = 0; b < _balls.Count; b++)
            {
                bool intersects = _balls[b]._boundingSphere.Intersects(bs);
                if (intersects)
                {
                    _balls[b]._color = Color.Purple;
                    intersectNum = b;
                }
                else
                    _balls[b]._color = Color.Red;
            }

            if (_tries > 0 && intersectNum != -1)
            {
                if (tc.IsButtonDown(Buttons.A)
                ||  tc.IsButtonDown(Buttons.B)
                ||  tc.IsButtonDown(Buttons.X)
                ||  tc.IsButtonDown(Buttons.Y)
                ||  tc.IsButtonDown(Buttons.LeftTrigger)
                ||  tc.IsButtonDown(Buttons.RightTrigger)
                )
                {
                    // explode
                    explodeNum = intersectNum;
                }
            }
        }

        private void DrawController(Matrix view, Matrix projection, Matrix pworld, float vibe)
        {
            Matrix world;

            world = Matrix.Identity;
            world *= Matrix.CreateRotationX(-MathHelper.Tau / 4f);
            world *= Matrix.CreateScale(new Vector3(0.01f, 0.01f, 0.08f));
            world *= pworld;
            _cylinderPrimitive.Draw(world, view, projection, Color.DarkGreen);

            world = Matrix.Identity;
            world *= Matrix.CreateTranslation(0.0f, 0.5f, 0.0f);
            world *= Matrix.CreateRotationX(-MathHelper.Tau / 4f);
            world *= Matrix.CreateScale(new Vector3(0.01f, 0.01f, 0.2f - 0.01f));
            world *= pworld;
            _cylinderPrimitive.Draw(world, view, projection, Color.DarkGreen);

            Color color = Color.Lerp(Color.Purple, Color.CornflowerBlue, vibe);

            world = Matrix.Identity;
            world *= Matrix.CreateRotationX(MathHelper.Tau / 8f);
            world *= Matrix.CreateRotationY(MathHelper.Tau / 8f);
            world *= Matrix.CreateScale(new Vector3(0.01f, 0.01f, 0.01f + (0.005f * vibe)));
            world *= Matrix.CreateTranslation(0.0f, 0.0f, -0.2f);
            world *= pworld;
            _cubePrimitive.Draw(world, view, projection, color);
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
            lastMouseState = currentMouseState;
            lastTouchControllerState = currentTouchControllerState;

            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);
            currentMouseState = Mouse.GetState();
            currentTouchState = TouchPanel.GetState();
            currentTouchControllerState = TouchController.GetState(TouchControllerType.Touch);

            // Check for exit.
            if (IsPressed(Keys.Escape, Buttons.Back))
            {
                try { Exit(); }
                catch (PlatformNotSupportedException) { /* ignore exit */ }
            }

            float maxThumpY = 0;
            maxThumpY = MathHelper.Max(maxThumpY, currentTouchControllerState.ThumbSticks.Left.Y);
            maxThumpY = MathHelper.Max(maxThumpY, currentTouchControllerState.ThumbSticks.Right.Y);
            float minThumpY = 0;
            minThumpY = MathHelper.Min(minThumpY, currentTouchControllerState.ThumbSticks.Left.Y);
            minThumpY = MathHelper.Min(minThumpY, currentTouchControllerState.ThumbSticks.Right.Y);

            if (Math.Abs(maxThumpY) > Math.Abs(minThumpY))
            {
                timescale = 1f + maxThumpY * 3; // minThumpY [0, +1]
            }
            else
            {
                timescale = 1f + minThumpY * 0.75f; // minThumpY [-1, 0]
            }
            

            lvibe *= 0.85f;
            rvibe *= 0.85f;
            if (lvibe <= 0.1f) lvibe = 0;
            if (rvibe <= 0.1f) rvibe = 0;

            // Change primitive?
            if (IsPressed(Keys.A, Buttons.A))
                rvibe = 1f;
            if (IsPressed(Keys.B, Buttons.B))
                rvibe = 1f;
            if (IsPressed(Keys.X, Buttons.X))
                lvibe = 1f;
            if (IsPressed(Keys.Y, Buttons.Y))
                lvibe = 1f;
            if (IsPressed(Keys.X, Buttons.LeftTrigger))
                lvibe = 1f;
            if (IsPressed(Keys.Y, Buttons.RightTrigger))
                rvibe = 1f;
            if (IsPressed(Keys.X, Buttons.LeftGrip))
                lvibe = 1f;
            if (IsPressed(Keys.Y, Buttons.RightGrip))
                rvibe = 1f;


            TouchController.SetVibration(TouchControllerType.LTouch, lvibe);
            TouchController.SetVibration(TouchControllerType.RTouch, rvibe);
        }

        float timescale = 1f;

        /// <summary>
        /// Checks whether the specified key or button has been pressed.
        /// </summary>
        bool IsPressed(Keys key, Buttons button)
        {
            return (currentKeyboardState.IsKeyDown(key) && lastKeyboardState.IsKeyUp(key))
                || (currentGamePadState.IsButtonDown(button) && lastGamePadState.IsButtonUp(button))
                || (currentTouchControllerState.IsButtonDown(button) && !lastTouchControllerState.IsButtonDown(button)
                );
        }

        #endregion
    }


}
