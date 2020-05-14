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
        /// Linear Attack/Decay/Sustain/Release envelope on a PCM stream.
        /// </summary>
        public class GenADSR : GenBase
        {
            GenBase input;

            int offset;
            int attackIt;
            int attackTotal;
            int decayLeft;
            int decayTotal;
            int releaseLeft;
            int releaseTotal;
            float sustain = 0.5f;
            int saftey;
            bool released = false;

            public GenADSR(
                GenBase input,
                int samplesPerSec, 
                float offset, 
                float attackTime,
                float decayTime,
                float sustainValue,
                float releaseTime)
                : base(0.0f, samplesPerSec)
            { 
                this.input = input;
                this.sustain = sustainValue;

                this.offset = (int)(offset * samplesPerSec);
                this.attackIt = 0;
                this.attackTotal = (int)(attackTime * samplesPerSec);
                this.decayTotal = (int)(decayTime * samplesPerSec);
                this.decayLeft = this.decayTotal;

                if (releaseTime <= 0)
                { 
                    this.releaseTotal = 0;
                    this.releaseLeft = 0;
                    this.saftey = 0;
                }
                else
                { 
                    this.releaseTotal = (int)(releaseTime * samplesPerSec);
                    this.releaseLeft = this.releaseTotal;
                    this.saftey = samplesPerSec;
                }
            }

            public override void AccumulateImpl(float[] data, int size, IFPCMFactory pcmFactory)
            {
                // Early release
                if (this.released == true)
                {
                    if (this.releaseLeft > 0)
                    {
                        FPCM fpcmRl = pcmFactory.GetFPCM(size, true);
                        float[] fprl = fpcmRl.buffer;

                        // We could probably optimize this by only calling AccumulateImpl_NonReleasePart()
                        // when we're not in sustain yet.
                        this.AccumulateImpl_NonReleasePart(fprl, size, pcmFactory);

                        float total = this.releaseTotal;
                        float left = this.releaseLeft;
                        int sampsCt = Mathf.Min(this.releaseLeft, size);

                        for (int ei = 0; ei < sampsCt; ++ei)
                        {
                            data[ei] = fprl[ei] * (left / total);
                            left -= 1.0f;
                        }
                        this.releaseLeft -= sampsCt;
                        return;
                    }
                    else if (this.saftey > 0)
                        this.saftey -= size;
                }
                else
                {
                    this.AccumulateImpl_NonReleasePart(data, size, pcmFactory);
                }
            }

            public void AccumulateImpl_NonReleasePart(float[] data, int size, IFPCMFactory pcmFactory)
            { 
                FPCM fpcm = pcmFactory.GetFPCM(size, true);
                float[] fp = fpcm.buffer;
                this.input.Accumulate(fp, size, pcmFactory);

                // If we still have an offset, we do nothing for that many more samples
                int i = 0;
                if(this.offset > 0)
                { 
                    int ofrm = Mathf.Min(this.offset, size);
                    this.offset -= ofrm;
                    i = ofrm;

                    if(i >= size)
                        return;

                    // Set the index and continue
                    i = ofrm;
                }

                // ATTACK
                if (this.attackIt < this.attackTotal)
                { 
                    int atrm = Mathf.Min(this.attackTotal - this.attackIt, size - i);
                    int end = i + atrm;
                    float totalAt = this.attackTotal;
                    float at = this.attackIt;
                    for(; i < end; ++i)
                    {
                        data[i] += fp[i] * (at/totalAt);
                        at += 1.0f;
                    }
                    this.attackIt += atrm;
                    if(i >= size)
                        return;

                    i = end;
                }

                // DECAY
                if(this.decayLeft > 0)
                { 
                    int dcrm = Mathf.Min(this.decayLeft, size - i);
                    int dcend = i + dcrm;
                    float totalDc = this.decayTotal;
                    float cd = this.decayLeft;
                    float susDiff = 1.0f - this.sustain;
                    for(; i < dcend; ++i)
                    { 
                        data[i] += fp[i] * (this.sustain + (cd / totalDc) * susDiff);
                        cd -= 1.0f;
                    }
                    this.decayLeft -= dcrm;
                    if(i >= size)
                        return;

                    i = dcend;
                }

                // If we're still here, all that's left is sustain
                for( ; i < size; ++i)
                    data[i] += fp[i] * this.sustain;
            }

            public override PlayState Finished()
            { 
                if(this.input == null)
                    return PlayState.Finished;

                if(this.released == false)
                    return this.input.Finished();

                if(this.saftey <= 0)
                    return PlayState.Finished;

                return PlayState.Playing;

            }

            public override void ReportChildren(List<GenBase> lst)
            { 
                if(this.input != null)
                    lst.Add(this.input);
            }

            public override void Release()
            { 
                this.released = true;
            }
        }
    }
}