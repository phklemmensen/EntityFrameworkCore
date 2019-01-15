// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public abstract class TableExpressionBase : Expression
    {
        protected TableExpressionBase(string alias)
        {
            Alias = alias;
        }

        public string Alias { get; }

        public override Type Type => typeof(object);
        public override ExpressionType NodeType => ExpressionType.Extension;
    }
}
