# Piano Metadata Implementation - Scientific Documentation

## Overview
This document explains the scientific basis and implementation of piano metadata collection in AurisMeasure, based on peer-reviewed acoustical research.

## Scientific Rationale

### 1. Inharmonicity Coefficient (B) Variation

**Reference**: Conklin, H.A. (1996). "Design and Tone in the Mechanoacoustic Piano". Journal of the Acoustical Society of America, 100(2), 695-708.

The inharmonicity coefficient B describes how much piano string partials deviate from perfect harmonic ratios:

```
f_n = n·f?·?(1 + B·n²)
```

Where:
- f_n = frequency of partial n
- f? = fundamental frequency
- B = inharmonicity coefficient (typically 10?? to 10?³)

**Key Finding**: B varies dramatically with piano size:
- Concert Grands (>250cm): B ? 30-150 × 10??
- Baby Grands (150-170cm): B ? 150-400 × 10??
- Professional Uprights (120-135cm): B ? 200-500 × 10??
- Spinets (<110cm): B ? 500-2000 × 10??

**Implication for AurisMeasure**: 
- High-B pianos require wider FFT search windows for upper partials
- Partial n=8 in a spinet may be 50+ Hz sharp compared to 8·f?
- Future tuning curve calculation must account for expected B range

### 2. Scale Break Location

**Reference**: Askenfelt, A. & Jansson, E. (1990). "Five Lectures on the Acoustics of the Piano". Royal Swedish Academy of Music, Publication No. 64.

The scale break (typically E2-F#2, MIDI 40-42) marks the **physical transition from copper-wound bass strings to plain steel tenor/treble strings**.

**Physical Construction**:

**Wound Strings (Below Scale Break)**:
- Steel core wire + copper wire wrapping
- Purpose: Increase mass per unit length without excessive tension
- Result: Lower fundamental frequency for given string length
- Trade-off: Higher inharmonicity coefficient (B)

**Plain Steel Strings (Above Scale Break)**:
- Single steel wire, no wrapping
- Lighter mass per unit length
- Lower inharmonicity coefficient (B)
- Brighter, more harmonic tone

**Key Finding**: The bridge design at this transition often creates an **abrupt discontinuity** in the inharmonicity curve, which can be a "weak spot" in tuning if not carefully measured.

**Quantitative Data from Literature**:

Conklin (1996) measured B coefficients around scale break:
- Last wound string (e.g., E2): B ? 600-1200 × 10??
- First plain steel (e.g., F2): B ? 200-400 × 10??
- **Reduction factor**: 2-4× (abrupt change)

**Implication for AurisMeasure**:
- Metadata allows flagging this critical region for extra attention
- Software logs warnings when measuring within ±2 semitones of scale break
- Future analysis can alert tuners to verify smooth transition
- Tuning curve calculation should use smoothing algorithms around this point

**Non-Standard Scale Breaks**:

Small uprights (< 110cm) often have **higher scale breaks** due to space constraints:
- Standard: E2-F2 (MIDI 40-42)
- Small consoles: May be E3-F3 (MIDI 52-53)
- Reason: Shorter bass strings require extended wound section for adequate bass response

Example: Fazer Console 107cm has empirically observed scale break at E3-F3 (MIDI 52-53).
