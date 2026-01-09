# Piano Metadata User Guide

## Why Piano Metadata Matters

Piano metadata (piano type, dimensions, scale break) significantly affects:
- **Measurement Accuracy**: Different piano sizes have different acoustic characteristics
- **Tuning Curve Calculation**: Small pianos need more "stretch" than concert grands
- **Data Documentation**: Essential for professional piano tuning records

## How to Fill In Piano Metadata

### 1. Piano Type
Select the category that matches your piano:

**Upright Pianos** (vertical strings):
- **Spinet** (< 110cm): Compact upright, usually found in apartments
- **Console/Studio** (110-120cm): Standard home upright
- **Professional Upright** (120-135cm): Large upright, institutional quality

**Grand Pianos** (horizontal strings):
- **Baby Grand** (150-170cm): Smallest grand piano
- **Parlor Grand** (170-210cm): Medium-sized grand
- **Semi-Concert Grand** (210-250cm): Large professional grand
- **Concert Grand** (> 250cm): Full-size concert hall instrument (e.g., Steinway Model D)

### 2. Dimension
**For Uprights**: Measure height from floor to top of the piano (in cm)  
**For Grands**: Measure length from keyboard to tail (in cm)

*Tip*: Most piano manufacturers list this in their specifications.

### 3. Scale Break
This is the MIDI note where the bass strings (copper-wound) transition to tenor strings (plain steel).

**Physical Description**:
- **Below scale break**: Copper wire wrapped around steel core (wound strings)
  - Heavier, warmer tone
  - Higher inharmonicity
- **Above scale break**: Single steel wire (plain strings)
  - Brighter, clearer tone  
  - Lower inharmonicity

**Default**: F2 (MIDI 41) - works for most pianos  
**Range**: E2 to F#3 (MIDI 40-54)

**How to verify physically**:
1. Open the piano lid/panel
2. Look at the strings from left (bass) to right (treble)
3. Find where the appearance changes:
   - **Wound strings**: Copper-colored, thicker, wrapped appearance
   - **Plain steel strings**: Silver/gray, thinner, smooth appearance

**Example observations**:
- Standard grand piano: Transition at E2-F2 (MIDI 40-41)
- Small console (107cm): May be as high as E3-F3 (MIDI 52-53)
- Concert grand (280cm): Typically F2 (MIDI 41)

**Why this matters scientifically** (Askenfelt & Jansson 1990):
- Wound strings have 2-4× higher inharmonicity than plain steel
- The transition point shows an **abrupt change** in acoustic behavior
- This affects tuning curve calculation (sudden "kink" may appear)
- Measurement quality typically **improves dramatically** after the break

**Visual Guide**:
```
Bass Section (Wound)          Scale Break    Tenor/Treble (Plain Steel)
???????????????????????????????????????????????????????????????
Copper-wrapped strings      ? ?  Smooth steel strings
Thick, copper color         ? ?  Thin, silver/gray
High inharmonicity (B)      ? ?  Low inharmonicity (B)
                            ? ?
                            ? ?
                        E2-F3 range
                     (depends on piano)
```

### 4. Manufacturer & Model
Fill in the piano brand and model if known. Examples:
- Manufacturer: Yamaha, Model: U1
- Manufacturer: Steinway & Sons, Model: Model D
- Manufacturer: Kawai, Model: K-300

This helps with future reference and professional documentation.

## When to Update Metadata

- **Before starting measurements**: Set metadata for proper data context
- **When loading old files**: Update metadata if it was unknown before
- **When measuring different pianos**: Always update metadata when switching instruments!

## Metadata in JSON Files

Metadata is saved with your measurements in the JSON file:

```json
{
  "Version": "1.1",
  "PianoMetadata": {
    "Type": "ParlorGrand",
    "DimensionCm": 185,
    "ScaleBreakMidiNote": 41,
    "Manufacturer": "Yamaha",
    "Model": "C3X"
  }
}
```

This ensures that the calculation app (AurisCalculate) has all the information it needs for accurate tuning curve generation.

## Scientific References

- Piano type affects inharmonicity coefficient (Conklin 1996)
- Scale break is acoustically significant (Askenfelt & Jansson 1990)
- Stretch curve depends on piano size (Railsback 1938)

---

**For more technical details, see**: `docs\PianoMetadata_Scientific_Basis.md`
