using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Jace.Operations;
using Jace.Util;
using System.Numerics;

namespace Jace.Execution
{
    public class DynamicCompiler : IExecutor
    {
        public Complex Execute(Operation operation, IFunctionRegistry functionRegistry)
        {
            return Execute(operation, functionRegistry, new Dictionary<string, Complex>());
        }

        public Complex Execute(Operation operation, IFunctionRegistry functionRegistry, 
            IDictionary<string, Complex> variables)
        {
            return BuildFormula(operation, functionRegistry)(variables);
        }

        public Func<IDictionary<string, Complex>, Complex> BuildFormula(Operation operation,
            IFunctionRegistry functionRegistry)
        {
            Func<FormulaContext, Complex> func = BuildFormulaInternal(operation, functionRegistry);
            return variables =>
                {
                    variables = EngineUtil.ConvertVariableNamesToLowerCase(variables);
                    FormulaContext context = new FormulaContext(variables, functionRegistry);
                    return func(context);
                };
        }

        private Func<FormulaContext, Complex> BuildFormulaInternal(Operation operation, 
            IFunctionRegistry functionRegistry)
        {
            ParameterExpression contextParameter = Expression.Parameter(typeof(FormulaContext), "context");

            LabelTarget returnLabel = Expression.Label(typeof(Complex));

            return Expression.Lambda<Func<FormulaContext, Complex>>(
                Expression.Block(
                    Expression.Return(returnLabel, GenerateMethodBody(operation, contextParameter, functionRegistry)),
                    Expression.Label(returnLabel, Expression.Constant(Complex.Zero))
                ),
                contextParameter
            ).Compile();
        }

        private Expression GenerateMethodBody(Operation operation, ParameterExpression contextParameter,
            IFunctionRegistry functionRegistry)
        {
            if (operation == null)
                throw new ArgumentNullException("operation");

            if (operation.GetType() == typeof(IntegerConstant))
            {
                IntegerConstant constant = (IntegerConstant)operation;

                return Expression.Convert(Expression.Constant(constant.Value, typeof(int)), typeof(Complex));
            }
            else if (operation.GetType() == typeof(FloatingPointConstant))
            {
                FloatingPointConstant constant = (FloatingPointConstant)operation;

                return Expression.Convert(Expression.Constant(constant.Value, typeof(double)), typeof(Complex));
            }
            else if (operation.GetType() == typeof(ComplexConstant))
            {
                ComplexConstant constant = (ComplexConstant)operation;

                return Expression.Constant(constant.Value, typeof(Complex));
            }
            else if (operation.GetType() == typeof(Variable))
            {
                Type contextType = typeof(FormulaContext);
                Type dictionaryType = typeof(IDictionary<string, Complex>);

                Variable variable = (Variable)operation;

                Expression getVariables = Expression.Property(contextParameter, "Variables");
                ParameterExpression value = Expression.Variable(typeof(Complex), "value");

                Expression variableFound = Expression.Call(getVariables,
                    dictionaryType.GetRuntimeMethod("TryGetValue", new Type[] { typeof(string), typeof(Complex).MakeByRefType() }),
                    Expression.Constant(variable.Name),
                    value);

                Expression throwException = Expression.Throw(
                    Expression.New(typeof(VariableNotDefinedException).GetConstructor(new Type[] { typeof(string) }),
                        Expression.Constant(string.Format("The variable \"{0}\" used is not defined.", variable.Name))));

                LabelTarget returnLabel = Expression.Label(typeof(Complex));

                return Expression.Block(
                    new[] { value },
                    Expression.IfThenElse(
                        variableFound,
                        Expression.Return(returnLabel, value),
                        throwException
                    ),
                    Expression.Label(returnLabel, Expression.Constant(Complex.Zero))
                );
            }
            else if (operation.GetType() == typeof(Multiplication))
            {
                Multiplication multiplication = (Multiplication)operation;
                Expression argument1 = GenerateMethodBody(multiplication.Argument1, contextParameter, functionRegistry);
                Expression argument2 = GenerateMethodBody(multiplication.Argument2, contextParameter, functionRegistry);

                return Expression.Multiply(argument1, argument2);
            }
            else if (operation.GetType() == typeof(Addition))
            {
                Addition addition = (Addition)operation;
                Expression argument1 = GenerateMethodBody(addition.Argument1, contextParameter, functionRegistry);
                Expression argument2 = GenerateMethodBody(addition.Argument2, contextParameter, functionRegistry);

                return Expression.Add(argument1, argument2);
            }
            else if (operation.GetType() == typeof(Subtraction))
            {
                Subtraction addition = (Subtraction)operation;
                Expression argument1 = GenerateMethodBody(addition.Argument1, contextParameter, functionRegistry);
                Expression argument2 = GenerateMethodBody(addition.Argument2, contextParameter, functionRegistry);

                return Expression.Subtract(argument1, argument2);
            }
            else if (operation.GetType() == typeof(Division))
            {
                Division division = (Division)operation;
                Expression dividend = GenerateMethodBody(division.Dividend, contextParameter, functionRegistry);
                Expression divisor = GenerateMethodBody(division.Divisor, contextParameter, functionRegistry);

                return Expression.Divide(dividend, divisor);
            }
            else if (operation.GetType() == typeof(Exponentiation))
            {
                Exponentiation exponentation = (Exponentiation)operation;
                Expression @base = GenerateMethodBody(exponentation.Base, contextParameter, functionRegistry);
                Expression exponent = GenerateMethodBody(exponentation.Exponent, contextParameter, functionRegistry);

                return Expression.Call(null, typeof(Complex).GetRuntimeMethod("Pow", new Type[] { typeof(Complex), typeof(Complex) }), @base, exponent);
            }
            else if (operation.GetType() == typeof(UnaryMinus))
            {
                UnaryMinus unaryMinus = (UnaryMinus)operation;
                Expression argument = GenerateMethodBody(unaryMinus.Argument, contextParameter, functionRegistry);
                return Expression.Negate(argument);
            }
            else if (operation.GetType() == typeof(Function))
            {
                Function function = (Function)operation;

                FunctionInfo functionInfo = functionRegistry.GetFunctionInfo(function.FunctionName);
                Type funcType = GetFuncType(functionInfo.NumberOfParameters);
                Type[] parameterTypes = (from i in Enumerable.Range(0, functionInfo.NumberOfParameters)
                                            select typeof(Complex)).ToArray();

                Expression[] arguments = new Expression[functionInfo.NumberOfParameters];
                for (int i = 0; i < functionInfo.NumberOfParameters; i++)
                    arguments[i] = GenerateMethodBody(function.Arguments[i], contextParameter, functionRegistry);

                Expression getFunctionRegistry = Expression.Property(contextParameter, "FunctionRegistry");

                ParameterExpression functionInfoVariable = Expression.Variable(typeof(FunctionInfo));

                return Expression.Block(
                    new[] { functionInfoVariable },
                    Expression.Assign(
                        functionInfoVariable,
                        Expression.Call(getFunctionRegistry, typeof(IFunctionRegistry).GetRuntimeMethod("GetFunctionInfo", new Type[] { typeof(string) }), Expression.Constant(function.FunctionName))
                    ),
                    Expression.Call(
                        Expression.Convert(Expression.Property(functionInfoVariable, "Function"), funcType),
                        funcType.GetRuntimeMethod("Invoke", parameterTypes),
                        arguments));
            }
            else
            {
                throw new ArgumentException(string.Format("Unsupported operation \"{0}\".", operation.GetType().FullName), "operation");
            }
        }

        private Type GetFuncType(int numberOfParameters)
        {
            string funcTypeName = string.Format("System.Func`{0}", numberOfParameters + 1);
            Type funcType = Type.GetType(funcTypeName);

            Type[] typeArguments = new Type[numberOfParameters + 1];
            for (int i = 0; i < typeArguments.Length; i++)
                typeArguments[i] = typeof(Complex);

            return funcType.MakeGenericType(typeArguments);
        }
    }
}
