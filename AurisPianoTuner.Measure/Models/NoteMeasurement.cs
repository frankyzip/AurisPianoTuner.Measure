using System.Collections.Generic;

namespace AurisPianoTuner.Measure.Models
{
    public class NoteMeasurement
    {
        public string NoteName { get; set; } = string.Empty;
        public int MidiIndex { get; set; }
        public double TargetFrequency { get; set; }
        public List<PartialResult> DetectedPartials { get; set; } = new();
        public string Quality { get; set; } = string.Empty; // Groen, Oranje, Rood
    }

    public class PartialResult
    {
        public int n { get; set; } // Partieel nummer
        public double Frequency { get; set; }
        public double Amplitude { get; set; }
    }
}
