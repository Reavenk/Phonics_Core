using System.Collections.Generic;

namespace PxPre
{
    namespace Phonics
    {
        public class GenZero : GenBase
        {
            public GenZero()
                : base(0.0f, 0.0, 0, 0.0f)
            {
            }

            public override void AccumulateImpl(float[] data, int size, IFPCMFactory pcmFactory)
            {}

            public override PlayState Finished()
            {
                return PlayState.Constant;
            }

            public override void ReportChildren(List<GenBase> lst)
            {}
        }
    }
}