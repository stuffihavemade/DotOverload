using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DotOverload
{
    internal class DotTransformer<T, U>:ExpressionVisitor
    {
        private Expression<Func<T, Func<T, U>, U>> exp;
        private Type returnType;
        public DotTransformer(Func<T, Func<T, U>, U> func, Type returnType)
        {
            this.exp = (x, y) => func(x, y);
            this.returnType = returnType;
        }

        public Expression Transform(Expression exp)
        {
            return this.Visit(exp);
        }

        private Expression VisitAny(PipelineCreator pc)
        {
            var pipelines = pc.Pipeline();
            return new BindInserter<T, U>(pipelines, exp).Insert();
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            if (u.NodeType == ExpressionType.ArrayLength)
                return VisitAny(new PipelineCreator(u));
            else
                return base.VisitUnary(u);
        }
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            return VisitAny(new PipelineCreator(m));
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            return VisitAny(new PipelineCreator(m));
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            if (b.NodeType == ExpressionType.ArrayIndex)
                return VisitAny(new PipelineCreator(b));
            else
                return base.VisitBinary(b);
        }

        protected override Expression VisitInvocation(InvocationExpression iv)
        {
            return VisitAny(new PipelineCreator(iv));
        }
    }
}
