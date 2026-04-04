using System.Numerics;

using MiniAudioEx;
using MiniAudioEx.Native;

using Serilog;

namespace Velvet.Audio
{
    /// <summary>
    /// A loaded sound that can be played, paused, stopped, and configured.
    /// Supports volume, pitch, panning, looping, and spatial (3D) audio.
    /// 
    /// Construct with a file path and a <see cref="VelvetAudioEngine"/>, then call <see cref="Play"/>.
    /// Dispose when the sound is no longer needed.
    /// </summary>
    public sealed class VelvetAudio : IDisposable
    {
        private readonly ILogger        _logger = Log.ForContext<VelvetAudio>();
        private readonly ma_sound_ptr   _sound;
        private readonly VelvetAudioEngine _engine;

        private bool _disposed = false;

        // Construction

        /// <summary>
        /// Loads an audio file ready for playback.
        /// </summary>
        /// <param name="engine">The active audio engine.</param>
        /// <param name="filePath">Path to the audio file (wav, mp3, ogg, flac, etc.).</param>
        /// <param name="flags">Optional miniaudio sound flags. Defaults to none.</param>
        public VelvetAudio(VelvetAudioEngine engine, string filePath, ma_sound_flags flags = 0)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Audio file not found: {filePath}", filePath);

            _engine = engine;
            _sound  = new ma_sound_ptr(true);

            ma_result result = MiniAudioNative.ma_sound_init_from_file(
                _engine.Engine, filePath, flags,
                new ma_sound_group_ptr(false),
                new ma_fence_ptr(false),
                _sound);

            if (result != ma_result.success)
                throw new AudioException($"Failed to load audio file '{filePath}': {result}");

            _logger.Information("Audio loaded: {Path}", filePath);
        }

        // Playback

        /// <summary>Starts or resumes playback.</summary>
        public void Play()
        {
            ThrowIfDisposed();
            MiniAudioNative.ma_sound_start(_sound);
        }

        /// <summary>Pauses playback without resetting the playback position.</summary>
        public void Pause()
        {
            ThrowIfDisposed();
            MiniAudioNative.ma_sound_stop(_sound);
        }

        /// <summary>Stops playback and rewinds to the beginning.</summary>
        public void Stop()
        {
            ThrowIfDisposed();
            MiniAudioNative.ma_sound_stop(_sound);
            MiniAudioNative.ma_sound_seek_to_pcm_frame(_sound, 0);
        }

        /// <summary>Seeks to a specific PCM frame.</summary>
        public void SeekToFrame(ulong frameIndex)
        {
            ThrowIfDisposed();
            MiniAudioNative.ma_sound_seek_to_pcm_frame(_sound, frameIndex);
        }

        /// <summary>Returns true if the sound is currently playing.</summary>
        public bool IsPlaying
        {
            get
            {
                ThrowIfDisposed();
                return MiniAudioNative.ma_sound_is_playing(_sound) != 0;
            }
        }

        /// <summary>Returns true if the sound has reached the end.</summary>
        public bool IsAtEnd
        {
            get
            {
                ThrowIfDisposed();
                return MiniAudioNative.ma_sound_at_end(_sound) != 0;
            }
        }

        // Basic properties

        /// <summary>
        /// Playback volume. 1.0 is original volume, 0.0 is silent.
        /// Values above 1.0 amplify (may clip).
        /// </summary>
        public float Volume
        {
            get { ThrowIfDisposed(); return MiniAudioNative.ma_sound_get_volume(_sound); }
            set { ThrowIfDisposed(); MiniAudioNative.ma_sound_set_volume(_sound, Math.Max(0f, value)); }
        }

        /// <summary>
        /// Playback pitch multiplier. 1.0 is original pitch, 2.0 is one octave up, 0.5 is one octave down.
        /// </summary>
        public float Pitch
        {
            get { ThrowIfDisposed(); return MiniAudioNative.ma_sound_get_pitch(_sound); }
            set { ThrowIfDisposed(); MiniAudioNative.ma_sound_set_pitch(_sound, Math.Max(0.001f, value)); }
        }

        /// <summary>
        /// Stereo pan in the range [-1.0, 1.0]. 0.0 is centre, -1.0 is full left, 1.0 is full right.
        /// Has no effect when spatial audio is enabled.
        /// </summary>
        public float Pan
        {
            get { ThrowIfDisposed(); return MiniAudioNative.ma_sound_get_pan(_sound); }
            set { ThrowIfDisposed(); MiniAudioNative.ma_sound_set_pan(_sound, Math.Clamp(value, -1f, 1f)); }
        }

        /// <summary>Whether the sound loops when it reaches the end.</summary>
        public bool Loop
        {
            get { ThrowIfDisposed(); return MiniAudioNative.ma_sound_is_looping(_sound) != 0; }
            set { ThrowIfDisposed(); MiniAudioNative.ma_sound_set_looping(_sound, value ? 1u : 0u); }
        }

        // Fading

        /// <summary>
        /// Fades the volume from <paramref name="fromVolume"/> to <paramref name="toVolume"/>
        /// over <paramref name="durationMs"/> milliseconds.
        /// Pass -1 for <paramref name="fromVolume"/> to start from the current volume.
        /// </summary>
        public void FadeTo(float fromVolume, float toVolume, ulong durationMs)
        {
            ThrowIfDisposed();
            MiniAudioNative.ma_sound_set_fade_in_milliseconds(_sound, fromVolume, toVolume, durationMs);
        }

        /// <summary>Fades in from silence to full volume over the given duration.</summary>
        public void FadeIn(ulong durationMs)  => FadeTo(0f, 1f, durationMs);

        /// <summary>Fades out to silence over the given duration.</summary>
        public void FadeOut(ulong durationMs) => FadeTo(-1f, 0f, durationMs);

        // Spatial / 3D audio

        /// <summary>
        /// Enables or disables spatial (3D positional) audio processing.
        /// When disabled, <see cref="Pan"/> is used for stereo positioning instead.
        /// </summary>
        public bool Spatialization
        {
            get { ThrowIfDisposed(); return MiniAudioNative.ma_sound_is_spatialization_enabled(_sound) != 0; }
            set { ThrowIfDisposed(); MiniAudioNative.ma_sound_set_spatialization_enabled(_sound, value ? 1u : 0u); }
        }

        /// <summary>
        /// Sets the 3D world-space position of this sound.
        /// Requires <see cref="Spatialization"/> to be enabled.
        /// </summary>
        public void SetPosition(Vector3 pos)
        {
            ThrowIfDisposed();
            MiniAudioNative.ma_sound_set_position(_sound, pos.X, pos.Y, pos.Z);
        }

        /// <summary>
        /// Sets the velocity of this sound source, used for Doppler effect simulation.
        /// </summary>
        public void SetVelocity(Vector3 vel)
        {
            ThrowIfDisposed();
            MiniAudioNative.ma_sound_set_velocity(_sound, vel.X, vel.Y, vel.Z);
        }

        /// <summary>
        /// Sets the distance attenuation range.
        /// The sound is at full volume within <paramref name="minDistance"/> and
        /// fully silent beyond <paramref name="maxDistance"/>.
        /// </summary>
        public void SetAttenuationRange(float minDistance, float maxDistance)
        {
            ThrowIfDisposed();
            MiniAudioNative.ma_sound_set_min_distance(_sound, minDistance);
            MiniAudioNative.ma_sound_set_max_distance(_sound, maxDistance);
        }

        // Helpers

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(VelvetAudio));
        }

        // IDisposable

        /// <summary>
        /// Frees the resources used by this sound. After calling Dispose, the sound can no longer be played or configured.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            MiniAudioNative.ma_sound_uninit(_sound);
            _sound.Free();

            _logger.Debug("VelvetAudio disposed.");
        }
    }
}