// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public class TableExpression : TableExpressionBase
    {
        public TableExpression(string table, string schema, string alias)
            : base(alias)
        {
            Table = table;
            Schema = schema;
        }

        public string Table { get; }
        public string Schema { get; }
    }
}
