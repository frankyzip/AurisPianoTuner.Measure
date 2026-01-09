using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using AurisPianoTuner.Measure.Services;
using AurisPianoTuner.Measure.Models;

namespace AurisPianoTuner.Measure
{
    public partial class MainWindow : Window
    {
        private readonly IAudioService _audioService;
        private readonly IFftAnalyzerService _fftAnalyzer;
        private readonly IMeasurementStorageService _storageService;
        private bool _isMeasuring = false;

        // Measurement storage en averaging
        private Dictionary<int, NoteMeasurement> _storedMeasurements = new();
        private List<NoteMeasurement> _currentMeasurementBuffer = new();
        private const int MaxMeasurementsPerNote = 10;
        private int _measurementCount = 0;

        private string? _currentProjectFile = null;

        public MainWindow()
        {
            InitializeComponent();
            _audioService = new AsioAudioService();
            _fftAnalyzer = new FftAnalyzerService();
            _storageService = new MeasurementStorageService();
            
            // Vul de combobox met beschikbare drivers
            ComboAsioDrivers.ItemsSource = _audioService.GetAsioDrivers();

            // Subscribe naar piano keyboard events
            PianoKeyboard.KeyPressed += OnPianoKeyPressed;

            // Subscribe naar FFT resultaten
            _fftAnalyzer.MeasurementUpdated += OnMeasurementUpdated;

            // Koppel START knop
            BtnStart.Click += BtnStart_Click;
            
            // Koppel Save/Load knoppen
            BtnSave.Click += BtnSave_Click;
            BtnLoad.Click += BtnLoad_Click;
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

            // Check of deze noot al gemeten is
            if (_storedMeasurements.TryGetValue(midiIndex, out var storedMeasurement))
            {
                // Toon opgeslagen meting
                DisplayStoredMeasurement(storedMeasurement);
            }

            // Geef door aan de FFT Analyzer
            _fftAnalyzer.SetTargetNote(midiIndex, freq);
            
            // Reset measurement buffer voor nieuwe noot
            _currentMeasurementBuffer.Clear();
            _measurementCount = 0;
        }

        private void DisplayStoredMeasurement(NoteMeasurement measurement)
        {
            // Toon berekende fundamentele (afgeleid van gemeten partial)
            if (measurement.CalculatedFundamental > 0)
            {
                LblMeasuredHz.Text = $"{measurement.CalculatedFundamental:F2} Hz";
            }
            else
            {
                var fundamental = measurement.DetectedPartials.FirstOrDefault(p => p.n == 1);
                if (fundamental != null)
                {
                    LblMeasuredHz.Text = $"{fundamental.Frequency:F2} Hz";
                }
            }

            QualityIndicator.Fill = measurement.Quality switch
            {
                "Groen" => System.Windows.Media.Brushes.Lime,
                "Oranje" => System.Windows.Media.Brushes.Orange,
                "Rood" => System.Windows.Media.Brushes.Red,
                _ => System.Windows.Media.Brushes.Gray
            };

            UpdateSpectrumDisplay(measurement);
        }

        private string GetNoteName(int midiIndex)
        {
            string[] notes = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            int octave = (midiIndex / 12) - 1;
            return notes[midiIndex % 12] + octave;
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            _isMeasuring = !_isMeasuring;

            if (_isMeasuring)
            {
                BtnStart.Content = "STOP";
                BtnStart.Background = System.Windows.Media.Brushes.DarkRed;
                _measurementCount = 0;
                _currentMeasurementBuffer.Clear();
            }
            else
            {
                BtnStart.Content = "START MEASUREMENT";
                BtnStart.Background = System.Windows.Media.Brushes.Green;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_storedMeasurements.Count == 0)
            {
                MessageBox.Show("Geen metingen om op te slaan!", "Waarschuwing", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "JSON Bestanden (*.json)|*.json|Alle Bestanden (*.*)|*.*",
                DefaultExt = "json",
                FileName = string.IsNullOrEmpty(TxtProjectName.Text) 
                    ? $"Piano_Measurement_{DateTime.Now:yyyyMMdd_HHmmss}.json"
                    : $"{TxtProjectName.Text}_{DateTime.Now:yyyyMMdd_HHmmss}.json"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    _storageService.SaveMeasurements(saveDialog.FileName, _storedMeasurements);
                    _currentProjectFile = saveDialog.FileName;
                    MessageBox.Show($"Metingen opgeslagen!\n{_storedMeasurements.Count} noten bewaard.", 
                        "Opslaan Gelukt", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fout bij opslaan:\n{ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "JSON Bestanden (*.json)|*.json|Alle Bestanden (*.*)|*.*",
                DefaultExt = "json"
            };

            if (openDialog.ShowDialog() == true)
            {
                try
                {
                    var loadedMeasurements = _storageService.LoadMeasurements(openDialog.FileName);
                    _storedMeasurements = loadedMeasurements;
                    _currentProjectFile = openDialog.FileName;

                    // Update alle toetsen met kwaliteitskleuren
                    foreach (var kvp in _storedMeasurements)
                    {
                        PianoKeyboard.SetKeyQuality(kvp.Key, kvp.Value.Quality);
                    }

                    MessageBox.Show($"Metingen geladen!\n{_storedMeasurements.Count} noten hersteld.", 
                        "Laden Gelukt", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    Title = $"AurisMeasure - {System.IO.Path.GetFileNameWithoutExtension(openDialog.FileName)}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fout bij laden:\n{ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ComboAsioDrivers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string? selectedDriver = ComboAsioDrivers.SelectedItem as string;
            if (!string.IsNullOrEmpty(selectedDriver))
            {
                try
                {
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
            UpdateVuMeter(samples);

            if (_isMeasuring)
            {
                _fftAnalyzer.ProcessAudioBuffer(samples);
            }
        }

        private void UpdateVuMeter(float[] samples)
        {
            float max = 0;
            for (int i = 0; i < samples.Length; i++)
            {
                float abs = Math.Abs(samples[i]);
                if (abs > max) max = abs;
            }

            double db = 20 * Math.Log10(max);
            if (double.IsInfinity(db)) db = -80;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                double progressValue = Math.Clamp((db + 60) * (100.0 / 60.0), 0, 100);
                VolumeBar.Value = progressValue;
                LblDbStatus.Text = $"{db:F1} dB";

                if (db > -3) VolumeBar.Foreground = System.Windows.Media.Brushes.Red;
                else if (db > -12) VolumeBar.Foreground = System.Windows.Media.Brushes.Orange;
                else VolumeBar.Foreground = System.Windows.Media.Brushes.Lime;
            }));
        }

        private void OnMeasurementUpdated(object? sender, NoteMeasurement measurement)
        {
            if (!_isMeasuring) return;

            _currentMeasurementBuffer.Add(measurement);
            _measurementCount++;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                // Toon berekende fundamentele (afgeleid van optimale partial voor dit register)
                if (measurement.CalculatedFundamental > 0)
                {
                    LblMeasuredHz.Text = $"{measurement.CalculatedFundamental:F2} Hz";
                }
                else
                {
                    LblMeasuredHz.Text = "--- Hz";
                }

                QualityIndicator.Fill = measurement.Quality switch
                {
                    "Groen" => System.Windows.Media.Brushes.Lime,
                    "Oranje" => System.Windows.Media.Brushes.Orange,
                    "Rood" => System.Windows.Media.Brushes.Red,
                    _ => System.Windows.Media.Brushes.Gray
                };

                Title = $"AurisMeasure - {measurement.NoteName} ({_measurementCount}/{MaxMeasurementsPerNote} samples) [n={measurement.MeasuredPartialNumber}]";
                UpdateSpectrumDisplay(measurement);

                // Automatisch stoppen na X metingen
                if (_measurementCount >= MaxMeasurementsPerNote)
                {
                    FinalizeMeasurement();
                }
            }));
        }

        private void FinalizeMeasurement()
        {
            if (_currentMeasurementBuffer.Count == 0) return;

            // Bereken gemiddelde meting
            var averaged = AverageMeasurements(_currentMeasurementBuffer);
            
            // Sla op in geheugen
            _storedMeasurements[averaged.MidiIndex] = averaged;

            // Update toets kleur op basis van kwaliteit
            PianoKeyboard.SetKeyQuality(averaged.MidiIndex, averaged.Quality);

            // Stop automatisch
            _isMeasuring = false;
            BtnStart.Content = "START MEASUREMENT";
            BtnStart.Background = System.Windows.Media.Brushes.Green;

            Title = $"AurisMeasure - {averaged.NoteName} COMPLETED";
        }

        private NoteMeasurement AverageMeasurements(List<NoteMeasurement> measurements)
        {
            var first = measurements.First();
            var result = new NoteMeasurement
            {
                MidiIndex = first.MidiIndex,
                TargetFrequency = first.TargetFrequency,
                NoteName = first.NoteName,
                MeasuredPartialNumber = first.MeasuredPartialNumber
            };

            // Voor elke partial (n=1 tot 16), bereken gemiddelde
            for (int n = 1; n <= 16; n++)
            {
                var partials = measurements
                    .SelectMany(m => m.DetectedPartials)
                    .Where(p => p.n == n)
                    .ToList();

                if (partials.Any())
                {
                    result.DetectedPartials.Add(new PartialResult
                    {
                        n = n,
                        Frequency = partials.Average(p => p.Frequency),
                        Amplitude = partials.Average(p => p.Amplitude)
                    });
                }
            }

            // Bereken gemiddelde fundamentele uit gemeten partials
            var calculatedFundamentals = measurements
                .Where(m => m.CalculatedFundamental > 0)
                .Select(m => m.CalculatedFundamental)
                .ToList();

            if (calculatedFundamentals.Any())
            {
                result.CalculatedFundamental = calculatedFundamentals.Average();
            }

            // Bepaal kwaliteit op basis van gemiddelde
            result.Quality = result.DetectedPartials.Count > 5 ? "Groen" :
                           result.DetectedPartials.Count > 2 ? "Oranje" : "Rood";

            return result;
        }

        private void UpdateSpectrumDisplay(NoteMeasurement measurement)
        {
            if (measurement.DetectedPartials.Count == 0)
            {
                return;
            }

            var spectrumText = $"Target: {measurement.NoteName} ({measurement.TargetFrequency:F2} Hz)\n";
            spectrumText += $"Quality: {measurement.Quality}\n";
            spectrumText += $"Progress: {_measurementCount}/{MaxMeasurementsPerNote}\n";
            spectrumText += $"Measured using: Partial n={measurement.MeasuredPartialNumber}\n";
            
            if (measurement.CalculatedFundamental > 0)
            {
                double deviation = measurement.CalculatedFundamental - measurement.TargetFrequency;
                double cents = 1200 * Math.Log2(measurement.CalculatedFundamental / measurement.TargetFrequency);
                spectrumText += $"Calculated Fundamental: {measurement.CalculatedFundamental:F2} Hz (Δ {deviation:+0.00;-0.00} Hz, {cents:+0.0;-0.0} cent)\n";
            }
            
            spectrumText += "\nDetected Partials:\n";
            
            foreach (var partial in measurement.DetectedPartials.Take(8))
            {
                double deviation = partial.Frequency - (measurement.TargetFrequency * partial.n);
                string marker = partial.n == measurement.MeasuredPartialNumber ? " ★" : "";
                spectrumText += $"n={partial.n}: {partial.Frequency:F2} Hz (Δ {deviation:+0.00;-0.00} Hz) | {partial.Amplitude:F1} dB{marker}\n";
            }

            var spectrumBorder = this.FindName("SpectrumBorder") as System.Windows.Controls.Border;
            if (spectrumBorder?.Child is System.Windows.Controls.TextBlock tb)
            {
                tb.Text = spectrumText;
                tb.FontSize = 14;
                tb.TextAlignment = System.Windows.TextAlignment.Left;
                tb.Margin = new Thickness(10);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _audioService.AudioDataAvailable -= OnDataReceived;
            _audioService.Stop();
            base.OnClosed(e);
        }
    }
}