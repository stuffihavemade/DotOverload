using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DotOverload
{
    internal class PipelineCreator
    {
        Expression _exp;
        public PipelineCreator(MemberExpression exp)
        {
            _exp = exp;
        }

        public PipelineCreator(MethodCallExpression exp)
        {
            _exp = exp;
        }

        public PipelineCreator(InvocationExpression exp)
        {
            _exp = exp;
        }

        /// <summary>
        /// exp must be an ArrayIndex expression, or
        /// an <exception>ArgumentException</exception> will be thrown.
        /// </summary>
        public PipelineCreator(BinaryExpression exp)
        {
            if (exp.NodeType == ExpressionType.ArrayIndex)
                _exp = exp;
            else
            {
                var errorMessage = "exp must be an ArrayIndex expression.";
                throw new ArgumentException(errorMessage);
            }
        }

        /// <summary>
        /// exp must be an ArrayLength expression, or
        /// an <exception>ArgumentException</exception> will be thrown.
        /// </summary>
        public PipelineCreator(UnaryExpression exp)
        {
            if (exp.NodeType == ExpressionType.ArrayLength)
                _exp = exp;
            else
            {
                var errorMessage = "exp must be an ArrayLength expression.";
                throw new ArgumentException(errorMessage);
            }
        }


        /// <summary>
        /// Takes the leaf of an expression tree, say
        /// <code>
        /// "hello".ToUpper().Replace('H','Y')
        /// </code>
        /// and returns a collection of expressions looking like
        /// <code>
        /// {"hello",
        /// (x) => x.ToUpper(),
        /// (x) => x.Replace('H', 'Y')}
        /// </code>
        /// </summary>
        public PipelinedExpressions Pipeline()
        {
            var results = _pipeline(_exp, new List<Expression>());
            return new PipelinedExpressions(results.First(), results.Skip(1).Cast<LambdaExpression>());
        }

        private IEnumerable<Expression> _pipeline(Expression exp, IEnumerable<Expression> stack)
        {
            if (exp.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpression = (MemberExpression)exp;
                var paramType = memberExpression.Expression.Type;
                var param = Expression.Parameter(paramType, "x");
                var parameterizedDot = 
                    Expression.Lambda(
                        Expression.MakeMemberAccess(
                            param,
                            memberExpression.Member
                        ),
                        param
                    );
                var pushed = stack.Push(parameterizedDot);
                return _pipeline(memberExpression.Expression, pushed);
            }
            else if (exp.NodeType == ExpressionType.Call)
            {
                var callExpression = (MethodCallExpression)exp;
                var param = Expression.Parameter(callExpression.Object.Type, "x");
                var parameterizedDot = 
                    Expression.Lambda(
                        Expression.Call(
                            param,
                            callExpression.Method,
                            callExpression.Arguments
                        ),
                        param
                    );
                var pushed = stack.Push(parameterizedDot);
                return _pipeline(callExpression.Object, pushed);
            }
            else if (exp.NodeType == ExpressionType.ArrayLength)
            {
                var lengthExpression = (UnaryExpression)exp;
                var paramType = lengthExpression.Operand.Type;
                var param = Expression.Parameter(paramType, "x");
                var parameterizedDot = 
                    Expression.Lambda(
                        Expression.MakeUnary(
                            ExpressionType.ArrayLength,
                            param,
                            lengthExpression.Operand.Type
                        ),
                        param
                    );
                var pushed = stack.Push(parameterizedDot);
                return _pipeline(lengthExpression.Operand, pushed);
            }
            else if (exp.NodeType == ExpressionType.ArrayIndex)
            {
                var indexExpression = (BinaryExpression)exp;
                var paramType = indexExpression.Left.Type;
                var param = Expression.Parameter(paramType, "x");
                var parameterizedDot =
                    Expression.Lambda(
                        Expression.MakeBinary(
                            ExpressionType.ArrayIndex,
                            param,
                            indexExpression.Right
                        ),
                        param
                    );
                var pushed = stack.Push(parameterizedDot);
                return _pipeline(indexExpression.Left, pushed);
            }
            else if (exp.NodeType == ExpressionType.Invoke)
            {
                var invokeExpression = (InvocationExpression)exp;
                var paramType = invokeExpression.Expression.Type;
                var param = Expression.Parameter(paramType, "x");
                var parameterizedDot =
                    Expression.Lambda(
                        Expression.Invoke(
                            param,
                            invokeExpression.Arguments
                        ),
                        param
                    );
                var pushed = stack.Push(parameterizedDot);
                return _pipeline(invokeExpression.Expression, pushed);
            }
            else
            {
                //There are many other possiblities that can
                //be dotted into (e.g. constants, list inites, etc.)
                //but they all start at the front of a dot expression.
                return stack.Push(exp);
            }
        }
    }
}
