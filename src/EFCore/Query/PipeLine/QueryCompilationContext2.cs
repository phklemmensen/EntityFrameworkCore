// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;

namespace Microsoft.EntityFrameworkCore.Query.PipeLine
{
    public class QueryCompilationContext2
    {
        private readonly IEntityQueryableExpressionVisitorFactory2 _entityQueryableExpressionVisitorFactory;
        private readonly IShapedQueryExpressionVisitorFactory _shapedQueryExpressionVisitorFactory;

        public static readonly ParameterExpression QueryContextParameter
            = Expression.Parameter(typeof(QueryContext), "queryContext");
        private readonly IQueryableMethodTranslatingExpressionVisitorFactory _queryableMethodTranslatingExpressionVisitorFactory;

        public QueryCompilationContext2(
            IEntityQueryableExpressionVisitorFactory2 entityQueryableExpressionVisitorFactory,
            IShapedQueryExpressionVisitorFactory shapedQueryExpressionVisitorFactory,
            IQueryableMethodTranslatingExpressionVisitorFactory queryableMethodTranslatingExpressionVisitorFactory)
        {
            _entityQueryableExpressionVisitorFactory = entityQueryableExpressionVisitorFactory;
            _shapedQueryExpressionVisitorFactory = shapedQueryExpressionVisitorFactory;
            _queryableMethodTranslatingExpressionVisitorFactory = queryableMethodTranslatingExpressionVisitorFactory;
        }

        public virtual Func<QueryContext, TResult> CreateQueryExecutor<TResult>(Expression query)
        {
            // Convert EntityQueryable to ShapedQueryExpression
            query = _entityQueryableExpressionVisitorFactory.Create().Visit(query);

            query = _queryableMethodTranslatingExpressionVisitorFactory.Create(new Dictionary<Expression, Expression>()).Visit(query);

            // Inject actual entity materializer
            // Inject tracking
            query = _shapedQueryExpressionVisitorFactory.Create().Visit(query);

            return Expression.Lambda<Func<QueryContext, TResult>>(
                query,
                QueryContextParameter)
                .Compile();
        }
    }
}
