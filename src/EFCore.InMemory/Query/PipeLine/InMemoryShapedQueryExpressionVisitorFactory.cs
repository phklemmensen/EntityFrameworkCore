// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.PipeLine;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.PipeLine
{
    public class InMemoryShapedQueryExpressionVisitorFactory : IShapedQueryExpressionVisitorFactory
    {
        private readonly IEntityMaterializerSource _entityMaterializerSource;

        public InMemoryShapedQueryExpressionVisitorFactory(IEntityMaterializerSource entityMaterializerSource)
        {
            _entityMaterializerSource = entityMaterializerSource;
        }

        public ShapedQueryExpressionVisitor Create()
        {
            return new InMemoryShapedQueryExpressionVisitor(_entityMaterializerSource);
        }
    }

}
