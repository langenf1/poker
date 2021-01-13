using System;
using Microsoft.Xna.Framework;

namespace Poker
{
    public class Resolution
    {
        private Vector3 _scalingFactor;
        private int _preferredBackBufferWidth;
        private int _preferredBackBufferHeight;

        public Vector2 VirtualScreen = new Vector2(1280, 800);
        private Vector2 ScreenAspectRatio = new Vector2(1, 1);
        public Matrix Scale;
        public Vector2 ScreenScale;

        public void Update(GraphicsDeviceManager device)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));

            // Calculate ScalingFactor
            _preferredBackBufferWidth = device.PreferredBackBufferWidth;
            var widthScale = _preferredBackBufferWidth / VirtualScreen.X;

            _preferredBackBufferHeight = device.PreferredBackBufferHeight;
            var heightScale = _preferredBackBufferHeight / VirtualScreen.Y;

            ScreenScale = new Vector2(widthScale, heightScale);

            ScreenAspectRatio = new Vector2(widthScale / heightScale);
            _scalingFactor = new Vector3(widthScale, heightScale, 1);
            Scale = Matrix.CreateScale(_scalingFactor);
            device.ApplyChanges();
        }
    }
}