﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Geometry;
using Penumbra.Utilities;

namespace Penumbra.Graphics.Providers
{
    internal class CameraProvider : GraphicsProvider
    {
        public Matrix ViewProjection = Matrix.Identity;
        public BoundingRectangle Bounds = new BoundingRectangle(new Vector2(float.MinValue), new Vector2(float.MaxValue));

        private Projections _projections = Projections.OriginCenter_XRight_YUp | Projections.Custom;
        private Matrix _inverseViewProjection = Matrix.Identity;        
        private Matrix _ndcToScreen = Matrix.Identity;
        private Matrix _custom = Matrix.Identity;
        private bool _loaded;

        public bool InvertedY { get; private set; }

        public Projections Projections
        {
            get { return _projections; }
            set
            {
                _projections = value;
                if (_loaded)
                    CalculateViewProjectionAndBounds();
            }
        }

        public Matrix Custom
        {
            get { return _custom; }
            set
            {
                _custom = value;
                if ((_projections & Projections.Custom) != 0 && _loaded)
                    CalculateViewProjectionAndBounds();
            }
        }

        public override void Load(PenumbraEngine engine)
        {
            base.Load(engine);

            CalculateNdcToScreen();
            CalculateViewProjectionAndBounds();

            _loaded = true;
        }

        /// <summary>
        /// Calculates a screen space rectangle based on world space bounds.
        /// </summary>        
        public BoundingRectangle GetScissorRectangle(Light light)
        {
            Matrix transform;
            Matrix.Multiply(ref ViewProjection, ref _ndcToScreen, out transform);

            var c1 = new Vector2(light.Bounds.Min.X, light.Bounds.Max.Y);
            var c2 = light.Bounds.Max;
            var c3 = new Vector2(light.Bounds.Max.X, light.Bounds.Min.Y);
            var c4 = light.Bounds.Min;
            
            Vector2.Transform(ref c1, ref transform, out c1);
            Vector2.Transform(ref c2, ref transform, out c2);
            Vector2.Transform(ref c3, ref transform, out c3);
            Vector2.Transform(ref c4, ref transform, out c4);

            Vector2 min, max;

            Vector2.Min(ref c1, ref c2, out min);
            Vector2.Min(ref min, ref c3, out min);
            Vector2.Min(ref min, ref c4, out min);

            Vector2.Max(ref c1, ref c2, out max);
            Vector2.Max(ref max, ref c3, out max);
            Vector2.Max(ref max, ref c4, out max);

            return new BoundingRectangle(min, max);
        }        

        protected override void OnSizeChanged()
        {
            Logger.Write($"Screen size changed to {BackBufferWidth}x{BackBufferHeight}.");
            CalculateNdcToScreen();
            CalculateViewProjectionAndBounds();            
        }

        private void CalculateNdcToScreen()
        {
            PresentationParameters pp = Engine.Device.PresentationParameters;
            _ndcToScreen = Matrix.Invert(Matrix.CreateOrthographicOffCenter(0, pp.BackBufferWidth, pp.BackBufferHeight, 0, 0, 1));            
        }

        private void CalculateViewProjectionAndBounds()
        {
            // Calculate view projection transform.
            PresentationParameters pp = Engine.Device.PresentationParameters;

            ViewProjection = Matrix.Identity;
            if ((_projections & Projections.Custom) != 0)
                ViewProjection *= Custom;
            if ((_projections & Projections.SpriteBatch) != 0)
                ViewProjection *= Matrix.CreateOrthographicOffCenter(
                    0,
                    pp.BackBufferWidth,
                    pp.BackBufferHeight,
                    0,
                    0.0f, 1.0f);
            if ((_projections & Projections.OriginCenter_XRight_YUp) != 0)
                ViewProjection *= Matrix.CreateOrthographicOffCenter(
                    -pp.BackBufferWidth / 2f,
                    pp.BackBufferWidth / 2f,
                    -pp.BackBufferHeight / 2f,
                    pp.BackBufferHeight / 2f,
                    0.0f, 1.0f);
            if ((_projections & Projections.OriginBottomLeft_XRight_YUp) != 0)
                ViewProjection *= Matrix.CreateOrthographicOffCenter(
                    0,
                    pp.BackBufferWidth,
                    0,
                    pp.BackBufferHeight,
                    0.0f, 1.0f);
            //LogViewProjection();

            // Calculate inversion of viewprojection.
            Matrix.Invert(ref ViewProjection, out _inverseViewProjection);

            // Determine if we are dealing with an inverted (upside down) Y axis.
            InvertedY = ViewProjection.M22 < 0 && ViewProjection.M11 >= 0 ||
                        ViewProjection.M22 >= 0 && ViewProjection.M11 < 0;

            // Calculate fustum bounds. We need all four corners to take rotation into account.
            Vector2 c1 = Vector2.Transform(new Vector2(+1.0f, +1.0f), _inverseViewProjection);
            Vector2 c2 = Vector2.Transform(new Vector2(+1.0f, -1.0f), _inverseViewProjection);
            Vector2 c3 = Vector2.Transform(new Vector2(-1.0f, -1.0f), _inverseViewProjection);
            Vector2 c4 = Vector2.Transform(new Vector2(-1.0f, +1.0f), _inverseViewProjection);

            Vector2 min, max;

            Vector2.Min(ref c1, ref c2, out min);
            Vector2.Min(ref min, ref c3, out min);
            Vector2.Min(ref min, ref c4, out min);

            Vector2.Max(ref c1, ref c2, out max);
            Vector2.Max(ref max, ref c3, out max);
            Vector2.Max(ref max, ref c4, out max);

            Bounds = new BoundingRectangle(min, max);
        }

        private void LogViewProjection()
        {
            var m = ViewProjection;
            Logger.Write("-----------------");
            Logger.Write($"{m.M11:+0.0000;-0.0000} {m.M12:+0.0000;-0.0000} {m.M13:+0.0000;-0.0000} {m.M14:+0.0000;-0.0000}");
            Logger.Write($"{m.M21:+0.0000;-0.0000} {m.M22:+0.0000;-0.0000} {m.M23:+0.0000;-0.0000} {m.M24:+0.0000;-0.0000}");
            Logger.Write($"{m.M31:+0.0000;-0.0000} {m.M32:+0.0000;-0.0000} {m.M33:+0.0000;-0.0000} {m.M34:+0.0000;-0.0000}");
            Logger.Write($"{m.M41:+0.0000;-0.0000} {m.M42:+0.0000;-0.0000} {m.M43:+0.0000;-0.0000} {m.M44:+0.0000;-0.0000}");
        }
    }
}
