// MIT License
//
// Copyright(c) 2020 Pixel Precision LLC
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        /// <summary>
        /// Linear decay envelope.
        /// </summary>
        public class GenLinDecay : GenBase
        {
            /// <summary>
            /// The PCM stream to apply the decay to.
            /// </summary>
            GenBase gen;

            /// <summary>
            /// The duration of the decay, in seconds.
            /// </summary>
            double duration;

            /// <summary>
            /// The number of audio samples to let pass before starting the envelope.
            /// This should euqla startTime * samplesPerSec.
            /// </summary>
            int offsetSamples = 0;

            /// <summary>
            /// The start time of the envelope.
            /// </summary>
            double startTime;

            /// <summary>
            /// The number of samples left before the decay is finished.
            /// </summary>
            int durationSamples = 0;

            /// <summary>
            /// The number of samples the duration should last. This should equal
            /// samplesPerSec * duration
            /// </summary>
            int totalDurationSamples = 0;

            /// <summary>
            /// The final sustain volume after the decay is finished.
            /// </summary>
            float sustain = 0.0f;

            /// <summary>
            /// The cached inverted value of the sustain value. This should
            /// equal 1.0 - sustain.
            /// </summary>
            float invSustain = 1.0f;


            public GenLinDecay(double startTime, double duration, float sustain, int samplesPerSec, GenBase gen)
               : base(0.0f, samplesPerSec)
            {
                this.startTime = startTime;

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