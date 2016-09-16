using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Codeworx.Core.Service;

namespace Codeworx.Service
{
    [Export(typeof(ICallAggregatonService))]
    public class CallAggregationService : ICallAggregatonService
    {
        private static MethodInfo CallServiceMethodInfo;

        static CallAggregationService()
        {
            CallServiceMethodInfo = typeof(CallAggregationService).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(p => p.Name == "CallServiceAsync" && p.IsGenericMethod)
                .Single();
        }

        public async Task<IEnumerable<CallAggregationResult>> GetResultsAsync(IEnumerable<CallAggregationServiceDescription> services)
        {
            var requests = from s in services
                           from c in s.Calls
                           select new { Service = s, Call = c };

            var tasks = requests.Select(p => CallServiceAsync(p.Service, p.Call)).ToList();

#if(!SILVERLIGHT && !NET4)
            await Task.WhenAll(tasks);
#else
            await TaskEx.WhenAll(tasks);
#endif
            return tasks.Select(p => p.Result);
        }

        private async Task<CallAggregationResult> CallServiceAsync(CallAggregationServiceDescription service, CallAggregationCallDescription call)
        {
#if(!SILVERLIGHT)
            Stopwatch sw = null;
            if (call.MeasureDuration) {
                sw = new Stopwatch();
                sw.Start();
            }
#endif
            var param1 = Expression.Parameter(typeof(CallAggregationService));
            var method = service.ContractType.GetMethod(call.MethodName);
            var returnType = method.ReturnType.GetGenericArguments().First();
            var callServiceMethod = CallServiceMethodInfo.MakeGenericMethod(service.ContractType, returnType);

            var delegateParam1 = Expression.Parameter(service.ContractType);
            var callDelegate = Expression.Lambda(
                                    Expression.Call(
                                        delegateParam1,
                                        method,
                                        call.Parameters.Select((p, i) => Expression.Constant(p, method.GetParameters()[i].ParameterType))
                                    ),
                                    delegateParam1);

            var body = Expression.Call(
                param1,
                callServiceMethod,
                callDelegate
                );

            var compiled = Expression.Lambda<Func<CallAggregationService, Task<object>>>(body, param1).Compile();
            var result = await compiled(this);
            var aggResult = call.CreateResult(result);
#if(!SILVERLIGHT)
            if (call.MeasureDuration) {
                sw.Stop();
                aggResult.Duration = sw.Elapsed;
            }
#endif

            return aggResult;
        }

        private async Task<object> CallServiceAsync<TService, TResult>(Func<TService, Task<TResult>> call) where TService : class
        {
            var service = await ServiceContext.Current.GetServiceAsync<TService>();
            var result = await call(service);
            return result;
        }
    }
}
