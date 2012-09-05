using Windows.ApplicationModel.Core;

namespace Samurai.Client.Win8
{
    public static class Program
    {
        static void Main()
        {
            var factory = new SamuraiGameViewSource();
            CoreApplication.Run(factory);
        }
    }
}
