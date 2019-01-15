// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.PipeLine;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public class SqlTranslator : ExpressionVisitor
    {
        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly SelectExpression _selectExpression;
        private readonly TypeMappingInferringExpressionVisitor _typeInference;

        public SqlTranslator(IRelationalTypeMappingSource typeMappingSource, SelectExpression selectExpression)
        {
            _typeInference = new TypeMappingInferringExpressionVisitor();
            _typeMappingSource = typeMappingSource;
            _selectExpression = selectExpression;
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var innerExpression = Visit(memberExpression.Expression);
            if (innerExpression is EntityShaperExpression entityShaper)
            {
                var entityType = entityShaper.EntityType;
                var property = entityType.FindProperty(memberExpression.Member.GetSimpleMemberName());

                return _selectExpression.BindProperty(entityShaper.ValueBufferExpression, property);
            }

            return memberExpression.Update(innerExpression);
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            var newExpression = base.VisitBinary(binaryExpression);

            newExpression = _typeInference.Visit(newExpression);

            return newExpression;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is EntityShaperExpression)
            {
                return extensionExpression;
            }

            return base.VisitExtension(extensionExpression);
        }

        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            var operand = Visit(unaryExpression.Operand);

            if (operand is SqlExpression
                && unaryExpression.Type != typeof(object)
                && unaryExpression.NodeType == ExpressionType.Convert)
            {
                var typeMapping = _typeMappingSource.FindMapping(unaryExpression.Type);
                return new SqlExpression(
                    new SqlCastExpression(operand, unaryExpression.Type, typeMapping.StoreType),
                    typeMapping);
            }

            return unaryExpression.Update(operand);
        }
    }
}
