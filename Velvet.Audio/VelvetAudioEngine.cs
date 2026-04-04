using MiniAudioEx;
using MiniAudioEx.Native;

using Serilog;

namespace Velvet.Audio
{
    /// <summary>
    /// Manages the audio device, context, resource manager, and engine.
    /// Create one instance per application and dispose it on shutdown.
    /// </summary>
    public sealed class VelvetAudioEngine : IDisposable
    {
        private readonly ILogger _logger = Log.ForContext<VelvetAudioEngine>();

        private readonly ma_engine_ptr          _engine;
        private readonly ma_context_ptr         _context;
        private readonly ma_device_ptr          _device;
        private readonly ma_resource_manager_ptr _resourceManager;

        // Kept alive to prevent the delegate from being GC'd while the device is running.
        private readonly ma_device_data_proc _deviceDataProc;

        private bool _disposed = false;

        internal ma_engine_ptr Engine => _engine;

        // Construction

        /// <summary>
        /// Initializes the audio engine, creating the audio context, device, and resource manager, and starting the device.
        /// </summary>
        public VelvetAudioEngine()
        {
            _engine          = new ma_engine_ptr(true);
            _context         = new ma_context_ptr(true);
            _device          = new ma_device_ptr(true);
            _resourceManager = new ma_resource_manager_ptr(true);
            _deviceDataProc  = OnDeviceData;

            InitContext();
            InitDevice();
            InitResourceManager();
            InitEngine();
            StartDevice();

            _logger.Information("VelvetAudioEngine ready.");
        }

        // Init steps
        // Each throws on failure so the constructor stays clean

        private void InitContext()
        {
            if (MiniAudioNative.ma_context_init(null, _context) != ma_result.success)
                throw new AudioException("Failed to initialize audio context.");

            _logger.Debug("Audio context initialized.");
        }

        private void InitDevice()
        {
            ma_device_config cfg = MiniAudioNative.ma_device_config_init(ma_device_type.playback);
            cfg.playback.format   = ma_format.f32;
            cfg.playback.channels = 2;
            cfg.sampleRate        = 44100;
            cfg.periodSizeInFrames = 2048;
            cfg.SetDataCallback(_deviceDataProc);

            // Pick the system default playback device.
            if (MiniAudioNative.ma_context_get_devices(
                    _context,
                    out ma_device_info_ex[] playbackDevices,
                    out _) == ma_result.success
                && playbackDevices?.Length > 0)
            {
                foreach (var dev in playbackDevices)
                {
                    if (dev.deviceInfo.isDefault > 0)
                    {
                        cfg.playback.pDeviceID = dev.pDeviceId;
                        _logger.Information("Audio device: {Name}", dev.deviceInfo.GetName());
                        break;
                    }
                }
            }

            if (MiniAudioNative.ma_device_init(_context, ref cfg, _device) != ma_result.success)
                throw new AudioException("Failed to initialize audio device.");

            _logger.Debug("Audio device initialized.");
        }

        private void InitResourceManager()
        {
            ma_decoding_backend_vtable_ptr[] vtables =
            [
                MiniAudioNative.ma_libvorbis_get_decoding_backend_ptr()
            ];

            ma_resource_manager_config cfg = MiniAudioNative.ma_resource_manager_config_init();
            cfg.SetCustomDecodingBackendVTables(vtables);

            ma_result result = MiniAudioNative.ma_resource_manager_init(ref cfg, _resourceManager);
            cfg.FreeCustomDecodingBackendVTables();

            if (result != ma_result.success)
                throw new AudioException("Failed to initialize audio resource manager.");

            _logger.Debug("Audio resource manager initialized.");
        }

        private void InitEngine()
        {
            ma_engine_config cfg = MiniAudioNative.ma_engine_config_init();
            cfg.listenerCount    = MiniAudioNative.MA_ENGINE_MAX_LISTENERS;
            cfg.pDevice          = _device;
            cfg.pResourceManager = _resourceManager;

            if (MiniAudioNative.ma_engine_init(ref cfg, _engine) != ma_result.success)
                throw new AudioException("Failed to initialize audio engine.");

            // Store engine pointer in device user data so the data callback can reach it.
            unsafe
            {
                ma_device* dev = (ma_device*)_device.pointer;
                dev->pUserData = _engine.pointer;
            }

            _logger.Debug("Audio engine initialized.");
        }

        private void StartDevice()
        {
            if (MiniAudioNative.ma_device_start(_device) != ma_result.success)
                throw new AudioException("Failed to start audio device.");

            _logger.Debug("Audio device started.");
        }

        // Device data callback

        private unsafe void OnDeviceData(
            ma_device_ptr pDevice, IntPtr pOutput, IntPtr pInput, uint frameCount)
        {
            ma_device* dev = pDevice.Get();
            if (dev == null) return;

            var engine = new ma_engine_ptr(dev->pUserData);
            MiniAudioNative.ma_engine_read_pcm_frames(engine, pOutput, frameCount);
        }

        // IDisposable

        /// <summary>
        /// Frees the resources used by the audio engine, including the audio context, device, and resource manager. After calling Dispose, the engine can no longer be used to play audio.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _logger.Information("Shutting down VelvetAudioEngine...");

            MiniAudioNative.ma_engine_uninit(_engine);
            MiniAudioNative.ma_device_uninit(_device);
            MiniAudioNative.ma_resource_manager_uninit(_resourceManager);
            MiniAudioNative.ma_context_uninit(_context);

            _engine.Free();
            _device.Free();
            _resourceManager.Free();
            _context.Free();
        }
    }
}