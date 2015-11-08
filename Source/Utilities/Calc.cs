﻿using System;
using Microsoft.Xna.Framework;

namespace Penumbra.Utilities
{
    internal static class Calc
    {
        public static float Cross(ref Vector2 a, ref Vector2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        public static void CreateTransform(ref Vector2 position, ref Vector2 origin, ref Vector2 scale, float rotation,
            out Matrix transform)
        {
            var cos = (float)Math.Cos(rotation);
            var sin = (float)Math.Sin(rotation);

            Vector2 scaledOrigin;
            Vector2.Multiply(ref origin, ref scale, out scaledOrigin);

            Vector2 transformedOrigin;
            transformedOrigin.X = scaledOrigin.X * cos - scaledOrigin.Y * sin;
            transformedOrigin.Y = scaledOrigin.X * sin + scaledOrigin.Y * cos;            
            
            transform.M11 = scale.X * cos;
            transform.M12 = scale.X * sin;
            transform.M13 = 0.0f;
            transform.M14 = 0.0f;
            transform.M21 = scale.Y * -sin;
            transform.M22 = scale.Y * cos;
            transform.M23 = 0.0f;
            transform.M24 = 0.0f;            
            transform.M31 = 0.0f;
            transform.M32 = 0.0f;
            transform.M33 = 1.0f;
            transform.M34 = 0.0f;
            transform.M41 = position.X - transformedOrigin.X;
            transform.M42 = position.Y - transformedOrigin.Y;
            transform.M43 = 0.0f;
            transform.M44 = 1.0f;
        }
    }
}
