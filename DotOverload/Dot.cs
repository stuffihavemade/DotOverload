using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DotOverload
{
    public static class Dot
    {
        public static T Safe<T>(Expression<Func<T>> expression)
        {
            return Bind<object, T>((current, rest) =>
            {
                if (current == null)
                    return default(T);
                else
                    return rest(current);
            })(expression);
        }

        public static U Id<T, U>(Expression<Func<U>> expression)
        {
            return Bind<T, U>((x, y) => y(x))(expression);
        }

        public static Func<Expression<Func<U>>, U> Bind<T, U>(Func<T, Func<T, U>, U> func)
        {
            return (exp) =>
            {
                var returnType = exp.Body.Type;
                LambdaExpression transformed = (LambdaExpression)new DotTransformer<T, U>(func, returnType).Transform(exp);
                var compiled = transformed.Compile();
                return (U)compiled.DynamicInvoke(null);
            };
        }
    }
}
