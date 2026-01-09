# GitHub Copilot Instructions for AurisPianoTuner.Measure

## Project Overview
**AurisPianoTuner.Measure** is a professional-grade piano tuning measurement application built with .NET 10 and WPF. The software performs high-precision FFT analysis of acoustic piano strings to measure inharmonicity and assist in professional piano tuning.

---

## Scientific Standards & Source Requirements

### ? REQUIRED: Use Only Credible Scientific Sources

When providing advice, implementing algorithms, or making technical decisions, **ALWAYS** base recommendations on:

1. **Peer-Reviewed Academic Literature**
   - Journal papers (e.g., Journal of the Acoustical Society of America)
   - Conference proceedings from reputable institutions
   - University research publications

2. **Established Textbooks & Reference Works**
   - Acoustics textbooks
   - Signal processing references
   - Music technology standards

3. **Industry Standards & Specifications**
   - ISO standards
   - ASIO documentation
   - Audio engineering best practices

4. **Open Source Academic Projects**
   - Entropy Piano Tuner (peer-reviewed)
   - Academic DSP libraries
   - University research codebases

### ? FORBIDDEN: Do NOT Use

- Forums (Reddit, Stack Overflow, piano forums, etc.)
- Blog posts without scientific backing
- YouTube videos or tutorials
- Social media discussions
- Unverified "tips and tricks" websites
- Marketing materials or product claims

### ?? Key Scientific References for This Project

**Piano Acoustics:**
- Askenfelt, A. & Jansson, E. (1990). "Five Lectures on the Acoustics of the Piano". Royal Swedish Academy of Music.
- Conklin, H.A. (1996). "Design and Tone in the Mechanoacoustic Piano". Journal of the Acoustical Society of America.
- Fletcher, N.H. & Rossing, T.D. (1998). "The Physics of Musical Instruments". Springer.

**Signal Processing:**
- Oppenheim, A.V. & Schafer, R.W. (2010). "Discrete-Time Signal Processing". Pearson.
- Smith, J.O. (2011). "Spectral Audio Signal Processing". W3K Publishing.

**Piano Tuning Theory:**
- Barbour, J.M. (1943). "Piano Tuning: A Simple and Accurate Method". Michigan State University.
- Railsback, O.L. (1938). "Scale Temperament as Applied to Piano Tuning". Journal of the Acoustical Society of America.

---

## Technical Architecture

### Core Components

**1. FFT Analysis (`FftAnalyzerService.cs`)**
- Uses MathNet.Numerics for FFT computation
- 32,768 sample FFT window (96 kHz sample rate)
- Blackman-Harris 4-term window function
- Parabolic interpolation for 0.01 Hz precision
- **Register-based partial selection** (scientific literature-backed)

**2. Register-Based Measurement Strategy**
```
Deep Bass (MIDI 21-35):  Use partial n=6-8  (Askenfelt & Jansson 1990)
Bass (MIDI 36-47):       Use partial n=3-4  (Barbour 1943)
Tenor (MIDI 48-60):      Use partial n=2    (Conklin 1996)
Mid-High (MIDI 61-72):   Use partial n=1    (Common practice)
Treble (MIDI 73+):       Use partial n=1    (Common practice)
```

**3. Audio Input (`AsioAudioService.cs`)**
- ASIO driver support for low-latency audio
- 96 kHz sample rate (professional standard)
- Real-time buffer management

**4. Data Storage (`MeasurementStorageService.cs`)**
- JSON format for measurement archives
- Includes all 16 partials per note
- Version tracking for backward compatibility

---

## Code Style Guidelines

### General Principles
1. **Scientific Accuracy First**: Prioritize correctness over convenience
2. **Document Algorithms**: Include references to papers/books in comments
3. **Minimal Comments**: Code should be self-explanatory; only comment complex DSP logic
4. **Consistent Naming**: Use `_camelCase` for private fields, `PascalCase` for public

### Example of Proper Documentation
```csharp
/// <summary>
/// Determines optimal partial for measurement based on piano register.
/// Based on scientific literature:
/// - Askenfelt & Jansson (1990): Deep bass (MIDI 21-35) ? n=6-8
/// - Barbour (1943): Bass (MIDI 36-47) ? n=3-4
/// - Conklin (1996): Tenor (MIDI 48-60) ? n=2
/// </summary>
private int GetOptimalPartialForRegister(int midiIndex) { ... }
```

---

## Workflow & Testing

### Development Environment
- **Target Framework**: .NET 10
- **UI Framework**: WPF (Windows Presentation Foundation)
- **Audio Backend**: ASIO (via NAudio.Asio wrapper)
- **DSP Library**: MathNet.Numerics

### Testing Strategy
1. **Current Testing**: Roland FP-30 digital piano (development phase)
   - Understand limitations: sample-based audio, possible aliasing
   - Document digital artifacts vs. real piano behavior
   
2. **Target Environment**: Acoustic grand/upright pianos
   - Physical string inharmonicity
   - Real-world tuning stability
   - Professional tuner validation

### Quality Standards
- **Frequency Precision**: 0.01 Hz (via parabolic interpolation)
- **Measurement Quality**: Green (>5 partials), Orange (3-5), Red (<3)
- **Sample Averaging**: 10 FFT analyses per note for stability

---

## Key Algorithms & Formulas

### 1. MIDI to Frequency Conversion
```
f = 440 × 2^((n-69)/12)
where n = MIDI note number, 69 = A4
```

### 2. Inharmonicity Coefficient (Piano Strings)
```
f_n = n·f?·?(1 + B·n²)
where B = inharmonicity coefficient, n = partial number
```

### 3. Cent Deviation
```
cents = 1200 × log?(f_measured / f_target)
```

### 4. Parabolic Peak Interpolation
```
p = 0.5 × (? - ?) / (? - 2? + ?)
where ?, ?, ? = log-magnitude of adjacent bins
```

---

## Common Patterns & Anti-Patterns

### ? DO
- Base all DSP decisions on published papers
- Cite sources in code comments for complex algorithms
- Use scientifically validated window functions (Blackman-Harris)
- Implement register-based partial selection
- Store complete partial data (all 16 harmonics)

### ? DON'T
- Implement "hacks" without theoretical justification
- Use arbitrary thresholds without scientific basis
- Copy code from forums without understanding
- Ignore inharmonicity in piano strings
- Use hardcoded magic numbers without explanation

---

## Future Development Guidelines

### Planned Features
1. **Inharmonicity Curve Fitting** (Conklin 1996 model)
2. **Automatic Tuning Curve Generation** (Railsback stretch)
3. **Professional Tuner Comparison Mode**
4. **Export to CSV/Excel** for analysis

### When Adding New Features
1. Research scientific literature first
2. Document theoretical basis in code comments
3. Validate with acoustic piano (not digital)
4. Compare with professional tuning software if possible

---

## Response Format for AI Assistant

When answering questions or suggesting changes:

1. **State Theoretical Basis**
   - "According to [Author, Year]..."
   - "Based on [Journal/Book]..."

2. **Provide Source Details**
   - Full citation (author, year, publication)
   - Page numbers or section references if applicable

3. **Explain Trade-offs**
   - Why this approach vs. alternatives
   - Scientific justification for chosen method

4. **Implementation Guidance**
   - Show code with scientific comments
   - Explain formulas step-by-step

---

## Example Interaction Pattern

**? Bad Response:**
"You should use a Hann window because it's commonly used."

**? Good Response:**
"Based on Harris (1978) 'On the Use of Windows for Harmonic Analysis with the Discrete Fourier Transform', the Blackman-Harris 4-term window offers superior sidelobe suppression (-92 dB) compared to Hann (-31 dB), which is critical for accurate partial detection in piano strings where adjacent harmonics may have large amplitude differences. The current implementation correctly uses Blackman-Harris."

---

## Contact & Collaboration

- **Project Type**: Professional piano tuning measurement software
- **Target Users**: Piano technicians, tuners, researchers
- **Quality Standard**: Publication-ready accuracy

**Remember:** This software will be used for real-world piano tuning. Accuracy and scientific validity are paramount.

---

## Version History
- v1.0 (2026-01-09): Initial implementation with register-based partial selection
- Tested with Roland FP-30 (digital piano) during development
- Awaiting acoustic piano validation

---

**Last Updated**: January 9, 2026  
**Maintained By**: Development Team  
**Primary Reference**: Scientific literature in acoustics and signal processing
