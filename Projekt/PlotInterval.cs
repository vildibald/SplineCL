namespace Projekt
{
   public struct PlotInterval
    {
        public float X0;
        public float Y0;
        public float X1;      
        public float Y1;

        public PlotInterval(float x0, float y0, float x1, float y1) : this()
        {
            X0 = x0;
            X1 = x1;
            Y0 = y0;
            Y1 = y1;
        }
    }
}