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

            public abstract void AccumulateImpl(float [] data, int size, IFPCMFactory pcmFactory);

            public void Accumulate(float [] data, int size, IFPCMFactory pcmFactory)
            {
                this.AccumulateImpl(data, size, pcmFactory);

                this.curTime += size * timePerSample;
                this.it += size;
            }

            public void Set(float [] data, int size, IFPCMFactory pcmFactory)
            {
                for(int i = 0; i < data.Length; ++i)
                    data[i] = 0.0f;

                this.AccumulateImpl(data, size, pcmFactory);

                this.curTime += data.Length * timePerSample;
                this.it += data.Length;
            }

            public void ReaderCallback(float[] data)
            {
                IFPCMFactory pcmFactory = FPCMFactory.Instance;
                this.Set(data, data.Length, pcmFactory);
            }

            public void SetPositionCallback(int position)
            { /* Do nothing */ }

            public abstract PlayState Finished();
        }
    }
}