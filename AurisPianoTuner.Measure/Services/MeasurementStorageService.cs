using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using AurisPianoTuner.Measure.Models;

namespace AurisPianoTuner.Measure.Services
{
    public class MeasurementStorageService : IMeasurementStorageService
    {
        private readonly JsonSerializerOptions _jsonOptions;

        public MeasurementStorageService()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never
            };
        }

        public void SaveMeasurements(string filePath, Dictionary<int, NoteMeasurement> measurements)
        {
            try
            {
                // Converteer naar een serializeerbaar formaat
                var data = new MeasurementFileData
                {
                    Version = "1.0",
                    CreatedAt = DateTime.Now,
                    SampleRate = 96000,
                    FftSize = 32768,
                    Measurements = new List<NoteMeasurement>(measurements.Values)
                };

                string json = JsonSerializer.Serialize(data, _jsonOptions);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Fout bij opslaan metingen: {ex.Message}", ex);
            }
        }

        public Dictionary<int, NoteMeasurement> LoadMeasurements(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Bestand niet gevonden: {filePath}");
                }

                string json = File.ReadAllText(filePath);
                var data = JsonSerializer.Deserialize<MeasurementFileData>(json, _jsonOptions);

                if (data == null || data.Measurements == null)
                {
                    throw new Exception("Ongeldig bestandsformaat");
                }

                // Converteer terug naar dictionary
                var result = new Dictionary<int, NoteMeasurement>();
                foreach (var measurement in data.Measurements)
                {
                    result[measurement.MidiIndex] = measurement;
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Fout bij laden metingen: {ex.Message}", ex);
            }
        }

        private class MeasurementFileData
        {
            public string Version { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public int SampleRate { get; set; }
            public int FftSize { get; set; }
            public List<NoteMeasurement> Measurements { get; set; } = new();
        }
    }
}
