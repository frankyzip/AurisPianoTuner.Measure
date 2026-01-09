# Analysis Report Feature - Quick Guide

## Datum: 9 januari 2026

## Overzicht

De **Analysis Report** feature genereert een gedetailleerd statistisch rapport van je metingen. Perfect voor:
- ? Workflow validatie (digitale piano test)
- ? Kopiëren naar clipboard voor externe analyse
- ? Export naar tekstbestand
- ? Detectie van digitale piano artifacts

---

## Hoe Te Gebruiken

### Tijdens Test Sessie:

```
1. Meet enkele noten (bijv. 10-20 verspreid over keyboard)
   OF
   Laad een bestaand JSON bestand

2. Klik op "?? Analysis Report" knop (paars, in toolbar)

3. Venster opent met volledig rapport

4. Opties:
   - Lees direct in venster (scrollbaar)
   - Klik "?? Copy to Clipboard" ? Plak in email/chat
   - Klik "?? Export as .TXT" ? Sla op als bestand
```

---

## Rapport Inhoud

Het rapport bevat:

### 1. Piano Informatie
```
PIANO INFORMATION
-----------------------------------------------------------
Manufacturer:     Roland
Model:            FP-30
Type:             Unknown
Dimension:        0 cm
Scale Break:      F2 (MIDI 41)
Serial Number:    N/A

MEASUREMENT CONDITIONS
-----------------------------------------------------------
Temperature:      21.5 °C
Humidity:         48 %
Date/Time:        2026-01-09 13:45:32
```

### 2. Measurement Statistics
```
MEASUREMENT STATISTICS
-----------------------------------------------------------
Total Notes:      88
MIDI Range:       21 - 108
```

### 3. Quality Distribution
```
QUALITY DISTRIBUTION
-----------------------------------------------------------
?? Groen:          85 ( 96.6%)
?? Oranje:          3 (  3.4%)
?? Rood:            0 (  0.0%)
```

### 4. Partial Detection
```
PARTIAL DETECTION STATISTICS
-----------------------------------------------------------
Average:          14.2 partials
Median:           15 partials
Minimum:          10 partials
Maximum:          16 partials
Std Deviation:    1.82
```

### 5. Register Breakdown
```
PARTIAL COUNT BY REGISTER
-----------------------------------------------------------
Deep Bass (A0-B1)     : 12.3 avg partials (15 notes)
Bass (C2-B2)          : 13.8 avg partials (12 notes)
Tenor (C3-C4)         : 14.5 avg partials (13 notes)
Mid-High (C#4-C5)     : 15.1 avg partials (12 notes)
Treble (C#5+)         : 15.8 avg partials (36 notes)
```

### 6. Scale Break Analysis
```
SCALE BREAK ANALYSIS
-----------------------------------------------------------
Scale Break Location: F2 (MIDI 41)

Notes around scale break:
  MIDI  Note   Partials  Quality  Used n  Status
  ----  ----   --------  -------  ------  ------
     39   D#2   14/16     Groen    n=3     Normal
     40   E2    14/16     Groen    n=3     Normal
??   41   F2    14/16     Groen    n=3     Last Wound
??   42   F#2   15/16     Groen    n=2     First Steel
     43   G2    15/16     Groen    n=2     Normal
```

### 7. Frequency Accuracy
```
FREQUENCY ACCURACY (vs Theoretical)
-----------------------------------------------------------
Average Deviation:    -0.12 cents
Std Deviation:        ±0.85 cents
Max Deviation:        2.31 cents
Range:                -2.1 to +2.3 cents
```

### 8. Optimal Partial Usage
```
OPTIMAL PARTIAL USAGE (Register-Based Selection)
-----------------------------------------------------------
Partial n=1:        36 notes ( 40.9%)
Partial n=2:        25 notes ( 28.4%)
Partial n=3:        12 notes ( 13.6%)
Partial n=6:        15 notes ( 17.0%)
```

### 9. Digital Piano Detection ??
```
============================================================
??  DIGITAL PIANO DETECTED
============================================================
Analysis indicators suggest this is a digital/sample-based piano:

Observed characteristics:
  • Very high partial count (avg > 14)
  • Near-perfect harmonicity (low deviation)
  • Consistent quality across all registers
  • No scale break transition effect

Expected artifacts:
  • Inharmonicity coefficient B ? 0 (no physical strings)
  • Possible aliasing in high partials (n > 12)
  • Sample-based frequency quantization

? For WORKFLOW testing: This is acceptable
? For SCIENTIFIC analysis: Measure acoustic piano

Recommendation:
  Use this data to validate software functionality, but
  re-measure with acoustic piano for publication-grade data.
============================================================
```

### 10. Recommendations
```
RECOMMENDATIONS
-----------------------------------------------------------
? All measurements GREEN quality - excellent!
? Workflow validated - ready for acoustic piano testing
```

---

## Kopiëren Voor Analyse

### Stap 1: Genereer Rapport
```
Klik "?? Analysis Report" in AurisMeasure
```

### Stap 2: Kopieer
```
Klik "?? Copy to Clipboard" in rapport venster
```

### Stap 3: Plak in Chat/Email
```
[Plak hier] Ctrl+V

Stuur naar:
- GitHub Copilot Chat voor analyse
- Email naar collega's
- Discord/Slack voor discussie
- Tekstbestand voor archivering
```

### Voorbeeld Chat Prompt:
```
Hier is mijn AurisMeasure rapport van een Roland FP-30 digitale piano test:

[GEPLAKT RAPPORT]

Vragen:
1. Is de workflow correct gevalideerd?
2. Welke problemen zie je?
3. Klaar voor akoestische piano test?
```

---

## Export Naar Bestand

### Option 1: Via UI
```
1. Klik "?? Export as .TXT" in rapport venster
2. Kies locatie en bestandsnaam
3. Sla op ? Bestand klaar voor delen
```

### Option 2: Copy-Paste
```
1. Klik "?? Copy to Clipboard"
2. Open Notepad/VSCode/etc.
3. Plak (Ctrl+V)
4. Save As ? .txt bestand
```

---

## Interpretatie Gids

### Voor Digitale Piano Test:

**Verwacht**:
- ? Alle noten groen
- ? 14-16 partials overal
- ? Zeer lage frequency deviation (< 1 cent)
- ? "DIGITAL PIANO DETECTED" waarschuwing

**Betekenis**:
- Software werkt correct
- Audio pipeline functioneel
- FFT algoritme operationeel
- Save/Load workflow gevalideerd

**Volgende Stap**:
- Test op akoestische piano voor échte validatie

---

### Voor Akoestische Piano (Later):

**Verwacht**:
- ?? Meer variatie in partial count (6-16)
- ?? Hogere frequency deviation (1-5 cents)
- ?? Oranje kwaliteit in deep bass
- ? Zichtbare scale break overgang (E3?F3 bij Fazer)

**Wetenschappelijk Interessant**:
```
Scale Break Analysis:
  MIDI  Note   Partials  Quality  Used n  Status
  ----  ----   --------  -------  ------  ------
     50   D3    11/16     Groen    n=2     Normal
     51   D#3   11/16     Groen    n=2     Normal
??   52   E3    11/16     Groen    n=2     Last Wound (Copper)
??   53   F3    14/16     Groen    n=2     First Steel ? JUMP!
     54   F#3   15/16     Groen    n=2     Normal
     55   G3    15/16     Groen    n=2     Normal
```

**Dit is publicatie-waardige data!**

---

## Troubleshooting

### "Geen metingen beschikbaar voor analyse"
**Oorzaak**: Geen noten gemeten of geladen  
**Oplossing**: Meet minimaal 1 noot OF laad JSON bestand

### "Failed to copy to clipboard"
**Oorzaak**: Clipboard in gebruik door andere applicatie  
**Oplossing**: Sluit andere apps die clipboard gebruiken, probeer opnieuw

### Rapport toont "Unknown" voor piano info
**Oorzaak**: Metadata niet ingevuld  
**Oplossing**: Vul metadata in vóór meten (optioneel voor test)

---

## Wetenschappelijke Validatie Checklist

### Voor Jouw Copilot Chat Analyse:

Kopieer rapport en vraag:

```
? Is partial count distributie realistisch?
? Is frequency accuracy binnen literatuur waarden?
? Is register-based partial selection correct toegepast?
? Zijn er red flags voor akoestische piano test?
? Welke aanpassingen nodig voor Fazer Console 107cm?
```

---

## Later: Inactief Maken

Als je de Analysis Report later wilt verbergen/disablen:

### Optie 1: UI Hide
```csharp
// MainWindow.xaml.cs
BtnAnalyzeReport.Visibility = Visibility.Collapsed;
```

### Optie 2: Comment Out
```csharp
// MainWindow.xaml
<!-- <Button x:Name="BtnAnalyzeReport" ... /> -->
```

### Optie 3: Conditional Compilation
```csharp
#if DEBUG
    BtnAnalyzeReport.Visibility = Visibility.Visible;
#else
    BtnAnalyzeReport.Visibility = Visibility.Collapsed;
#endif
```

---

## Voorbeeld Workflow: Test Sessie Vandaag

```
1. Start AurisMeasure
2. Selecteer ASIO driver (Roland FP-30)
3. Vul optionele metadata in (temp/humidity)
4. Meet 10 noten verspreid over keyboard:
   - A0, C2, E3, F3, A4, C5, C7, etc.
5. Klik "?? Analysis Report"
6. Bekijk rapport:
   - Check: Alle groen?
   - Check: 14-16 partials?
   - Check: "DIGITAL PIANO DETECTED"?
7. Klik "?? Copy to Clipboard"
8. Plak in chat/email naar mij
9. Ik analyseer en geef feedback
10. Klaar voor akoestische piano test!
```

---

## Conclusie

**Analysis Report** is jouw validatie tool voor:
- ? Workflow testing (nu, digitale piano)
- ? Quality assurance (later, akoestische piano)
- ? Scientific documentation (publicatie)
- ? Troubleshooting (debug data)

**Nu kun je testen en mij direct feedback geven via copy-paste!** ????

---

**Versie**: 1.0  
**Datum**: 9 januari 2026  
**Status**: Production Ready  
**Feature**: Copy-to-Clipboard + Export
