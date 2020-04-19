using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{ 
    namespace Phonics
    { 
        public static class WesternFreqUtils
        {
            public const float pi = 3.14159265359f;
            public const float tau = pi * 2.0f;

            public enum Note
            {
                A,  As,
                B,
                C,  Cs,
                D,  Ds,
                E,
                F,  Fs,
                G,  Gs
            }

            public static void ZeroBuffer(float [] rf, int start, int len)
            { 
                System.Array.Clear(rf, start, len);
            }

            public static float SetSine(float [] rf, int start, int len, float time, float freq, float amp, int sampsSec)
            { 
                // Multiply tau for 1 hertz.
                // Multiply by frequency because that's requested as a paramter
                float sincr = 1.0f / sampsSec;
                float s = time;
                for(int i = start; i < start + len; ++i)
                { 
                    rf[i] = Mathf.Sin(s * freq * tau) * amp;
                    s += sincr;
                }
                return time + (float)len / (float)sampsSec;
            }

            public static float SetSine(float[] rf, int start, int len, float phase, float time, float freq, int sampsSec, float env0, float env1, float env2, float env3)
            {
                float fincr = 1.0f / sampsSec;
                float sincr = fincr * freq * tau;
                float s = time + phase;
                float f = s;
                for (int i = start; i < start + len; ++i)
                {
                    float f2 = f * f;
                    float f3 = f2 * f;
                    float amp = env0 + f * env1 + f2 * env2 + f3 * env3;

                    rf[i] = Mathf.Sin(s) * amp;
                    s += sincr;
                    f += fincr;
                }
                return time + (float)len / (float)sampsSec;
            }

            public static float AddSine(float [] rf, int start, int len, float phase, float time, float freq, float amp, int sampsSec)
            {
                float sincr = 1.0f / sampsSec * freq * tau;
                float s = time + phase;
                for (int i = start; i < start + len; ++i)
                {
                    rf[i] += Mathf.Sin(s) * amp;
                    s += sincr;
                }
                return time + (float)len / (float)sampsSec;
            }

            public static float SetThSquare(float [] rf, int start, int len, float phase, float time, float freq, float amp, int sampsSec)
            {
                float s = (time + phase) * freq * 2.0f;
                float sincr = (1.0f / sampsSec * freq) * 2.0f; 
                for (int i = start; i < start + len; ++i)
                {
                    int cross = (int)s;
                    rf[i] = ((cross & 1) != 0) ? -amp : amp;
                    s += sincr;
                }
                return time + (float)len / (float)sampsSec;
            }

            public static float SetThSquare(float[] rf, int start, int len, float phase, float time, float freq, int sampsSec, float env0, float env1, float env2, float env3)
            {
                float s = (time + phase) * freq * 2.0f;
                float f = s;

                float fincr = 1.0f / sampsSec;
                float sincr = fincr * freq * 2.0f;

                for (int i = start; i < start + len; ++i)
                {
                    float f2 = f * f;
                    float f3 = f2 * f;
                    float amp = env0 + f * env1 + f2 * env2 + f3 * env3;

                    int cross = (int)s;
                    rf[i] = ((cross & 1) != 0) ? -amp : amp;

                    s += sincr;
                    f += fincr;
                }
                return time + (float)len / (float)sampsSec;
            }

            public static float AddThSquare(float [] rf, int start, int len, float phase, float time, float freq, float amp, int sampsSec)
            {
                float sincr = 1.0f / sampsSec * freq * tau;
                float s = time + phase;
                for (int i = start; i < start + len; ++i)
                {
                    int cross = (int)(s * freq * 2.0f);
                    rf[i] += ((cross & 1) != 0) ? -amp : amp;
                    s += sincr;
                }
                return time + (float)len / (float)sampsSec;
            }

            public static float SetTri(float [] rf, int start, int len, float phase, float time, float freq, float amp, int sampsSec)
            {
                float tamp = amp * 2.0f;
                float sincr = (1.0f / sampsSec * freq) * 2.0f;
                float s = (time + phase) * sampsSec * freq * 2.0f;
                for (int i = start; i < start + len; ++i)
                {
                    int cross = (int)s;
                    float lambda = s - cross;

                    if((cross & 1) != 0)
                        rf[i] = -amp + tamp * lambda;
                    else
                        rf[i] = amp - tamp * lambda;
                    
                    s += sincr;
                }
                return time + (float)len / (float)sampsSec;
            }

            public static float AddTri(float [] rf, int start, int len, float phase, float time, float freq, float amp, int sampsSec)
            {
                float tamp = amp * 2.0f;
                float sincr = (1.0f / sampsSec * freq) * 2.0f;
                float s = (time + phase) * sampsSec * freq * 2.0f;
                for (int i = start; i < start + len; ++i)
                {
                    int cross = (int)s;
                    float lambda = s - cross;

                    if ((cross & 1) != 0)
                        rf[i] += -amp + tamp * lambda;
                    else
                        rf[i] += amp - tamp * lambda;

                    s += sincr;
                }
                return time + (float)len / (float)sampsSec;
            }

            public static void GetSetWestKeyInfo(int key, out Note n, out int octave)
            { 
                int o = key / 12;
                int st = key % 12;

                switch(st)
                { 
                    case 0:
                        n = Note.A;
                        octave = o - 1;
                        break;

                    case 1:
                        n = Note.As;
                        octave = o - 1;
                        break;

                    case 2:
                        n = Note.B;
                        octave = o - 1;
                        break;

                    case 3:
                        n = Note.C;
                        octave = o;
                        break;

                    case 4:
                        n = Note.Cs;
                        octave = o;
                        break;

                    case 5:
                        n = Note.D;
                        octave = o;
                        break;

                    case 6:
                        n = Note.Ds;
                        octave = o;
                        break;

                    case 7:
                        n = Note.E;
                        octave = o;
                        break;

                    case 8:
                        n = Note.F;
                        octave = o;
                        break;

                    case 9:
                        n = Note.Fs;
                        octave = o;
                        break;

                    case 10:
                        n = Note.G;
                        octave = o;
                        break;

                    case 11:
                        n = Note.Gs;
                        octave = o;
                        break;

                    default:
                        n = Note.A;
                        octave = -1;
                        break;
                }
            }

            public static int GetStdWestKey(Note n, int octave)
            {
                int octbase = octave * 12;

                switch (n)
                {
                    case Note.A:
                        return octbase + 12;

                    case Note.As:
                        return octbase + 13;

                    case Note.B:
                        return octbase + 14;

                    case Note.C:
                        return octbase + 3;

                    case Note.Cs:
                        return octbase + 4;

                    case Note.D:
                        return octbase + 5;

                    case Note.Ds:
                        return octbase + 6;

                    case Note.E:
                        return octbase + 7;

                    case Note.F:
                        return octbase + 8;

                    case Note.Fs:
                        return octbase + 9;

                    case Note.G:
                        return octbase + 10;

                    case Note.Gs:
                        return octbase + 11;

                    default:
                        return -1;
                }
            }

            public static float GetStdWestFrequency(Note n, int octave)
            { 
                int baseline = GetStdWestKey(Note.A, 4);
                int key = GetStdWestKey(n, octave);
                int diff = key - baseline;

                float A4Fr = 440.0f;
                return A4Fr * Mathf.Pow(2.0f, (float)diff/12.0f);
            }

            public static float GetStdWestFrequency(int key)
            {
                Note n;
                int o;
                GetSetWestKeyInfo(key, out n, out o);

                return GetStdWestFrequency(n, o);
            }
        }
    }
}