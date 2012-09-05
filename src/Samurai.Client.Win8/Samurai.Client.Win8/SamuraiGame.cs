using Microsoft.Xna.Framework;

namespace Samurai.Client.Win8
{
    public class SamuraiGame : Game
    {
        private GraphicsDeviceManager _graphics;

        public SamuraiGame()
            : base()
        {
            _graphics = new GraphicsDeviceManager(this);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            base.Draw(gameTime);
        }
    }
}
