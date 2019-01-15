// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public class SqlCastExpression : Expression
    {

        public SqlCastExpression(Expression expression, Type type, string storeType)
        {
            Expression = expression;
            Type = type;
            StoreType = storeType;
        }


        public override ExpressionType NodeType => ExpressionType.Extension;

        public Expression Expression { get; }
        public override Type Type { get; }
        public string StoreType { get; }
    }
}
