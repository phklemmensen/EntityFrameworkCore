// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.PipeLine;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public class SelectExpression : TableExpressionBase
    {
        private IDictionary<ProjectionMember, Expression> _projectionMapping
            = new Dictionary<ProjectionMember, Expression>();

        private List<TableExpressionBase> _tables = new List<TableExpressionBase>();
        private readonly List<Expression> _projection = new List<Expression>();
        private Expression _predicate;

        public IReadOnlyList<Expression> Projection => _projection;
        public IReadOnlyList<TableExpressionBase> Tables => _tables;
        public Expression Predicate => _predicate;

        public SelectExpression(IEntityType entityType)
            : base("")
        {
            var tableExpression = new TableExpression(
                entityType.Relational().TableName,
                entityType.Relational().Schema,
                entityType.Relational().TableName.ToLower().Substring(0,1));

            _tables.Add(tableExpression);

            _projectionMapping[new ProjectionMember()] = new EntityProjectionExpression(entityType, tableExpression);
        }

        public Expression BindProperty(Expression projectionExpression, IProperty property)
        {
            var member = (projectionExpression as ProjectionBindingExpression).ProjectionMember;

            return ((EntityProjectionExpression)_projectionMapping[member]).GetProperty(property);
        }

        public IDictionary<ProjectionMember, int> ApplyProjection()
        {
            var index = 0;
            var result = new Dictionary<ProjectionMember, int>();
            foreach (var keyValuePair in _projectionMapping)
            {
                result[keyValuePair.Key] = index;
                if (keyValuePair.Value is EntityProjectionExpression entityProjection)
                {
                    foreach (var property in entityProjection.EntityType.GetProperties())
                    {
                        _projection.Add(entityProjection.GetProperty(property));
                        index++;
                    }
                }
                else
                {
                    _projection.Add(keyValuePair.Value);
                    index++;
                }
            }

            return result;
        }

        public void AddToPredicate(Expression expression)
        {
            _predicate = expression;
        }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public void ApplyProjection(IDictionary<ProjectionMember, Expression> projectionMapping)
        {
            _projectionMapping = projectionMapping;
        }

        public Expression GetProjectionExpression(ProjectionMember projectionMember)
        {
            return _projectionMapping[projectionMember];
        }
    }
}
