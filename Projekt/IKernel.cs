using Cloo;

namespace Projekt
{
    public interface IKernel
    {
        ComputeContext Context { get; set; }
    }
}