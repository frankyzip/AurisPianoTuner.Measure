using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AurisPianoTuner.Measure.Models;
using MathNet.Numerics.IntegralTransforms;

namespace AurisPianoTuner.Measure.Services
{
    public class FftAnalyzerService : IFftAnalyzerService
    {
        private const int FftSize = 32768; // 2^15 voor hoge resolutie
        private const int SampleRate = 96000;
        private readonly double[] _window;
        private readonly float[] _audioBuffer = new float[FftSize];
        private int _bufferWritePos = 0;

        private int _targetMidi;
        private double _targetFreq;
        private bool _hasTarget = false;
        private PianoMetadata? _pianoMetadata;

        public event EventHandler<NoteMeasurement>? MeasurementUpdated;

        public FftAnalyzerService()
        {
            // Oplossing Audit Punt 2: Gebruik Blackman-Harris (beter dan Hann/Hamming)
            _window = new double[FftSize];
            for (int i = 0; i < FftSize; i++)
            {
                _window[i] = 0.35875 
                           - 0.48829 * Math.Cos(2 * Math.PI * i / (FftSize - 1))
                           + 0.14128 * Math.Cos(4 * Math.PI * i / (FftSize - 1))
                           - 0.01168 * Math.Cos(6 * Math.PI * i / (FftSize - 1));
            }
        }

        public void SetPianoMetadata(PianoMetadata metadata)
        {
            _pianoMetadata = metadata;
            System.Diagnostics.Debug.WriteLine($"[FftAnalyzer] Piano metadata set: {metadata.Type}, {metadata.DimensionCm}cm, Scale Break: {GetNoteName(metadata.ScaleBreakMidiNote)}");
        }

        public void SetTargetNote(int midiIndex, double theoreticalFrequency)
        {
            _targetMidi = midiIndex;
            _targetFreq = theoreticalFrequency;
            _bufferWritePos = 0;
            _hasTarget = true;
        }

        public void ProcessAudioBuffer(float[] samples)
        {
            if (!_hasTarget) return;

            // Oplossing Audit Punt 5: Efficiënt buffer management
            foreach (var sample in samples)
            {
                _audioBuffer[_bufferWritePos++] = sample;
                if (_bufferWritePos >= FftSize)
                {
                    Analyze();
                    // 50% overlap om geen transiënten te missen
                    Array.Copy(_audioBuffer, FftSize / 2, _audioBuffer, 0, FftSize / 2);
                    _bufferWritePos = FftSize / 2;
                }
            }
        }

        private void Analyze()
        {
            Complex[] fftBuffer = new Complex[FftSize];
            for (int i = 0; i < FftSize; i++)
                fftBuffer[i] = new Complex(_audioBuffer[i] * _window[i], 0);

            Fourier.Forward(fftBuffer, FourierOptions.NoScaling);

            var result = new NoteMeasurement {
                MidiIndex = _targetMidi,
                TargetFrequency = _targetFreq,
                NoteName = GetNoteName(_targetMidi)
            };

            // Check if we're near the scale break for extra logging
            bool isNearScaleBreak = false;
            if (_pianoMetadata != null)
            {
                int scaleBreak = _pianoMetadata.ScaleBreakMidiNote;
                isNearScaleBreak = Math.Abs(_targetMidi - scaleBreak) <= 2;
            }

            for (int n = 1; n <= 16; n++)
            {
                double centerFreq = _targetFreq * n;
                var partial = FindPrecisePeak(fftBuffer, centerFreq, n);
                if (partial != null) result.DetectedPartials.Add(partial);
            }

            var measuredPartial = SelectBestPartialForMeasurement(result.DetectedPartials, _targetMidi);
            
            if (measuredPartial != null)
            {
                result.MeasuredPartialNumber = measuredPartial.n;
                result.CalculatedFundamental = measuredPartial.Frequency / measuredPartial.n;
            }
            else
            {
                result.MeasuredPartialNumber = 1;
                result.CalculatedFundamental = 0;
            }

            result.Quality = result.DetectedPartials.Count > 5 ? "Groen" : 
                           result.DetectedPartials.Count > 0 ? "Oranje" : "Rood";

            // Metadata-aware logging
            if (_pianoMetadata != null)
            {
                string scaleBreakWarning = isNearScaleBreak ? " [NEAR SCALE BREAK]" : "";
                System.Diagnostics.Debug.WriteLine(
                    $"[{_pianoMetadata.Type}] {result.NoteName} (MIDI {_targetMidi}): " +
                    $"{result.DetectedPartials.Count}/16 partials, " +
                    $"using n={result.MeasuredPartialNumber}, " +
                    $"Quality: {result.Quality}{scaleBreakWarning}"
                );

                if (isNearScaleBreak)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"  ??  Scale break region - expect possible inharmonicity transition"
                    );
                }
            }

            MeasurementUpdated?.Invoke(this, result);
        }

        /// <summary>
        /// Bepaalt de optimale partial voor meting op basis van piano register.
        /// Gebaseerd op wetenschappelijke literatuur:
        /// - Askenfelt & Jansson (1990): Deep bass (MIDI 21-35) ? n=6-8
        /// - Barbour (1943): Bass (MIDI 36-47) ? n=3-4
        /// - Conklin (1996): Tenor (MIDI 48-60) ? n=2
        /// - Common practice: Mid-High (MIDI 61-72) ? n=1
        /// - Common practice: Treble (MIDI 73+) ? n=1
        /// </summary>
        /// <param name="midiIndex">MIDI note number (21-108 for 88-key piano)</param>
        /// <returns>Optimal partial number to use for tuning measurement</returns>
        private int GetOptimalPartialForRegister(int midiIndex)
        {
            return midiIndex switch
            {
                <= 35 => 6,    // Deep Bass (A0-B1): gebruik 6e partial (Askenfelt & Jansson 1990)
                <= 47 => 3,    // Bass (C2-B2): gebruik 3e partial (Barbour 1943)
                <= 60 => 2,    // Tenor (C3-C4): gebruik 2e partial (Conklin 1996)
                _ => 1         // Mid-High & Treble (C#4+): gebruik fundamentele (common practice)
            };
        }

        /// <summary>
        /// Bepaalt de beste partial voor meting op basis van amplitude.
        /// Kiest automatisch de sterkste partial binnen het optimale bereik voor het register.
        /// </summary>
        private PartialResult? SelectBestPartialForMeasurement(List<PartialResult> partials, int midiIndex)
        {
            if (partials == null || partials.Count == 0) return null;

            int optimalN = GetOptimalPartialForRegister(midiIndex);

            // Zoek eerst naar de optimale partial
            var optimalPartial = partials.FirstOrDefault(p => p.n == optimalN);
            
            // Als optimale partial gevonden en sterk genoeg, gebruik die
            if (optimalPartial != null && optimalPartial.Amplitude > 0)
            {
                return optimalPartial;
            }

            // Fallback: zoek sterkste partial in acceptabel bereik voor dit register
            var acceptablePartials = midiIndex switch
            {
                <= 35 => partials.Where(p => p.n >= 4 && p.n <= 8),   // Deep bass: n=4-8
                <= 47 => partials.Where(p => p.n >= 2 && p.n <= 4),   // Bass: n=2-4
                <= 60 => partials.Where(p => p.n >= 1 && p.n <= 3),   // Tenor: n=1-3
                _ => partials.Where(p => p.n == 1)                     // High: alleen n=1
            };

            // Kies sterkste uit acceptabele partials
            return acceptablePartials.OrderByDescending(p => p.Amplitude).FirstOrDefault();
        }

        private PartialResult? FindPrecisePeak(Complex[] fftData, double targetFreq, int n)
        {
            double binFreq = (double)SampleRate / FftSize;
            int centerBin = (int)(targetFreq / binFreq);
            int range = 15; // Zoekvenster van ca. 45 Hz rond de doel-bin

            int bestBin = -1;
            double maxMag = -1;

            for (int i = centerBin - range; i <= centerBin + range; i++)
            {
                if (i <= 0 || i >= FftSize / 2) continue;
                double mag = fftData[i].Magnitude;
                if (mag > maxMag) { maxMag = mag; bestBin = i; }
            }

            // Noise threshold: negeer pieken die te zwak zijn
            if (maxMag < 0.001 || bestBin <= 0 || bestBin >= FftSize / 2 - 1) return null;

            // Oplossing Audit Punt 4.1: Parabolische Interpolatie (0.01 Hz precisie)
            double magPrev = Math.Max(fftData[bestBin - 1].Magnitude, 1e-10);
            double magPeak = Math.Max(fftData[bestBin].Magnitude, 1e-10);
            double magNext = Math.Max(fftData[bestBin + 1].Magnitude, 1e-10);

            double y1 = Math.Log(magPrev);
            double y2 = Math.Log(magPeak);
            double y3 = Math.Log(magNext);

            double denominator = y1 - 2 * y2 + y3;
            if (Math.Abs(denominator) < 1e-10) return null;

            double d = (y1 - y3) / (2 * denominator);
            double preciseFreq = (bestBin + d) * binFreq;

            return new PartialResult { n = n, Frequency = preciseFreq, Amplitude = 20 * Math.Log10(maxMag) };
        }

        private string GetNoteName(int midi) {
            string[] names = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            return names[midi % 12] + ((midi / 12) - 1);
        }

        public void Reset() => _bufferWritePos = 0;
    }
}
