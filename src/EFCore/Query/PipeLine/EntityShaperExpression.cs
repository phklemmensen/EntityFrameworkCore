// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query.PipeLine
{
    public class EntityShaperExpression : Expression
    {
        public EntityShaperExpression(IEntityType entityType, Expression valueBufferExpression)
        {
            EntityType = entityType;
            ValueBufferExpression = valueBufferExpression;
        }

        public override Type Type => EntityType.ClrType;

        public override ExpressionType NodeType => ExpressionType.Extension;

        public IEntityType EntityType { get; }
        public Expression ValueBufferExpression { get; }
    }

}
