// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.PipeLine;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public class RelationalShapedQueryExpressionVisitorFactory : IShapedQueryExpressionVisitorFactory
    {
        private readonly IEntityMaterializerSource _entityMaterializerSource;
        private readonly IQuerySqlGeneratorFactory2 _querySqlGeneratorFactory;

        public RelationalShapedQueryExpressionVisitorFactory(IEntityMaterializerSource entityMaterializerSource,
            IQuerySqlGeneratorFactory2 querySqlGeneratorFactory)
        {
            _entityMaterializerSource = entityMaterializerSource;
            _querySqlGeneratorFactory = querySqlGeneratorFactory;
        }

        public ShapedQueryExpressionVisitor Create()
        {
            return new RelationalShapedQueryExpressionVisitor(_entityMaterializerSource,
                _querySqlGeneratorFactory);
        }
    }
}
