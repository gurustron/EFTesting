using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EFTesting.Extensions
{
    public static class LinqExtensions
    {
        // LINQKit
        public static IQueryable<TSource> Like<TSource>(this IQueryable<TSource> source,
            Expression<Func<TSource, string>> expression, string[] words)
        {
            return source.AsExpandable()
                .Where(x => words.Any(w => EF.Functions.Like(expression.Invoke(x), "%" + w + "%")));
        }

        // Manual expression trees manipulation
        public static IQueryable<TSource> Like1<TSource>(this IQueryable<TSource> source,
            Expression<Func<TSource, string>> expression, string[] words)
        {
            Expression<Func<string, string, bool>> efLikeSource = (o, s) => EF.Functions.Like(o, "%" + s + "%");
            Expression<Func<TSource, bool>> wordsAny = x => words.Any(word => true);

            var wordsAnyBody = wordsAny.Body as MethodCallExpression;
            var efLikeBody =
                new ReplacingExpressionVisitor(new[] { efLikeSource.Parameters[0] }, new[] { expression.Body }).Visit(
                    efLikeSource.Body);
            var efLikeLambda = Expression.Lambda<Func<string, bool>>(efLikeBody, efLikeSource.Parameters[1]);
            var wordsAnyNewBody = Expression.Call(wordsAnyBody.Method, wordsAnyBody.Arguments.First(), efLikeLambda);
            var result = Expression.Lambda<Func<TSource, bool>>(wordsAnyNewBody, expression.Parameters);
            return source.Where(result);
        }
    }
}
