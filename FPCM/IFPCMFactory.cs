using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        public interface IFPCMFactory
        {
            FPCM GetFPCM(int samples, bool zero);
            FPCM GetGlobalFPCM(int samples, bool zero);
        }
    }
}