using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AurisPianoTuner.Measure.Views
{
    public class PianoKeyboard : Canvas
    {
        public event EventHandler<int>? KeyPressed;

        private const int TotalWhiteKeys = 52; // 88 toetsen = 52 witte + 36 zwarte
        private double _whiteKeyWidth;
        private const double WhiteKeyHeight = 120;
        private const double BlackKeyHeight = 75;
        private const double BlackKeyWidthRatio = 0.6; // Zwarte toets is 60% van witte

        private int _selectedMidiIndex = -1;
        private Rectangle? _selectedKeyRectangle = null;
        private Brush _selectedKeyOriginalBrush = Brushes.White;

        // Dictionary om toetsen te vinden per MIDI index
        private Dictionary<int, Rectangle> _keyRectangles = new();

        public PianoKeyboard()
        {
            this.SizeChanged += OnSizeChanged;
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            GenerateKeys();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged && this.ActualWidth > 0)
            {
                GenerateKeys();
            }
        }

        private void GenerateKeys()
        {
            this.Children.Clear();
            _keyRectangles.Clear();

            if (this.ActualWidth <= 0) return;

            // Bereken dynamische breedte per witte toets
            _whiteKeyWidth = this.ActualWidth / TotalWhiteKeys;
            double blackKeyWidth = _whiteKeyWidth * BlackKeyWidthRatio;

            int whiteKeyIndex = 0;

            // Eerst alle witte toetsen
            for (int midi = 21; midi <= 108; midi++)
            {
                if (!IsBlackKey(midi))
                {
                    DrawWhiteKey(midi, whiteKeyIndex * _whiteKeyWidth);
                    whiteKeyIndex++;
                }
            }

            // Dan alle zwarte toetsen (bovenop de witte)
            whiteKeyIndex = 0;
            for (int midi = 21; midi <= 108; midi++)
            {
                if (IsBlackKey(midi))
                {
                    // Bereken positie: tussen twee witte toetsen
                    double xPos = (whiteKeyIndex * _whiteKeyWidth) + (_whiteKeyWidth * 0.7);
                    DrawBlackKey(midi, xPos, blackKeyWidth);
                }
                else
                {
                    whiteKeyIndex++;
                }
            }
        }

        private void DrawWhiteKey(int midiIndex, double x)
        {
            Rectangle rect = new Rectangle
            {
                Width = _whiteKeyWidth - 1, // -1 voor scheidingslijn
                Height = WhiteKeyHeight,
                Fill = Brushes.White,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Tag = midiIndex,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            _keyRectangles[midiIndex] = rect;

            rect.MouseDown += (s, e) =>
            {
                if (s is Rectangle r && r.Tag is int midi)
                {
                    SelectKey(r, midi, Brushes.White);
                }
            };

            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, 0);
            Canvas.SetZIndex(rect, 0);

            this.Children.Add(rect);

            // Label (klein, onderaan)
            TextBlock label = new TextBlock
            {
                Text = GetNoteName(midiIndex),
                FontSize = 8,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Canvas.SetLeft(label, x + (_whiteKeyWidth / 2) - 10);
            Canvas.SetTop(label, WhiteKeyHeight - 15);
            Canvas.SetZIndex(label, 1);

            this.Children.Add(label);
        }

        private void DrawBlackKey(int midiIndex, double x, double width)
        {
            Rectangle rect = new Rectangle
            {
                Width = width,
                Height = BlackKeyHeight,
                Fill = Brushes.Black,
                Stroke = Brushes.DarkGray,
                StrokeThickness = 1,
                Tag = midiIndex,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            _keyRectangles[midiIndex] = rect;

            rect.MouseDown += (s, e) =>
            {
                if (s is Rectangle r && r.Tag is int midi)
                {
                    SelectKey(r, midi, Brushes.Black);
                }
            };

            Canvas.SetLeft(rect, x - (width / 2));
            Canvas.SetTop(rect, 0);
            Canvas.SetZIndex(rect, 2); // Zwarte toetsen boven witte

            this.Children.Add(rect);
        }

        private void SelectKey(Rectangle keyRect, int midiIndex, Brush originalBrush)
        {
            // Reset vorige selectie
            if (_selectedKeyRectangle != null)
            {
                _selectedKeyRectangle.Fill = _selectedKeyOriginalBrush;
            }

            // Stel nieuwe selectie in
            _selectedMidiIndex = midiIndex;
            _selectedKeyRectangle = keyRect;
            _selectedKeyOriginalBrush = originalBrush;

            // Maak geselecteerde toets blauw
            keyRect.Fill = Brushes.CornflowerBlue;

            // Fire event
            KeyPressed?.Invoke(this, midiIndex);
        }

        public void SetKeyQuality(int midiIndex, string quality)
        {
            if (!_keyRectangles.TryGetValue(midiIndex, out var keyRect))
                return;

            Brush qualityBrush = quality switch
            {
                "Groen" => Brushes.LimeGreen,
                "Oranje" => Brushes.Orange,
                "Rood" => Brushes.IndianRed,
                _ => IsBlackKey(midiIndex) ? Brushes.Black : Brushes.White
            };

            // Update de originele kleur zodat deze behouden blijft bij deselectie
            if (keyRect == _selectedKeyRectangle)
            {
                _selectedKeyOriginalBrush = qualityBrush;
                // Laat blauw voor nu (actieve selectie)
            }
            else
            {
                keyRect.Fill = qualityBrush;
            }
        }

        private bool IsBlackKey(int midiIndex)
        {
            int noteInOctave = (midiIndex - 9) % 12; // A=0, A#=1, B=2, C=3, etc.
            string[] notes = { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" };
            return notes[noteInOctave].Contains("#");
        }

        private string GetNoteName(int midiIndex)
        {
            string[] notes = { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" };
            int octave = (midiIndex / 12) - 1;
            int noteIndex = (midiIndex - 9) % 12;
            return notes[noteIndex] + octave;
        }
    }
}
