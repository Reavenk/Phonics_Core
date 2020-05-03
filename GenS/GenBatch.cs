using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        public class GenBatch : GenBase
        {
            GenBase [] batch;

            public GenBatch(double startTime, int samplesPerSec, GenBase [] batch)
                : base(0.0f, startTime, samplesPerSec, 1.0f)
            {
                this.batch = batch;
            }

            public override void AccumulateImpl(float[] data, int size, IFPCMFactory pcmFactory)
            {
                if(this.batch.Length == 0)
                    return;

                float inv = 1.0f / this.batch.Length;

                foreach(GenBase gb in this.batch)
                {
                    FPCM fpcm = pcmFactory.GetFPCM(data.Length, true);
                    float [] rf = fpcm.buffer;

                    gb.Accumulate(rf, size, pcmFactory);

                    for(int i = 0; i < size; ++i)
                        data[i] += rf[i] * inv;
                }
                
            }

            public override PlayState Finished()
            {
                return  PlayState.Constant; // TODO:
            }

            public override void ReportChildren(List<GenBase> lst)
            {
                foreach(GenBase gb in this.batch)
                    lst.Add(gb);
            }
        }
    }
}
