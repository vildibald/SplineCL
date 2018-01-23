using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCLTemplate;
using Cloo;
using OpenCLTemplate.CLGLInterop;
using Projekt.AproximationEngine;

namespace Projekt
{
    class Program
    {

        private const string MeshFile = "mesh.txt";
   
        static void ErrMess()
        {
            Console.WriteLine("Chyba! Program bude ukonceny.");
            Console.Read();
            Environment.Exit(0);
        }

        private static void LoadInput(out AproximationExpression expression, out PlotInterval interval)
        {
            Console.WriteLine("Zadajte funkciu f(x, y) na aproximaciu.\nAlebo priamo stlacte enter pre f(x,y)=sin(sqrt(x^2+y^2))");
            var func = Console.ReadLine();
            if (func == "") func = "sin(sqrt(x^2+y^2))";
               
            Console.WriteLine("Zadajte interval aproximacie v tvare: \n x0 y0 x1 y1.\nAlebo priamo stlacte enter pre hodnoty (0, 0, 50, 50)");
            var line = Console.ReadLine();
            if (line == "") line = "0 0 50 50";
            var vals = line.Split(' ');
            if (vals.Length<4) 
                ErrMess();
            interval = new PlotInterval(float.Parse(vals[0]), float.Parse(vals[1]), float.Parse(vals[2]), float.Parse(vals[3]));
            expression = new AproximationExpression(func,"x","y");
        }

        static void Main(string[] args)
        {
           
            PlotInterval interval;
            AproximationExpression expression;
            LoadInput(out expression, out interval);

          
            var platform = LoadPlatform();

            var device = LoadDevice(platform);

            var properties = new ComputeContextPropertyList(platform);
            var context = new ComputeContext(device.Type, properties, null, IntPtr.Zero);

            
            var bhcl = new BicubicHermiteCl(context);
            var sw = Stopwatch.StartNew();
            var mesh = bhcl.CreateMesh(expression,interval);
            var time = sw.ElapsedMilliseconds;
            sw.Stop();
            
            Console.WriteLine("Cas vypoctu: "+time+" ms.");
            Console.ReadLine();

        }

        private static void SaveMesh(Float4[] mesh, string filePath)
        {
            using (var sw = new StreamWriter(filePath, false))
            {
                for (int i = 0; i < mesh.Length; i++)
                {
                    sw.WriteLine(mesh[i].ToString());    
                }
                
                
            }
        }

        private static ComputeDevice LoadDevice(ComputePlatform platform)
        {
            int idx;
            while (true)
            {
                Console.WriteLine("\n\nZvolte cislo zariadenia na vypocet:");
                var devicesCount = platform.Devices.Count;
                for (int i = 0; i < devicesCount; i++)
                {
                    Console.WriteLine(i + ": " + platform.Devices[i].Name);
                }

               
                var parseSucceded = int.TryParse(Console.ReadLine(), out idx);


                if (parseSucceded && -1 < idx && idx < devicesCount) break;

            }
            var device = platform.Devices[idx];
            return device;
        }

        private static ComputePlatform LoadPlatform()
        {
            int idx;
            while (true)
            {
                Console.WriteLine("\n\nZvolte cislo platformy na vypocet:");
                var platformsCount = ComputePlatform.Platforms.Count;
                for (int i = 0; i < platformsCount; i++)
                {
                    Console.WriteLine(i + ": " + ComputePlatform.Platforms[i].Name);
                }

                var parseSucceded = int.TryParse(Console.ReadLine(), out idx);
                if (parseSucceded && -1 < idx && idx < platformsCount) break;
            }
            var platform = ComputePlatform.Platforms[idx];
            return platform;
        }
    }
}
