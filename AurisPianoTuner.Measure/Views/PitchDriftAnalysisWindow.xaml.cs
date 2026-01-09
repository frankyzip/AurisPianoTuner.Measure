using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using AurisPianoTuner.Measure.Models;
using AurisPianoTuner.Measure.Services;

namespace AurisPianoTuner.Measure.Views
{
    public partial class PitchDriftAnalysisWindow : Window
    {
        private readonly Dictionary<int, NoteMeasurement> _oldMeasurements;
        private readonly Dictionary<int, NoteMeasurement> _newMeasurements;
        private readonly PianoMetadata? _oldMetadata;
        private readonly PianoMetadata? _newMetadata;
        private readonly PitchOffsetCalculator _calculator;

        public bool ApplyOffsetRequested { get; private set; }
        public double CalculatedOffsetHz { get; private set; }

        public PitchDriftAnalysisWindow(
            Dictionary<int, NoteMeasurement> oldMeasurements,
            Dictionary<int, NoteMeasurement> newMeasurements,
            PianoMetadata? oldMetadata = null,
            PianoMetadata? newMetadata = null)
        {
            InitializeComponent();

            _oldMeasurements = oldMeasurements;
            _newMeasurements = newMeasurements;
            _oldMetadata = oldMetadata;
            _newMetadata = newMetadata;
            _calculator = new PitchOffsetCalculator();

            BtnApplyOffset.Click += BtnApplyOffset_Click;
            BtnClose.Click += (s, e) => Close();

            PerformAnalysis();
        }

        private void PerformAnalysis()
        {
            // Display measurement metadata
            DisplayMetadata();

            // Calculate drift statistics
            var (isUniform, averageDrift, outliers) = _calculator.ValidateDriftUniformity(_oldMeasurements, _newMeasurements);

            // Display results
            DisplayDriftSummary(averageDrift, isUniform, outliers);
            DisplayCheckpointData();
        }

        private void DisplayMetadata()
        {
            // Old measurement info
            if (_oldMetadata?.MeasurementDateTime.HasValue == true)
            {
                TxtOldDate.Text = _oldMetadata.MeasurementDateTime.Value.ToString("yyyy-MM-dd HH:mm");
            }

            if (_oldMetadata?.MeasurementTemperatureCelsius.HasValue == true)
            {
                TxtOldTemp.Text = $"{_oldMetadata.MeasurementTemperatureCelsius.Value:F1} °C";
            }

            if (_oldMetadata?.MeasurementHumidityPercent.HasValue == true)
            {
                TxtOldHumidity.Text = $"{_oldMetadata.MeasurementHumidityPercent.Value:F0} %";
            }

            // New measurement info
            TxtNewDate.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

            if (_newMetadata?.MeasurementTemperatureCelsius.HasValue == true)
            {
                TxtNewTemp.Text = $"{_newMetadata.MeasurementTemperatureCelsius.Value:F1} °C";
            }

            if (_newMetadata?.MeasurementHumidityPercent.HasValue == true)
            {
                TxtNewHumidity.Text = $"{_newMetadata.MeasurementHumidityPercent.Value:F0} %";
            }

            // Time elapsed
            if (_oldMetadata?.MeasurementDateTime.HasValue == true)
            {
                var elapsed = DateTime.Now - _oldMetadata.MeasurementDateTime.Value;
                double months = elapsed.TotalDays / 30.44;
                TxtTimeElapsed.Text = $"{elapsed.Days} days ({months:F1} months)";
            }
        }

        private void DisplayDriftSummary(double averageDrift, bool isUniform, List<int> outliers)
        {
            TxtAverageDrift.Text = $"{averageDrift:+0.0;-0.0} cents";
            TxtAverageDrift.Foreground = averageDrift < 0 ? Brushes.Red : Brushes.Green;

            // Calculate standard deviation
            var drifts = _newMeasurements
                .Where(kvp => _oldMeasurements.ContainsKey(kvp.Key))
                .Select(kvp =>
                {
                    var oldFreq = _oldMeasurements[kvp.Key].CalculatedFundamental;
                    var newFreq = kvp.Value.CalculatedFundamental;
                    return 1200 * Math.Log2(newFreq / oldFreq);
                })
                .ToList();

            double stdDev = CalculateStandardDeviation(drifts);
            TxtStdDev.Text = $"±{stdDev:F2} cents";

            // Expected drift
            if (_oldMetadata?.MeasurementDateTime.HasValue == true)
            {
                var elapsed = DateTime.Now - _oldMetadata.MeasurementDateTime.Value;
                double months = elapsed.TotalDays / 30.44;

                double expectedDrift = _calculator.EstimateExpectedDrift(
                    months,
                    _oldMetadata?.MeasurementTemperatureCelsius,
                    _newMetadata?.MeasurementTemperatureCelsius,
                    _oldMetadata?.MeasurementHumidityPercent,
                    _newMetadata?.MeasurementHumidityPercent
                );

                TxtExpectedDrift.Text = $"{expectedDrift:+0.0;-0.0} cents (theoretical)";
            }

            // Assessment
            if (isUniform)
            {
                TxtAssessment.Text = "? Uniform drift detected - safe to apply offset";
                TxtAssessment.Foreground = Brushes.Green;
                BtnApplyOffset.IsEnabled = true;
            }
            else if (outliers.Count > 0)
            {
                TxtAssessment.Text = $"? {outliers.Count} outlier(s) detected - check for mechanical issues";
                TxtAssessment.Foreground = Brushes.Orange;
                BtnApplyOffset.IsEnabled = false;
            }
            else
            {
                TxtAssessment.Text = "? Insufficient data for analysis";
                TxtAssessment.Foreground = Brushes.Gray;
                BtnApplyOffset.IsEnabled = false;
            }
        }

        private void DisplayCheckpointData()
        {
            var checkpoints = new List<CheckpointData>();

            foreach (var kvp in _newMeasurements.OrderBy(x => x.Key))
            {
                int midi = kvp.Key;
                if (_oldMeasurements.TryGetValue(midi, out var oldMeasurement))
                {
                    double oldFreq = oldMeasurement.CalculatedFundamental;
                    double newFreq = kvp.Value.CalculatedFundamental;

                    if (oldFreq > 0 && newFreq > 0)
                    {
                        var (driftHz, driftCents) = _calculator.CalculateOffset(oldFreq, newFreq);

                        string status = Math.Abs(driftCents) < 3 ? "? Normal" :
                                      Math.Abs(driftCents) < 10 ? "? Moderate" :
                                      "? High";

                        checkpoints.Add(new CheckpointData
                        {
                            NoteName = kvp.Value.NoteName,
                            OldFrequency = oldFreq,
                            NewFrequency = newFreq,
                            DriftHz = driftHz,
                            DriftCents = driftCents,
                            Status = status
                        });
                    }
                }
            }

            DgCheckpoints.ItemsSource = checkpoints;
        }

        private double CalculateStandardDeviation(List<double> values)
        {
            if (values.Count < 2) return 0;
            double average = values.Average();
            double sumOfSquares = values.Sum(v => Math.Pow(v - average, 2));
            return Math.Sqrt(sumOfSquares / (values.Count - 1));
        }

        private void BtnApplyOffset_Click(object sender, RoutedEventArgs e)
        {
            // Calculate offset from A4 if available, otherwise use average
            if (_newMeasurements.ContainsKey(69) && _oldMeasurements.ContainsKey(69))
            {
                var (offsetHz, _) = _calculator.CalculateOffset(
                    _oldMeasurements[69].CalculatedFundamental,
                    _newMeasurements[69].CalculatedFundamental
                );
                CalculatedOffsetHz = offsetHz;
            }
            else
            {
                // Use average drift
                var drifts = _newMeasurements
                    .Where(kvp => _oldMeasurements.ContainsKey(kvp.Key))
                    .Select(kvp =>
                    {
                        var oldFreq = _oldMeasurements[kvp.Key].CalculatedFundamental;
                        var newFreq = kvp.Value.CalculatedFundamental;
                        return newFreq - oldFreq;
                    });
                CalculatedOffsetHz = drifts.Average();
            }

            ApplyOffsetRequested = true;
            Close();
        }

        private class CheckpointData
        {
            public string NoteName { get; set; } = "";
            public double OldFrequency { get; set; }
            public double NewFrequency { get; set; }
            public double DriftHz { get; set; }
            public double DriftCents { get; set; }
            public string Status { get; set; } = "";
        }
    }
}
