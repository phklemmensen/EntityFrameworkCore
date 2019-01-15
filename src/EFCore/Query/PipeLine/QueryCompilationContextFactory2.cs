// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Query.PipeLine
{
    public class QueryCompilationContextFactory2 : IQueryCompilationContextFactory2
    {
        private readonly IEntityQueryableExpressionVisitorFactory2 _entityQueryableExpressionVisitorFactory;
        private readonly IShapedQueryExpressionVisitorFactory _shapedQueryExpressionVisitorFactory;
        private readonly IQueryableMethodTranslatingExpressionVisitorFactory _queryableMethodTranslatingExpressionVisitorFactory;

        public QueryCompilationContextFactory2(
            IEntityQueryableExpressionVisitorFactory2 entityQueryableExpressionVisitorFactory,
            IShapedQueryExpressionVisitorFactory shapedQueryExpressionVisitorFactory,
            IQueryableMethodTranslatingExpressionVisitorFactory queryableMethodTranslatingExpressionVisitorFactory)
        {
            _entityQueryableExpressionVisitorFactory = entityQueryableExpressionVisitorFactory;
            _shapedQueryExpressionVisitorFactory = shapedQueryExpressionVisitorFactory;
            _queryableMethodTranslatingExpressionVisitorFactory = queryableMethodTranslatingExpressionVisitorFactory;
        }

        public QueryCompilationContext2 Create(bool async)
        {
            return new QueryCompilationContext2(
                _entityQueryableExpressionVisitorFactory,
                _shapedQueryExpressionVisitorFactory,
                _queryableMethodTranslatingExpressionVisitorFactory);
        }
    }
}
