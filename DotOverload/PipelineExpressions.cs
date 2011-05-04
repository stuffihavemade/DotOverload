using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DotOverload
{
    internal class PipelinedExpressions
    {
        public Expression Root { get; private set; }

        private IEnumerable<LambdaExpression> _lambdas;
        public IEnumerable<LambdaExpression> Lambdas
        {
            get { return _lambdas; }
            private set { _lambdas = _valid(Root, value); }
        }


        public PipelinedExpressions(Expression root, IEnumerable<LambdaExpression> lamdas)
        {
            Root = root;
            Lambdas = lamdas.ToList();
        }

        public bool OnlyOneLambda()
        {
            return ! Lambdas.Skip(1).Any();
        }

        private IEnumerable<LambdaExpression> _valid(Expression root, IEnumerable<LambdaExpression> lambdas)
        {
            if (!lambdas.Any())
                throw new ArgumentException("There must exist at least one lambda.");

            if (root.Type != lambdas.First().Parameters.First().Type)
                throw new ArgumentException("Root value must be able to be passed into first lambda");

            if (!_lambasAllHaveOneParameter(lambdas))
                throw new ArgumentException("Each lambda expression must take exactly one parameter.");

            if (!_lambdasCanPipeline(lambdas))
            {
                var msg = "The return type of each lamda must match the type of the next lambas's parameter";
                throw new ArgumentException(msg);
            }

            return lambdas;
        }

        private bool _lambasAllHaveOneParameter(IEnumerable<LambdaExpression> lambdas)
        {
            return !lambdas
                .Select(e => e.Parameters.Count)
                .Where(c => c != 1)
                .Any();
        }

        private bool _lambdasCanPipeline(IEnumerable<LambdaExpression> lambdas)
        {
            if (lambdas.Skip(1).Any())
            {
                if (lambdas.First().Body.Type != lambdas.Skip(1).First().Parameters.First().Type)
                {
                    return false;
                }
                else
                {
                    return _lambdasCanPipeline(lambdas.Skip(1));
                }
            }
            else
            {
                return true;
            }
        }
    }
}