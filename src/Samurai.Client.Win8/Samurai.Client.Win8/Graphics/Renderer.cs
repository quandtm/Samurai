using Windows.UI.Core;

namespace Samurai.Client.Win8.Graphics
{
    public class Renderer
    {
        private CoreWindow _window;

        public Renderer()
        {
        }

        public bool Initialise(CoreWindow window)
        {
            return false;
        }

        public void Resize()
        {
            if (_window == null) return;
        }

        public void Draw()
        {
        }
    }
}
