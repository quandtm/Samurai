using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;
using Windows.UI.Core;
using DXGI = SharpDX.DXGI;

namespace Samurai.Client.Win8.Graphics
{
    public class Renderer
    {
        private double _width, _height;
        public CoreWindow Window { get; set; }

        private Device1 _device;
        private DeviceContext1 _context;
        private DXGI.SwapChain1 _swap;

        private RenderTargetView _rtv;
        private DepthStencilView _dsv;

        public Renderer()
        {
        }

        public bool InitialiseNonWindow()
        {
            try
            {
                var device = new SharpDX.Direct3D11.Device(DriverType.Hardware, DeviceCreationFlags.BgraSupport);
                var device1 = Device.As<Device1>(device);
                var context = new DeviceContext1(device1);
                _swap = null;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool InitWindowResources()
        {
            if (Window == null) throw new ArgumentException();

            if (_rtv != null) _rtv.Dispose();
            if (_dsv != null) _dsv.Dispose();

            _width = Window.Bounds.Width;
            _height = Window.Bounds.Height;

            try
            {
                if (_swap != null)
                {
                    if (!_swap.ResizeBuffers(2, (int)_width, (int)_height, DXGI.Format.B8G8R8A8_UNorm, DXGI.SwapChainFlags.None).Success)
                        return false;
                }
                else
                {
                    var desc = new DXGI.SwapChainDescription1();
                    desc.Width = 0;
                    desc.Height = 0;
                    desc.Stereo = false;
                    desc.Format = DXGI.Format.B8G8R8A8_UNorm;
                    desc.SampleDescription.Count = 1;
                    desc.SampleDescription.Quality = 0;
                    desc.BufferCount = 2;
                    desc.Usage = DXGI.Usage.RenderTargetOutput;
                    desc.Scaling = DXGI.Scaling.None;
                    desc.SwapEffect = DXGI.SwapEffect.FlipSequential;
                    desc.Flags = DXGI.SwapChainFlags.None;

                    var dxgiDev = Device1.As<DXGI.Device1>(_device);
                    var adapter = dxgiDev.Adapter;
                    var factory = adapter.GetParent<DXGI.Factory2>();

                    using (var comWindow = new ComObject(Window))
                        _swap = factory.CreateSwapChainForCoreWindow(_device, comWindow, ref desc, null);

                    // Helps with battery for low end devices, only draws after vsync
                    dxgiDev.MaximumFrameLatency = 1;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Resize()
        {
            if (Window == null) return;

            if (Window.Bounds.Width != _width || Window.Bounds.Height != _height)
            {
                if (_rtv != null)
                {
                    _rtv.Dispose();
                    _rtv = null;
                }

                if (_dsv != null)
                {
                    _dsv.Dispose();
                    _dsv = null;
                }

                InitWindowResources();
            }
        }

        public void Draw()
        {

        }
    }
}
