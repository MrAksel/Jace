using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Jace.Operations;
using System.Numerics;

namespace Jace.Util
{
    /// <summary>
    /// An adapter for creating a func wrapper around a func accepting a dictionary. The wrapper
    /// can create a func that has an argument for every expected key in the dictionary.
    /// </summary>
    public class FuncAdapter
    {
        /// <summary>
        /// Wrap the parsed the function into a delegate of the specified type. The delegate must accept 
        /// the parameters defined in the parameters collection. The order of parameters is respected as defined
        /// in parameters collection.
        /// <br/>
        /// The function must accept a dictionary of strings and doubles as input. The values passed to the 
        /// wrapping function will be passed to the function using the dictionary. The keys in the dictionary
        /// are the names of the parameters of the wrapping function.
        /// </summary>
        /// <param name="parameters">The required parameters of the wrapping function delegate.</param>
        /// <param name="function">The function that must be wrapped.</param>
        /// <returns>A delegate instance of the required type.</returns>
        public Delegate Wrap(IEnumerable<Jace.Execution.ParameterInfo> parameters, 
            Func<Dictionary<string, Complex>, Complex> function)
        {
            Jace.Execution.ParameterInfo[] parameterArray = parameters.ToArray();

            return GenerateDelegate(parameterArray, function);
        }

        // Uncomment for debugging purposes
        //public void CreateDynamicModuleBuilder()
        //{
        //    AssemblyName assemblyName = new AssemblyName("JaceDynamicAssembly");
        //    AppDomain domain = AppDomain.CurrentDomain;
        //    AssemblyBuilder assemblyBuilder = domain.DefineDynamicAssembly(assemblyName,
        //        AssemblyBuilderAccess.RunAndSave);
        //    ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name, "test.dll");

        //    TypeBuilder typeBuilder = moduleBuilder.DefineType("MyTestClass");

        //    MethodBuilder method = typeBuilder.DefineMethod("MyTestMethod", MethodAttributes.Static, typeof(double),
        //       new Type[] { typeof(FuncAdapterArguments), typeof(int), typeof(double) });

        //    ILGenerator generator = method.GetILGenerator();
        //    GenerateMethodBody(generator, new List<Calculator.Execution.ParameterInfo>() { 
        //        new Calculator.Execution.ParameterInfo() { Name = "test1", DataType = DataType.Integer },
        //        new Calculator.Execution.ParameterInfo() { Name = "test2", DataType = DataType.FloatingPoint }},
        //        (a) => 0.0);

        //    typeBuilder.CreateType();

        //    assemblyBuilder.Save(@"test.dll");
        //}

        private Delegate GenerateDelegate(Jace.Execution.ParameterInfo[] parameterArray,
            Func<Dictionary<string, Complex>, Complex> function)
        {
            Type delegateType = GetDelegateType(parameterArray);
            Type dictionaryType = typeof(Dictionary<string, Complex>);

            LabelTarget returnLabel = Expression.Label(typeof(Complex));

            ParameterExpression dictionaryExpression =
                Expression.Variable(typeof(Dictionary<string, Complex>), "dictionary");
            BinaryExpression dictionaryAssignExpression =
                Expression.Assign(dictionaryExpression, Expression.New(dictionaryType));

            ParameterExpression[] parameterExpressions = new ParameterExpression[parameterArray.Length];

            List<Expression> methodBody = new List<Expression>();
            methodBody.Add(dictionaryAssignExpression);

            for (int i = 0; i < parameterArray.Length; i++)
            {
                // Create parameter expression for each func parameter
                Type parameterType;
                switch (parameterArray[i].DataType)
                {
                    case DataType.Integer:
                        parameterType = typeof(int); break;
                    case DataType.FloatingPoint:
                        parameterType = typeof(float); break;
                    default:
                    case DataType.Complex:
                        parameterType = typeof(Complex); break;
                }
                parameterExpressions[i] = Expression.Parameter(parameterType, parameterArray[i].Name);

                methodBody.Add(Expression.Call(dictionaryExpression,
                    dictionaryType.GetRuntimeMethod("Add", new Type[] { typeof(string), typeof(Complex) }),
                    Expression.Constant(parameterArray[i].Name),
                    Expression.Convert(parameterExpressions[i], typeof(Complex)))
                    );
            }

            InvocationExpression invokeExpression = Expression.Invoke(Expression.Constant(function), dictionaryExpression);
            methodBody.Add(invokeExpression);
            methodBody.Add(Expression.Return(returnLabel, invokeExpression));
            methodBody.Add(Expression.Label(returnLabel, Expression.Constant(Complex.Zero)));

            LambdaExpression lambdaExpression = Expression.Lambda(delegateType,
                Expression.Block(new[] { dictionaryExpression }, methodBody),
                parameterExpressions);

            return lambdaExpression.Compile();
        }

        private Type GetDelegateType(Jace.Execution.ParameterInfo[] parameters)
        {
            string funcTypeName = string.Format("System.Func`{0}", parameters.Length + 1);
            Type funcType = Type.GetType(funcTypeName);

            Type[] typeArguments = new Type[parameters.Length + 1];
            for (int i = 0; i < parameters.Length; i++)
            {
                switch (parameters[i].DataType)
                {
                    case DataType.Integer:
                        typeArguments[i] = typeof(int); break;
                    case DataType.FloatingPoint:
                        typeArguments[i] = typeof(float); break;
                    default:
                    case DataType.Complex:
                        typeArguments[i] = typeof(Complex); break;
                }
            }
            typeArguments[typeArguments.Length - 1] = typeof(Complex);

            return funcType.MakeGenericType(typeArguments);
        }

        private class FuncAdapterArguments
        {
            private readonly Func<Dictionary<string, Complex>, Complex> function;

            public FuncAdapterArguments(Func<Dictionary<string, Complex>, Complex> function)
            {
                this.function = function;
            }
        }
    }
}
