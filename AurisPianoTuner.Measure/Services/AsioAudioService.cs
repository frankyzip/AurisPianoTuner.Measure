using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;
using NAudio.Wave.Asio;

namespace AurisPianoTuner.Measure.Services
{
    public class AsioAudioService : IAudioService, IDisposable
    {
        private AsioOut? _asioOut;
        public bool IsRunning { get; private set; }
        public event EventHandler<float[]>? AudioDataAvailable;

        public IEnumerable<string> GetAsioDrivers()
        {
            return AsioOut.GetDriverNames();
        }

        public void Start(string driverName, int sampleRate)
        {
            if (IsRunning) Stop();

            _asioOut = new AsioOut(driverName);
            
            // We stellen de driver in op de gevraagde sample rate (96000)
            // De UMC202HD ondersteunt dit native.
            _asioOut.InputChannelOffset = 0; // Kanaal 1 van de interface (Mic in)
            
            _asioOut.InitRecordAndPlayback(null, 1, sampleRate); 
            _asioOut.AudioAvailable += OnAudioAvailable;
            
            // Start playback/recording
            _asioOut.Play();
            IsRunning = true;
        }

        public void ShowControlPanel()
        {
            _asioOut?.ShowControlPanel();
        }

        private void OnAudioAvailable(object? sender, AsioAudioAvailableEventArgs e)
        {
            // Dit is de "hot path". Hier komen de samples binnen op 96kHz.
            // We converteren ze naar een float array voor makkelijke berekeningen.
            float[] samples = new float[e.SamplesPerBuffer];
            
            // We lezen alleen het eerste kanaal (uw ECM8000 mic)
            e.GetAsInterleavedSamples(samples);

            // Stuur de data door naar wie er luistert (de analyzer)
            AudioDataAvailable?.Invoke(this, samples);
        }

        public void Stop()
        {
            if (_asioOut != null)
            {
                _asioOut.Stop();
                _asioOut.Dispose();
                _asioOut = null;
            }
            IsRunning = false;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
