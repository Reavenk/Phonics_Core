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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        public class GenCycle : GenBase
        {
            public enum OffsetPass
            { 
                Pass,
                Silent,
                Hold
            }

            struct Recording
            { 
                public FPCM buffer;
                public readonly int cachedStart;
                public readonly int cachedEnd;
                public readonly int length;

                public Recording(FPCM buffer, int offset)
                { 
                    this.buffer = buffer;
                    this.cachedStart = offset;
                    this.length = this.buffer.buffer.Length;
                    this.cachedEnd = offset + this.length;
                }
            }

            // The amount to skip before recording?
            // This will be passed
            int offset = 0;
            int recordAmt;

            /// <summary>
            /// The input to process (cycle)
            /// </summary>
            GenBase input;
            
            int recordingIt = 0;

            // When doing playback, the buffer to stream from
            int bufferIdx = 0;
            int playbackIt = 0;

            OffsetPass passOffset = OffsetPass.Pass;

            // The entire buffer being recorded and cycled through. There's an argument to be made 
            // that we chould store an array of chunks in case the entire buffer isn't played, but
            // we've made out decision for now.
            float [] rfs = null;

            public GenCycle(int offsetSamples, int recordAmt, OffsetPass passOffset, GenBase input)
                : base(0.0f, 0)
            { 
                this.offset = offsetSamples;
                this.input = input;
                this.recordAmt = recordAmt;
                this.passOffset = passOffset;

                // Kept until ready to playback
                this.playbackIt = 0;

                this.rfs = new float[recordAmt];
            }
        
            unsafe public override void AccumulateImpl(float * data, int start, int size, int prefBuffSz, FPCMFactoryGenLimit pcmFactory)
            {

                if(this.offset > 0)
                {
                    int of = Min(this.offset, size);
                    if(this.passOffset == OffsetPass.Silent)
                    {
                        // If silent, we still need to let time pass for it, so
                        // we need to burn it.
                        FPCM fpcm = pcmFactory.GetZeroedFPCM(start, size);
                        float [] a = fpcm.buffer;

                        fixed(float * pa = a)
                        {
                            this.input.Accumulate(pa, start, of, prefBuffSz, pcmFactory);
                        }
                    }
                    else if(this.passOffset == OffsetPass.Pass)
                    { 
                        this.input.Accumulate(data, start, of, prefBuffSz, pcmFactory);
                    }
                    else if(this.passOffset == OffsetPass.Hold)
                    { } // Do nothing

                    this.offset -= of;
                    start += of;
                    size -= of;

                    if(size == 0)
                        return;
                }

                if(this.recordingIt < this.rfs.Length)
                { 
                    int recAmt = Min(size, this.rfs.Length - this.recordingIt);

                    FPCM fpcm = pcmFactory.GetZeroedFPCM(0, size);
                    float [] lbuf = fpcm.buffer;

                    fixed(float * plbuf = lbuf)
                    {
                        this.input.Accumulate(plbuf, 0, recAmt, prefBuffSz, pcmFactory);

                        for(int i = 0; i < recAmt; ++i)
                        { 
                            data[start + i] = plbuf[i];
                            rfs[recordingIt + i] = plbuf[i];
                        }
                    }

                    this.recordingIt += recAmt;
                    start += recAmt;
                    size -= recAmt;

                    if(this.recordingIt == this.rfs.Length)
                    { 
                        // If the buffer size is tiny, we're going to duplicate it to a sane amount
                        // so we don't have many tiny cycles when it comes to replaying it.
                        if(this.rfs.Length <  prefBuffSz)
                        { 
                            int repCt = this.rfs.Length / prefBuffSz;
                            if(repCt > 1)
                            {
                                float [] rfOld = this.rfs;
                                int oldSz = this.rfs.Length;

                                // Repeat the buffer
                                this.rfs = new float[oldSz * repCt];
                                for(int i = 1; i < repCt; ++i)
                                { 
                                    int baseIdx = i * oldSz;
                                    for(int j = 0; j < oldSz; ++j)
                                        this.rfs[baseIdx + j] = rfOld[j];
                                }
                            }
                        }
                    }
                }

                while(size > 0)
                { 
                    this.playbackIt %= this.rfs.Length;

                    int sameCt = Min(this.rfs.Length - this.playbackIt, size);

                    for(int i = 0; i < sameCt; ++i)
                    { 
                        data[start + i] = rfs[this.playbackIt + i];
                    }
                    
                    this.playbackIt += sameCt;
                    start += sameCt;
                    size -= sameCt;
                }
            }
        
            public override PlayState Finished()
            {
                if(this.input == null || this.rfs == null)
                    return PlayState.Finished;
        
                if(this.recordingIt < this.rfs.Length)
                    return this.input.Finished();

                return PlayState.Constant;
            }
        
            public override void Deconstruct()
            {
            }

            public override void ReportChildren(List<GenBase> lst)
            {
                lst.Add(this.input);
            }

        }
    }
}
