using System;
using System.Linq.Expressions;

namespace API.Models.Handle
{
    public class DictionaryApiResponse
    {
        public static Expression<Func<T, bool>> AndAlso<T>(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            if (left == null) return right;
            if (right == null) return left;
            Expression<Func<T, bool>> ExpressionResult = Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(
                    left.Body, new ExpressionParameterReplacer(right.Parameters, left.Parameters).Visit(right.Body)),
                left.Parameters);
            return ExpressionResult;
        }
    }
}
