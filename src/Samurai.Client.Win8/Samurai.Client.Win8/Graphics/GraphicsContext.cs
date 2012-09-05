using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;
using Windows.UI.Core;
using DXGI = SharpDX.DXGI;

namespace Samurai.Client.Win8.Graphics
{
    public class GraphicsContext : IDisposable
    {
        private double _width, _height;
        private CoreWindow _window;
        public CoreWindow Window
        {
            get { return _window; }
            set
            {
                if (_window == value) return;
                if (_window != null)
                    _window.SizeChanged -= HandleWindowSizeChange;
                _window = value;
                _window.SizeChanged += HandleWindowSizeChange;
                CleanupWindowResources();
                InitWindowResources();
            }
        }

        private Device1 _device;
        private DeviceContext1 _context;
        private DXGI.SwapChain1 _swap;

        private RenderTargetView _rtv;
        private DepthStencilView _dsv;

        private bool _isReady;

        public GraphicsContext()
        {
            _isReady = false;
        }

        public bool InitialiseNonWindow()
        {
            try
            {
                using (var device = new SharpDX.Direct3D11.Device(DriverType.Hardware, DeviceCreationFlags.BgraSupport))
                    _device = device.QueryInterface<Device1>();
                _context = _device.ImmediateContext.QueryInterface<DeviceContext1>();

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
            if (_window == null) throw new ArgumentException();

            if (_rtv != null) _rtv.Dispose();
            if (_dsv != null) _dsv.Dispose();

            _width = _window.Bounds.Width;
            _height = _window.Bounds.Height;

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

                    var dxgiDev = _device.QueryInterface<DXGI.Device2>();
                    var adapter = dxgiDev.Adapter;
                    var factory = adapter.GetParent<DXGI.Factory2>();


                    using (var comWindow = new ComObject(_window))
                        _swap = factory.CreateSwapChainForCoreWindow(_device, comWindow, ref desc, null);

                    // Helps with battery for low end devices, only draws after vsync
                    dxgiDev.MaximumFrameLatency = 1;
                }

                using (var bbTex = _swap.GetBackBuffer<Texture2D>(0))
                {
                    _rtv = new RenderTargetView(_device, bbTex);
                }

                _context.OutputMerger.SetTargets(_rtv);
                _context.Rasterizer.SetViewport(0, 0, (float)_width, (float)_height);

                _isReady = true;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Resize()
        {
            if (_window == null) return;

            if (_window.Bounds.Width != _width || _window.Bounds.Height != _height)
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

        private void HandleWindowSizeChange(CoreWindow sender, WindowSizeChangedEventArgs args)
        {
            Resize();
        }

        public void Draw()
        {
            if (!_isReady) return;

            _context.ClearRenderTargetView(_rtv, Colors.CornflowerBlue);
            _swap.Present(1, DXGI.PresentFlags.None, new DXGI.PresentParameters());
        }

        public void Dispose()
        {
            CleanupWindowResources();
            CleanupDeviceResources();
        }

        private void CleanupDeviceResources()
        {
            SafeRelease(ref _context);
            SafeRelease(ref _device);
        }

        private void CleanupWindowResources()
        {
            SafeRelease(ref _rtv);
            SafeRelease(ref _dsv);
            SafeRelease(ref _swap);
        }

        private static void SafeRelease<T>(ref T obj) where T : ComObject
        {
            if (obj != null)
            {
                obj.Dispose();
                obj = null;
            }
        }
    }
}
