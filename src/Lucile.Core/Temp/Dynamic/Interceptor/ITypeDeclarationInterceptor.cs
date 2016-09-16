using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace Codeworx.Dynamic.Interceptor
{
    public interface ITypeDeclarationInterceptor
    {
        void Intercept(DynamicTypeBuilder config, TypeBuilder builder);
    }
}
