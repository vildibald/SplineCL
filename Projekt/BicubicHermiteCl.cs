using System;
using System.IO;
using Cloo;
using Projekt.AproximationEngine;

namespace Projekt
{
    public class BicubicHermiteCl : IKernel
    {
        private const float Density = 0.01f;
        private ComputeContext _context;
        private ComputeKernel _kernel;

        public BicubicHermiteCl(ComputeContext context)
        {
            // var src = File.ReadAllText("BicubicHermiteCl.cl");
            Context = context;
        }

        public ComputeContext Context
        {
            get { return _context; }
            set
            {
                _context = value;
                var src = File.ReadAllText("BicubicHermiteCl.cl");
                var program = new ComputeProgram(value, src);
                program.Build(null, null, null, IntPtr.Zero);
                _kernel = program.CreateKernel("bicubicHermiteSegment");
            }
        }

        public Float4[] CreateMesh(AproximationExpression expression, PlotInterval interval)
        {
            return CreateMesh(AproximationFunction.FromExpression(expression), interval);
        }

        public Float4[] CreateMesh(AproximationFunction function, PlotInterval interval)
        {
            var basis = BasisMatrix(function, interval);
            var u0 = interval.X0;
            var u1 = interval.X1;
            var v0 = interval.Y0;
            var v1 = interval.Y1;

            var uKnotsDistance = Math.Abs(u1 - u0);
            var xCount = Math.Ceiling(uKnotsDistance/Density);
            var yKnotDistance = Math.Abs(v1 - v0);
            var yCount = Math.Ceiling(yKnotDistance/Density);

            var verticesCount = (int) ((++xCount)*(++yCount));

            var result = new Float4[verticesCount];
           

            // Create the input buffers and fill them with data from the arrays.
            // Access modifiers should match those in a kernel.
            // CopyHostPointer means the buffer should be filled with the data provided in the last argument.
            var knotsBuffer = new ComputeBuffer<float>(Context,
               ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, new[] { u0,v0,u1,v1 });
            var densityBuffer = new ComputeBuffer<float>(Context,
               ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, new[] { Density });
            var basisBuffer = new ComputeBuffer<float>(Context,
                ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, basis);
           
           
            var eventList = new ComputeEventList();

           // var localBasisBuffer = new ComputeBuffer<float>(Context, ComputeMemoryFlags.ReadWrite , 16L);
          
            var resultBuffer = new ComputeBuffer<Float4>(Context, ComputeMemoryFlags.WriteOnly, verticesCount);

            _kernel.SetMemoryArgument(0, resultBuffer);
//            _kernel.SetMemoryArgument(1, u0Buffer);
//            _kernel.SetMemoryArgument(2, u1Buffer);
//            _kernel.SetMemoryArgument(3, v0Buffer);
//            _kernel.SetMemoryArgument(4, v1Buffer);
            _kernel.SetMemoryArgument(1, knotsBuffer);
            _kernel.SetMemoryArgument(2, densityBuffer);
            _kernel.SetMemoryArgument(3, basisBuffer);
           // _kernel.SetLocalArgument(7,16*sizeof(float));

            var commands = new ComputeCommandQueue(_context, _context.Devices[0], ComputeCommandQueueFlags.None);
            // a.k.a. number of threads .... uCount*vCount
            var globalWorkSize = new[] {(long) xCount, (long) yCount};
            
            //var globalWorkSize = new[] {(long) verticesCount};
            //var localWorkSize = new long[] { 4,4};
            commands.Execute(_kernel, null, globalWorkSize, null, eventList);

            commands.ReadFromBuffer(resultBuffer, ref result, false, eventList);

            //Wait for the events in the list to finish,
            //eventList.Wait();

            //Or simply use
            commands.Finish();

            return result;
        }

        private float[] BasisMatrix(AproximationFunction function, PlotInterval interval)
        {
            var matrix = new float[16];

            matrix[0] = (float) function.Z(interval.X0, interval.Y0);
            matrix[1] = (float) function.Z(interval.X0, interval.Y1);
            matrix[2] = (float) function.Dy(interval.X0, interval.Y0);
            matrix[3] = (float) function.Dy(interval.X0, interval.Y1);
            matrix[4] = (float) function.Z(interval.X1, interval.Y0);
            matrix[5] = (float) function.Z(interval.X1, interval.Y1);
            matrix[6] = (float) function.Dy(interval.X1, interval.Y0);
            matrix[7] = (float) function.Dy(interval.X1, interval.Y1);
            matrix[8] = (float) function.Dx(interval.X0, interval.Y0);
            matrix[9] = (float) function.Dx(interval.X0, interval.Y1);
            matrix[10] = (float) function.Dxy(interval.X0, interval.Y0);
            matrix[11] = (float) function.Dxy(interval.X0, interval.Y1);
            matrix[12] = (float) function.Dx(interval.X1, interval.Y0);
            matrix[13] = (float) function.Dx(interval.X1, interval.Y1);
            matrix[14] = (float) function.Dxy(interval.X1, interval.Y0);
            matrix[15] = (float) function.Dxy(interval.X1, interval.Y1);

            return matrix;
        }
    }
}