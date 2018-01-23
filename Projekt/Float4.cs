using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCLTemplate.CLGLInterop;

namespace Projekt
{
    public struct Float4
    {
        public float S0;
        public float S1;
        public float S2;
        public float S3;

        public Float4(float s0, float s1, float s2, float s3) : this()
        {
            S0 = s0;
            S1 = s1;
            S2 = s2;
            S3 = s3;
        }

        public Float4(float value)
        {
            S0 = S1 = S2 = S3 = value;
        }

        public override string ToString()
        {
            return "["+S0 + ", " + S1 + ", " + S2 + ", " + S3+"]";
        }

    }
}
