using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AurisPianoTuner.Measure.Services;
using AurisPianoTuner.Measure.Models;

namespace AurisPianoTuner.Measure
{
    public partial class MainWindow : Window
    {
        private readonly IAudioService _audioService;
        private readonly IFftAnalyzerService _fftAnalyzer;
        private bool _isRecording = false;

        public MainWindow()
        {
            InitializeComponent();
            _audioService = new AsioAudioService();
            _fftAnalyzer = new FftAnalyzerService();
            
            // Vul de combobox met beschikbare drivers
            ComboAsioDrivers.ItemsSource = _audioService.GetAsioDrivers();

            // Subscribe naar piano keyboard events
            PianoKeyboard.KeyPressed += OnPianoKeyPressed;

            // Subscribe naar FFT resultaten
            _fftAnalyzer.MeasurementUpdated += OnMeasurementUpdated;

            // Koppel START knop
            BtnStart.Click += BtnStart_Click;
        }

        private void OnPianoKeyPressed(object? sender, int midiIndex)
        {
            // Bereken theoretische frequentie: f = 440 * 2^((n-69)/12)
            double freq = 440.0 * Math.Pow(2, (midiIndex - 69) / 12.0);
            string noteName = GetNoteName(midiIndex);

            LblNoteName.Text = noteName;
            LblTargetHz.Text = $"{freq:F2} Hz";
            LblMeasuredHz.Text = "--.--- Hz";
            QualityIndicator.Fill = System.Windows.Media.Brushes.Gray;

            // Geef door aan de FFT Analyzer
            _fftAnalyzer.SetTargetNote(midiIndex, freq);
        }

        private string GetNoteName(int midiIndex)
        {
            string[] notes = { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" };
            int octave = (midiIndex / 12) - 1;
            int noteIndex = (midiIndex - 9) % 12;
            return notes[noteIndex] + octave;
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            // Extra initialisatie na UI render indien nodig
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            _isRecording = !_isRecording;

            if (_isRecording)
            {
                BtnStart.Content = "STOP RECORDING";
                BtnStart.Background = System.Windows.Media.Brushes.DarkRed;
            }
            else
            {
                BtnStart.Content = "START RECORDING";
                BtnStart.Background = System.Windows.Media.Brushes.Green;
            }
        }

        private void ComboAsioDrivers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string? selectedDriver = ComboAsioDrivers.SelectedItem as string;
            if (!string.IsNullOrEmpty(selectedDriver))
            {
                try
                {
                    // We starten de test op 96kHz
                    _audioService.Start(selectedDriver, 96000);
                    _audioService.AudioDataAvailable += OnDataReceived;
                    BtnStart.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fout bij starten ASIO: {ex.Message}");
                }
            }
        }

        private void OnDataReceived(object? sender, float[] samples)
        {
            // 1. Bestaande VU-meter logica (voor visuele feedback - altijd actief)
            UpdateVuMeter(samples);

            // 2. Stuur naar FFT Analyzer (alleen als op START is gedrukt)
            if (_isRecording)
            {
                _fftAnalyzer.ProcessAudioBuffer(samples);
            }
        }

        private void UpdateVuMeter(float[] samples)
        {
            // Bereken de hoogste waarde (Peak) in deze buffer
            float max = 0;
            for (int i = 0; i < samples.Length; i++)
            {
                float abs = Math.Abs(samples[i]);
                if (abs > max) max = abs;
            }

            // Zet om naar Decibels voor een natuurlijke weergave
            double db = 20 * Math.Log10(max);
            if (double.IsInfinity(db)) db = -80;

            // Update de UI op de hoofd-thread
            Dispatcher.BeginInvoke(new Action(() =>
            {
                // We schalen -60dB tot 0dB naar 0-100% op de bar
                double progressValue = Math.Clamp((db + 60) * (100.0 / 60.0), 0, 100);
                VolumeBar.Value = progressValue;
                LblDbStatus.Text = $"{db:F1} dB";

                // Kleurindicator voor clipping
                if (db > -3) VolumeBar.Foreground = System.Windows.Media.Brushes.Red;
                else if (db > -12) VolumeBar.Foreground = System.Windows.Media.Brushes.Orange;
                else VolumeBar.Foreground = System.Windows.Media.Brushes.Lime;
            }));
        }

        private void OnMeasurementUpdated(object? sender, NoteMeasurement measurement)
        {
            // Update UI met FFT resultaten (thread-safe)
            Dispatcher.BeginInvoke(new Action(() =>
            {
                // Toon gemeten fundamentele frequentie (n=1)
                var fundamental = measurement.DetectedPartials.FirstOrDefault(p => p.n == 1);
                if (fundamental != null)
                {
                    LblMeasuredHz.Text = $"{fundamental.Frequency:F2} Hz";
                }

                // Update quality indicator
                QualityIndicator.Fill = measurement.Quality switch
                {
                    "Groen" => System.Windows.Media.Brushes.Lime,
                    "Oranje" => System.Windows.Media.Brushes.Orange,
                    "Rood" => System.Windows.Media.Brushes.Red,
                    _ => System.Windows.Media.Brushes.Gray
                };
            }));
        }

        protected override void OnClosed(EventArgs e)
        {
            _audioService.AudioDataAvailable -= OnDataReceived;
            _audioService.Stop();
            base.OnClosed(e);
        }
    }
}