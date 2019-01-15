// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.PipeLine;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.PipeLine
{
    public class InMemoryQueryableMethodTranslatingExpressionVisitorFactory : IQueryableMethodTranslatingExpressionVisitorFactory
    {
        public QueryableMethodTranslatingExpressionVisitor Create(IDictionary<Expression, Expression> parameterBindings)
        {
            return new InMemoryQueryableMethodTranslatingExpressionVisitor(this, parameterBindings);
        }
    }

}
