using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Codeworx.Dynamic.Interceptor;

namespace Codeworx.Dynamic
{
    public abstract class DynamicMethod : DynamicMember
    {
        List<Type> argumentTypes;

        List<GenericTypeParameterBuilder> genericParameters;
        List<GenericTypeParameterBuilder> genericImplementationParameters;

        public ReadOnlyCollection<Type> ArgumentTypes { get; private set; }

        public ReadOnlyCollection<GenericTypeParameterBuilder> GenericParameters { get; private set; }
        public ReadOnlyCollection<GenericTypeParameterBuilder> GenericImplementationParameters { get; private set; }

        public MethodBuilder ImplementationMethod { get; protected set; }
        public MethodBuilder Method { get; protected set; }

        public MethodInfo BaseMethod { get; private set; }

        public bool IsOverride { get; private set; }

        public bool IsProtected { get; set; }

        protected virtual IEnumerable<string> GetGenericArguments()
        {
            yield break;
        }

        public virtual bool IsExplicitImplementation(MethodInfo methodInfo) {
            return false;
        }

        public DynamicMethod(string methodName, Delegate method, bool isProtected)
            : base(methodName, method.Method.ReturnType)
        {
            this.IsProtected = isProtected;
            this.genericParameters = new List<GenericTypeParameterBuilder>();
            this.GenericParameters = new ReadOnlyCollection<GenericTypeParameterBuilder>(this.genericParameters);

            this.genericImplementationParameters = new List<GenericTypeParameterBuilder>();
            this.GenericImplementationParameters = new ReadOnlyCollection<GenericTypeParameterBuilder>(this.genericImplementationParameters);

            this.argumentTypes = new List<Type>(method.Method.GetParameters().Select(p => p.ParameterType));
            this.ArgumentTypes = new ReadOnlyCollection<Type>(this.argumentTypes);
        }

        public DynamicMethod(string memberName, Type memberType, bool isProtected, params Type[] arguments)
            : base(memberName, memberType)
        {
            this.IsProtected = isProtected;

            this.genericParameters = new List<GenericTypeParameterBuilder>();
            this.GenericParameters = new ReadOnlyCollection<GenericTypeParameterBuilder>(this.genericParameters);

            this.genericImplementationParameters = new List<GenericTypeParameterBuilder>();
            this.GenericImplementationParameters = new ReadOnlyCollection<GenericTypeParameterBuilder>(this.genericImplementationParameters);

            this.argumentTypes = new List<Type>(arguments);
            this.ArgumentTypes = new ReadOnlyCollection<Type>(this.argumentTypes);
        }

        public override void CreateDeclarations(System.Reflection.Emit.TypeBuilder typeBuilder)
        {
            this.BaseMethod = typeBuilder.BaseType.GetMethods().OfType<MethodInfo>()
                                .Where(p => p.Name == this.MemberName)
                                .FirstOrDefault(p =>
                                    p.ReturnType == this.MemberType
                                    && p.GetParameters().Length == this.ArgumentTypes.Count
                                    && p.GetParameters().Select((x, i) => new { Index = i, Type = x.ParameterType }).All(x => this.ArgumentTypes[x.Index] == x.Type));

            if (BaseMethod != null && !BaseMethod.IsVirtual) {
                throw new MemberCreationException(this.MemberName, "The method has to be declared virtual on the base class.");
            } else if (BaseMethod != null) {
                IsOverride = true;
            }

            MethodAttributes methodAttributes = MethodAttributes.Public;

            if (IsProtected) {
                methodAttributes = MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
            } else {
                methodAttributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
            }
            var implementationAttributes = MethodAttributes.Private | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
            Method = typeBuilder.DefineMethod(this.MemberName, methodAttributes);
            ImplementationMethod = typeBuilder.DefineMethod(string.Format("{0}_Implementation", this.MemberName), implementationAttributes);

            var genericParameters = GetGenericArguments().ToList();

            if (genericParameters.Any()) {
                this.genericParameters.AddRange(Method.DefineGenericParameters(genericParameters.ToArray()));
                this.genericImplementationParameters.AddRange(ImplementationMethod.DefineGenericParameters(genericParameters.ToArray()));
            }

            this.Method.SetReturnType(TypeLookup(this.MemberType));
            this.Method.SetParameters(TypeLookup(this.ArgumentTypes).ToArray());

            this.ImplementationMethod.SetReturnType(ImplementationTypeLookup(this.MemberType));
            this.ImplementationMethod.SetParameters(ImplementationTypeLookup(this.ArgumentTypes).ToArray());

            if (IsOverride) {
                typeBuilder.DefineMethodOverride(this.Method, this.BaseMethod);
            }
        }

        protected Type TypeLookup(Type type)
        {
            if (type is GenericType) {
                return this.GenericParameters.FirstOrDefault(p => p.Name == ((GenericType)type).GenericName);
            }
            return type;
        }

        protected Type ImplementationTypeLookup(Type type)
        {
            if (type is GenericType) {
                return this.GenericImplementationParameters.FirstOrDefault(p => p.Name == ((GenericType)type).GenericName);
            }
            return type;
        }

        protected IEnumerable<Type> TypeLookup(IEnumerable<Type> types)
        {
            foreach (var item in types) {
                yield return TypeLookup(item);
            }
        }

        protected IEnumerable<Type> ImplementationTypeLookup(IEnumerable<Type> types)
        {
            foreach (var item in types) {
                yield return ImplementationTypeLookup(item);
            }
        }

        public override bool MemberEquals(MemberInfo member)
        {
            var method = member as MethodInfo;

            if (method != null) {
                var parameters = method.GetParameters().Select(p => p.ParameterType).ToList();

                return this.MemberName == member.Name &&
                        this.TypeEquals(this.MemberType, method.ReturnType) &&
                        parameters.Count == this.ArgumentTypes.Count &&
                        this.ArgumentTypes.Select((d, i) => new { Type = d, Index = i }).All(p => this.TypeEquals(p.Type, parameters[p.Index]));

            }
            return false;
        }

        public override sealed void Implement(DynamicTypeBuilder config, TypeBuilder typeBuilder)
        {
            Implement(config, typeBuilder, ImplementationMethod.GetILGenerator());
            var il = this.Method.GetILGenerator();
            var parameterArrayVar = il.DeclareLocal(typeof(object[]));

            bool isAsync = false;
            if (this.MemberType != null && typeof(Task).IsAssignableFrom(TypeLookup(this.MemberType))) {
                isAsync = true;
            }

            var ctxType = isAsync ? typeof(AsyncInterceptionContext) : typeof(InterceptionContext);

            LocalBuilder ctxVar = il.DeclareLocal(ctxType);
            ConstructorInfo icCtor = ctxType.GetConstructors().First();

            ConstructorInfo delegateCtor = null;
            if (this.MemberType == null) {
                Assembly assembly = null;
                if (this.ArgumentTypes.Count < 9) {
                    assembly = typeof(Action).Assembly;
                } else {
                    assembly = typeof(Enumerable).Assembly;
                }
                
                var type = assembly.GetType(string.Format("System.Action{0}", this.ArgumentTypes.Count > 0 ? "`" + this.argumentTypes.Count.ToString() : string.Empty));
                delegateCtor = type.GetConstructor(new[] { typeof(object), typeof(IntPtr) });
                if (argumentTypes.Count > 0)
                    type = type.MakeGenericType(TypeLookup(this.ArgumentTypes).ToArray());

                if (this.argumentTypes.OfType<GenericType>().Any())
                    delegateCtor = TypeBuilder.GetConstructor(type, delegateCtor);
                else
                    delegateCtor = type.GetConstructor(new[] { typeof(object), typeof(IntPtr) });
            } else {
                Assembly assembly = null;
                if (this.ArgumentTypes.Count < 9) {
                    assembly = typeof(Action).Assembly;
                } else {
                    assembly = typeof(Enumerable).Assembly;
                }
                var type = assembly.GetType(string.Format("System.Func`{0}", this.ArgumentTypes.Count + 1));
                delegateCtor = type.GetConstructor(new[] { typeof(object), typeof(IntPtr) });
                type = type.MakeGenericType(TypeLookup(this.ArgumentTypes).Concat(new[] { TypeLookup(this.MemberType) }).ToArray());

                if (this.argumentTypes.OfType<GenericType>().Any() || this.MemberType is GenericType)
                    delegateCtor = TypeBuilder.GetConstructor(type, delegateCtor);
                else
                    delegateCtor = type.GetConstructor(new[] { typeof(object), typeof(IntPtr) });
            }

            var returnLabel = il.DefineLabel();

            il.Emit(OpCodes.Ldc_I4, this.ArgumentTypes.Count);
            il.Emit(OpCodes.Newarr, typeof(object));
            il.Emit(OpCodes.Stloc, parameterArrayVar);

            for (int i = 1; i <= this.ArgumentTypes.Count; i++) {
                il.Emit(OpCodes.Ldloc, parameterArrayVar);
                il.Emit(OpCodes.Ldc_I4, i - 1);
                il.Emit(OpCodes.Ldarg, i);
                var type = TypeLookup(this.argumentTypes[i - 1]);
                il.Emit(OpCodes.Box, type);
                il.Emit(OpCodes.Stelem_Ref);
            }
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldstr, this.MemberName);

            il.Emit(OpCodes.Ldarg_0);
            if (this.ImplementationMethod.IsGenericMethod) {
                il.Emit(OpCodes.Ldftn, this.ImplementationMethod.MakeGenericMethod(this.GenericParameters.ToArray()));
            } else {
                il.Emit(OpCodes.Ldftn, this.ImplementationMethod);
            }
            il.Emit(OpCodes.Newobj, delegateCtor);
            il.Emit(OpCodes.Ldloc, parameterArrayVar);
            il.Emit(OpCodes.Newobj, icCtor);
            il.Emit(OpCodes.Stloc, ctxVar);

            List<MethodInfo> interceptors = new List<MethodInfo>();

            if (isAsync) {

                interceptors = typeBuilder.BaseType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(p => !p.IsPrivate && p.GetCustomAttributes(typeof(MethodInterceptorAttribute), true).Any() && p.GetParameters().Count() == 1 && p.GetParameters().First().ParameterType == typeof(AsyncInterceptionContext) && p.ReturnType == typeof(Task))
                    .ToList();
            } else {
                interceptors = typeBuilder.BaseType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(p => !p.IsPrivate && p.GetCustomAttributes(typeof(MethodInterceptorAttribute), true).Any() && p.GetParameters().Count() == 1 && p.GetParameters().First().ParameterType == typeof(InterceptionContext))
                        .ToList();
            }

            var registerMethod = ctxType.GetMethod("RegisterInterceptor");

            foreach (var item in interceptors) {
                il.Emit(OpCodes.Ldloc, ctxVar);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldftn, item);
                if (isAsync) {
                    il.Emit(OpCodes.Newobj, typeof(Func<AsyncInterceptionContext, Task>).GetConstructor(new[] { typeof(object), typeof(IntPtr) }));
                } else {
                    il.Emit(OpCodes.Newobj, typeof(Action<InterceptionContext>).GetConstructor(new[] { typeof(object), typeof(IntPtr) }));
                }
                il.Emit(OpCodes.Ldc_I4, (int)item.GetCustomAttributes(typeof(MethodInterceptorAttribute), true).OfType<MethodInterceptorAttribute>().First().InterceptionMode);
                if (isAsync) {
                    il.Emit(OpCodes.Newobj, typeof(DelegateAsyncMethodInterceptor).GetConstructors().First());
                } else {
                    il.Emit(OpCodes.Newobj, typeof(DelegateMethodInterceptor).GetConstructors().First());
                }
                il.EmitCall(OpCodes.Callvirt, registerMethod, null);
            }

            if (isAsync) {
                var taskType = TypeLookup(this.MemberType).IsGenericType ? TypeLookup(this.MemberType).GetGenericArguments().First() : typeof(object);

                il.Emit(OpCodes.Ldloc, ctxVar);
                il.EmitCall(OpCodes.Callvirt, ctxType.GetMethod("ExecuteAsync").MakeGenericMethod(taskType), null);
            } else {
                il.Emit(OpCodes.Ldloc, ctxVar);
                il.EmitCall(OpCodes.Callvirt, typeof(InterceptionContext).GetMethod("Execute"), null);
                if (this.MemberType != null) {
                    il.Emit(OpCodes.Ldloc, ctxVar);
                    il.Emit(OpCodes.Callvirt, typeof(InterceptionContext).GetProperty("Result").GetGetMethod());
                    var returnType = TypeLookup(this.MemberType);
                    if (returnType.IsValueType || returnType.IsGenericParameter) {
                        il.Emit(OpCodes.Unbox_Any, TypeLookup(this.MemberType));
                    } else {
                        il.Emit(OpCodes.Castclass, returnType);
                    }
                }
            }
            //else {

            //    il.Emit(OpCodes.Ldarg_0);
            //    for (int i = 1; i <= this.ArgumentTypes.Count(); i++) {
            //        il.Emit(OpCodes.Ldarg, i);
            //    }
            //    if (this.ImplementationMethod.IsGenericMethod) {
            //        il.EmitCall(OpCodes.Call, this.ImplementationMethod.MakeGenericMethod(this.GenericParameters.ToArray()), null);
            //    } else {
            //        il.EmitCall(OpCodes.Call, this.ImplementationMethod, null);
            //    }
            //}
            il.Emit(OpCodes.Ret);
        }

        protected abstract void Implement(DynamicTypeBuilder config, TypeBuilder typeBuilder, ILGenerator il);
    }
}
