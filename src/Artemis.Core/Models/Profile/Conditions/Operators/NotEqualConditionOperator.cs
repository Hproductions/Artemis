﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Artemis.Core.Models.Profile.Conditions.Operators
{
    public class NotEqualConditionOperator : DisplayConditionOperator
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => new List<Type> { typeof(object) };

        public override string Description => "Does not equal";
        public override string Icon => "NotEqualVariant";

        public override BinaryExpression CreateExpression(Expression leftSide, Expression rightSide)
        {
            return Expression.NotEqual(leftSide, rightSide);
        }
    }
}