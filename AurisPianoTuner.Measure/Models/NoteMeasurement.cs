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
        
        /// <summary>
        /// De partial die gebruikt is voor de meting (register-afhankelijk).
        /// Bijvoorbeeld: n=1 voor hoge noten, n=6 voor lage basnoten.
        /// </summary>
        public int MeasuredPartialNumber { get; set; } = 1;
        
        /// <summary>
        /// De werkelijke fundamentele frequentie, berekend uit de gemeten partial.
        /// Formula: TrueFundamental = MeasuredPartial.Frequency / MeasuredPartialNumber
        /// </summary>
        public double CalculatedFundamental { get; set; }
    }

    public class PartialResult
    {
        public int n { get; set; } // Partieel nummer
        public double Frequency { get; set; }
        public double Amplitude { get; set; }
    }
}
