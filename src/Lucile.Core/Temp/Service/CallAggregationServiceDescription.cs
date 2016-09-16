using Codeworx.Reflection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Codeworx.Service
{
    [DataContract]
    public class CallAggregationServiceDescription
    {
        private string contractTypeName;
        private Type contractType;

        private object callLocker = new object();

        public CallAggregationServiceDescription(Type contractType) : this()
        {
            this.ContractType = contractType;
        }

        public CallAggregationServiceDescription()
        {
            this.Calls = new Collection<CallAggregationCallDescription>();
        }

        [DataMember]
        public string ContractTypeName
        {
            get
            {
                return contractTypeName;
            }
            set
            {
                this.contractTypeName = value;
                this.contractType = null;
            }
        }

        [IgnoreDataMember]
        public Type ContractType
        {
            get
            {
                if (contractType == null)
                    contractType = TypeResolver.GetType(this.ContractTypeName);
                return contractType;
            }
            set
            {
                this.ContractTypeName = value != null ? value.AssemblyQualifiedName : null;
                contractType = value;

            }
        }

        [DataMember]
        public Collection<CallAggregationCallDescription> Calls { get; private set; }

        public CallAggregationCallDescription GetOrAddCall(string methodName, IEnumerable<object> parameters)
        {
            CallAggregationCallDescription result = null;
            lock(callLocker){
                foreach(var item in Calls){
                    if(item.MethodName == methodName && parameters.SequenceEqual(item.Parameters)){
                        result = item;
                        break;
                    }    
                }
                if(result == null){
                    result = new CallAggregationCallDescription(methodName,parameters);
                    Calls.Add(result);
                }
            }

            return result;
        }
    }
}
