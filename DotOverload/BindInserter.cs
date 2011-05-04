using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DotOverload
{
    internal class BindInserter<T, U>
    {
        private PipelinedExpressions _pipelines;
        private Expression<Func<T, Func<T, U>, U>> _bind;

        public BindInserter(PipelinedExpressions pipelines, Expression<Func<T, Func<T, U>, U>> bind)
        {
            _pipelines = pipelines;
            _bind = bind;
        }

        public Expression Insert()
        {
            return _insert(_pipelines.Lambdas.Cast<Expression>(), _bind, _pipelines.Root, true);
        }
        private Expression _insert(IEnumerable<Expression> exps, Expression<Func<T, Func<T, U>, U>> bind, Expression buildUp, bool root)
        {
            if (root)
            {
                var free = Expression.Parameter(typeof(T), "x");
                return Expression.Invoke(bind, Expression.Convert(buildUp, typeof(T)), Expression.Lambda(_insert(exps, bind, Expression.Convert(free, buildUp.Type), false), free));
            }
            else if (exps.Skip(1).Any())
            {
                var constant = buildUp;
                var callLambda = exps.First();
                var invoked = Expression.Invoke(callLambda, constant);
                var free = Expression.Parameter(typeof(T), "x");
                var rest = Expression.Lambda(_insert(exps.Skip(1), bind, Expression.Convert(free, invoked.Type), false), free);
                return Expression.Invoke(bind, Expression.Convert(invoked, typeof(T)), rest);
            }
            else
            {
                var constant = buildUp;
                var callLambda = exps.First();
                var invoked = Expression.Invoke(callLambda, constant);
                var free = Expression.Parameter(typeof(T), "x");
                var rest = Expression.Lambda(Expression.Convert(free, invoked.Type), free);
                return Expression.Invoke(bind, Expression.Convert(invoked, typeof(T)), rest);
            }
        }
    }
}
