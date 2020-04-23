using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        public static class WesternFreqUtils
        {
            public enum Key
            {
                A, As,
                B,
                C, Cs,
                D, Ds,
                E,
                F, Fs,
                G, Gs
            }

            public static void GetKeyInfo(int key, out Key n, out int octave)
            {
                int o = key / 12;
                int st = key % 12;

                switch (st)
                {
                    case 0:
                        n = Key.A;
                        octave = o - 1;
                        break;

                    case 1:
                        n = Key.As;
                        octave = o - 1;
                        break;

                    case 2:
                        n = Key.B;
                        octave = o - 1;
                        break;

                    case 3:
                        n = Key.C;
                        octave = o;
                        break;

                    case 4:
                        n = Key.Cs;
                        octave = o;
                        break;

                    case 5:
                        n = Key.D;
                        octave = o;
                        break;

                    case 6:
                        n = Key.Ds;
                        octave = o;
                        break;

                    case 7:
                        n = Key.E;
                        octave = o;
                        break;

                    case 8:
                        n = Key.F;
                        octave = o;
                        break;

                    case 9:
                        n = Key.Fs;
                        octave = o;
                        break;

                    case 10:
                        n = Key.G;
                        octave = o;
                        break;

                    case 11:
                        n = Key.Gs;
                        octave = o;
                        break;

                    default:
                        n = Key.A;
                        octave = -1;
                        break;
                }
            }

            public static int GetNote(Key k, int octave)
            {
                int octbase = octave * 12;

                switch (k)
                {
                    case Key.A:
                        return octbase + 12;

                    case Key.As:
                        return octbase + 13;

                    case Key.B:
                        return octbase + 14;

                    case Key.C:
                        return octbase + 3;

                    case Key.Cs:
                        return octbase + 4;

                    case Key.D:
                        return octbase + 5;

                    case Key.Ds:
                        return octbase + 6;

                    case Key.E:
                        return octbase + 7;

                    case Key.F:
                        return octbase + 8;

                    case Key.Fs:
                        return octbase + 9;

                    case Key.G:
                        return octbase + 10;

                    case Key.Gs:
                        return octbase + 11;

                    default:
                        return -1;
                }
            }

            public static float GetFrequency(Key k, int octave)
            {
                int baseline = GetNote(Key.A, 4);
                int key = GetNote(k, octave);
                int diff = key - baseline;

                float A4Fr = 440.0f;
                return A4Fr * Mathf.Pow(2.0f, (float)diff / 12.0f);
            }

            public static float GetFrequency(int note)
            {
                Key k;
                int o;
                GetKeyInfo(note, out k, out o);

                return GetFrequency(k, o);
            }
        }
    }
}
