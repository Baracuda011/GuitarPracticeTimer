using Avalonia.Platform;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Enums;
using SoundFlow.Providers;
using SoundFlow.Structs;

namespace GuitarPracticeTimer.Ui.Audio;

/// <summary>The two-note signal that a practice block has ended.</summary>
public interface IChime
{
    void Play();
}

/// <summary>Does nothing. Used by tests and any host with no audio stack.</summary>
public sealed class SilentChime : IChime
{
    public void Play() { }
}

/// <summary>
/// Plays <c>Assets/chime.wav</c> through miniaudio, which is the one backend that
/// covers Windows, Linux, macOS, Android and iOS alike.
/// </summary>
/// <remarks>
/// Every failure path here is swallowed on purpose. The original Windows build
/// wrapped <c>Console.Beep</c> in a bare try/catch on the reasoning that on a
/// machine with no speaker the colour change is the real signal; that reasoning
/// holds far harder now, since a headless CI box or a locked-down audio daemon
/// must not take the timer down with it. The first failure latches
/// <see cref="_unavailable"/> so we stop retrying a device that will not open.
/// </remarks>
public sealed class Chime : IChime, IDisposable
{
    private static readonly Uri AssetUri =
        new("avares://GuitarPracticeTimer.Ui/Assets/chime.wav");

    /// <summary>Matches what make_chime.py writes: 44.1 kHz, 16-bit, mono.</summary>
    private static readonly AudioFormat Format = new()
    {
        Format = SampleFormat.S16,
        Channels = 1,
        Layout = ChannelLayout.Mono,
        SampleRate = 44100
    };

    private readonly Lock _gate = new();

    private byte[]? _wav;
    private MiniAudioEngine? _engine;
    private AudioPlaybackDevice? _device;
    private SoundPlayer? _player;
    private bool _unavailable;
    private bool _disposed;

    /// <summary>Fire and forget - opening the device can block, so it never runs on the UI thread.</summary>
    public void Play()
    {
        if (_unavailable || _disposed) return;
        Task.Run(PlayCore);
    }

    private void PlayCore()
    {
        try
        {
            SoundPlayer player;

            lock (_gate)
            {
                if (_unavailable || _disposed) return;

                EnsureDevice();
                DiscardPreviousPlayer();

                var source = new AssetDataProvider(_engine!, _wav!);
                player = new SoundPlayer(_engine!, Format, source);
                _device!.MasterMixer.AddComponent(player);
                _player = player;
            }

            player.Play();
        }
        catch
        {
            _unavailable = true;
        }
    }

    private void EnsureDevice()
    {
        if (_device is not null) return;

        _wav ??= LoadAsset();
        _engine = new MiniAudioEngine();
        _device = _engine.InitializePlaybackDevice(null, Format);
        _device.Start();
    }

    /// <summary>
    /// A finished player stays parked on the mixer, so the previous one is torn
    /// down before the next is added rather than accumulating one per block.
    /// </summary>
    private void DiscardPreviousPlayer()
    {
        if (_player is null) return;

        try
        {
            _player.Stop();
            _device?.MasterMixer.RemoveComponent(_player);
        }
        catch
        {
            // Already gone; nothing to unwind.
        }

        _player = null;
    }

    private static byte[] LoadAsset()
    {
        using var stream = AssetLoader.Open(AssetUri);
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                DiscardPreviousPlayer();
                _device?.Dispose();
                _engine?.Dispose();
            }
            catch
            {
                // Shutting down anyway.
            }

            _device = null;
            _engine = null;
        }
    }
}
