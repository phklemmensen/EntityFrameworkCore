// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.PipeLine;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.PipeLine
{
    public class InMemoryQueryableMethodTranslatingExpressionVisitor : QueryableMethodTranslatingExpressionVisitor
    {
        private readonly InMemoryQueryableMethodTranslatingExpressionVisitorFactory _inMemoryQueryableMethodTranslatingExpressionVisitorFactory;
        private readonly IDictionary<Expression, Expression> _parameterBindings;

        public InMemoryQueryableMethodTranslatingExpressionVisitor(
            InMemoryQueryableMethodTranslatingExpressionVisitorFactory inMemoryQueryableMethodTranslatingExpressionVisitorFactory,
            IDictionary<Expression, Expression> parameterBindings)
        {
            _inMemoryQueryableMethodTranslatingExpressionVisitorFactory = inMemoryQueryableMethodTranslatingExpressionVisitorFactory;
            _parameterBindings = parameterBindings;
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.DeclaringType == typeof(Queryable))
            {
                var source = Visit(methodCallExpression.Arguments[0]);
                if (source is InMemoryShapedQueryExpression shapedQueryExpression)
                {
                    var inMemoryQueryExpression = (InMemoryQueryExpression)shapedQueryExpression.QueryExpression;
                    // TODO: check number of args to each method
                    switch (methodCallExpression.Method.Name)
                    {
                        // Single Result - Scalar - Projection Independent
                        case nameof(Queryable.All):
                            {
                                inMemoryQueryExpression.ServerQueryExpression =
                                    Expression.Call(
                                        InMemoryLinqOperatorProvider.All.MakeGenericMethod(typeof(ValueBuffer)),
                                        inMemoryQueryExpression.ServerQueryExpression,
                                        TranslateLambdaExpression(
                                            shapedQueryExpression, methodCallExpression.Arguments[1]));

                                inMemoryQueryExpression.MakeSingleProjection(methodCallExpression.Type);

                                shapedQueryExpression.ShaperExpression
                                    = Expression.Lambda(
                                        new ProjectionBindingExpression(
                                            inMemoryQueryExpression,
                                            new ProjectionMember(),
                                            methodCallExpression.Type),
                                        shapedQueryExpression.ShaperExpression.Parameters);

                                return shapedQueryExpression;
                            }

                        case nameof(Queryable.Any):
                            {
                                if (methodCallExpression.Arguments.Count == 1)
                                {
                                    inMemoryQueryExpression.ServerQueryExpression =
                                        Expression.Call(
                                            InMemoryLinqOperatorProvider.Any.MakeGenericMethod(typeof(ValueBuffer)),
                                            inMemoryQueryExpression.ServerQueryExpression);
                                }
                                else
                                {
                                    inMemoryQueryExpression.ServerQueryExpression =
                                        Expression.Call(
                                            InMemoryLinqOperatorProvider.AnyPredicate.MakeGenericMethod(typeof(ValueBuffer)),
                                            inMemoryQueryExpression.ServerQueryExpression,
                                            TranslateLambdaExpression(
                                                shapedQueryExpression, methodCallExpression.Arguments[1]));
                                }

                                inMemoryQueryExpression.MakeSingleProjection(methodCallExpression.Type);

                                shapedQueryExpression.ShaperExpression
                                    = Expression.Lambda(
                                        new ProjectionBindingExpression(
                                            inMemoryQueryExpression,
                                            new ProjectionMember(),
                                            methodCallExpression.Type),
                                        shapedQueryExpression.ShaperExpression.Parameters);

                                return shapedQueryExpression;
                            }

                        case nameof(Queryable.Count):
                            {
                                if (methodCallExpression.Arguments.Count == 1)
                                {
                                    inMemoryQueryExpression.ServerQueryExpression =
                                        Expression.Call(
                                            InMemoryLinqOperatorProvider.Count.MakeGenericMethod(typeof(ValueBuffer)),
                                            inMemoryQueryExpression.ServerQueryExpression);
                                }
                                else
                                {
                                    inMemoryQueryExpression.ServerQueryExpression =
                                        Expression.Call(
                                            InMemoryLinqOperatorProvider.CountPredicate.MakeGenericMethod(typeof(ValueBuffer)),
                                            inMemoryQueryExpression.ServerQueryExpression,
                                            TranslateLambdaExpression(
                                                shapedQueryExpression, methodCallExpression.Arguments[1]));
                                }

                                inMemoryQueryExpression.MakeSingleProjection(methodCallExpression.Type);

                                shapedQueryExpression.ShaperExpression
                                    = Expression.Lambda(
                                        new ProjectionBindingExpression(
                                            inMemoryQueryExpression,
                                            new ProjectionMember(),
                                            methodCallExpression.Type),
                                        shapedQueryExpression.ShaperExpression.Parameters);

                                return shapedQueryExpression;
                            }

                        case nameof(Queryable.LongCount):
                            {
                                if (methodCallExpression.Arguments.Count == 1)
                                {
                                    inMemoryQueryExpression.ServerQueryExpression =
                                        Expression.Call(
                                            InMemoryLinqOperatorProvider.LongCount.MakeGenericMethod(typeof(ValueBuffer)),
                                            inMemoryQueryExpression.ServerQueryExpression);
                                }
                                else
                                {
                                    inMemoryQueryExpression.ServerQueryExpression =
                                        Expression.Call(
                                            InMemoryLinqOperatorProvider.LongCountPredicate.MakeGenericMethod(typeof(ValueBuffer)),
                                            inMemoryQueryExpression.ServerQueryExpression,
                                            TranslateLambdaExpression(
                                                shapedQueryExpression, methodCallExpression.Arguments[1]));
                                }

                                inMemoryQueryExpression.MakeSingleProjection(methodCallExpression.Type);

                                shapedQueryExpression.ShaperExpression
                                    = Expression.Lambda(
                                        new ProjectionBindingExpression(
                                            inMemoryQueryExpression,
                                            new ProjectionMember(),
                                            methodCallExpression.Type),
                                        shapedQueryExpression.ShaperExpression.Parameters);

                                return shapedQueryExpression;
                            }

                        // Single Result - Scalar - Projection Type dependent
                        case nameof(Queryable.Average):
                            {
                                if (methodCallExpression.Arguments.Count == 1)
                                {
                                    source = inMemoryQueryExpression.GetScalarProjection();

                                    inMemoryQueryExpression.ServerQueryExpression =
                                        Expression.Call(
                                            InMemoryLinqOperatorProvider
                                                .GetAggregateMethod(
                                                    nameof(Enumerable.Average), source.Type.TryGetSequenceType()),
                                            source);
                                }
                                else
                                {
                                    var selector = TranslateLambdaExpression(
                                        shapedQueryExpression, methodCallExpression.Arguments[1]);

                                    inMemoryQueryExpression.ServerQueryExpression =
                                        Expression.Call(
                                            InMemoryLinqOperatorProvider
                                                .GetAggregateMethod(
                                                    nameof(Enumerable.Average), selector.ReturnType, parameterCount: 1)
                                                .MakeGenericMethod(typeof(ValueBuffer)),
                                            inMemoryQueryExpression.ServerQueryExpression,
                                            selector);
                                }

                                inMemoryQueryExpression.MakeSingleProjection(methodCallExpression.Type);

                                shapedQueryExpression.ShaperExpression
                                    = Expression.Lambda(
                                        new ProjectionBindingExpression(
                                            inMemoryQueryExpression,
                                            new ProjectionMember(),
                                            methodCallExpression.Type),
                                        shapedQueryExpression.ShaperExpression.Parameters);

                                return shapedQueryExpression;
                            }

                        case nameof(Queryable.Sum):
                            {
                                if (methodCallExpression.Arguments.Count == 1)
                                {
                                    source = inMemoryQueryExpression.GetScalarProjection();

                                    inMemoryQueryExpression.ServerQueryExpression =
                                        Expression.Call(
                                            InMemoryLinqOperatorProvider
                                                .GetAggregateMethod(
                                                    nameof(Enumerable.Sum), source.Type.TryGetSequenceType()),
                                            source);
                                }
                                else
                                {
                                    var selector = TranslateLambdaExpression(
                                        shapedQueryExpression, methodCallExpression.Arguments[1]);

                                    inMemoryQueryExpression.ServerQueryExpression =
                                        Expression.Call(
                                            InMemoryLinqOperatorProvider
                                                .GetAggregateMethod(
                                                    nameof(Enumerable.Sum), selector.ReturnType, parameterCount: 1)
                                                .MakeGenericMethod(typeof(ValueBuffer)),
                                            inMemoryQueryExpression.ServerQueryExpression,
                                            selector);
                                }

                                inMemoryQueryExpression.MakeSingleProjection(methodCallExpression.Type);

                                shapedQueryExpression.ShaperExpression
                                    = Expression.Lambda(
                                        new ProjectionBindingExpression(
                                            inMemoryQueryExpression,
                                            new ProjectionMember(),
                                            methodCallExpression.Type),
                                        shapedQueryExpression.ShaperExpression.Parameters);

                                return shapedQueryExpression;
                            }

                        case nameof(Queryable.Min):
                            {
                                if (methodCallExpression.Arguments.Count == 1)
                                {
                                    source = inMemoryQueryExpression.GetScalarProjection();

                                    inMemoryQueryExpression.ServerQueryExpression =
                                        Expression.Call(
                                            InMemoryLinqOperatorProvider
                                                .GetAggregateMethod(
                                                    nameof(Enumerable.Min), source.Type.TryGetSequenceType()),
                                            source);
                                }
                                else
                                {
                                    var selector = TranslateLambdaExpression(
                                        shapedQueryExpression, methodCallExpression.Arguments[1]);

                                    inMemoryQueryExpression.ServerQueryExpression =
                                        Expression.Call(
                                            InMemoryLinqOperatorProvider
                                                .GetAggregateMethod(
                                                    nameof(Enumerable.Min), selector.ReturnType, parameterCount: 1)
                                                .MakeGenericMethod(typeof(ValueBuffer)),
                                            inMemoryQueryExpression.ServerQueryExpression,
                                            selector);
                                }

                                inMemoryQueryExpression.MakeSingleProjection(methodCallExpression.Type);

                                shapedQueryExpression.ShaperExpression
                                    = Expression.Lambda(
                                        new ProjectionBindingExpression(
                                            inMemoryQueryExpression,
                                            new ProjectionMember(),
                                            methodCallExpression.Type),
                                        shapedQueryExpression.ShaperExpression.Parameters);

                                return shapedQueryExpression;
                            }

                        case nameof(Queryable.Max):
                            {
                                if (methodCallExpression.Arguments.Count == 1)
                                {
                                    source = inMemoryQueryExpression.GetScalarProjection();

                                    inMemoryQueryExpression.ServerQueryExpression =
                                        Expression.Call(
                                            InMemoryLinqOperatorProvider
                                                .GetAggregateMethod(
                                                    nameof(Enumerable.Min), source.Type.TryGetSequenceType()),
                                            source);
                                }
                                else
                                {
                                    var selector = TranslateLambdaExpression(
                                        shapedQueryExpression, methodCallExpression.Arguments[1]);

                                    inMemoryQueryExpression.ServerQueryExpression =
                                        Expression.Call(
                                            InMemoryLinqOperatorProvider
                                                .GetAggregateMethod(
                                                    nameof(Enumerable.Min), selector.ReturnType, parameterCount: 1)
                                                .MakeGenericMethod(typeof(ValueBuffer)),
                                            inMemoryQueryExpression.ServerQueryExpression,
                                            selector);
                                }

                                inMemoryQueryExpression.MakeSingleProjection(methodCallExpression.Type);

                                shapedQueryExpression.ShaperExpression
                                    = Expression.Lambda(
                                        new ProjectionBindingExpression(
                                            inMemoryQueryExpression,
                                            new ProjectionMember(),
                                            methodCallExpression.Type),
                                        shapedQueryExpression.ShaperExpression.Parameters);

                                return shapedQueryExpression;
                            }

                        case nameof(Queryable.Contains):
                            {
                                var item = TranslateExpression(
                                            inMemoryQueryExpression,
                                            methodCallExpression.Arguments[1]);

                                inMemoryQueryExpression.ServerQueryExpression =
                                    Expression.Call(
                                        InMemoryLinqOperatorProvider.Contains.MakeGenericMethod(item.Type),
                                        inMemoryQueryExpression.GetScalarProjection(),
                                        item);

                                inMemoryQueryExpression.MakeSingleProjection(methodCallExpression.Type);

                                shapedQueryExpression.ShaperExpression
                                    = Expression.Lambda(
                                        new ProjectionBindingExpression(
                                            inMemoryQueryExpression,
                                            new ProjectionMember(),
                                            methodCallExpression.Type),
                                        shapedQueryExpression.ShaperExpression.Parameters);

                                return shapedQueryExpression;
                            }

                        // Projection
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
                                newSelectorBody = new InMemoryProjectionBindingExpressionVisitor(inMemoryQueryExpression)
                                    .Translate(newSelectorBody);

                                shapedQueryExpression.ShaperExpression =
                                    Expression.Lambda(
                                        newSelectorBody,
                                    shapedQueryExpression.ShaperExpression.Parameters);

                                return shapedQueryExpression;
                            }

                        // Server operation - Non shape changing - type independent
                        case nameof(Queryable.Where):
                            {
                                inMemoryQueryExpression.ServerQueryExpression
                                    = Expression.Call(
                                        InMemoryLinqOperatorProvider.Where
                                            .MakeGenericMethod(typeof(ValueBuffer)),
                                        inMemoryQueryExpression.ServerQueryExpression,
                                        TranslateLambdaExpression(shapedQueryExpression, methodCallExpression.Arguments[1]));

                                return shapedQueryExpression;
                            }

                        case nameof(Queryable.Skip):
                            {
                                inMemoryQueryExpression.ServerQueryExpression
                                    = Expression.Call(
                                        InMemoryLinqOperatorProvider.Skip
                                            .MakeGenericMethod(typeof(ValueBuffer)),
                                        inMemoryQueryExpression.ServerQueryExpression,
                                        TranslateExpression(
                                            inMemoryQueryExpression,
                                            methodCallExpression.Arguments[1]));

                                return shapedQueryExpression;
                            }

                        case nameof(Queryable.Take):
                            {
                                inMemoryQueryExpression.ServerQueryExpression
                                    = Expression.Call(
                                        InMemoryLinqOperatorProvider.Take
                                            .MakeGenericMethod(typeof(ValueBuffer)),
                                        inMemoryQueryExpression.ServerQueryExpression,
                                        TranslateExpression(
                                            inMemoryQueryExpression,
                                            methodCallExpression.Arguments[1]));

                                return shapedQueryExpression;
                            }

                        // Server operation - Non shape changing - type dependent
                        case nameof(Queryable.OrderBy):
                            {
                                var newKeySelector = TranslateLambdaExpression(shapedQueryExpression,
                                    methodCallExpression.Arguments[1]);

                                inMemoryQueryExpression.ServerQueryExpression
                                    = Expression.Call(
                                        InMemoryLinqOperatorProvider.OrderBy
                                            .MakeGenericMethod(typeof(ValueBuffer), newKeySelector.ReturnType),
                                        inMemoryQueryExpression.ServerQueryExpression,
                                        newKeySelector);

                                return shapedQueryExpression;
                            }

                        case nameof(Queryable.OrderByDescending):
                            {
                                var newKeySelector = TranslateLambdaExpression(shapedQueryExpression,
                                    methodCallExpression.Arguments[1]);

                                inMemoryQueryExpression.ServerQueryExpression
                                    = Expression.Call(
                                        InMemoryLinqOperatorProvider.OrderByDescending
                                            .MakeGenericMethod(typeof(ValueBuffer), newKeySelector.ReturnType),
                                        inMemoryQueryExpression.ServerQueryExpression,
                                        newKeySelector);

                                return shapedQueryExpression;
                            }

                        case nameof(Queryable.ThenBy):
                            {
                                var newKeySelector = TranslateLambdaExpression(shapedQueryExpression,
                                    methodCallExpression.Arguments[1]);

                                inMemoryQueryExpression.ServerQueryExpression
                                    = Expression.Call(
                                        InMemoryLinqOperatorProvider.ThenBy
                                            .MakeGenericMethod(typeof(ValueBuffer), newKeySelector.ReturnType),
                                        inMemoryQueryExpression.ServerQueryExpression,
                                        newKeySelector);

                                return shapedQueryExpression;
                            }

                        case nameof(Queryable.ThenByDescending):
                            {
                                var newKeySelector = TranslateLambdaExpression(shapedQueryExpression,
                                    methodCallExpression.Arguments[1]);

                                inMemoryQueryExpression.ServerQueryExpression
                                    = Expression.Call(
                                        InMemoryLinqOperatorProvider.ThenByDescending
                                            .MakeGenericMethod(typeof(ValueBuffer), newKeySelector.ReturnType),
                                        inMemoryQueryExpression.ServerQueryExpression,
                                        newKeySelector);

                                return shapedQueryExpression;
                            }

                        // Requires projection on server side
                        case nameof(Queryable.Distinct):
                            {
                                inMemoryQueryExpression.ApplyServerProjection();
                                inMemoryQueryExpression.ServerQueryExpression
                                    = Expression.Call(
                                        InMemoryLinqOperatorProvider.Distinct.MakeGenericMethod(typeof(ValueBuffer)),
                                        inMemoryQueryExpression.ServerQueryExpression);

                                return shapedQueryExpression;
                            }

                        case nameof(Queryable.First):
                            {
                                if (methodCallExpression.Arguments.Count == 1)
                                {
                                    inMemoryQueryExpression.ServerQueryExpression =
                                        Expression.Call(
                                            InMemoryLinqOperatorProvider.First.MakeGenericMethod(typeof(ValueBuffer)),
                                            inMemoryQueryExpression.ServerQueryExpression);
                                }
                                else
                                {
                                    inMemoryQueryExpression.ServerQueryExpression =
                                        Expression.Call(
                                            InMemoryLinqOperatorProvider.FirstPredicate
                                                .MakeGenericMethod(typeof(ValueBuffer)),
                                            inMemoryQueryExpression.ServerQueryExpression,
                                            TranslateLambdaExpression(
                                                shapedQueryExpression, methodCallExpression.Arguments[1]));
                                }

                                inMemoryQueryExpression.SingleResult = true;

                                return shapedQueryExpression;
                            }

                        case nameof(Queryable.FirstOrDefault):
                            {
                                if (methodCallExpression.Arguments.Count == 1)
                                {
                                    inMemoryQueryExpression.ServerQueryExpression =
                                        Expression.Call(
                                            InMemoryLinqOperatorProvider.FirstOrDefault.MakeGenericMethod(typeof(ValueBuffer)),
                                            inMemoryQueryExpression.ServerQueryExpression);
                                }
                                else
                                {
                                    inMemoryQueryExpression.ServerQueryExpression =
                                        Expression.Call(
                                            InMemoryLinqOperatorProvider.FirstOrDefaultPredicate
                                                .MakeGenericMethod(typeof(ValueBuffer)),
                                            inMemoryQueryExpression.ServerQueryExpression,
                                            TranslateLambdaExpression(
                                                shapedQueryExpression, methodCallExpression.Arguments[1]));
                                }

                                inMemoryQueryExpression.SingleResult = true;

                                return shapedQueryExpression;
                            }

                        case nameof(Queryable.Last):
                            {
                                if (methodCallExpression.Arguments.Count == 1)
                                {
                                    inMemoryQueryExpression.ServerQueryExpression =
                                        Expression.Call(
                                            InMemoryLinqOperatorProvider.Last.MakeGenericMethod(typeof(ValueBuffer)),
                                            inMemoryQueryExpression.ServerQueryExpression);
                                }
                                else
                                {
                                    inMemoryQueryExpression.ServerQueryExpression =
                                        Expression.Call(
                                            InMemoryLinqOperatorProvider.LastPredicate
                                                .MakeGenericMethod(typeof(ValueBuffer)),
                                            inMemoryQueryExpression.ServerQueryExpression,
                                            TranslateLambdaExpression(
                                                shapedQueryExpression, methodCallExpression.Arguments[1]));
                                }

                                inMemoryQueryExpression.SingleResult = true;

                                return shapedQueryExpression;
                            }

                        case nameof(Queryable.LastOrDefault):
                            {
                                if (methodCallExpression.Arguments.Count == 1)
                                {
                                    inMemoryQueryExpression.ServerQueryExpression =
                                        Expression.Call(
                                            InMemoryLinqOperatorProvider.LastOrDefault.MakeGenericMethod(typeof(ValueBuffer)),
                                            inMemoryQueryExpression.ServerQueryExpression);
                                }
                                else
                                {
                                    inMemoryQueryExpression.ServerQueryExpression =
                                        Expression.Call(
                                            InMemoryLinqOperatorProvider.LastOrDefaultPredicate
                                                .MakeGenericMethod(typeof(ValueBuffer)),
                                            inMemoryQueryExpression.ServerQueryExpression,
                                            TranslateLambdaExpression(
                                                shapedQueryExpression, methodCallExpression.Arguments[1]));
                                }

                                inMemoryQueryExpression.SingleResult = true;

                                return shapedQueryExpression;
                            }

                        case nameof(Queryable.Single):
                            {
                                if (methodCallExpression.Arguments.Count == 1)
                                {
                                    inMemoryQueryExpression.ServerQueryExpression =
                                        Expression.Call(
                                            InMemoryLinqOperatorProvider.Single.MakeGenericMethod(typeof(ValueBuffer)),
                                            inMemoryQueryExpression.ServerQueryExpression);
                                }
                                else
                                {
                                    inMemoryQueryExpression.ServerQueryExpression =
                                        Expression.Call(
                                            InMemoryLinqOperatorProvider.SinglePredicate
                                                .MakeGenericMethod(typeof(ValueBuffer)),
                                            inMemoryQueryExpression.ServerQueryExpression,
                                            TranslateLambdaExpression(
                                                shapedQueryExpression, methodCallExpression.Arguments[1]));
                                }

                                inMemoryQueryExpression.SingleResult = true;

                                return shapedQueryExpression;
                            }

                        case nameof(Queryable.SingleOrDefault):
                            {
                                if (methodCallExpression.Arguments.Count == 1)
                                {
                                    inMemoryQueryExpression.ServerQueryExpression =
                                        Expression.Call(
                                            InMemoryLinqOperatorProvider.SingleOrDefault.MakeGenericMethod(typeof(ValueBuffer)),
                                            inMemoryQueryExpression.ServerQueryExpression);
                                }
                                else
                                {
                                    inMemoryQueryExpression.ServerQueryExpression =
                                        Expression.Call(
                                            InMemoryLinqOperatorProvider.SingleOrDefaultPredicate
                                                .MakeGenericMethod(typeof(ValueBuffer)),
                                            inMemoryQueryExpression.ServerQueryExpression,
                                            TranslateLambdaExpression(
                                                shapedQueryExpression, methodCallExpression.Arguments[1]));
                                }

                                inMemoryQueryExpression.SingleResult = true;

                                return shapedQueryExpression;
                            }

                        // Complex
                        case nameof(Queryable.Join):
                            {

                                if (base.Visit(methodCallExpression.Arguments[1]) is InMemoryShapedQueryExpression innerSource)
                                {
                                    var outerKeySelector = TranslateLambdaExpression(
                                        shapedQueryExpression, methodCallExpression.Arguments[2]);

                                    var innerKeySelector = TranslateLambdaExpression(
                                        innerSource, methodCallExpression.Arguments[3]);

                                    if (outerKeySelector != null && innerKeySelector != null)
                                    {

                                    }
                                }

                                break;
                            }
                        case nameof(Queryable.GroupJoin):
                        case nameof(Queryable.GroupBy):
                        case nameof(Queryable.DefaultIfEmpty):

                        // Future improvements - Not supported in 2.2
                        case nameof(Queryable.ElementAt):
                        case nameof(Queryable.ElementAtOrDefault):
                        case nameof(Queryable.Aggregate):
                        case nameof(Queryable.Zip):
                        case nameof(Queryable.TakeWhile):
                        case nameof(Queryable.SkipWhile):
                        case nameof(Queryable.Reverse):
                        case nameof(Queryable.SequenceEqual):

                        // Waiting for Maumar
                        case nameof(Queryable.SelectMany):

                        // Breaking this
                        case nameof(Queryable.OfType):
                        case nameof(Queryable.Cast):
                        case nameof(Queryable.Concat):
                        case nameof(Queryable.Union):
                        case nameof(Queryable.Intersect):
                        case nameof(Queryable.Except):
                            break;
                    }
                }


                throw new NotImplementedException();
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        private static Expression TranslateExpression(
            InMemoryQueryExpression inMemoryQueryExpression,
            Expression expression)
        {
            return new Translator(inMemoryQueryExpression).Visit(expression);
        }

        private static LambdaExpression TranslateLambdaExpression(
            InMemoryShapedQueryExpression shapedQueryExpression, Expression expression)
        {
            var lambdaExpression = (LambdaExpression)((UnaryExpression)expression).Operand;

            var parameterBindings = new Dictionary<Expression, Expression>
            {
                { lambdaExpression.Parameters.Single(), shapedQueryExpression.ShaperExpression.Body }
            };

            var lambdaBody = new ReplacingExpressionVisitor(parameterBindings).Visit(lambdaExpression.Body);

            return Expression.Lambda(
                TranslateExpression((InMemoryQueryExpression)shapedQueryExpression.QueryExpression, lambdaBody),
                InMemoryQueryExpression.ValueBufferParameter);
        }
    }

}
