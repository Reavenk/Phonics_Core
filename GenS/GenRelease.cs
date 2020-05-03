using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        public class GenRelease : GenBase
        { 
            GenBase input;

            int maxLen;
            int curLen;
            bool active = false;

            public GenRelease(int samplesPerSec, GenBase input, float seconds)
                : base(0.0f, 0.0, samplesPerSec, 0.0f)
            { 
                this.input = input;

                this.maxLen = (int)(samplesPerSec * seconds);
                this.maxLen = Mathf.Max(this.maxLen, 1);
                this.curLen = this.maxLen;
            }

            public override void AccumulateImpl(float[] data, int size, IFPCMFactory pcmFactory)
            {
                if(this.active == false)
                { 
                    this.input.Accumulate(data, size, pcmFactory);
                    return;
                }

                if(this.curLen <= 0)
                {
                    this.curLen -= size;
                    return;
                }

                FPCM fa = pcmFactory.GetFPCM(size, true);
                float[] a = fa.buffer;
                this.input.Accumulate(a, size, pcmFactory);

                float c = this.curLen;
                int sz = Mathf.Min(this.curLen, size);
                this.curLen -= sz;

                float fmax = (float)this.maxLen;
                for (int i = 0; i < sz; ++i)
                { 
                    c -= 1.0f;
                    float lam = c / fmax;
                    lam = lam * lam;
                    data[i] += lam * a[i];
                }
            }

            public override PlayState Finished()
            { 
                if(this.input == null)
                    return PlayState.Finished;

                PlayState psInput = this.input.Finished();

                if(this.active == false)
                    return psInput;

                if(psInput == PlayState.Finished)
                    return PlayState.Finished;

                // We don't check if it's equal than, because we want 
                // AccumulateImpl() to be reached one more time before finishing
                // so we know the buffer written right before was played.
                    if (this.curLen < -this.SamplesPerSec)
                    return PlayState.Finished;

                return PlayState.Playing;
            }

            public override void Release()
            {
                this.active = true;
            }

            public override void ReportChildren(List<GenBase> lst)
            {
                lst.Add(this.input);
            }
        }
    }
}
