using Windows.ApplicationModel.Core;

namespace Samurai.Client.Win8
{
    public class SamuraiGameViewSource : IFrameworkViewSource
    {
        public IFrameworkView CreateView()
        {
            return new SamuraiGameView();
        }
    }
}
