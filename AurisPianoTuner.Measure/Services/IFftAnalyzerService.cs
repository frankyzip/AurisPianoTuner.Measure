using AurisPianoTuner.Measure.Models;
using System;

namespace AurisPianoTuner.Measure.Services
{
    public interface IFftAnalyzerService
    {
        void ProcessAudioBuffer(float[] samples);
        void SetTargetNote(int midiIndex, double theoreticalFrequency);
        event EventHandler<NoteMeasurement> MeasurementUpdated;
        void Reset();
    }
}
