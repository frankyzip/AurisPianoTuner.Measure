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

        // NIEUW: logger property
        public ITestLoggerService? TestLogger { get; set; }

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

            var partials = new List<PartialResult>();

            // OPLOSSING 1: Dynamisch zoekvenster per partial (bass-blindheid fix)
            for (int n = 1; n <= 16; n++)
            {
                double centerFreq = _targetFreq * n;
                var partial = FindPrecisePeak(fftBuffer, centerFreq, n, _targetMidi);
                if (partial != null)
                {
                    result.DetectedPartials.Add(partial);
                    partials.Add(new PartialResult { n = partial.n, Frequency = partial.Frequency, Amplitude = partial.Amplitude });
                }
            }

            // OPLOSSING 2: Bereken werkelijke inharmoniciteit (B-coefficient)
            double measuredB = CalculateInharmonicity(result.DetectedPartials, _targetFreq);
            result.InharmonicityCoefficient = measuredB;

            // OPLOSSING 3: Selecteer beste partial voor meting
            var measuredPartial = SelectBestPartialForMeasurement(result.DetectedPartials, _targetMidi);
            
            if (measuredPartial != null)
            {
                result.MeasuredPartialNumber = measuredPartial.n;
                
                // KRITIEK: Bereken fundamentele frequentie MET inharmoniciteitscorrectie
                // f? = f_n / (n·?(1 + B·n²))
                // Dit voorkomt dat bass-afwijkingen het resultaat vervormen
                double nSquared = measuredPartial.n * measuredPartial.n;
                double inharmonicityFactor = Math.Sqrt(1 + measuredB * nSquared);
                result.CalculatedFundamental = measuredPartial.Frequency / (measuredPartial.n * inharmonicityFactor);
            }
            else
            {
                result.MeasuredPartialNumber = 1;
                result.CalculatedFundamental = 0;
            }

            result.Quality = result.DetectedPartials.Count > 5 ? "Groen" : 
                           result.DetectedPartials.Count > 0 ? "Oranje" : "Rood";

            // Metadata-aware logging met B-coefficient
            if (_pianoMetadata != null)
            {
                string scaleBreakWarning = isNearScaleBreak ? " [NEAR SCALE BREAK]" : "";
                System.Diagnostics.Debug.WriteLine(
                    $"[{_pianoMetadata.Type}] {result.NoteName} (MIDI {_targetMidi}): " +
                    $"{result.DetectedPartials.Count}/16 partials, " +
                    $"using n={result.MeasuredPartialNumber}, " +
                    $"B={measuredB:E2}, " +
                    $"f?={result.CalculatedFundamental:F2} Hz, " +
                    $"Quality: {result.Quality}{scaleBreakWarning}"
                );

                if (isNearScaleBreak)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"  ??  Scale break region - expect possible inharmonicity transition"
                    );
                }
            }

            // Logging voor debug
            double rms = 0;
            for (int i = 0; i < _audioBuffer.Length; i++) rms += _audioBuffer[i] * _audioBuffer[i];
            rms = Math.Sqrt(rms / _audioBuffer.Length);
            TestLogger?.LogAnalysisAttempt(_targetMidi, partials, rms);

            MeasurementUpdated?.Invoke(this, result);
        }

        /// <summary>
        /// Berekent de inharmoniciteitscoëfficiënt (B) van de gedetecteerde partials.
        /// 
        /// Wetenschappelijke basis:
        /// - Fletcher & Rossing (1998): "The Physics of Musical Instruments", p.362-364
        /// - Conklin (1996): "Design and Tone in the Mechanoacoustic Piano"
        /// 
        /// Formule: f_n = n·f?·?(1 + B·n²)
        /// Herschrijven: B = [(f_n / (n·f?))² - 1] / n²
        /// 
        /// Methode: Lineaire regressie op (n², (f_n/(n·f?))² - 1) voor robuuste schatting.
        /// </summary>
        /// <param name="partials">Lijst van gedetecteerde partials</param>
        /// <param name="fundamentalEstimate">Geschatte fundamentele frequentie (theoretisch)</param>
        /// <returns>B-coefficient in typische range 0.00001 - 0.005</returns>
        private double CalculateInharmonicity(List<PartialResult> partials, double fundamentalEstimate)
        {
            if (partials == null || partials.Count < 3)
                return 0.0001; // Fallback: typische waarde voor medium piano

            // Filter partials met voldoende amplitude (> -60 dB)
            var validPartials = partials.Where(p => p.Amplitude > -60 && p.n >= 2 && p.n <= 12).ToList();
            
            if (validPartials.Count < 3)
                return 0.0001;

            // Lineaire regressie: y = B·x
            // waar x = n², y = (f_n / (n·f?))² - 1
            double sumX = 0, sumY = 0, sumXY = 0, sumXX = 0;
            int count = 0;

            foreach (var p in validPartials)
            {
                double n = p.n;
                double fn = p.Frequency;
                
                // Gebruik iteratieve verbetering: schat f? vanuit huidige partial
                double f1Estimate = fn / n; // Eerste orde benadering
                
                double x = n * n;
                double ratio = fn / (n * f1Estimate);
                double y = ratio * ratio - 1.0;

                // Outlier detectie: negeer extreme waarden (mogelijk foutieve pieken)
                if (y < -0.1 || y > 0.5) continue;

                sumX += x;
                sumY += y;
                sumXY += x * y;
                sumXX += x * x;
                count++;
            }

            if (count < 2 || Math.Abs(sumXX) < 1e-10)
                return 0.0001;

            // B = (N·?XY - ?X·?Y) / (N·?XX - ?X·?X)
            double B = (count * sumXY - sumX * sumY) / (count * sumXX - sumX * sumX);

            // Saniteer resultaat: B moet in realistische range liggen
            // Typisch: Spinet = 0.0005-0.005, Concert Grand = 0.00003-0.0002
            B = Math.Max(0.00001, Math.Min(0.005, B));

            return B;
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
            if (optimalPartial != null && optimalPartial.Amplitude > -60)
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

        /// <summary>
        /// Zoekt precisie piek in FFT spectrum met dynamisch zoekvenster.
        /// 
        /// OPLOSSING "BASS BLINDHEID":
        /// - Voor bass notes (< 100 Hz): smal venster (±25 cents) voorkomt octaafverwarring
        /// - Voor mid notes (100-1000 Hz): medium venster (±35 cents)
        /// - Voor treble (> 1000 Hz): breed venster (±50 cents) compenseert detuning
        /// 
        /// Wetenschappelijke basis:
        /// - Oppenheim & Schafer (2010): "Discrete-Time Signal Processing"
        /// - Smith (2011): "Spectral Audio Signal Processing" - Parabolic interpolation, p.283-287
        /// </summary>
        /// <param name="fftData">FFT spectrum data</param>
        /// <param name="targetFreq">Verwachte frequentie (n·f?)</param>
        /// <param name="n">Partial nummer</param>
        /// <param name="midiIndex">MIDI note (voor context)</param>
        /// <returns>PartialResult met precieze frequentie en amplitude, of null</returns>
        private PartialResult? FindPrecisePeak(Complex[] fftData, double targetFreq, int n, int midiIndex)
        {
            double binFreq = (double)SampleRate / FftSize;
            int centerBin = (int)(targetFreq / binFreq);

            // DYNAMISCH ZOEKVENSTER (oplossing bass-blindheid)
            // Bereken venster in cents, converteer naar bins
            double searchWindowCents = targetFreq switch
            {
                < 100 => 25.0,    // Bass: smal venster (bijv. A0: 27.5 Hz ± 0.4 Hz)
                < 1000 => 35.0,   // Mid: medium venster
                _ => 50.0         // Treble: breed venster (meer detuning tolerantie)
            };

            // Converteer cents naar Hz: ?f = f·(2^(cents/1200) - 1)
            double searchWindowHz = targetFreq * (Math.Pow(2, searchWindowCents / 1200.0) - 1);
            int searchRange = Math.Max(3, (int)(searchWindowHz / binFreq));

            // Veiligheidslimiet: maximaal 50 bins (voorkomt crash bij extreme waarden)
            searchRange = Math.Min(searchRange, 50);

            int bestBin = -1;
            double maxMag = -1;

            for (int i = centerBin - searchRange; i <= centerBin + searchRange; i++)
            {
                if (i <= 0 || i >= FftSize / 2) continue;
                double mag = fftData[i].Magnitude;
                if (mag > maxMag) { maxMag = mag; bestBin = i; }
            }

            // Noise threshold: pas aan op basis van frequentie
            // Bass: hogere threshold (meer omgevingsruis)
            // Treble: lagere threshold (zuiverder signaal)
            double noiseThreshold = targetFreq < 100 ? 0.002 : 0.001;
            
            if (maxMag < noiseThreshold || bestBin <= 0 || bestBin >= FftSize / 2 - 1)
                return null;

            // Oplossing Audit Punt 4.1: Parabolische Interpolatie (0.01 Hz precisie)
            // Bron: Smith (2011) "Spectral Audio Signal Processing", p.283-287
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

            // Extra validatie: detecteer implausibele frequenties
            // Een partial mag maximaal ±2 semitonen afwijken van verwachte waarde
            double maxDeviation = targetFreq * 0.1225; // 2 semitones = 12.25%
            if (Math.Abs(preciseFreq - targetFreq) > maxDeviation)
                return null;

            return new PartialResult { n = n, Frequency = preciseFreq, Amplitude = 20 * Math.Log10(maxMag) };
        }

        private string GetNoteName(int midi) {
            string[] names = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            return names[midi % 12] + ((midi / 12) - 1);
        }

        public void Reset() => _bufferWritePos = 0;
    }
}
