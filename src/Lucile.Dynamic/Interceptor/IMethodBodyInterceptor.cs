using System.Reflection.Emit;

namespace Lucile.Dynamic.Interceptor
{
    public interface IMethodBodyInterceptor
    {
        void Intercept(DynamicMember parent, MethodBuilder builder, ILGenerator generator, ref Label returnLabel);
    }
}