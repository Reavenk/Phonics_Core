using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        public class GenLinDecay : GenBase
        {
            GenBase gen;
            bool passed = false;
            double attacktime;

            public GenLinDecay(double startTime, double attackTime, int samplesPerSec, GenBase gen)
               : base(0.0f, startTime, samplesPerSec, 1.0f)
            {
                this.gen = gen;
                this.attacktime = attackTime;

            }

            public override void AccumulateImpl(float[] data, int size, IFPCMFactory pcmFactory)
            {
                // TODO: Convert, right now the implementation is a duplicate of Attack
                if (this.passed == true)
                {
                    this.gen.Accumulate(data, size, pcmFactory);
                }
                else
                {
                    double inTime = this.CurTime / this.attacktime;
                    double incr = 1.0 / (this.SamplesPerSec * this.attacktime);

                    FPCM fa = pcmFactory.GetFPCM(size, true);
                    float[] a = fa.buffer;
                    this.gen.Accumulate(a, size, pcmFactory);

                    // NOTE: This could be optimized if we figure out
                    // the sample where the attack ends and avoid
                    // checking inside the loop.
                    for (int i = 0; i < size; ++i)
                    {
                        if (inTime >= 1.0)
                        {
                            // The attack is now finished.
                            this.passed = true;

                            // Finishe the rest as a direct transfer and exit out
                            for (int j = i; j < size; ++j)
                                data[j] = a[j];

                            return;
                        }

                        data[i] = (float)inTime * a[i];
                        inTime += incr;
                    }
                }
            }

            public override PlayState Finished()
            {
                return PlayState.Playing;
            }
        }
    }
}