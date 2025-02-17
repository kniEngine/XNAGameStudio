#region File Description
//-----------------------------------------------------------------------------
// StraightLineMovementComponent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


#endregion

namespace NinjAcademy
{
    /// <summary>
    /// A component that moves between two points in a straight line at a specified speed.
    /// </summary>
    class StraightLineMovementComponent : AnimatedComponent
    {
        #region Fields/Properties/Events

        /// <summary>
        /// Velocity in pixels per second.
        /// </summary>
        Vector2 velocity;

        // Distance remaining until the destination is reached.
        float distance;

        bool isEventFired = true;

        /// <summary>
        /// An event fired once the component has reached its destination. This event will only fire once
        /// per call of <see cref="Move"/>.
        /// </summary>
        public event EventHandler FinishedMoving;

        /// <summary>
        /// Component's center, which is also it's position.
        /// </summary>
        public override Vector2 Center
        {
            get
            {
                return Position;
            }
        }

        #endregion

        #region Initialization


        /// <summary>
        /// Creates a new instance of the straight line motion game component.
        /// </summary>
        /// <param name="game">Associated game object.</param>
        /// <param name="gameScreen">Game screen where the component will be presented.</param>
        /// <param name="texture">Texture asset which represents the component.</param>
        public StraightLineMovementComponent(Game game, GameplayScreen gameScreen, Texture2D texture)
            : base(game, gameScreen, texture)
        {
        }

        /// <summary>
        /// Creates a new instance of the straight line motion game component.
        /// </summary>
        /// <param name="game">Associated game object.</param>
        /// <param name="gameScreen">Game screen where the component will be presented.</param>
        /// <param name="animation">Animation object which represents the component.</param>
        public StraightLineMovementComponent(Game game, GameplayScreen gameScreen, Animation animation)
            : base(game, gameScreen, animation)
        {
        }


        #endregion

        #region Update


        /// <summary>
        /// Updates the component's position.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 movement = velocity * elapsedSeconds;
            Position += movement;
            distance -= movement.Length();

            // Check whether the event generated by reaching the destination needs to be fired.
            if (!isEventFired && distance <= 0)
            {
                // Move back in case we moved too much
                velocity.Normalize();
                Position += velocity * distance;

                // Fire the event and stop moving
                if (FinishedMoving != null)
                {
                    FinishedMoving(this, EventArgs.Empty);
                }

                isEventFired = true;
                velocity = Vector2.Zero;
            }
        }


        #endregion

        #region Rendering


        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            animation.Draw(spriteBatch, Position, 0, VisualCenter, 1, SpriteEffects.None, 0);

            spriteBatch.End();
        }


        #endregion

        #region Public Methods


        /// <summary>
        /// Causes the component to move from a specified location, in a straight, to another location.
        /// </summary>
        /// <param name="velocity">Movement velocity in pixels per second. Must be a positive number.</param>
        /// <param name="initialPosition">Component's starting point.</param>
        /// <param name="destination">The component's movement destination.</param>
        public void Move(float velocity, Vector2 initialPosition, Vector2 destination)
        {
            if (velocity <= 0)
            {
                throw new ArgumentOutOfRangeException("velocity", "Argument must be greater than 0.");
            }

            Vector2 toDestinationVector = (destination - initialPosition);
            Vector2 velocityUnitVector = toDestinationVector;
            velocityUnitVector.Normalize();

            this.velocity = velocityUnitVector * velocity;
            this.distance = toDestinationVector.Length();
            Position = initialPosition;
            isEventFired = false;
        }

        /// <summary>
        /// Causes the component to move from a specified location, in a straight, to another location.
        /// </summary>
        /// <param name="time">The time it should take the component to reach its destination. Must be a positive
        /// time span.</param>
        /// <param name="initialPosition">Component's starting point.</param>
        /// <param name="destination">The component's movement destination.</param>
        public void Move(TimeSpan time, Vector2 initialPosition, Vector2 destination)
        {
            if (time <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("time", "Argument must be a positive time span.");
            }

            Vector2 toDestinationVector = (destination - initialPosition);
            float distance = toDestinationVector.Length();

            float requiredVelocity = distance / (float)time.TotalSeconds;

            Move(requiredVelocity, initialPosition, destination);
        }


        #endregion
    }
}
