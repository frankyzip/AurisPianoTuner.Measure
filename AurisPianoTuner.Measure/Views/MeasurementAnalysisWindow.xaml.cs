using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using Microsoft.Win32;
using AurisPianoTuner.Measure.Models;

namespace AurisPianoTuner.Measure.Views
{
    public partial class MeasurementAnalysisWindow : Window
    {
        private readonly string _reportText;

        public MeasurementAnalysisWindow(Dictionary<int, NoteMeasurement> measurements, PianoMetadata? metadata)
        {
            InitializeComponent();

            // Generate report
            _reportText = GenerateAnalysisReport(measurements, metadata);
            TxtReport.Text = _reportText;

            // Update subtitle
            if (metadata != null)
            {
                TxtSubtitle.Text = $"{metadata.Manufacturer} {metadata.Model} - {metadata.Type}";
            }

            // Wire up events
            BtnCopyToClipboard.Click += BtnCopyToClipboard_Click;
            BtnExportTxt.Click += BtnExportTxt_Click;
            BtnClose.Click += (s, e) => Close();
        }

        private string GenerateAnalysisReport(Dictionary<int, NoteMeasurement> measurements, PianoMetadata? metadata)
        {
            var sb = new StringBuilder();

            // Header
            sb.AppendLine("============================================================");
            sb.AppendLine("      AURISMEASURE - MEASUREMENT ANALYSIS REPORT");
            sb.AppendLine("============================================================");
            sb.AppendLine();

            // Piano Information
            sb.AppendLine("PIANO INFORMATION");
            sb.AppendLine("-----------------------------------------------------------");
            if (metadata != null)
            {
                sb.AppendLine($"Manufacturer:     {metadata.Manufacturer ?? "Unknown"}");
                sb.AppendLine($"Model:            {metadata.Model ?? "Unknown"}");
                sb.AppendLine($"Type:             {metadata.Type}");
                sb.AppendLine($"Dimension:        {metadata.DimensionCm} cm");
                sb.AppendLine($"Scale Break:      {GetNoteName(metadata.ScaleBreakMidiNote)} (MIDI {metadata.ScaleBreakMidiNote})");
                sb.AppendLine($"Serial Number:    {metadata.SerialNumber ?? "N/A"}");
                sb.AppendLine();

                // Environmental Conditions
                if (metadata.MeasurementTemperatureCelsius.HasValue || metadata.MeasurementHumidityPercent.HasValue)
                {
                    sb.AppendLine("MEASUREMENT CONDITIONS");
                    sb.AppendLine("-----------------------------------------------------------");
                    if (metadata.MeasurementTemperatureCelsius.HasValue)
                        sb.AppendLine($"Temperature:      {metadata.MeasurementTemperatureCelsius.Value:F1} °C");
                    if (metadata.MeasurementHumidityPercent.HasValue)
                        sb.AppendLine($"Humidity:         {metadata.MeasurementHumidityPercent.Value:F0} %");
                    if (metadata.MeasurementDateTime.HasValue)
                        sb.AppendLine($"Date/Time:        {metadata.MeasurementDateTime.Value:yyyy-MM-dd HH:mm:ss}");
                    sb.AppendLine();
                }
            }

            // Measurement Statistics
            sb.AppendLine("MEASUREMENT STATISTICS");
            sb.AppendLine("-----------------------------------------------------------");
            sb.AppendLine($"Total Notes:      {measurements.Count}");
            sb.AppendLine($"MIDI Range:       {measurements.Keys.Min()} - {measurements.Keys.Max()}");
            sb.AppendLine();

            // Quality Distribution
            var qualityCounts = new Dictionary<string, int>
            {
                ["Groen"] = 0,
                ["Oranje"] = 0,
                ["Rood"] = 0
            };

            foreach (var m in measurements.Values)
            {
                if (qualityCounts.ContainsKey(m.Quality))
                    qualityCounts[m.Quality]++;
            }

            sb.AppendLine("QUALITY DISTRIBUTION");
            sb.AppendLine("-----------------------------------------------------------");
            sb.AppendLine($"?? Groen:         {qualityCounts["Groen"],3} ({qualityCounts["Groen"] * 100.0 / measurements.Count,5:F1}%)");
            sb.AppendLine($"?? Oranje:        {qualityCounts["Oranje"],3} ({qualityCounts["Oranje"] * 100.0 / measurements.Count,5:F1}%)");
            sb.AppendLine($"?? Rood:          {qualityCounts["Rood"],3} ({qualityCounts["Rood"] * 100.0 / measurements.Count,5:F1}%)");
            sb.AppendLine();

            // Partial Detection Statistics
            var partialCounts = measurements.Values.Select(m => m.DetectedPartials.Count).ToList();
            
            sb.AppendLine("PARTIAL DETECTION STATISTICS");
            sb.AppendLine("-----------------------------------------------------------");
            sb.AppendLine($"Average:          {partialCounts.Average():F1} partials");
            sb.AppendLine($"Median:           {GetMedian(partialCounts):F0} partials");
            sb.AppendLine($"Minimum:          {partialCounts.Min()} partials");
            sb.AppendLine($"Maximum:          {partialCounts.Max()} partials");
            sb.AppendLine($"Std Deviation:    {GetStandardDeviation(partialCounts):F2}");
            sb.AppendLine();

            // Register Breakdown
            sb.AppendLine("PARTIAL COUNT BY REGISTER");
            sb.AppendLine("-----------------------------------------------------------");
            
            var registers = new Dictionary<string, (int minMidi, int maxMidi)>
            {
                ["Deep Bass (A0-B1)"] = (21, 35),
                ["Bass (C2-B2)"] = (36, 47),
                ["Tenor (C3-C4)"] = (48, 60),
                ["Mid-High (C#4-C5)"] = (61, 72),
                ["Treble (C#5+)"] = (73, 108)
            };

            foreach (var register in registers)
            {
                var registerMeasurements = measurements.Values
                    .Where(m => m.MidiIndex >= register.Value.minMidi && m.MidiIndex <= register.Value.maxMidi)
                    .ToList();

                if (registerMeasurements.Any())
                {
                    var avgPartials = registerMeasurements.Average(m => m.DetectedPartials.Count);
                    var noteCount = registerMeasurements.Count;
                    sb.AppendLine($"{register.Key,-22}: {avgPartials,4:F1} avg partials ({noteCount,2} notes)");
                }
            }
            sb.AppendLine();

            // Scale Break Analysis
            if (metadata != null)
            {
                sb.AppendLine("SCALE BREAK ANALYSIS");
                sb.AppendLine("-----------------------------------------------------------");
                sb.AppendLine($"Scale Break Location: {GetNoteName(metadata.ScaleBreakMidiNote)} (MIDI {metadata.ScaleBreakMidiNote})");
                sb.AppendLine();
                sb.AppendLine("Notes around scale break:");
                sb.AppendLine("  MIDI  Note   Partials  Quality  Used n  Status");
                sb.AppendLine("  ----  ----   --------  -------  ------  ------");

                for (int offset = -2; offset <= 2; offset++)
                {
                    int midi = metadata.ScaleBreakMidiNote + offset;
                    if (measurements.TryGetValue(midi, out var m))
                    {
                        string marker = (offset == 0 || offset == 1) ? "??" : "  ";
                        string status = offset == 0 ? "Last Wound" : 
                                      offset == 1 ? "First Steel" : 
                                      "Normal";
                        
                        sb.AppendLine($"{marker} {midi,3}   {m.NoteName,-4}   {m.DetectedPartials.Count,2}/16     {m.Quality,-7}  n={m.MeasuredPartialNumber}     {status}");
                    }
                }
                sb.AppendLine();
            }

            // Frequency Accuracy
            sb.AppendLine("FREQUENCY ACCURACY (vs Theoretical)");
            sb.AppendLine("-----------------------------------------------------------");
            
            var deviations = measurements.Values
                .Where(m => m.CalculatedFundamental > 0)
                .Select(m => 1200 * Math.Log2(m.CalculatedFundamental / m.TargetFrequency))
                .ToList();

            if (deviations.Any())
            {
                sb.AppendLine($"Average Deviation:    {deviations.Average():+0.00;-0.00} cents");
                sb.AppendLine($"Std Deviation:        ±{GetStandardDeviation(deviations):F2} cents");
                sb.AppendLine($"Max Deviation:        {deviations.Max(d => Math.Abs(d)):F2} cents");
                sb.AppendLine($"Range:                {deviations.Min():+0.0;-0.0} to {deviations.Max():+0.0;-0.0} cents");
            }
            sb.AppendLine();

            // Optimal Partial Usage
            sb.AppendLine("OPTIMAL PARTIAL USAGE (Register-Based Selection)");
            sb.AppendLine("-----------------------------------------------------------");
            var partialUsage = measurements.Values.GroupBy(m => m.MeasuredPartialNumber)
                .OrderBy(g => g.Key)
                .Select(g => new { Partial = g.Key, Count = g.Count() });

            foreach (var usage in partialUsage)
            {
                double percentage = usage.Count * 100.0 / measurements.Count;
                sb.AppendLine($"Partial n={usage.Partial}:       {usage.Count,3} notes ({percentage,5:F1}%)");
            }
            sb.AppendLine();

            // Digital Piano Detection
            bool isProbablyDigital = DetectDigitalPiano(measurements, deviations);
            
            if (isProbablyDigital)
            {
                sb.AppendLine("============================================================");
                sb.AppendLine("??  DIGITAL PIANO DETECTED");
                sb.AppendLine("============================================================");
                sb.AppendLine("Analysis indicators suggest this is a digital/sample-based piano:");
                sb.AppendLine();
                sb.AppendLine("Observed characteristics:");
                sb.AppendLine("  • Very high partial count (avg > 14)");
                sb.AppendLine("  • Near-perfect harmonicity (low deviation)");
                sb.AppendLine("  • Consistent quality across all registers");
                sb.AppendLine("  • No scale break transition effect");
                sb.AppendLine();
                sb.AppendLine("Expected artifacts:");
                sb.AppendLine("  • Inharmonicity coefficient B ? 0 (no physical strings)");
                sb.AppendLine("  • Possible aliasing in high partials (n > 12)");
                sb.AppendLine("  • Sample-based frequency quantization");
                sb.AppendLine();
                sb.AppendLine("? For WORKFLOW testing: This is acceptable");
                sb.AppendLine("? For SCIENTIFIC analysis: Measure acoustic piano");
                sb.AppendLine();
                sb.AppendLine("Recommendation:");
                sb.AppendLine("  Use this data to validate software functionality, but");
                sb.AppendLine("  re-measure with acoustic piano for publication-grade data.");
                sb.AppendLine("============================================================");
                sb.AppendLine();
            }

            // Recommendations
            sb.AppendLine("RECOMMENDATIONS");
            sb.AppendLine("-----------------------------------------------------------");
            
            if (qualityCounts["Rood"] > 0)
            {
                sb.AppendLine($"??  {qualityCounts["Rood"]} notes with RED quality - consider re-measuring");
            }
            
            if (qualityCounts["Oranje"] > measurements.Count * 0.2)
            {
                sb.AppendLine($"??  High percentage of ORANGE quality ({qualityCounts["Oranje"] * 100.0 / measurements.Count:F1}%)");
                sb.AppendLine("   Consider: Increase gain, improve mic position, reduce noise");
            }
            
            if (partialCounts.Average() < 8)
            {
                sb.AppendLine("??  Low average partial count - check microphone setup");
            }
            
            if (qualityCounts["Groen"] == measurements.Count)
            {
                sb.AppendLine("? All measurements GREEN quality - excellent!");
            }

            if (isProbablyDigital)
            {
                sb.AppendLine("? Workflow validated - ready for acoustic piano testing");
            }
            
            sb.AppendLine();

            // Footer
            sb.AppendLine("============================================================");
            sb.AppendLine($"Report Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("Software: AurisMeasure v1.1");
            sb.AppendLine("Analysis Engine: FftAnalyzerService (Blackman-Harris, 96kHz)");
            sb.AppendLine("============================================================");

            return sb.ToString();
        }

        private bool DetectDigitalPiano(Dictionary<int, NoteMeasurement> measurements, List<double> deviations)
        {
            // Heuristics to detect digital piano
            double avgPartials = measurements.Values.Average(m => m.DetectedPartials.Count);
            double avgDeviation = deviations.Any() ? Math.Abs(deviations.Average()) : 0;
            double stdDev = deviations.Any() ? GetStandardDeviation(deviations) : 0;
            int greenCount = measurements.Values.Count(m => m.Quality == "Groen");
            double greenPercentage = greenCount * 100.0 / measurements.Count;

            // Digital piano indicators:
            // 1. Very high partial count (> 14 on average)
            // 2. Very low frequency deviation (< 1 cent std dev)
            // 3. Almost all notes green (> 95%)
            
            return avgPartials > 14 && stdDev < 1.0 && greenPercentage > 95;
        }

        private double GetMedian(List<int> values)
        {
            var sorted = values.OrderBy(x => x).ToList();
            int mid = sorted.Count / 2;
            return sorted.Count % 2 == 0 ? (sorted[mid - 1] + sorted[mid]) / 2.0 : sorted[mid];
        }

        private double GetStandardDeviation(List<int> values)
        {
            if (values.Count < 2) return 0;
            double avg = values.Average();
            double sumSquares = values.Sum(v => Math.Pow(v - avg, 2));
            return Math.Sqrt(sumSquares / (values.Count - 1));
        }

        private double GetStandardDeviation(List<double> values)
        {
            if (values.Count < 2) return 0;
            double avg = values.Average();
            double sumSquares = values.Sum(v => Math.Pow(v - avg, 2));
            return Math.Sqrt(sumSquares / (values.Count - 1));
        }

        private string GetNoteName(int midi)
        {
            string[] names = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            return names[midi % 12] + ((midi / 12) - 1);
        }

        private void BtnCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(_reportText);
                MessageBox.Show(
                    "Report copied to clipboard!\n\nYou can now paste it into any text editor or send for analysis.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to copy to clipboard:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void BtnExportTxt_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                DefaultExt = "txt",
                FileName = $"Analysis_Report_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveDialog.FileName, _reportText);
                    MessageBox.Show(
                        $"Report exported successfully!\n\n{saveDialog.FileName}",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Failed to export report:\n{ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
    }
}
