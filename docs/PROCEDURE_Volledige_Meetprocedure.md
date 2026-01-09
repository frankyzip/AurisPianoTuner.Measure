# AurisMeasure - Volledige Meetprocedure

## Overzicht

Dit document beschrijft de **complete meetprocedure** voor het meten van een piano met AurisMeasure, van voorbereiding tot opslag en hergebruik voor toekomstige stemmingen.

---

## Inhoudsopgave

1. [Voorbereiding](#1-voorbereiding)
2. [Hardware Setup](#2-hardware-setup)
3. [Software Configuratie](#3-software-configuratie)
4. [Piano Metadata Invoeren](#4-piano-metadata-invoeren)
5. [Metingen Uitvoeren](#5-metingen-uitvoeren)
6. [Kwaliteitscontrole](#6-kwaliteitscontrole)
7. [Opslaan van Resultaten](#7-opslaan-van-resultaten)
8. [Hergebruik voor Stemming](#8-hergebruik-voor-stemming-later)
9. [Pitch Drift Analyse](#9-pitch-drift-analyse-optioneel)
10. [Troubleshooting](#10-troubleshooting)

---

## 1. Voorbereiding

### 1.1 Benodigde Apparatuur

**Audio Interface**:
- ASIO-compatibele interface (bijv. Behringer UMC202HD)
- Sample rate: 96 kHz (vereist)
- Bit depth: 24-bit aanbevolen

**Microfoon**:
- Meetmicrofoon: Behringer ECM8000 of vergelijkbaar
- Frequentiebereik: 20 Hz - 20 kHz (vlak)
- Gevoeligheid: -60 dB @ 1 kHz

**Overig**:
- Microfoon statief
- XLR kabel
- Phantom power (48V) - vaak ingebouwd in interface
- Laptop/PC met Windows 10/11

### 1.2 Software Installatie

1. Download en installeer **AurisMeasure**
2. Installeer ASIO drivers voor uw audio-interface
3. (Optioneel) Installeer ASIO4ALL als fallback driver

### 1.3 Omgevingsvoorbereiding

**Ruimte**:
- Stille omgeving (<40 dB achtergrondgeluid)
- Sluit deuren en ramen
- Zet airconditioning/verwarming uit tijdens meting
- Geen verkeer of machines in de buurt

**Piano**:
- Open de piano klep (vleugel) of verwijder voorpaneel (buffet)
- Verwijder voorwerpen van de piano
- Zorg dat snaren vrij zijn van stof

**Omgevingscondities** (optioneel maar aanbevolen):
- Meet kamertemperatuur (thermometer)
- Meet luchtvochtigheid (hygrometer)
- Noteer deze waarden voor metadata

---

## 2. Hardware Setup

### 2.1 Microfoon Plaatsing

**Voor Vleugel**:
```
???????????????????????????????????????
?         PIANO VLEUGEL               ?
?  ????????????????????????????       ?
?  ?      Snaren              ?       ?
?  ?                          ?       ?
?  ?         ?? ? 15-20 cm    ?       ?
?  ?            boven snaren  ?       ?
?  ?                          ?       ?
?  ????????????????????????????       ?
?                                     ?
?  Positie: Midden klaviatuur        ?
?  Richting: Naar beneden (snaren)    ?
???????????????????????????????????????
```

**Voor Buffetpiano**:
```
???????????????????????
?   BUFFET PIANO      ?
?                     ?
?   ???????????????   ?
?   ?   Snaren    ?   ?
?   ?             ?   ?
?   ?  ??? 10-15cm?   ?
?   ?             ?   ?
?   ???????????????   ?
?                     ?
?  Positie: Midden    ?
?  Hoogte: Ter hoogte ?
?          C4-A4      ?
???????????????????????
```

**Belangrijke Punten**:
- ? **Niet** te dicht bij snaren (< 5 cm) ? overbelasting
- ? **Niet** te ver weg (> 30 cm) ? te veel ruisgeluid
- ? Vermijd reflecties van piano deksel
- ? Gericht naar midden van resonantieplaat

### 2.2 Audio Interface Aansluiting

1. **Sluit microfoon aan**:
   - XLR kabel in Input 1 van UMC202HD
   - Zet Phantom Power (48V) AAN (rode knop)

2. **Sluit interface aan op PC**:
   - USB kabel naar computer
   - Wacht tot drivers geladen zijn

3. **Stel gain in**:
   - Start met gain knop op 12 uur positie
   - Speel midden C (C4) op piano
   - Clip LED mag NIET branden (rood)
   - Signal LED moet knipperen (groen)
   - Pas gain aan indien nodig

4. **Controleer sample rate**:
   - Open ASIO Control Panel
   - Stel in op **96000 Hz** (96 kHz)
   - Buffer size: 512-1024 samples

---

## 3. Software Configuratie

### 3.1 AurisMeasure Opstarten

1. Start **AurisMeasure.exe**
2. Hoofdvenster opent met:
   - Metadata bar (boven)
   - Piano metadata bar (tweede rij)
   - Note card (links)
   - Spectrum visualizer (rechts)
   - Piano keyboard (onder)
   - START MEASUREMENT knop (rechtsonder)

### 3.2 ASIO Driver Selecteren

**Stap 1**: Klik op dropdown "ASIO Driver"

**Stap 2**: Selecteer uw interface:
```
Opties:
- Behringer UMC ASIO Driver
- Focusrite USB ASIO
- ASIO4ALL v2
- [Andere geïnstalleerde drivers]
```

**Stap 3**: Wacht op bevestiging
```
[AurisMeasure] ASIO Driver geïnitialiseerd: 96000 Hz, 1 kanaal
```

**Stap 4**: START knop wordt **groen** en enabled

### 3.3 Audio Niveau Test

1. Speel een **midden C (C4)** op de piano
2. Kijk naar **Input Level** VU-meter (note card)
3. Controleer dB status:
   - ? **-12 tot -3 dB**: Optimaal (groen/oranje)
   - ?? **-3 tot 0 dB**: Te luid (oranje/rood)
   - ? **> 0 dB**: Clipping! (rood) ? Verlaag gain

4. Pas gain bij indien nodig op audio interface

---

## 4. Piano Metadata Invoeren

### 4.1 Essentiele Metadata (Rij 1)

**Project Naam**:
```
Project: [Fazer_Console_2026_01_09]
```

**Piano Type**:
```
Piano Type: [Spinet (< 110cm) ?]

Keuzes:
- Unknown
- Spinet (< 110cm)
- Console/Studio (110-120cm)
- Professional Upright (120-135cm)
- Baby Grand (150-170cm)
- Parlor Grand (170-210cm)
- Semi-Concert Grand (210-250cm)
- Concert Grand (> 250cm)
```

**Afmetingen**:
```
Dimension: [107] cm

- Voor buffetpiano: Hoogte van vloer tot bovenkant
- Voor vleugel: Lengte van keyboard tot achterkant
```

**Scale Break** (koper ? staal overgang):
```
Scale Break: [E3 (MIDI 52) ?]

Hoe vinden:
1. Open piano
2. Zoek overgang koper (donker) ? staal (zilver)
3. Tel toetsen vanaf A0 (linksonder)
4. Standaard: F2 (MIDI 41)
5. Jouw Fazer: E3 (MIDI 52)
```

**Fabrikant & Model**:
```
Manufacturer: [Fazer]
Model:        [Console]
```

### 4.2 Optionele Metadata (Rij 2)

**Omgevingscondities**:
```
Environment (Optional):
Temp:     [21.5] °C
Humidity: [48] %
```

**Waarom invullen?**:
- ? Verklaart seizoensvariatie in pitch
- ? Valideert drift-analyse later
- ? Professionele documentatie voor klant

**Automatische timestamp**:
```
Last measured: 2026-01-09 14:30
(Wordt automatisch ingevuld bij opslaan)
```

---

## 5. Metingen Uitvoeren

### 5.1 Meetstrategie

**Optie A: Volledige Piano** (Aanbevolen bij eerste meting)
- Meet alle 88 noten (A0 tot C8)
- Tijdsduur: ~45-60 minuten
- Resultaat: Complete inharmonicity dataset

**Optie B: Checkpoint Metingen** (Voor drift-analyse)
- Meet 5-7 specifieke noten
- Tijdsduur: ~5-10 minuten
- Gebruikt voor pitch offset berekening

**Optie C: Problematische Regio's**
- Focus op scale break (E3-F3 bij Fazer)
- Diep bas (A0-B1)
- Tijdsduur: variabel

### 5.2 Meten Per Noot (Standaard Procedure)

**Stap 1: Selecteer Noot**
- Klik op toets in piano keyboard (onderaan scherm)
- Of: Speel noot op fysieke piano ? software detecteert

**Noot Card Updates**:
```
???????????????????????????
?        A4               ?  ? Note naam
?     440.00 Hz           ?  ? Theoretische freq
?     --.--- Hz           ?  ? Gemeten freq (wachtend)
?        ? Gray          ?  ? Quality indicator
?                         ?
?   Input Level           ?
?   ???????????? -8 dB    ?  ? Audio niveau
???????????????????????????
```

**Stap 2: Start Meting**
- Klik **START MEASUREMENT** (wordt rood)
- Wacht 1 seconde

**Stap 3: Speel de Noot**

**Aanslagtechniek**:
- **Mezzo-forte** (matig hard)
- **Niet** te zacht ? zwakke partials
- **Niet** te hard ? clipping
- **Consistent** bij alle noten

**Speel de noot**:
- Druk toets in
- Houd **2-3 seconden** vast
- Laat langzaam los

**Stap 4: Software Analyseert**

Realtime feedback in title bar:
```
AurisMeasure - A4 (1/10 samples) [n=1]
AurisMeasure - A4 (2/10 samples) [n=1]
AurisMeasure - A4 (3/10 samples) [n=1]
...
AurisMeasure - A4 (10/10 samples) [n=1]
AurisMeasure - A4 COMPLETED
```

**Auto-stop na 10 samples** (~5 seconden)

**Stap 5: Resultaat Bekijken**

**Note Card Update**:
```
???????????????????????????
?        A4               ?
?     440.00 Hz           ?  ? Target
?     439.54 Hz           ?  ? Gemeten ?
?        ?? Groen         ?  ? Kwaliteit
???????????????????????????
```

**Spectrum Display (rechts)**:
```
Target: A4 (440.00 Hz)
Quality: Groen
Progress: 10/10
Measured using: Partial n=1

Calculated Fundamental: 439.54 Hz (? -0.46 Hz, -1.8 cent)

Detected Partials:
n=1:  439.55 Hz (? +0.00 Hz) | 47.0 dB ?
n=2:  880.91 Hz (? +1.82 Hz) | 38.7 dB
n=3:  1322.6 Hz (? +2.61 Hz) | 13.7 dB
n=4:  1768.1 Hz (? +8.14 Hz) | 24.7 dB
...
```

**Keyboard Update**:
- Gemeten toets wordt **groen** ??

**Stap 6: Volgende Noot**
- Klik volgende toets
- Herhaal stap 2-5

### 5.3 Kwaliteitsindicatoren

**Groen ??** (>5 partials gedetecteerd):
- ? Uitstekende meting
- ? Alle partials duidelijk zichtbaar
- ? Gebruik voor stemming zonder twijfel

**Oranje ??** (3-5 partials):
- ?? Acceptabel, maar niet optimaal
- ?? Mogelijk ruis of zwakke aanslag
- ?? Overweeg hermeting bij kritieke noten

**Rood ??** (<3 partials):
- ? Onvoldoende data
- ? Hermeten verplicht
- ? Check: gain, microfoon positie, achtergrondgeluid

### 5.4 Register-Specifieke Aandachtspunten

**Deep Bass (A0-B1, MIDI 21-35)**:
```
Verwachting: n=6 gebruikt voor meting
Uitdaging:  - Zwakke fundamentele
            - Hoge inharmonicity
            - Lange decay tijd
Tip:        - Speel forter (harder)
            - Wacht 3-4 seconden
            - Verwacht "Oranje" kwaliteit
```

**Bass (C2-B2, MIDI 36-47)**:
```
Verwachting: n=3 gebruikt voor meting
Uitdaging:  - Matige inharmonicity
            - Koper-omwonden snaren
Tip:        - Normale aanslag
            - 2-3 seconden
            - "Groen" verwacht
```

**Tenor (C3-C4, MIDI 48-60)**:
```
Verwachting: n=2 gebruikt voor meting
Uitdaging:  - Mogelijk scale break in deze regio
Tip:        - Let op waarschuwingen bij E3-F3
            - Consistent aanslaan
            - "Groen" verwacht
```

**Mid-High & Treble (C#4+, MIDI 61-108)**:
```
Verwachting: n=1 (fundamentele) gebruikt
Uitdaging:  - Korte decay
            - Mogelijk zwakke low partials
Tip:        - Normale aanslag
            - 2 seconden voldoende
            - "Groen" gegarandeerd
```

### 5.5 Scale Break Specifieke Procedure

**Voor Fazer Console (E3-F3 break)**:

**Noten om extra aandacht te geven**:
```
D3  (MIDI 50) - 2 semitones voor break
D#3 (MIDI 51) - 1 semitone voor break
E3  (MIDI 52) - LAATSTE KOPER-OMWONDEN ??
F3  (MIDI 53) - EERSTE STAAL ??
F#3 (MIDI 54) - 1 semitone na break
G3  (MIDI 55) - 2 semitones na break
```

**Debug Output** (Visual Studio Output venster):
```
[Spinet] E3 (MIDI 52): 11/16 partials, using n=2, Quality: Groen [NEAR SCALE BREAK]
  ??  Scale break region - expect possible inharmonicity transition

[Spinet] F3 (MIDI 53): 14/16 partials, using n=2, Quality: Groen [NEAR SCALE BREAK]
  ??  Scale break region - expect possible inharmonicity transition
```

**Verwacht**:
- E3: 10-12 partials (koper, hoge B)
- F3: 13-15 partials (staal, lage B) ? Plotselinge verbetering!

---

## 6. Kwaliteitscontrole

### 6.1 Visuele Check During Measurement

**Keyboard Overzicht**:
```
[Piano Keyboard View]
?????????????????????????????????????????????????
????????????????????????????????????????...
A0  C1  E1  A1  C2  E2  A2  C3  E3  F3  A3  C4...
```

**Interpretatie**:
- Groen dominant ? Prima!
- Oranje in deep bass ? Verwacht
- Rood ergens ? Hermeten

### 6.2 Debug Log Analyse

**Open Visual Studio Output venster**:
```
View ? Output ? Show output from: Debug
```

**Voorbeeld log**:
```
[FftAnalyzer] Piano metadata set: Spinet, 107cm, Scale Break: E3
[Spinet] A0 (MIDI 21): 8/16 partials, using n=6, Quality: Oranje
[Spinet] C1 (MIDI 24): 9/16 partials, using n=6, Quality: Oranje
[Spinet] E1 (MIDI 28): 10/16 partials, using n=6, Quality: Groen
[Spinet] A1 (MIDI 33): 11/16 partials, using n=6, Quality: Groen
[Spinet] C2 (MIDI 36): 12/16 partials, using n=3, Quality: Groen
...
[Spinet] E3 (MIDI 52): 11/16 partials, using n=2, Quality: Groen [NEAR SCALE BREAK]
  ??  Scale break region - expect possible inharmonicity transition
[Spinet] F3 (MIDI 53): 14/16 partials, using n=2, Quality: Groen [NEAR SCALE BREAK]
  ??  Scale break region - expect possible inharmonicity transition
...
[Spinet] A4 (MIDI 69): 16/16 partials, using n=1, Quality: Groen
[Spinet] C8 (MIDI 108): 15/16 partials, using n=1, Quality: Groen
```

### 6.3 Statistische Validatie

**Verwachte Partial Count Distribution**:
```
MIDI Range    Expected Partials    Reden
21-35         6-10                 Deep bass, hoge B
36-47         8-12                 Bass, koper-omwonden
48-60         10-14                Tenor, overgang
61-108        12-16                Mid-high/treble, lage B
```

**Alarm Signalen**:
- ? Geen enkele noot >10 partials ? Microfoon/gain probleem
- ? Plotselinge drop in partials (niet bij scale break) ? Stille mechaniek
- ? Alle noten "Rood" ? Geen audio input

---

## 7. Opslaan van Resultaten

### 7.1 Save Procedure

**Stap 1**: Klik **?? Opslaan**

**Stap 2**: Kies locatie en bestandsnaam
```
Voorgestelde naam:
Fazer_Console_2026_01_09.json

Of met project naam:
[Projectnaam]_2026_01_09.json
```

**Stap 3**: Bevestig
```
[Success Dialog]
Metingen opgeslagen!
88 noten bewaard.
```

### 7.2 JSON Bestand Structuur

**Voorbeeld** (`Fazer_Console_2026_01_09.json`):
```json
{
  "Version": "1.1",
  "CreatedAt": "2026-01-09T14:30:15+01:00",
  "SampleRate": 96000,
  "FftSize": 32768,
  "PianoMetadata": {
    "Type": "Spinet",
    "DimensionCm": 107,
    "ScaleBreakMidiNote": 52,
    "Manufacturer": "Fazer",
    "Model": "Console",
    "SerialNumber": "",
    "LastTuningDate": null,
    "Notes": "",
    "MeasurementTemperatureCelsius": 21.5,
    "MeasurementHumidityPercent": 48.0,
    "MeasurementDateTime": "2026-01-09T14:30:00"
  },
  "Measurements": [
    {
      "NoteName": "A0",
      "MidiIndex": 21,
      "TargetFrequency": 27.5,
      "DetectedPartials": [
        { "n": 1, "Frequency": 27.52, "Amplitude": 38.2 },
        { "n": 2, "Frequency": 55.18, "Amplitude": 32.1 },
        ...
        { "n": 16, "Frequency": 445.8, "Amplitude": -12.3 }
      ],
      "Quality": "Oranje",
      "MeasuredPartialNumber": 6,
      "CalculatedFundamental": 27.51
    },
    ...
    {
      "NoteName": "C8",
      "MidiIndex": 108,
      "TargetFrequency": 4186.01,
      "DetectedPartials": [ ... ],
      "Quality": "Groen",
      "MeasuredPartialNumber": 1,
      "CalculatedFundamental": 4188.23
    }
  ]
}
```

### 7.3 Backup & Archivering

**Aanbeveling**:
1. Sla op in dedicated folder:
   ```
   D:\Piano_Metingen\Fazer_Console\
   ```

2. Maak backup:
   ```
   - Lokale backup op externe HD
   - Cloud backup (Google Drive, OneDrive)
   ```

3. Naamgeving conventie:
   ```
   [Merk]_[Model]_[Datum]_[Optioneel].json
   
   Voorbeelden:
   Fazer_Console_2026_01_09.json
   Steinway_ModelD_2026_01_15_AfterRestringing.json
   Yamaha_U1_2025_12_20.json
   ```

---

## 8. Hergebruik voor Stemming (Later)

### 8.1 Scenario: Stemming Na 6 Maanden

**Situatie**:
- Piano gemeten op 2026-01-09
- Nu is het 2026-07-09 (6 maanden later)
- Piano moet gestemd worden

**Vraag**: Moet ik opnieuw alle 88 noten meten? ? **NEE!**

### 8.2 Load & Tune Procedure

**Stap 1: Laad Oude Meting**
```
1. Start AurisMeasure
2. Klik "?? Laden"
3. Selecteer: Fazer_Console_2026_01_09.json
4. Wacht op laden
```

**Resultaat**:
```
[Success Dialog]
Metingen geladen!
88 noten hersteld.

[Keyboard View]
Alle toetsen tonen kleur van oude meting (groen/oranje)

[Metadata]
- Type: Spinet ?
- Dimension: 107 cm ?
- Scale Break: E3 ?
- Temp: 21.5°C (6 maanden geleden)
- Humidity: 48% (6 maanden geleden)
- Last measured: 2026-01-09 14:30
```

**Stap 2: Gebruik Oude Data**

**Optie A: Direct Stemmen** (Conservatief)
```
Gebruik oude frequenties als target:
- A0: Target = 27.51 Hz (oude meting)
- A4: Target = 439.54 Hz (oude meting)
- Etc.

Stem piano naar deze targets met stemapparaat
```

**Optie B: Pitch Drift Correctie** (Aanbevolen)
```
Zie sectie 9: Pitch Drift Analyse
```

---

## 9. Pitch Drift Analyse (Optioneel)

### 9.1 Wanneer Gebruiken?

**Scenario's**:
- ? Meer dan 3 maanden sinds laatste meting
- ? Seizoensverandering (winter ? zomer)
- ? Piano verplaatst naar andere ruimte
- ? Grote temperatuur/vochtigheidsverschil

**Doel**:
Corrigeer oude metingen voor pitch drift ? Nauwkeuriger stemming

### 9.2 Drift Analyse Procedure

**Stap 1: Laad Oude Meting**
```
Zie sectie 8.2 Stap 1
```

**Stap 2: Update Omgevingscondities** (Optioneel)
```
Environment:
Temp:     [18.0] °C  (was: 21.5°C)
Humidity: [55] %     (was: 48%)
```

**Stap 3: Meet Checkpoint Noten**

Klik **?? Analyze Drift**

Dialog verschijnt:
```
??????????????????????????????????????????????
? Checkpoint Metingen                        ?
??????????????????????????????????????????????
? Meet de volgende checkpoint noten voor    ?
? drift analyse:                             ?
?                                            ?
? A0, E2, A4, C5, C7                        ?
?                                            ?
? Klik OK wanneer klaar, of Cancel om te    ?
? annuleren.                                 ?
?                                            ?
?         [  OK  ]    [ Cancel ]             ?
??????????????????????????????????????????????
```

**Meet de 5 checkpoint noten**:
1. Klik A0 ? START ? Speel ? Wacht op COMPLETED
2. Klik E2 ? START ? Speel ? Wacht op COMPLETED
3. Klik A4 ? START ? Speel ? Wacht op COMPLETED
4. Klik C5 ? START ? Speel ? Wacht op COMPLETED
5. Klik C7 ? START ? Speel ? Wacht op COMPLETED

**Klik OK** in dialog

**Stap 4: Bekijk Drift Analyse**

**Pitch Drift Analysis venster opent**:

```
??????????????????????????????????????????????????????????????????
? Pitch Drift Analysis                                           ?
??????????????????????????????????????????????????????????????????
?                                                                ?
? Old Measurement:  2026-01-09 14:30 (21.5°C, 48% RH)          ?
? New Measurement:  2026-07-09 14:30 (18.0°C, 55% RH)          ?
? Time Elapsed:     182 days (6.0 months)                       ?
?                                                                ?
? Drift Summary:                                                 ?
? - Average Drift:      -9.2 cents                             ?
? - Std Deviation:      ±1.8 cents                             ?
? - Expected Drift:     -8.5 cents (theoretical)               ?
? - Assessment:         ? Uniform drift detected               ?
?                                                                ?
? Checkpoint Measurements:                                       ?
? ????????????????????????????????????????????????????????????? ?
? ? Note ? Old (Hz) ? New (Hz) ? Drift (Hz)? Drift (cents)    ? ?
? ????????????????????????????????????????????????????????????? ?
? ? A0   ? 27.52    ? 27.38    ? -0.14     ? -8.7  ? Normal   ? ?
? ? E2   ? 82.45    ? 82.21    ? -0.24     ? -5.0  ? Normal   ? ?
? ? A4   ? 439.8    ? 437.1    ? -2.90     ? -11.3 ? Moderate ? ?
? ? C5   ? 523.3    ? 520.5    ? -2.80     ? -9.2  ? Normal   ? ?
? ? C7   ? 2093.0   ? 2086.5   ? -6.50     ? -5.3  ? Normal   ? ?
? ????????????????????????????????????????????????????????????? ?
?                                                                ?
?  [Apply Offset to All Notes]              [Close]             ?
??????????????????????????????????????????????????????????????????
```

**Interpretatie**:
- ? **Uniform drift**: ? < 3 cents ? Veilig om offset toe te passen
- ? **Moderate drift** bij A4: -11.3 cents ? Binnen verwachting voor 6 maanden
- ? **Expected drift**: -8.5 cents ? Komt overeen met gemeten -9.2 cents

**Stap 5: Apply Offset**

Klik **Apply Offset to All Notes**

```
[Confirmation Dialog]
Pitch offset toegepast: -2.85 Hz

Alle noten zijn gecorrigeerd voor pitch drift.

Register-scaled corrections:
- Deep bass: -2.85 × 0.6 = -1.71 Hz
- Bass:      -2.85 × 0.8 = -2.28 Hz
- Mid-range: -2.85 × 1.0 = -2.85 Hz
- Treble:    -2.85 × 1.2 = -3.42 Hz

[OK]
```

**Resultaat**:
- Alle 88 noten hebben nu **gecorrigeerde target frequenties**
- Gebruik deze voor stemming
- **Geen volledige hermeting nodig!**

### 9.3 Wanneer NIET Offset Gebruiken

? **Gebruik GEEN offset bij**:
```
Assessment: ? 3 outlier(s) detected - check for mechanical issues

Reden: Niet-uniforme drift ? Mogelijke kapotte snaar of mechanisch probleem
Actie: Volledige hermeting vereist
```

? **Gebruik GEEN offset bij**:
- Piano verplaatst
- Snaren vervangen
- Mechaniek aangepast
- > 1 jaar sinds laatste meting

---

## 10. Troubleshooting

### 10.1 Geen Audio Input

**Symptoom**:
- VU-meter blijft op -80 dB
- Geen spectrum zichtbaar
- Alle metingen "Rood"

**Oplossingen**:
1. ? Check ASIO driver selectie
2. ? Check microfoon XLR kabel
3. ? Check Phantom Power (48V) AAN
4. ? Check gain knop (niet op 0)
5. ? Check input selector (Input 1)
6. ? Test met andere audio bron

### 10.2 Clipping / Overbelasting

**Symptoom**:
- VU-meter constant rood (> -3 dB)
- Clip LED brandt op interface
- Distorted spectrum

**Oplossingen**:
1. ? Verlaag gain op audio interface
2. ? Microfoon verder van piano
3. ? Speel zachter (mezzo-forte)

### 10.3 Te Weinig Partials Gedetecteerd

**Symptoom**:
- Alle noten "Rood" of "Oranje"
- < 5 partials gedetecteerd
- Zwakke amplitudes

**Oplossingen**:
1. ? Verhoog gain (maar vermijd clipping)
2. ? Microfoon dichter bij piano
3. ? Speel harder (forte)
4. ? Check achtergrondgeluid (< 40 dB)
5. ? Check microfoon positie (richting snaren)

### 10.4 Deep Bass Problemen

**Symptoom**:
- A0-C2 allemaal "Rood"
- Higher notes wel "Groen"

**Oplossingen**:
1. ? Speel deep bass **harder** (forte)
2. ? Wacht **3-4 seconden** (langere decay)
3. ? Check low-frequency response microfoon
4. ? Verwijder low-cut filter (indien aanwezig)

**Acceptabel**:
- Deep bass "Oranje" is normaal voor kleine piano's
- 6-10 partials in deze regio is goed genoeg

### 10.5 Scale Break Inconsistenties

**Symptoom**:
- E3 heeft MEER partials dan F3
- Geen plotselinge verbetering bij scale break

**Mogelijke Oorzaken**:
1. ?? Scale break verkeerd ingesteld (zou F2 moeten zijn?)
2. ?? Piano heeft non-standaard mensuur
3. ?? F3 snaar beschadigd of oud

**Actie**:
- Visueel controleer scale break (open piano)
- Corrigeer metadata indien nodig
- Hermeet F3 met forter aanslag

### 10.6 Drift Analyse Faalt

**Symptoom**:
```
Assessment: ? Insufficient data for analysis
```

**Oorzaak**:
- Te weinig checkpoint noten gemeten (< 3)

**Oplossing**:
- Meet minimaal A0, A4, C7
- Voorkeur: Alle 5 checkpoints (A0, E2, A4, C5, C7)

---

## Appendix A: Sneltoetsen & Tips

### Keyboard Shortcuts
```
(Nog niet geïmplementeerd - toekomstige feature)
Space:     Start/Stop measurement
Ctrl+S:    Save measurements
Ctrl+O:    Open file
F5:        Refresh ASIO driver
```

### Pro Tips

**Tip 1: Consistente Aanslag**
- Gebruik metronoom voor timing
- Elke noot 2-3 seconden
- Mezzo-forte (mf) als standaard

**Tip 2: Batch Meting**
- Meet eerst alle bass (A0-B2)
- Pauze (check kwaliteit)
- Meet tenor/treble (C3-C8)

**Tip 3: Scale Break Focus**
- Meet E3, F3 altijd 2× voor bevestiging
- Vergelijk partial count
- Verwacht +2-4 partials bij overgang

**Tip 4: Environmental Logging**
- Meet temp/humidity aan BEGIN en EINDE
- Gemiddelde in metadata
- Check voor drift tijdens lange sessie

**Tip 5: Backup Strategy**
- Auto-save elke 10 noten (toekomstige feature)
- Manual save na elke register
- Cloud sync direct na voltooiing

---

## Appendix B: Wetenschappelijke Referenties

**Gebruikte Literatuur**:

1. **Railsback, O.L. (1938)**. "Scale Temperament as Applied to Piano Tuning". *Journal of the Acoustical Society of America*, 9(3), 274-277.

2. **Barbour, J.M. (1943)**. "Piano Tuning: A Simple and Accurate Method". Michigan State University.

3. **Askenfelt, A. & Jansson, E. (1990)**. "Five Lectures on the Acoustics of the Piano". Royal Swedish Academy of Music.

4. **Conklin, H.A. (1996)**. "Design and Tone in the Mechanoacoustic Piano". *Journal of the Acoustical Society of America*, 100(2), 695-708.

5. **Fletcher, N.H. & Rossing, T.D. (1998)**. "The Physics of Musical Instruments" (2nd ed.). Springer-Verlag.

6. **Oppenheim, A.V. & Schafer, R.W. (2010)**. "Discrete-Time Signal Processing" (3rd ed.). Pearson.

7. **Smith, J.O. (2011)**. "Spectral Audio Signal Processing". W3K Publishing.

---

## Document Versie

**Versie**: 1.0  
**Datum**: 9 januari 2026  
**Auteur**: AurisMeasure Development Team  
**Status**: Production Ready  

**Changelog**:
- v1.0 (2026-01-09): Initiële versie met volledige meetprocedure

---

**Voor vragen of ondersteuning**: Zie GitHub repository of documentatie in `/docs` folder.

?? **Veel succes met het meten van uw piano!** ??
