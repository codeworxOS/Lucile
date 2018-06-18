using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace Lucile.Dynamic.Interceptor
{
    public interface ITypeDeclarationInterceptor
    {
        void Intercept(DynamicTypeBuilder config, TypeBuilder builder);
    }
}
