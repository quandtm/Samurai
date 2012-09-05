using Samurai.Client.Win8.Graphics;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Samurai.Client.Win8
{
    public class SamuraiGameView : IFrameworkView
    {
        private GraphicsContext _renderer;
        private bool _running;

        public void Initialize(CoreApplicationView applicationView)
        {
            _renderer = new GraphicsContext();
            _running = true;
        }

        public void Load(string entryPoint)
        {
        }

        public void Run()
        {
            if (!_renderer.IsReady)
            {
                _renderer.Window = CoreWindow.GetForCurrentThread();
                _renderer.InitialiseAll();
            }

            _renderer.Window.Activate();

            while (_running)
            {
                _renderer.ClearBackbuffer();
                _renderer.Present();
            }

            _renderer.Dispose();
        }

        public void SetWindow(CoreWindow window)
        {
            _renderer.Window = window;
        }

        public void Uninitialize()
        {
        }
    }
}
