using System;
using System.Collections.Generic;
using System.Linq;
using AurisPianoTuner.Measure.Models;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System.Numerics;

namespace AurisPianoTuner.Measure.Services
{
    public class FftAnalyzerService : IFftAnalyzerService
    {
        private const int FftSize = 32768; // Geeft ~2.93 Hz per bin bij 96kHz
        private const int SampleRate = 96000;
        private readonly float[] _buffer = new float[FftSize];
        private int _bufferOffset = 0;
        private readonly double[] _window;

        private int _currentTargetMidi;
        private double _targetFreq;
        private bool _isTargetSet = false;

        public event EventHandler<NoteMeasurement>? MeasurementUpdated;

        public FftAnalyzerService()
        {
            // Pre-calculate Blackman-Harris Window (4-term)
            _window = new double[FftSize];
            for (int i = 0; i < FftSize; i++)
            {
                _window[i] = 0.35875 
                           - 0.48829 * Math.Cos(2 * Math.PI * i / (FftSize - 1))
                           + 0.14128 * Math.Cos(4 * Math.PI * i / (FftSize - 1))
                           - 0.01168 * Math.Cos(6 * Math.PI * i / (FftSize - 1));
            }
        }

        public void SetTargetNote(int midiIndex, double theoreticalFrequency)
        {
            _currentTargetMidi = midiIndex;
            _targetFreq = theoreticalFrequency;
            _isTargetSet = true;
            Reset();
        }

        public void Reset() => _bufferOffset = 0;

        public void ProcessAudioBuffer(float[] samples)
        {
            if (!_isTargetSet) return;

            foreach (var sample in samples)
            {
                _buffer[_bufferOffset++] = sample;

                if (_bufferOffset >= FftSize)
                {
                    Analyze();
                    // 50% Overlap voor vloeiende analyse
                    Array.Copy(_buffer, FftSize / 2, _buffer, 0, FftSize / 2);
                    _bufferOffset = FftSize / 2;
                }
            }
        }

        private void Analyze()
        {
            // 1. Apply Window and convert to Complex
            Complex[] complexData = new Complex[FftSize];
            for (int i = 0; i < FftSize; i++)
            {
                complexData[i] = new Complex(_buffer[i] * _window[i], 0);
            }

            // 2. Perform Forward FFT
            Fourier.Forward(complexData, FourierOptions.NoScaling);

            // 3. Create Measurement Object
            var measurement = new NoteMeasurement
            {
                MidiIndex = _currentTargetMidi,
                TargetFrequency = _targetFreq,
                NoteName = GetNoteName(_currentTargetMidi)
            };

            // 4. Detect Partials (n = 1 tot 16)
            for (int n = 1; n <= 16; n++)
            {
                double centerFreq = _targetFreq * n;
                // +/- 50 cent window (ratio 1.0293)
                double minFreq = centerFreq * 0.971; 
                double maxFreq = centerFreq * 1.029;

                var peak = FindPrecisePeak(complexData, minFreq, maxFreq);
                if (peak != null)
                {
                    peak.n = n; // Zet partieel nummer
                    measurement.DetectedPartials.Add(peak);
                }
            }

            // 5. Kwaliteitscontrole
            measurement.Quality = DetermineQuality(measurement);

            MeasurementUpdated?.Invoke(this, measurement);
        }

        private PartialResult? FindPrecisePeak(Complex[] data, double minFreq, double maxFreq)
        {
            int minBin = (int)(minFreq / (SampleRate / (double)FftSize));
            int maxBin = (int)(maxFreq / (SampleRate / (double)FftSize));

            // Bescherm tegen out-of-range
            minBin = Math.Max(1, minBin);
            maxBin = Math.Min(FftSize / 2 - 2, maxBin);

            if (minBin >= maxBin) return null;

            int bestBin = -1;
            double maxMag = -1;

            // Zoek de hoogste bin in het venster
            for (int i = minBin; i <= maxBin; i++)
            {
                double mag = data[i].Magnitude;
                if (mag > maxMag)
                {
                    maxMag = mag;
                    bestBin = i;
                }
            }

            // Noise floor check
            if (maxMag < 0.0001 || bestBin < 1 || bestBin >= FftSize / 2 - 1) 
                return null;

            // 6. Parabolische Interpolatie voor 0.01 Hz precisie
            double magPrev = Math.Max(data[bestBin - 1].Magnitude, 1e-10);
            double magPeak = Math.Max(data[bestBin].Magnitude, 1e-10);
            double magNext = Math.Max(data[bestBin + 1].Magnitude, 1e-10);

            double alpha = 20 * Math.Log10(magPrev);
            double beta  = 20 * Math.Log10(magPeak);
            double gamma = 20 * Math.Log10(magNext);

            double denominator = alpha - 2 * beta + gamma;
            if (Math.Abs(denominator) < 1e-10) 
                return null;

            double p = 0.5 * (alpha - gamma) / denominator;
            
            // Bescherm tegen extreme offset waarden
            if (Math.Abs(p) > 0.5) 
                p = 0;

            double preciseBin = bestBin + p;
            double preciseFreq = preciseBin * (SampleRate / (double)FftSize);

            return new PartialResult { 
                n = 0, // Wordt later in Analyze() gezet
                Frequency = preciseFreq, 
                Amplitude = beta 
            };
        }

        private string DetermineQuality(NoteMeasurement m)
        {
            if (m.DetectedPartials.Count >= 6) return "Groen";
            if (m.DetectedPartials.Count >= 3) return "Oranje";
            return "Rood";
        }

        private string GetNoteName(int midi)
        {
            string[] names = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            int octave = (midi / 12) - 1;
            return names[midi % 12] + octave;
        }
    }
}
