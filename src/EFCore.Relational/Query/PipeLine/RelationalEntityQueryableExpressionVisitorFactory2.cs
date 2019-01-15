// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.PipeLine;
using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public class RelationalEntityQueryableExpressionVisitorFactory2 : IEntityQueryableExpressionVisitorFactory2
    {
        private readonly IModel _model;

        public RelationalEntityQueryableExpressionVisitorFactory2(IModel model)
        {
            _model = model;
        }

        public EntityQueryableExpressionVisitor2 Create()
        {
            return new RelationalEntityQueryableExpressionVisitor2(_model);
        }
    }
}
