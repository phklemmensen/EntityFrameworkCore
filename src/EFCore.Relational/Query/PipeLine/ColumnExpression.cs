// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public class ColumnExpression : Expression
    {
        private readonly IProperty _property;

        public ColumnExpression(IProperty property, TableExpressionBase table)
        {
            _property = property;
            Table = table;
        }

        public string Name => _property.Relational().ColumnName;

        public override Type Type => _property.ClrType;
        public override ExpressionType NodeType => ExpressionType.Extension;

        public TableExpressionBase Table { get; }
    }
}
