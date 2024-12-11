using System;
using Microsoft.Xna.Framework;

namespace Primitives3D
{
    internal class Ball
    {
        private GeometricPrimitive _spherePrimitive;
        private GeometricPrimitive _cylinderPrimitive;

        public BoundingSphere _boundingSphere;
        public Color _color;
        public Vector3 dir;
        public float vel;

        public float _inityaw;
        public float _initpitch;
        public float _initroll;

        public Ball(GeometricPrimitive spherePrimitive, GeometricPrimitive cylinderPrimitive, Vector3 pos)
        {
            this._spherePrimitive = spherePrimitive;
            this._cylinderPrimitive = cylinderPrimitive;

            _boundingSphere.Radius = 0.025f;
            _boundingSphere.Center = pos;
            _color = Color.Red;
        }

        internal void Draw(GameTime gameTime, Matrix view, Matrix projection)
        {

            // Create camera matrices, making the object spin.
            float time = (float)gameTime.TotalGameTime.TotalSeconds;
            //time = 0;

            float yaw = _inityaw + time * 0.4f;
            float pitch = _initpitch + time * 0.7f;
            float roll = _initroll + time * 1.1f;

            Matrix rot = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);

            Matrix world;

            world = Matrix.Identity;
            world *= Matrix.CreateScale(_boundingSphere.Radius * 2.0f);
            world *= rot;
            world *= Matrix.CreateTranslation(_boundingSphere.Center);
            // Draw the current primitive.
            _spherePrimitive.Draw(world, view, projection, _color);

            world = Matrix.Identity;
            world *= Matrix.CreateTranslation(0.0f, 0.5f, 0.0f);
            world *= Matrix.CreateScale(0.15f, 0.55f, 0.15f);
            world *= Matrix.CreateScale(_boundingSphere.Radius*2);
            world *= rot;
            world *= Matrix.CreateTranslation(_boundingSphere.Center);
            _cylinderPrimitive.Draw(world, view, projection, Color.Gold);
        }

        internal void Update(TimeSpan elapsedGameTime, ref BoundingBox bounds)
        {
            float dt = (float)elapsedGameTime.TotalSeconds;

            Vector3 disp = dir * vel * dt;
            this._boundingSphere.Center += disp;

            var pos = this._boundingSphere.Center;

            // bounce
            if (pos.X < bounds.Min.X && dir.X < 0)
                dir.X = -dir.X;
            if (pos.Y < bounds.Min.Y && dir.Y < 0)
                dir.Y = -dir.Y;
            if (pos.Z < bounds.Min.Z && dir.Z < 0)
                dir.Z = -dir.Z;

            if (pos.X > bounds.Max.X && dir.X > 0)
                dir.X = -dir.X;
            if (pos.Y > bounds.Max.Y && dir.Y > 0)
                dir.Y = -dir.Y;
            if (pos.Z > bounds.Max.Z && dir.Z > 0)
                dir.Z = -dir.Z;
        }
    }
}