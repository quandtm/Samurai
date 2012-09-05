using Samurai.Client.Win8.Graphics;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Samurai.Client.Win8
{
    public class SamuraiGameView : IFrameworkView
    {
        private Renderer _renderer;
        private bool _running;

        public void Initialize(CoreApplicationView applicationView)
        {
            _renderer = new Renderer();
            _renderer.InitialiseNonWindow();
            _running = true;
        }

        public void Load(string entryPoint)
        {
        }

        public void Run()
        {
            if (_renderer.Window == null)
                SetWindow(CoreWindow.GetForCurrentThread());

            _renderer.Window.Activate();

            while (_running)
            {
                _renderer.Draw();
            }
        }

        public void SetWindow(CoreWindow window)
        {
            _renderer.Window = window;
            _renderer.InitWindowResources();
        }

        public void Uninitialize()
        {
        }
    }
}
