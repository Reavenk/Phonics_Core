using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        public class GenMin : GenBase
        {
            GenBase gma;
            GenBase gmb;

            public GenMin(double startTime, int samplesPerSec, GenBase gma, GenBase gmb)
                : base(0.0f, startTime, samplesPerSec, 1.0f)
            {
                this.gma = gma;
                this.gmb = gmb;
            }

            public override void AccumulateImpl(float[] data, int size, IFPCMFactory pcmFactory)
            {
                FPCM fa = pcmFactory.GetFPCM(data.Length, true);
                FPCM fb = pcmFactory.GetFPCM(data.Length, true);

                float[] a = fa.buffer;
                float[] b = fb.buffer;

                gma.Accumulate(a, size, pcmFactory);
                gmb.Accumulate(b, size, pcmFactory);

                for (int i = 0; i < size; ++i)
                    data[i] += (a[i] < b[i]) ? a[i] : b[i];
            }

            public override PlayState Finished()
            {
                return ResolveTwoFinished(this.gma, this.gmb);
            }

            public override void ReportChildren(List<GenBase> lst)
            {
                lst.Add(this.gma);
                lst.Add(this.gmb);
            }
        }
    }
}