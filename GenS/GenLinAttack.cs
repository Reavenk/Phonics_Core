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

namespace PxPre
{
    namespace Phonics
    {
        /// <summary>
        /// A linear attack envelope.
        /// </summary>
        public class GenLinAttack : GenBase
        {
            /// <summary>
            /// The PCM stream to perform the envelope on
            /// </summary>
            GenBase gen;

            /// <summary>
            /// A recording on if the envelope has finished. Used as an optimization
            /// to know when to completly bypass the attack.
            /// </summary>
            bool passed = false;

            /// <summary>
            /// The duration of the attack.
            /// </summary>
            double attacktime;

            public GenLinAttack(double startTime, double attackTime, int samplesPerSec, GenBase gen)
               : base(0.0f, samplesPerSec)
            {
                this.gen = gen;
                this.attacktime = attackTime;

            }

            public override void AccumulateImpl(float[] data, int size, IFPCMFactory pcmFactory)
            {
                if(this.passed == true)
                { 
                    this.gen.Accumulate(data, size, pcmFactory);
                }
                else
                {
                    double inTime = this.CurTime / this.attacktime;
                    double incr = 1.0 / (this.SamplesPerSec * this.attacktime);

                    FPCM fa = pcmFactory.GetFPCM(size, true);
                    float [] a = fa.buffer;
                    this.gen.Accumulate(a, size, pcmFactory);

                    // NOTE: This could be optimized if we figure out
                    // the sample where the attack ends and avoid
                    // checking inside the loop.
                    for(int i = 0; i < size; ++i)
                    {
                        if(inTime >= 1.0)
                        { 
                            // The attack is now finished.
                            this.passed = true;

                            // Finishe the rest as a direct transfer and exit out
                            for(int j = i; j < size; ++j)
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
                if(this.gen == null)
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