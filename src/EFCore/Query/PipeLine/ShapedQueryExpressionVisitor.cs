// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.PipeLine
{
    public abstract class ShapedQueryExpressionVisitor : ExpressionVisitor
    {
        private readonly IEntityMaterializerSource _entityMaterializerSource;

        public ShapedQueryExpressionVisitor(IEntityMaterializerSource entityMaterializerSource)
        {
            _entityMaterializerSource = entityMaterializerSource;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case ShapedQueryExpression shapedQueryExpression:
                    return VisitShapedQueryExpression(shapedQueryExpression);
            }

            return base.VisitExtension(extensionExpression);
        }

        protected abstract Expression VisitShapedQueryExpression(ShapedQueryExpression shapedQueryExpression);

        protected virtual LambdaExpression InjectEntityMaterializer(
            LambdaExpression lambdaExpression)
        {
            var visitor = new EntityMaterializerInjectingExpressionVisitor(_entityMaterializerSource);

            var modifiedBody = visitor.Visit(lambdaExpression.Body);

            if (lambdaExpression.Body == modifiedBody)
            {
                return lambdaExpression;
            }

            var expressions = visitor.Expressions;
            expressions.Add(modifiedBody);

            return Expression.Lambda(Expression.Block(visitor.Variables, expressions), lambdaExpression.Parameters);
        }

        private class EntityMaterializerInjectingExpressionVisitor : ExpressionVisitor
        {
            private static readonly ConstructorInfo _materializationContextConstructor
                = typeof(MaterializationContext).GetConstructors().Single(ci => ci.GetParameters().Length == 2);
            private static readonly PropertyInfo _dbContextMemberInfo
                = typeof(QueryContext).GetProperty(nameof(QueryContext.Context));
            private static readonly MethodInfo _startTrackingMethodInfo
                = typeof(QueryContext).GetMethod(nameof(QueryContext.StartTracking), new[] { typeof(IEntityType), typeof(object) });
            private readonly IEntityMaterializerSource _entityMaterializerSource;

            public List<ParameterExpression> Variables { get; } = new List<ParameterExpression>();

            public List<Expression> Expressions { get; } = new List<Expression>();

            private int _currentEntityIndex;


            public EntityMaterializerInjectingExpressionVisitor(IEntityMaterializerSource entityMaterializerSource)
            {
                _entityMaterializerSource = entityMaterializerSource;
            }

            protected override Expression VisitExtension(Expression extensionExpresssion)
            {
                if (extensionExpresssion is EntityShaperExpression entityShaperExpression)
                {
                    var materializationContext = Expression.Variable(typeof(MaterializationContext), "materializationContext" + _currentEntityIndex);
                    Variables.Add(materializationContext);
                    Expressions.Add(
                        Expression.Assign(
                            materializationContext,
                            Expression.New(
                                _materializationContextConstructor,
                                entityShaperExpression.ValueBufferExpression,
                                Expression.MakeMemberAccess(
                                    QueryCompilationContext2.QueryContextParameter,
                                    _dbContextMemberInfo))));

                    var materializationExpression
                        = (BlockExpression)_entityMaterializerSource.CreateMaterializeExpression(
                            entityShaperExpression.EntityType,
                            "instance" + _currentEntityIndex++,
                            materializationContext);

                    Variables.AddRange(materializationExpression.Variables);
                    Expressions.AddRange(materializationExpression.Expressions.Take(materializationExpression.Expressions.Count - 1));
                    Expressions.Add(
                        Expression.Call(
                            QueryCompilationContext2.QueryContextParameter,
                            _startTrackingMethodInfo,
                            Expression.Constant(entityShaperExpression.EntityType),
                            materializationExpression.Expressions.Last()));

                    return materializationExpression.Expressions.Last();
                }

                if (extensionExpresssion is ProjectionBindingExpression)
                {
                    return extensionExpresssion;
                }

                return base.VisitExtension(extensionExpresssion);
            }
        }
    }

}
