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
        private PianoMetadata _pianoMetadata = new();

        public MainWindow()
        {
            InitializeComponent();
            _audioService = new AsioAudioService();
            _fftAnalyzer = new FftAnalyzerService();
            _storageService = new MeasurementStorageService();
            
            // Vul de combobox met beschikbare drivers
            ComboAsioDrivers.ItemsSource = _audioService.GetAsioDrivers();

            // Initialize piano metadata UI
            InitializePianoMetadataControls();

            // Subscribe naar piano keyboard events
            PianoKeyboard.KeyPressed += OnPianoKeyPressed;

            // Subscribe naar FFT resultaten
            _fftAnalyzer.MeasurementUpdated += OnMeasurementUpdated;

            // Koppel START knop
            BtnStart.Click += BtnStart_Click;
            
            // Koppel Save/Load knoppen
            BtnSave.Click += BtnSave_Click;
            BtnLoad.Click += BtnLoad_Click;
            BtnAnalyzeDrift.Click += BtnAnalyzeDrift_Click;
            BtnAnalyzeReport.Click += BtnAnalyzeReport_Click;
        }

        private void InitializePianoMetadataControls()
        {
            // Populate piano type dropdown
            ComboPianoType.ItemsSource = new[]
            {
                new { Display = "Unknown", Value = PianoType.Unknown },
                new { Display = "Spinet (< 110cm)", Value = PianoType.Spinet },
                new { Display = "Console/Studio (110-120cm)", Value = PianoType.Console },
                new { Display = "Professional Upright (120-135cm)", Value = PianoType.ProfessionalUpright },
                new { Display = "Baby Grand (150-170cm)", Value = PianoType.BabyGrand },
                new { Display = "Parlor Grand (170-210cm)", Value = PianoType.ParlorGrand },
                new { Display = "Semi-Concert Grand (210-250cm)", Value = PianoType.SemiConcertGrand },
                new { Display = "Concert Grand (> 250cm)", Value = PianoType.ConcertGrand }
            };
            ComboPianoType.DisplayMemberPath = "Display";
            ComboPianoType.SelectedValuePath = "Value";
            ComboPianoType.SelectedIndex = 0;

            // Populate scale break dropdown (extended range: E2 to F#3 to accommodate variations)
            // Scientific basis: Askenfelt & Jansson (1990) - scale break varies by piano design
            var scaleBreakNotes = new List<object>();
            for (int midi = 40; midi <= 54; midi++)
            {
                string noteName = GetNoteName(midi);
                scaleBreakNotes.Add(new { Display = $"{noteName} (MIDI {midi})", Value = midi });
            }
            ComboScaleBreak.ItemsSource = scaleBreakNotes;
            ComboScaleBreak.DisplayMemberPath = "Display";
            ComboScaleBreak.SelectedValuePath = "Value";
            ComboScaleBreak.SelectedValue = 41; // Default: F2

            // Subscribe to changes
            ComboPianoType.SelectionChanged += (s, e) => UpdatePianoMetadataFromUI();
            TxtDimension.TextChanged += (s, e) => UpdatePianoMetadataFromUI();
            ComboScaleBreak.SelectionChanged += (s, e) => UpdatePianoMetadataFromUI();
            TxtManufacturer.TextChanged += (s, e) => UpdatePianoMetadataFromUI();
            TxtModel.TextChanged += (s, e) => UpdatePianoMetadataFromUI();
            TxtTemperature.TextChanged += (s, e) => UpdatePianoMetadataFromUI();
            TxtHumidity.TextChanged += (s, e) => UpdatePianoMetadataFromUI();
        }

        private void UpdatePianoMetadataFromUI()
        {
            _pianoMetadata.Type = ComboPianoType.SelectedValue is PianoType type ? type : PianoType.Unknown;
            
            if (int.TryParse(TxtDimension.Text, out int dimension))
            {
                _pianoMetadata.DimensionCm = dimension;
            }

            if (ComboScaleBreak.SelectedValue is int scaleBreak)
            {
                _pianoMetadata.ScaleBreakMidiNote = scaleBreak;
            }

            _pianoMetadata.Manufacturer = TxtManufacturer.Text;
            _pianoMetadata.Model = TxtModel.Text;

            // Optional environmental data
            if (double.TryParse(TxtTemperature.Text, out double temp))
            {
                _pianoMetadata.MeasurementTemperatureCelsius = temp;
            }
            else
            {
                _pianoMetadata.MeasurementTemperatureCelsius = null;
            }

            if (double.TryParse(TxtHumidity.Text, out double humidity))
            {
                _pianoMetadata.MeasurementHumidityPercent = humidity;
            }
            else
            {
                _pianoMetadata.MeasurementHumidityPercent = null;
            }

            // Set measurement timestamp
            _pianoMetadata.MeasurementDateTime = DateTime.Now;
            UpdateMeasurementDateLabel();

            // Update FFT analyzer with new metadata
            _fftAnalyzer.SetPianoMetadata(_pianoMetadata);
        }

        private void UpdateMeasurementDateLabel()
        {
            if (_pianoMetadata.MeasurementDateTime.HasValue)
            {
                LblMeasurementDate.Text = $"Last measured: {_pianoMetadata.MeasurementDateTime.Value:yyyy-MM-dd HH:mm}";
            }
            else
            {
                LblMeasurementDate.Text = "";
            }
        }

        private void LoadPianoMetadataToUI(PianoMetadata metadata)
        {
            _pianoMetadata = metadata;
            
            ComboPianoType.SelectedValue = metadata.Type;
            TxtDimension.Text = metadata.DimensionCm > 0 ? metadata.DimensionCm.ToString() : "";
            ComboScaleBreak.SelectedValue = metadata.ScaleBreakMidiNote;
            TxtManufacturer.Text = metadata.Manufacturer ?? "";
            TxtModel.Text = metadata.Model ?? "";

            // Load environmental data if available
            TxtTemperature.Text = metadata.MeasurementTemperatureCelsius?.ToString("F1") ?? "";
            TxtHumidity.Text = metadata.MeasurementHumidityPercent?.ToString("F0") ?? "";
            UpdateMeasurementDateLabel();

            // Update FFT analyzer with loaded metadata
            _fftAnalyzer.SetPianoMetadata(_pianoMetadata);
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
                    : $"{TxtProjectName.Text}_{DateTime.Now:yyyyMMdd_HHmms}.json"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    UpdatePianoMetadataFromUI();
                    _storageService.SaveMeasurements(saveDialog.FileName, _storedMeasurements, _pianoMetadata);
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
                    var (loadedMeasurements, pianoMetadata) = _storageService.LoadMeasurements(openDialog.FileName);
                    _storedMeasurements = loadedMeasurements;
                    _currentProjectFile = openDialog.FileName;

                    if (pianoMetadata != null)
                    {
                        LoadPianoMetadataToUI(pianoMetadata);
                    }

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

        private void BtnAnalyzeDrift_Click(object sender, RoutedEventArgs e)
        {
            // Check if we have old measurements loaded
            if (_storedMeasurements.Count == 0)
            {
                MessageBox.Show(
                    "Laad eerst een oud meetbestand via de 'Laden' knop.\n\n" +
                    "Dit bestand wordt gebruikt als referentie om pitch drift te berekenen.",
                    "Geen Referentiemetingen",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            // Prompt user to measure checkpoint notes
            var checkpointNotes = new[] { 21, 40, 69, 72, 96 }; // A0, E2, A4, C5, C7
            var checkpointNames = string.Join(", ", checkpointNotes.Select(m => GetNoteName(m)));

            var result = MessageBox.Show(
                $"Meet de volgende checkpoint noten voor drift analyse:\n\n{checkpointNames}\n\n" +
                "Klik OK wanneer klaar, of Cancel om te annuleren.",
                "Checkpoint Metingen",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.OK)
            {
                return;
            }

            // Collect checkpoint measurements that have been measured
            var newCheckpointMeasurements = new Dictionary<int, NoteMeasurement>();
            var missingNotes = new List<string>();

            foreach (int midi in checkpointNotes)
            {
                // Check if this note was measured in current session
                // For now, we'll use any available measurements
                // In production, you'd track "newly measured" vs "loaded from file"
                if (_storedMeasurements.ContainsKey(midi))
                {
                    newCheckpointMeasurements[midi] = _storedMeasurements[midi];
                }
                else
                {
                    missingNotes.Add(GetNoteName(midi));
                }
            }

            if (newCheckpointMeasurements.Count < 3)
            {
                MessageBox.Show(
                    $"Te weinig checkpoint metingen voor analyse.\n\nOntbreekt: {string.Join(", ", missingNotes)}",
                    "Onvoldoende Data",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Open drift analysis window
            try
            {
                var driftWindow = new Views.PitchDriftAnalysisWindow(
                    _storedMeasurements, // Old measurements (from loaded file)
                    newCheckpointMeasurements, // New checkpoint measurements
                    _pianoMetadata, // Old metadata
                    _pianoMetadata // Current metadata (could be updated)
                );

                if (driftWindow.ShowDialog() == true && driftWindow.ApplyOffsetRequested)
                {
                    // Apply calculated offset to all notes
                    ApplyPitchOffsetToMeasurements(driftWindow.CalculatedOffsetHz);
                    
                    MessageBox.Show(
                        $"Pitch offset toegepast: {driftWindow.CalculatedOffsetHz:+0.00;-0.00} Hz\n\n" +
                        "Alle noten zijn gecorrigeerd voor pitch drift.",
                        "Offset Toegepast",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij drift analyse:\n{ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyPitchOffsetToMeasurements(double offsetHz)
        {
            var calculator = new Services.PitchOffsetCalculator();

            foreach (var kvp in _storedMeasurements.ToList())
            {
                int midi = kvp.Key;
                var measurement = kvp.Value;

                if (measurement.CalculatedFundamental > 0)
                {
                    // Apply scaled offset based on register
                    double correctedFreq = calculator.ApplyScaledOffset(
                        measurement.CalculatedFundamental,
                        midi,
                        offsetHz
                    );

                    // Update measurement with corrected frequency
                    measurement.TargetFrequency = correctedFreq;
                    
                    // Optional: Recalculate all partials with offset
                    // (In practice, this is not needed as relative inharmonicity stays constant)
                }
            }

            // Update display
            MessageBox.Show(
                "Opmerking: De gecorrigeerde frequenties zijn nu de nieuwe 'target' waarden.\n" +
                "Gebruik deze als referentie bij het stemmen.",
                "Info",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
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

        private void BtnAnalyzeReport_Click(object sender, RoutedEventArgs e)
        {
            // Check if we have measurements
            if (_storedMeasurements.Count == 0)
            {
                MessageBox.Show(
                    "Geen metingen beschikbaar voor analyse.\n\n" +
                    "Meet eerst enkele noten of laad een meetbestand.",
                    "Geen Data",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            try
            {
                // Open analysis window
                var analysisWindow = new Views.MeasurementAnalysisWindow(
                    _storedMeasurements,
                    _pianoMetadata
                );

                analysisWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Fout bij genereren analyse rapport:\n{ex.Message}",
                    "Fout",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}