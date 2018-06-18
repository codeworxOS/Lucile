using System;
using System.Linq;
using System.Reflection;

namespace Lucile.Dynamic.Interceptor
{
    public class ImplementInterfaceInterceptor : ITypeDeclarationInterceptor
    {
        public ImplementInterfaceInterceptor(Type interfaceType)
        {
            this.InterfaceType = interfaceType;
            if (!this.InterfaceType.GetTypeInfo().IsInterface)
            {
                throw new ArgumentOutOfRangeException(nameof(interfaceType), "The type parameter must be an interface.");
            }
        }

        public Type InterfaceType { get; private set; }

        public void Intercept(DynamicTypeBuilder config, System.Reflection.Emit.TypeBuilder builder)
        {
            builder.AddInterfaceImplementation(this.InterfaceType);
            var baseMethods = builder.BaseType.GetMethods().ToList();
            var baseEvents = builder.BaseType.GetEvents();
            var baseProperties = builder.BaseType.GetProperties();

            var baseAccessorMethods = baseEvents.Select(p => p.GetAddMethod()).Union(
                                      baseEvents.Select(p => p.GetRemoveMethod())).Union(
                                      baseProperties.Select(p => p.GetSetMethod())).Union(
                                      baseProperties.Select(p => p.GetGetMethod())).Where(p => p != null);

            var interfaceProperties = this.InterfaceType.GetProperties();
            var interfaceEvents = this.InterfaceType.GetEvents();

            var interfaceAccessorMethods = interfaceEvents.Select(p => p.GetAddMethod()).Union(
                                          interfaceEvents.Select(p => p.GetRemoveMethod())).Union(
                                          interfaceProperties.Select(p => p.GetSetMethod())).Union(
                                          interfaceProperties.Select(p => p.GetGetMethod())).Where(p => p != null);

            foreach (var item in this.InterfaceType.GetMethods())
            {
                if (!IsExplicitImplemented(config, builder, item))
                {
                    if (!interfaceAccessorMethods.Contains(item))
                    {
                        var method = config.DynamicMembers.OfType<DynamicMethod>()
                                        .Where(p => p.MemberEquals(item))
                                        .Select(p => p.Method)
                                        .OfType<MethodInfo>()
                                        .FirstOrDefault();

                        if (method == null)
                        {
                            method = baseMethods.Where(p =>
                                    p.Name == item.Name &&
                                    p.ReturnType == item.ReturnType &&
                                    p.GetParameters().Length == item.GetParameters().Length &&
                                    p.GetParameters().Select((x, i) => new { Index = i, Value = x.ParameterType }).All(x => item.GetParameters()[x.Index].ParameterType == x.Value))
                                .FirstOrDefault();
                        }

                        if (method == null)
                        {
#if NETSTANDARD1_6
                            throw new MissingMemberException($"Missiong member {item.Name} on type {builder.Name}.");
#else
                            throw new MissingMemberException(builder.Name, item.Name);
#endif
                        }

                        builder.DefineMethodOverride(method, item);
                    }
                }
            }

            foreach (var item in this.InterfaceType.GetProperties())
            {
                var get = item.GetGetMethod();
                if (get != null && IsExplicitImplemented(config, builder, get))
                {
                    get = null;
                }

                var set = item.GetSetMethod();
                if (set != null && IsExplicitImplemented(config, builder, set))
                {
                    set = null;
                }

                if (get == null && set == null)
                {
                    continue;
                }

                var prop = config.DynamicMembers.OfType<DynamicProperty>().Where(p => p.MemberName == item.Name).FirstOrDefault();
                if (prop != null)
                {
                    if (get != null)
                    {
                        builder.DefineMethodOverride(prop.PropertyGetMethod, get);
                    }

                    if (set != null)
                    {
                        builder.DefineMethodOverride(prop.PropertySetMethod, set);
                    }
                }
                else
                {
                    var baseProp = baseProperties.Where(p => p.Name == item.Name).FirstOrDefault();
                    if (baseProp == null)
                    {
#if NETSTANDARD1_6
                        throw new MissingMemberException($"Missiong member {item.Name} on type {builder.Name}.");
#else
                        throw new MissingMemberException(builder.Name, item.Name);
#endif
                    }

                    if (get != null)
                    {
                        builder.DefineMethodOverride(baseProp.GetGetMethod(), get);
                    }

                    if (set != null)
                    {
                        builder.DefineMethodOverride(baseProp.GetSetMethod(), set);
                    }
                }
            }

            foreach (var item in this.InterfaceType.GetEvents())
            {
                var add = item.GetAddMethod();
                if (add != null && IsExplicitImplemented(config, builder, add))
                {
                    add = null;
                }

                var remove = item.GetRemoveMethod();
                if (remove != null && IsExplicitImplemented(config, builder, remove))
                {
                    remove = null;
                }

                if (add == null && remove == null)
                {
                    continue;
                }

                var evt = config.DynamicMembers.OfType<DynamicEvent>().Where(p => p.MemberName == item.Name).FirstOrDefault();
                if (evt != null)
                {
                    if (add != null)
                    {
                        builder.DefineMethodOverride(evt.AddMethod, add);
                    }

                    if (remove != null)
                    {
                        builder.DefineMethodOverride(evt.RemoveMethod, remove);
                    }
                }
                else
                {
                    var baseEvent = baseEvents.Where(p => p.Name == item.Name).FirstOrDefault();
                    if (baseEvent == null)
                    {
#if NETSTANDARD1_6
                        throw new MissingMemberException($"Missiong member {item.Name} on type {builder.Name}.");
#else
                        throw new MissingMemberException(builder.Name, item.Name);
#endif
                    }

                    if (add != null)
                    {
                        builder.DefineMethodOverride(baseEvent.GetAddMethod(), add);
                    }

                    if (remove != null)
                    {
                        builder.DefineMethodOverride(baseEvent.GetRemoveMethod(), remove);
                    }
                }
            }
        }

        private static bool IsExplicitImplemented(DynamicTypeBuilder config, System.Reflection.Emit.TypeBuilder builder, MethodInfo item)
        {
            var explicitMethod = config.DynamicMembers.OfType<DynamicMethod>().FirstOrDefault(p => p.IsExplicitImplementation(item));

            if (explicitMethod != null)
            {
                builder.DefineMethodOverride(explicitMethod.Method, item);
                return true;
            }

            return false;
        }
    }
}