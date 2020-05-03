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
                octave = key / 12;
                int st = key % 12;

                switch (st)
                {
                    case 0:
                        n = Key.C;
                        break;

                    case 1:
                        n = Key.Cs;
                        break;

                    case 2:
                        n = Key.D;
                        break;

                    case 3:
                        n = Key.Ds;
                        break;

                    case 4:
                        n = Key.E;
                        break;

                    case 5:
                        n = Key.F;
                        break;

                    case 6:
                        n = Key.Fs;
                        break;

                    case 7:
                        n = Key.G;
                        break;

                    case 8:
                        n = Key.Gs;
                        break;

                    case 9:
                        n = Key.A;
                        break;

                    case 10:
                        n = Key.As;
                        break;

                    case 11:
                        n = Key.B;
                        break;

                    default:
                        n = Key.C;
                        octave = -1;
                        break;
                }
            }

            public static int GetNote(Key k, int octave)
            {
                int octbase = octave * 12;

                switch (k)
                {
                    case Key.C:
                        return octbase + 0;

                    case Key.Cs:
                        return octbase + 1;

                    case Key.D:
                        return octbase + 2;

                    case Key.Ds:
                        return octbase + 3;

                    case Key.E:
                        return octbase + 4;

                    case Key.F:
                        return octbase + 5;

                    case Key.Fs:
                        return octbase + 6;

                    case Key.G:
                        return octbase + 7;

                    case Key.Gs:
                        return octbase + 8;

                    case Key.A:
                        return octbase + 9;

                    case Key.As:
                        return octbase + 10;

                    case Key.B:
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
