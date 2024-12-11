using System;
using Microsoft.Xna.Framework;

namespace Primitives3D
{
    internal class Xplosion
    {
        private GeometricPrimitive _spherePrimitive;

        float _startingRadius;
        float _endRadius;

        public BoundingSphere _boundingSphere;
        public Color _color;

        public readonly TimeSpan TotalTime;
        private TimeSpan _elapsedTime;
        public float _factor;
        private float _invFactor;

        public Xplosion(GeometricPrimitive spherePrimitive,
            Vector3 pos,
            float startingRadius
            )
        {
            this._spherePrimitive = spherePrimitive;
            this._startingRadius = startingRadius;

            this._endRadius = 0.270f;
            //this._endRadius = 0.25f;
            //this._endRadius = 0.2f;

            _boundingSphere.Radius = _startingRadius;
            _boundingSphere.Center = pos;
            _color = Color.White;

            _elapsedTime = TimeSpan.Zero;
            TotalTime = TimeSpan.FromSeconds(2f);
        }

        internal void Update(TimeSpan elapsedGameTime)
        {
            _elapsedTime = _elapsedTime + elapsedGameTime;

            _factor = (float)(_elapsedTime.TotalSeconds / TotalTime.TotalSeconds);
            _factor = MathHelper.Clamp(_factor, 0f, 1f);
            _invFactor = 1f - _factor;


            float radiusFactor = (float)( 1f - Math.Pow(1f - _factor, 3) );
            float radius = MathHelper.Lerp(_startingRadius, _endRadius, radiusFactor);

            _boundingSphere.Radius = radius;
        }

        internal void Draw(Matrix view, Matrix projection)
        {
            float colorFactor = MathHelper.Lerp(0.8f, 0.3f, _factor);

            float radius = _boundingSphere.Radius;
            Color color = _color;
            color = color * colorFactor;

            Matrix world;

            world = Matrix.Identity;
            world *= Matrix.CreateScale(radius * 2.0f);
            world *= Matrix.CreateTranslation(_boundingSphere.Center);
            // Draw the current primitive.
            _spherePrimitive.Draw(world, view, projection, color);


        }

    }
}