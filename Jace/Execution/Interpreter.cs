using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jace.Operations;
using Jace.Util;
using System.Numerics;

namespace Jace.Execution
{
    public class Interpreter : IExecutor
    {
        public Func<IDictionary<string, Complex>, Complex> BuildFormula(Operation operation, 
            IFunctionRegistry functionRegistry)
        { 
            return variables =>
                {
                    variables = EngineUtil.ConvertVariableNamesToLowerCase(variables);
                    return Execute(operation, functionRegistry, variables);
                };
        }

        public Complex Execute(Operation operation, IFunctionRegistry functionRegistry)
        {
            return Execute(operation, functionRegistry, new Dictionary<string, Complex>());
        }

        public Complex Execute(Operation operation, IFunctionRegistry functionRegistry, 
            IDictionary<string, Complex> variables)
        {
            if (operation == null)
                throw new ArgumentNullException("operation");

            if (operation.GetType() == typeof(IntegerConstant))
            {
                IntegerConstant constant = (IntegerConstant)operation;
                return constant.Value;
            }
            else if (operation.GetType() == typeof(FloatingPointConstant))
            {
                FloatingPointConstant constant = (FloatingPointConstant)operation;
                return constant.Value;
            }
            else if (operation.GetType() == typeof(ComplexConstant))
            {
                ComplexConstant constant = (ComplexConstant)operation;
                return constant.Value;
            }
            else if (operation.GetType() == typeof(Variable))
            {
                Variable variable = (Variable)operation;

                Complex value;
                bool variableFound = variables.TryGetValue(variable.Name, out value);

                if (variableFound)
                    return value;
                else
                    throw new VariableNotDefinedException(string.Format("The variable \"{0}\" used is not defined.", variable.Name));
            }
            else if (operation.GetType() == typeof(Multiplication))
            {
                Multiplication multiplication = (Multiplication)operation;
                return Execute(multiplication.Argument1, functionRegistry, variables) * Execute(multiplication.Argument2, functionRegistry, variables);
            }
            else if (operation.GetType() == typeof(Addition))
            {
                Addition addition = (Addition)operation;
                return Execute(addition.Argument1, functionRegistry, variables) + Execute(addition.Argument2, functionRegistry, variables);
            }
            else if (operation.GetType() == typeof(Subtraction))
            {
                Subtraction addition = (Subtraction)operation;
                return Execute(addition.Argument1, functionRegistry, variables) - Execute(addition.Argument2, functionRegistry, variables);
            }
            else if (operation.GetType() == typeof(Division))
            {
                Division division = (Division)operation;
                return Execute(division.Dividend, functionRegistry, variables) / Execute(division.Divisor, functionRegistry, variables);
            }
            else if (operation.GetType() == typeof(Exponentiation))
            {
                Exponentiation exponentiation = (Exponentiation)operation;
                return Complex.Pow(Execute(exponentiation.Base, functionRegistry, variables), Execute(exponentiation.Exponent, functionRegistry, variables));
            }
            else if (operation.GetType() == typeof(UnaryMinus))
            {
                UnaryMinus unaryMinus = (UnaryMinus)operation;
                return -Execute(unaryMinus.Argument, functionRegistry, variables);
            }
            else if (operation.GetType() == typeof(Function))
            {
                Function function = (Function)operation;

                FunctionInfo functionInfo = functionRegistry.GetFunctionInfo(function.FunctionName);

                Complex[] arguments = new Complex[functionInfo.NumberOfParameters];
                for (int i = 0; i < arguments.Length; i++)
                    arguments[i] = Execute(function.Arguments[i], functionRegistry, variables);

                return Invoke(functionInfo.Function, arguments);
            }
            else
            {
                throw new ArgumentException(string.Format("Unsupported operation \"{0}\".", operation.GetType().FullName), "operation");
            }
        }

        private Complex Invoke(Delegate function, Complex[] arguments)
        {
            // DynamicInvoke is slow, so we first try to convert it to a Func
            if (function is Func<Complex>)
            {
                return ((Func<Complex>)function).Invoke();
            }
            else if (function is Func<Complex, Complex>)
            {
                return ((Func<Complex, Complex>)function).Invoke(arguments[0]);
            }
            else if (function is Func<Complex, Complex, Complex>)
            {
                return ((Func<Complex, Complex, Complex>)function).Invoke(arguments[0], arguments[1]);
            }
            else if (function is Func<Complex, Complex, Complex, Complex>)
            {
                return ((Func<Complex, Complex, Complex, Complex>)function).Invoke(arguments[0], arguments[1], arguments[2]);
            }
            else if (function is Func<Complex, Complex, Complex, Complex, Complex>)
            {
                return ((Func<Complex, Complex, Complex, Complex, Complex>)function).Invoke(arguments[0], arguments[1], arguments[2], arguments[3]);
            }
#if !WINDOWS_PHONE_7
            else if (function is Func<Complex, Complex, Complex, Complex, Complex, Complex>)
            {
                return ((Func<Complex, Complex, Complex, Complex, Complex, Complex>)function).Invoke(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4]);
            }
            else if (function is Func<Complex, Complex, Complex, Complex, Complex, Complex, Complex>)
            {
                return ((Func<Complex, Complex, Complex, Complex, Complex, Complex, Complex>)function).Invoke(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5]);
            }
#endif
            else
            {
                return (Complex)function.DynamicInvoke((from s in arguments select (object)s).ToArray());
            }
        }
    }
}
