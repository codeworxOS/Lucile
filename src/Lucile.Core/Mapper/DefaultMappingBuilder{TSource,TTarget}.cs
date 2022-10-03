using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Lucile.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace Lucile.Mapper
{
    public class DefaultMappingBuilder<TSource, TTarget> : IMappingBuilder<TSource, TTarget>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Expression<Func<TSource, TTarget>> _expression;

        public DefaultMappingBuilder(IServiceProvider serviceProvider, Expression<Func<TSource, TTarget>> expression)
        {
            _expression = expression;
            _serviceProvider = serviceProvider;
        }

        public IMappingBuilder<TSource, TNewTarget> Extend<TNewTarget>(System.Linq.Expressions.Expression<Func<TSource, TNewTarget>> mapping)
            where TNewTarget : TTarget
        {
            Dictionary<MemberInfo, Expression> members = new Dictionary<MemberInfo, Expression>();

            var init = _expression.Body as MemberInitExpression;
            var visitor = new ReplaceExpressionVisitor(_expression.Parameters.First(), mapping.Parameters.First());
            if (init != null)
            {
                foreach (var item in init.Bindings.OfType<MemberAssignment>())
                {
                    members[item.Member] = visitor.Visit(item.Expression);
                }
            }

            init = mapping.Body as MemberInitExpression;
            if (init == null)
            {
                throw new NotSupportedException("Only MemberInit expressions are supported!");
            }

            foreach (var item in init.Bindings.OfType<MemberAssignment>())
            {
                members[item.Member] = visitor.Visit(item.Expression);
            }

            return new DefaultMappingBuilder<TSource, TNewTarget>(
                    _serviceProvider,
                    Expression.Lambda<Func<TSource, TNewTarget>>(
                        Expression.MemberInit(
                            init.NewExpression,
                            members.Select(p => Expression.Bind(p.Key, p.Value))),
                        mapping.Parameters.First()));
        }

        public IMappingBuilder<TSource, TTarget> Partial<TPartial>()
        {
            return Partial<TSource, TPartial>();
        }

        public IMappingBuilder<TSource, TTarget> Partial<TPartialSource, TPartial>()
        {
            var partialType = typeof(TPartial);
            var targetType = typeof(TTarget);

            if (!targetType.GetBaseTypeStructure().Any(p => p.Value == partialType) && !targetType.GetInterfaces().Contains(partialType))
            {
                throw new NotSupportedException();
            }

            var partialSourceType = typeof(TPartialSource);
            var sourceType = typeof(TSource);

            if (!sourceType.GetBaseTypeStructure().Any(p => p.Value == partialSourceType) && !sourceType.GetInterfaces().Contains(partialSourceType))
            {
                throw new NotSupportedException();
            }

            var mapper = _serviceProvider.GetService<IMappingConfiguration<TPartialSource, TPartial>>();

            Dictionary<MemberInfo, Expression> members = new Dictionary<MemberInfo, Expression>();

            var currentInit = _expression.Body as MemberInitExpression;
            if (currentInit == null)
            {
                throw new NotSupportedException("Only MemberInit expressions are supported!");
            }

            foreach (var item in currentInit.Bindings.OfType<MemberAssignment>())
            {
                members[item.Member] = item.Expression;
            }

            var partialInit = mapper.Expression.Body as MemberInitExpression;
            var visitor = new ReplaceExpressionVisitor(mapper.Expression.Parameters.First(), _expression.Parameters.First());
            if (partialInit == null)
            {
                throw new NotSupportedException("Only MemberInit expressions are supported!");
            }

            foreach (var item in partialInit.Bindings.OfType<MemberAssignment>())
            {
                members[item.Member] = visitor.Visit(item.Expression);
            }

            return new DefaultMappingBuilder<TSource, TTarget>(
                   _serviceProvider,
                   Expression.Lambda<Func<TSource, TTarget>>(
                       Expression.MemberInit(
                           currentInit.NewExpression,
                           members.Select(p => Expression.Bind(p.Key, p.Value))),
                       _expression.Parameters.First()));
        }

        public IMappingConfiguration<TSource, TTarget> ToConfiguration()
        {
            var replacer = new FindMapExpressionVisitor();
            replacer.Visit(_expression.Body);

            var lambda = _expression;

            if (replacer.MapCalls.Any())
            {
                var body = _expression.Body;

                foreach (var item in replacer.MapCalls)
                {
                    var arguments = item.Method.GetGenericArguments();
                    var sourceType = arguments[0];
                    var targetType = arguments[1];

                    var subMapper = (IMappingConfiguration)_serviceProvider.GetRequiredService(typeof(IMappingConfiguration<,>).MakeGenericType(sourceType, targetType));

                    if (item.Method.ReturnType == targetType)
                    {
                        var subLambda = subMapper.ConversionExpression;
                        var subExpression = subLambda.Body;
                        subExpression = subExpression.Replace(subLambda.Parameters[0], item.Arguments[0]);

                        body = body.Replace(item, subExpression);
                    }
                    else if (item.Method.ReturnType == typeof(IEnumerable<>).MakeGenericType(targetType))
                    {
                        var selectMethod = DefaultMapperFactory.MethodInfoCache.EnumerableSelectMethod
                                                .MakeGenericMethod(sourceType, targetType);

                        body = body.Replace(item, Expression.Call(selectMethod, item.Arguments[0], subMapper.ConversionExpression));
                    }
                }

                lambda = Expression.Lambda<Func<TSource, TTarget>>(body, lambda.Parameters);
            }

            return new DefaultMappingConfiguration<TSource, TTarget>(lambda);
        }
    }
}