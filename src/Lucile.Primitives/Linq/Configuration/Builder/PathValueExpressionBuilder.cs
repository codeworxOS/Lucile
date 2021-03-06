﻿using System.Runtime.Serialization;

namespace Lucile.Linq.Configuration.Builder
{
    [DataContract(IsReference = true)]
    public class PathValueExpressionBuilder : ValueExpressionBuilder
    {
        [DataMember(Order = 1)]
        public string Path { get; set; }

        public override void LoadFrom(ValueExpression value)
        {
            var path = Get<PathValueExpression>(value);

            Path = path.Path;
        }

        protected override ValueExpression BuildTarget()
        {
            return new PathValueExpression(Path);
        }
    }
}