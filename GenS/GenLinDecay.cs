using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        public class GenLinDecay : GenBase
        {
            GenBase gen;
            double duration;

            int offsetSamples = 0;

            int durationSamples = 0;
            int totalDurationSamples = 0;

            float sustain = 0.0f;
            float invSustain = 1.0f;

            public GenLinDecay(double startTime, double duration, float sustain, int samplesPerSec, GenBase gen)
               : base(0.0f, startTime, samplesPerSec, 1.0f)
            {
                this.gen = gen;
                this.duration = duration;

                this.offsetSamples = (int)(startTime * samplesPerSec);
                this.durationSamples = (int)(duration * samplesPerSec);
                this.totalDurationSamples = this.durationSamples;
                this.sustain = sustain;
                this.invSustain = 1.0f - sustain;
            }

            public override void AccumulateImpl(float[] data, int size, IFPCMFactory pcmFactory)
            {
                if(this.durationSamples <= 0)
                {
                    if(this.sustain == 0.0f)
                    {
                        // If duration is over and we don't have sustain, early exit. We account
                        // for what we would have written as a timer for Finished() do it doesn't
                        // exit too early.
                        this.durationSamples -= size;
                    }
                    else
                    {
                        // If duration is over, we're in sustain
                        FPCM fsus = pcmFactory.GetFPCM(size, true);
                        float[] sus = fsus.buffer;
                        this.gen.Accumulate(sus, size, pcmFactory);
                        for(int i = 0; i < size; ++i)
                            data[i] += sus[i] * this.sustain;
                    }
                    return;
                }

                // Are we at a time before the decay is activated?
                if (this.offsetSamples > size)
                {
                    this.offsetSamples -= size;
                    this.gen.Accumulate(data, size, pcmFactory);
                    return;
                }

                FPCM fa = pcmFactory.GetFPCM(size, true);
                float[] a = fa.buffer;
                this.gen.Accumulate(a, size, pcmFactory);

                // Are we at a time before the decay is activated, but 
                // will need to start the decay before we exit?
                int it = 0;
                if (this.offsetSamples > 0)
                { 
                    for(int i = 0; i < this.offsetSamples; ++i)
                        data[i] = a[i];

                    it = this.offsetSamples;
                    this.offsetSamples = 0;
                }

                // The decay. We have two versions, because if the sustain ramps to zero, we can 
                // avoid some lerp math.
                float total = totalDurationSamples;
                int end = Mathf.Min(size, it + this.durationSamples);
                if(this.sustain == 0.0f)
                {
                    for (; it < end; ++it)
                    {
                        float lam = (float)this.durationSamples / total;
                        data[it] += a[it] * lam;

                        --this.durationSamples;
                    }
                }
                else
                {
                    for (; it < end; ++it)
                    {
                        float lam = sustain + (float)this.durationSamples / total * this.invSustain;
                        data[it] += a[it] * lam;

                        --this.durationSamples;
                    }

                    // If we finish the ramp in the middle, we need to fill the rest with sustain
                    for(; it < size; ++it)
                    {
                        data[it] += a[it] * this.sustain;
                    }
                }
            }

            public override PlayState Finished()
            {
                if(this.durationSamples <= -this.SamplesPerSec)
                    return PlayState.Finished;

                return this.gen.Finished();
            }

            public override void ReportChildren(List<GenBase> lst)
            {
                lst.Add(this.gen);
            }
        }
    }
}