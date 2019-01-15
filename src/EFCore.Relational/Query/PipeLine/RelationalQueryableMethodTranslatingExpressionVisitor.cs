// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.PipeLine;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public class RelationalQueryableMethodTranslatingExpressionVisitor : QueryableMethodTranslatingExpressionVisitor
    {
        private readonly IRelationalTypeMappingSource _typeMappingSource;

        public RelationalQueryableMethodTranslatingExpressionVisitor(IRelationalTypeMappingSource typeMappingSource)
        {
            _typeMappingSource = typeMappingSource;
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.DeclaringType == typeof(Queryable))
            {
                var source = Visit(methodCallExpression.Arguments[0]);
                if (source is RelationalShapedQueryExpression shapedQueryExpression)
                {
                    var selectExpression = (SelectExpression)shapedQueryExpression.QueryExpression;
                    switch (methodCallExpression.Method.Name)
                    {
                        case nameof(Queryable.Select):
                            {
                                var selector = (LambdaExpression)((UnaryExpression)methodCallExpression.Arguments[1]).Operand;
                                if (selector.Body == selector.Parameters[0])
                                {
                                    return shapedQueryExpression;
                                }

                                var parameterBindings = new Dictionary<Expression, Expression>
                                {
                                    { selector.Parameters.Single(), shapedQueryExpression.ShaperExpression.Body }
                                };

                                var newSelectorBody = new ReplacingExpressionVisitor(parameterBindings).Visit(selector.Body);
                                newSelectorBody = new RelationalProjectionBindingExpressionVisitor(_typeMappingSource, selectExpression)
                                    .Translate(newSelectorBody);

                                shapedQueryExpression.ShaperExpression =
                                    Expression.Lambda(
                                        newSelectorBody,
                                    shapedQueryExpression.ShaperExpression.Parameters);

                                return shapedQueryExpression;
                            }
                        case nameof(Queryable.Where):
                            {
                                var predicate = (LambdaExpression)((UnaryExpression)methodCallExpression.Arguments[1]).Operand;

                                var parameterBindings = new Dictionary<Expression, Expression>
                                {
                                    { predicate.Parameters.Single(), shapedQueryExpression.ShaperExpression.Body }
                                };

                                var lambdaBody = new ReplacingExpressionVisitor(parameterBindings).Visit(predicate.Body);

                                var translation = new SqlTranslator(_typeMappingSource, selectExpression).Visit(lambdaBody);

                                if (translation is SqlExpression sqlExpression
                                    && sqlExpression.IsCondition)
                                {
                                    selectExpression.AddToPredicate(sqlExpression);

                                    return shapedQueryExpression;
                                }
                            }

                            throw new InvalidOperationException();

                    }
                }

                throw new NotImplementedException();
            }

            return base.VisitMethodCall(methodCallExpression);
        }
    }
}
