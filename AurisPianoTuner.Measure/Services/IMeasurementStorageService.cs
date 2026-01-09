using System.Collections.Generic;
using AurisPianoTuner.Measure.Models;

namespace AurisPianoTuner.Measure.Services
{
    public interface IMeasurementStorageService
    {
        void SaveMeasurements(string filePath, Dictionary<int, NoteMeasurement> measurements);
        Dictionary<int, NoteMeasurement> LoadMeasurements(string filePath);
    }
}
