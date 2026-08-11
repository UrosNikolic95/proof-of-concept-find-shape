using System.Collections.Generic;
using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using monogame_cros_platform.classes;
using monogame_cros_platform.enums;
using Color = Microsoft.Xna.Framework.Color;

namespace monogame_cros_platform
{
    public class AppGame : Game
    {
        private GraphicsDeviceManager _graphics;
        public static SoundEffect sharpSound;
        public static SoundEffect softSound;
        private TileMap map;

        public AppGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferMultiSampling = true;
            _graphics.PreparingDeviceSettings += (s, e) =>
                e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 8;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
            Window.Title = "Find Shape";
        }

        protected override void LoadContent()
        {
            sharpSound = Content.Load<SoundEffect>("sharp-sound");
            softSound = Content.Load<SoundEffect>("soft-sound");
            map = new TileMap(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
        }

    
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            map.Draw(gameTime);



            base.Draw(gameTime);
        }
    }
}
