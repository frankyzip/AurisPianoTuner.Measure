using System.Collections.Generic;
using AurisPianoTuner.Measure.Models;

namespace AurisPianoTuner.Measure.Services
{
    public interface IMeasurementStorageService
    {
        void SaveMeasurements(string filePath, Dictionary<int, NoteMeasurement> measurements, PianoMetadata? pianoMetadata = null);
        (Dictionary<int, NoteMeasurement> measurements, PianoMetadata? metadata) LoadMeasurements(string filePath);
    }
}
