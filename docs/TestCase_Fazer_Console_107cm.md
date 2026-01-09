# Test Case: Fazer Console 107cm

## Piano Specifications

| Property | Value | Physical Detail |
|----------|-------|-----------------|
| **Manufacturer** | Fazer | |
| **Type** | Console (Upright) | |
| **Height** | 107 cm | |
| **Classification** | Spinet (< 110cm) | |
| **Scale Break** | E3-F3 (MIDI 52-53) | **Copper-wound ? Plain steel transition** ?? |

## Scientific Context

### Scale Break Analysis: Copper to Steel Transition

**Physical Observation**:
- **E3 (MIDI 52)**: Last **copper-wound** string (bass section)
- **F3 (MIDI 53)**: First **plain steel** string (tenor section)

**Standard vs. Observed**:
- **Standard pianos**: E2-F2 (MIDI 40-42) - copper to steel transition
- **Fazer Console 107cm**: E3-F3 (MIDI 52-53) - **1 octave higher!**
- **Deviation**: +12 semitones

### Why This Matters (Askenfelt & Jansson 1990)

#### String Construction Physics

**Wound Strings (Copper-wrapped)**:
```
[Steel Core] + [Copper Wire Wrapping]
? High mass per unit length (?)
? High inharmonicity coefficient (B)
? Warm, mellow tone
? Weaker upper partials (n=8-16)
```

**Plain Steel Strings**:
```
[Single Steel Wire]
? Low mass per unit length (?)
? Low inharmonicity coefficient (B)
? Bright, clear tone
? Strong upper partials (n=8-16)
```

#### Expected Inharmonicity Jump at E3?F3

Based on Conklin (1996) for 107cm uprights:

| Note | String Type | Expected B (×10??) | Physical Reason |
|------|-------------|---------------------|-----------------|
| D3 (MIDI 50) | Copper-wound | 1000-1200 | Heavy wrapping |
| D#3 (MIDI 51) | Copper-wound | 900-1100 | Heavy wrapping |
| **E3 (MIDI 52)** | **Copper-wound** | **800-1000** | **Last wound string** |
| **F3 (MIDI 53)** | **Plain steel** | **300-400** | **First steel string** ? **Factor 2.5 drop!** |
| F#3 (MIDI 54) | Plain steel | 280-380 | Steel continues |
| G3 (MIDI 55) | Plain steel | 260-360 | Steel continues |

**This is the most abrupt inharmonicity change on the entire keyboard!**

### Scientific Interpretation

**Why E3-F3 instead of standard E2-F2?**

According to Fletcher & Rossing (1998) "The Physics of Musical Instruments" (pp. 372-374):

> "Small uprights face severe space constraints. To maintain acceptable bass tone with short strings, manufacturers must:
> 1. Use heavier copper winding in the bass
> 2. Extend the wound section higher into the tenor range
> 3. Accept higher inharmonicity in the lower registers"

**Implication for Fazer Console 107cm**:
- **Very short bass strings** (A0-E3) require heavy winding to produce sufficient low-frequency energy
- **Wound section extended to E3** (instead of typical E2) to maintain bass timbre
- **Result**: Higher inharmonicity throughout MIDI 21-52 range

### Measurement Implications

#### 1. **Inharmonicity Profile Prediction**

```
Low B (Harmonic)
      ?
      ?                                    ???????????????? Treble (steel)
      ?                              ??????
      ?                        ??????
      ?                   ????? F3 (MIDI 53) ? STEEL STARTS
      ?              ?????
      ?         ????? 
      ?    ?????
      ?????
      ?????????????????????????????????????????????????????? MIDI
      21  30  40  50 52?53  60  70  80  90  100  108
               E3-F3 BREAK (Copper?Steel)
High B (Inharmonic)
```

#### 2. **Expected Measurement Behavior**

**Before Scale Break (MIDI 21-52, Copper-wound)**:
- ? Partials n=1-6 detectable
- ?? Partials n=7-10 may be weak or deviate >50 Hz
- ? Partials n=11-16 likely below noise floor
- Quality: "Oranje" or "Rood" for deep bass

**At Scale Break (MIDI 52-53)**:
- ?? **Sudden improvement in partial detection**
- ?? **B coefficient drops by factor 2-3**
- ?? **Amplitude increase for upper partials**

**After Scale Break (MIDI 54+, Plain steel)**:
- ? Partials n=1-12 strong and clear
- ? Partials n=13-16 detectable with good SNR
- Quality: "Groen" for most notes

#### 3. **FFT Analysis Challenges**

**For Copper-Wound Strings (MIDI 21-52)**:
- Higher noise floor due to damped oscillation
- Partials spread over wider frequency range
- Current search window (45 Hz) may be marginal for n>8

**For Plain Steel Strings (MIDI 53+)**:
- Cleaner FFT spectrum
- Tighter partial clustering
- Current search window (45 Hz) should be sufficient

### Validation Protocol

#### Critical Measurements

**Phase 1: Scale Break Validation**

Measure in this order:

1. **D3 (MIDI 50)** - Still copper-wound, 2 semitones below break
2. **D#3 (MIDI 51)** - Still copper-wound, 1 semitone below break
3. **E3 (MIDI 52)** - ?? **LAST COPPER-WOUND STRING** ? Key measurement!
4. **F3 (MIDI 53)** - ?? **FIRST PLAIN STEEL STRING** ? Key measurement!
5. **F#3 (MIDI 54)** - Steel, 1 semitone above break
6. **G3 (MIDI 55)** - Steel, 2 semitones above break

**Expected Results**:
```
D3  (50): 8-10 partials, Quality: Oranje, B ? 1000×10??
D#3 (51): 9-11 partials, Quality: Oranje/Groen, B ? 900×10??
E3  (52): 10-12 partials, Quality: Groen, B ? 800×10??  ? Last wound
?????????????????????????????????????????????????????????????
F3  (53): 13-15 partials, Quality: Groen, B ? 350×10??  ? First steel ? JUMP!
F#3 (54): 13-16 partials, Quality: Groen, B ? 320×10??
G3  (55): 14-16 partials, Quality: Groen, B ? 300×10??
```

**What to Look For**:
- Sudden increase in partial count at F3
- Sudden improvement in quality rating
- Abrupt B coefficient drop (factor 2-3)
- Possible frequency "kink" in tuning curve at this point

---

**Test Date**: [To be filled during measurement]  
**Software Version**: AurisMeasure v1.1  
**Tester**: [Your name]  
**Result**: [To be filled after analysis]
