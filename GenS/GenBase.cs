using System.Collections.Generic;

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

            public static PlayState ResolveTwoFinished(GenBase a, GenBase b)
            {
                if(a == null && b == null)
                    return PlayState.Finished;

                if(a == null)
                    return b.Finished();

                if(b == null)
                    return a.Finished();

                PlayState psA = a.Finished();
                PlayState psB = b.Finished();

                if (psA == PlayState.Playing || psB == PlayState.Playing)
                    return PlayState.Playing;

                // If we're modulating and one is signaling it's just going to
                // only return 0.0 from now one, we're done.
                if (psA == PlayState.Finished || psB == PlayState.Finished)
                    return PlayState.Finished;

                if (psA == PlayState.NotStarted && psB == PlayState.NotStarted)
                    return PlayState.NotStarted;

                if (psA == PlayState.Constant || psB == PlayState.Constant)
                    return PlayState.Constant;

                return PlayState.Playing;
            }

            public void DeconstructHierarchy()
            {
                List<GenBase> hierarchy = this.ReportChildrenHierarchy();
                foreach (GenBase gb in hierarchy)
                    gb.Deconstruct();
            }

            public void ReleaseHierarchy()
            {
                List<GenBase> heirarchy = this.ReportChildrenHierarchy();
                foreach(GenBase gb in heirarchy)
                    gb.Release();
            }

            public List<GenBase> ReportChildrenHierarchy()
            { 
                List<GenBase> ret = new List<GenBase>();
                ret.Add(this);
                int idx = 0;
                while(idx < ret.Count)
                { 
                    ret[idx].ReportChildren(ret);
                    ++idx;
                }
                return ret;
            }

            /// <summary>
            /// Add all your children to the list so we can send a signal to the entire heirarchy.
            /// </summary>
            /// <param name="gb"></param>
            public abstract void ReportChildren(List<GenBase> lst);

            // Dissassembly anything before the object gets thrown away. While garbage collection 
            // will handle most things, we need this function to signal global allocations from the
            // FPCM factory to be released back.
            public virtual void Deconstruct() 
            { }

            // Called when the key is released.
            public virtual void Release() 
            { }
        }
    }
}