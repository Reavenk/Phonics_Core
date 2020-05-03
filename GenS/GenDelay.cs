using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        public class GenDelay : GenBase
        {
            int voices;
            int sampleDist;
            float volAtten;
            GenBase input;

            public struct BufferRef
            { 
                public int totalOffset;
                public int sampleIdx;
                public int idx;
            }

            List<FPCM> buffers = new List<FPCM>();
            BufferRef [] rbuffRef = null;

            public GenDelay(int samplesPerSec, GenBase input, int voices, float offsetTime, float volAtten)
                : base(0.0f, 0.0, samplesPerSec, 1.0f)
            { 
                this.voices = Mathf.Max(voices, 1);
                this.volAtten = volAtten;

                this.sampleDist = (int)(samplesPerSec * offsetTime);
                this.sampleDist = Mathf.Max(1, this.sampleDist);

                this.input = input;

                this.rbuffRef = new BufferRef[this.voices];
                for(int i = 0; i < this.voices; ++i)
                {
                    this.rbuffRef[i] = new BufferRef();
                    this.rbuffRef[i].idx = -1;
                    this.rbuffRef[i].sampleIdx = 0;
                    this.rbuffRef[i].totalOffset = -this.sampleDist * (i + 1);
                }
            }

            public override void AccumulateImpl(float[] data, int size, IFPCMFactory pcmFactory)
            {
                // Get the current input's data
                FPCM fpcm = pcmFactory.GetGlobalFPCM(size, true);
                float [] fp = fpcm.buffer;
                this.input.Accumulate(fp, size, pcmFactory);
                // Record it
                this.buffers.Add(fpcm);

                // Transfer the raw
                for (int i = 0; i < size; ++i)
                    data[i] += fp[i];

                const int toohighIdx = 9999;
                int minidx = toohighIdx;

                float attamp = this.volAtten;
                for(int i = 0; i < this.rbuffRef.Length; ++i)
                {
                    BufferRef br = this.rbuffRef[i];
                    int writeIdx = 0;
                    if(br.idx == -1)
                    { 
                        // If we haven't advanced enough steps to start playing,
                        // just record of samples and move on.
                        if(br.totalOffset <= -size)
                        {
                            this.rbuffRef[i].totalOffset += size;
                            continue;
                        }

                        // Start writing at the first buffer we've queued
                        br.idx = 0;
                        // Start reading from that buffer at the first sample.
                        br.sampleIdx = 0;
                        // Jump to the correct position to start writing
                        writeIdx = -br.totalOffset;
                    }

                    while(writeIdx < size)
                    {
                        if(br.sampleIdx >= this.buffers[br.idx].buffer.Length)
                        { 
                            br.sampleIdx = 0;
                            ++br.idx;
                        }

                        float [] rf = this.buffers[br.idx].buffer;
                        int incr = Mathf.Min(rf.Length - br.sampleIdx, size - writeIdx);
                        int end = writeIdx + incr;
                        for(; writeIdx < end; ++writeIdx)
                        { 
                            data[writeIdx] += rf[br.sampleIdx] * attamp;
                            ++br.sampleIdx;
                        }
                        writeIdx = end;

                        // Do we need to advance the buffer index we're looking into?
                        if(br.sampleIdx >= rf.Length)
                        { 
                            ++br.idx;
                            rf = this.buffers[br.idx].buffer;
                            br.sampleIdx = 0;
                            br.totalOffset += incr;
                        }
                    }

                    minidx = Mathf.Max(minidx, br.idx);
                    this.rbuffRef[i] = br;
                    attamp *= this.volAtten;
                }

                if(minidx != toohighIdx && minidx > 0)
                { 
                    int samples = 0;
                    for(int i = 0; i < minidx; ++i)
                        samples += this.buffers[i].buffer.Length;

                    for(int i = minidx; minidx >= 0; --i)
                    {
                        buffers[i].Release();
                        buffers.RemoveAt(i);
                    }

                    for(int i = 0; i < this.rbuffRef.Length; ++i)
                    {
                        this.rbuffRef[i].idx -= minidx;
                        this.rbuffRef[i].totalOffset -= samples;
                    }
                }
            }

            public override PlayState Finished()
            { 
                if(this.input == null)
                    return PlayState.Finished;

                return this.input.Finished();
            }

            public override void Deconstruct()
            {
                foreach(FPCM fpcm in this.buffers)
                    fpcm.Release();

                this.buffers.Clear();
            }

            public override void ReportChildren(List<GenBase> lst)
            {
                lst.Add(this.input);
            }
        }
    }
}
