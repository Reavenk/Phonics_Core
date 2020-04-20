using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        public enum PlayState
        { 
            /// <summary>
            /// It's on a schedule to eventually output PCM, but not yet.
            /// </summary>
            NotStarted,

            /// <summary>
            /// It's currently playing audio.
            /// </summary>
            Playing,

            /// <summary>
            /// It's already output its PCM and is now only going to give out
            /// 0.0 PCM
            /// </summary>
            Finished,

            /// <summary>
            /// For generators. They're always giving a constant tone out.
            /// </summary>
            Constant
        }

        public abstract class GenBase
        {
            const float fTau = 6.28318530718f;
            const double dTau = 6.28318530718;
            //
            const float fPie = 3.14159265359f;
            const double dPie = 3.14159265359;

            public long it = 0;
            public long It { get{return this.it; } }
    
            private float freq;
            public float Freq { get{return this.freq; } }
    
            private double timeToFreq;
            public double TimeToFreq {get{return this.timeToFreq; } }
    
            private int samplesPerSec;
            public int SamplesPerSec {get{return this.samplesPerSec; } }
    
            private double curTime;
            public double CurTime {get{return this.curTime; } }
    
            public double TimePerSample {get{return this.timePerSample; } }
            private double timePerSample;

            public float amplitude = 0.8f;

            // Some reusable float buffer for everything to use.
            protected static float [] bufferA = null;
            protected static float [] bufferB = null;
            protected static float [] bufferC = null;
            protected static float [] bufferD = null;

            private static void AllocateBuffer(ref float [] buffer, int size)
            { 
                if(buffer == null || buffer.Length < size)
                    buffer = new float[size];
            }

            private static void ZeroedBuffer(ref float [] buffer, int size)
            { 
                AllocateBuffer(ref buffer, size);
                for(int i = 0; i < size; ++i)
                    buffer[i] = 0.0f;
            }

            public static float [] GetBufferA(int minsize)
            { 
                ZeroedBuffer(ref bufferA, minsize);
                return bufferA;
            }

            public static float [] GetBufferB(int minsize)
            {
                ZeroedBuffer(ref bufferB, minsize);
                return bufferB;
            }

            public static float [] GetBufferC(int minsize)
            {
                ZeroedBuffer(ref bufferC, minsize);
                return bufferC;
            }

            public static float [] GetBufferD(int minsize)
            {
                ZeroedBuffer(ref bufferD, minsize);
                return bufferD;
            }

            protected GenBase(float freq, double startTime, int samplesPerSec, float amplitude)
            { 
                this.samplesPerSec = samplesPerSec;
                this.freq = freq;
                this.timeToFreq = dTau * freq;
                this.amplitude = amplitude;

                this.curTime = startTime;
                this.it = (long)(samplesPerSec * startTime);
                this.timePerSample = 1.0 / samplesPerSec;
            }

            public abstract void AccumulateImpl(float [] data, int size);

            public void Accumulate(float [] data, int size)
            {
                this.AccumulateImpl(data, size);

                this.curTime += size * timePerSample;
                this.it += size;
            }

            public void Set(float [] data, int size)
            {
                for(int i = 0; i < data.Length; ++i)
                    data[i] = 0.0f;

                this.AccumulateImpl(data, size);

                this.curTime += data.Length * timePerSample;
                this.it += data.Length;
            }

            public void ReaderCallback(float[] data)
            {
                this.Set(data, data.Length);
            }

            public void SetPositionCallback(int position)
            { /* Do nothing */ }

            public abstract PlayState Finished();
        }
    }
}