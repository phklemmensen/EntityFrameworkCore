// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.PipeLine;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.PipeLine
{
    public class InMemoryEntityQueryableExpressionVisitorFactory2 : IEntityQueryableExpressionVisitorFactory2
    {
        private readonly IModel _model;

        public InMemoryEntityQueryableExpressionVisitorFactory2(IModel model)
        {
            _model = model;
        }

        public EntityQueryableExpressionVisitor2 Create()
        {
            return new InMemoryEntityQueryableExpressionVisitor2(_model);
        }
    }
}
