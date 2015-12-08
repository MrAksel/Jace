using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Jace.Execution;
using Jace.Operations;
using Jace.Tokenizer;
using Jace.Util;
using System.Numerics;

namespace Jace
{
    /// <summary>
    /// The CalculationEngine class is the main class of Jace.NET to convert strings containing
    /// mathematical formulas into .NET Delegates and to calculate the result.
    /// It can be configured to run in a number of modes based on the constructor parameters choosen.
    /// </summary>
    public class CalculationEngine
    {
        private readonly IExecutor executor;
        private readonly Optimizer optimizer;
        private readonly CultureInfo cultureInfo;
        private readonly MemoryCache<string, Func<IDictionary<string, Complex>, Complex>> executionFormulaCache;
        private readonly bool cacheEnabled;
        private readonly bool optimizerEnabled;

        /// <summary>
        /// Creates a new instance of the <see cref="CalculationEngine"/> class with
        /// default parameters.
        /// </summary>
        public CalculationEngine()
            : this(CultureInfo.CurrentCulture, ExecutionMode.Compiled)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CalculationEngine"/> class. The dynamic compiler
        /// is used for formula execution and the optimizer and cache are enabled.
        /// </summary>
        /// <param name="cultureInfo">
        /// The <see cref="CultureInfo"/> required for correctly reading floating poin numbers.
        /// </param>
        public CalculationEngine(CultureInfo cultureInfo)
            : this(cultureInfo, ExecutionMode.Compiled)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CalculationEngine"/> class. The optimizer and 
        /// cache are enabled.
        /// </summary>
        /// <param name="cultureInfo">
        /// The <see cref="CultureInfo"/> required for correctly reading floating poin numbers.
        /// </param>
        /// <param name="executionMode">The execution mode that must be used for formula execution.</param>
        public CalculationEngine(CultureInfo cultureInfo, ExecutionMode executionMode)
            : this(cultureInfo, executionMode, true, true)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CalculationEngine"/> class.
        /// </summary>
        /// <param name="cultureInfo">
        /// The <see cref="CultureInfo"/> required for correctly reading floating poin numbers.
        /// </param>
        /// <param name="executionMode">The execution mode that must be used for formula execution.</param>
        /// <param name="cacheEnabled">Enable or disable caching of mathematical formulas.</param>
        /// <param name="optimizerEnabled">Enable or disable optimizing of formulas.</param>
        public CalculationEngine(CultureInfo cultureInfo, ExecutionMode executionMode, bool cacheEnabled, bool optimizerEnabled)
        {
            this.executionFormulaCache = new MemoryCache<string, Func<IDictionary<string, Complex>, Complex>>();
            this.FunctionRegistry = new FunctionRegistry(false);
            this.ConstantRegistry = new ConstantRegistry(false);
            this.cultureInfo = cultureInfo;
            this.cacheEnabled = cacheEnabled;
            this.optimizerEnabled = optimizerEnabled;

            if (executionMode == ExecutionMode.Interpreted)
                executor = new Interpreter();
            else if (executionMode == ExecutionMode.Compiled)
                executor = new DynamicCompiler();
            else
                throw new ArgumentException(string.Format("Unsupported execution mode \"{0}\".", executionMode),
                    "executionMode");

            optimizer = new Optimizer(new Interpreter()); // We run the optimizer with the interpreter 

            // Register the default constants of Jace.NET into the constant registry
            RegisterDefaultConstants();

            // Register the default functions of Jace.NET into the function registry
            RegisterDefaultFunctions();
        }

        internal IFunctionRegistry FunctionRegistry { get; private set; }

        internal IConstantRegistry ConstantRegistry { get; private set; }

        public Complex Calculate(string formulaText)
        {
            return Calculate(formulaText, new Dictionary<string, Complex>());
        }

        public Complex Calculate(string formulaText, IDictionary<string, Complex> variables)
        {
            if (string.IsNullOrEmpty(formulaText))
                throw new ArgumentNullException("formulaText");

            if (variables == null)
                throw new ArgumentNullException("variables");


            variables = EngineUtil.ConvertVariableNamesToLowerCase(variables);
            VerifyVariableNames(variables);

            // Add the reserved variables to the dictionary
            foreach (ConstantInfo constant in ConstantRegistry)
                variables.Add(constant.ConstantName, constant.Value);

            if (IsInFormulaCache(formulaText))
            {
                Func<IDictionary<string, Complex>, Complex> formula = executionFormulaCache[formulaText];
                return formula(variables);
            }
            else
            {
                Operation operation = BuildAbstractSyntaxTree(formulaText);
                Func<IDictionary<string, Complex>, Complex> function = BuildFormula(formulaText, operation);

                return function(variables);
            }
        }

        public FormulaBuilder Formula(string formulaText)
        {
            if (string.IsNullOrEmpty(formulaText))
                throw new ArgumentNullException("formulaText");

            return new FormulaBuilder(formulaText, this);
        }

        /// <summary>
        /// Build a .NET func for the provided formula.
        /// </summary>
        /// <param name="formulaText">The formula that must be converted into a .NET func.</param>
        /// <returns>A .NET func for the provided formula.</returns>
        public Func<Dictionary<string, Complex>, Complex> Build(string formulaText)
        {
            if (string.IsNullOrEmpty(formulaText))
                throw new ArgumentNullException("formulaText");

            if (IsInFormulaCache(formulaText))
            {
                return executionFormulaCache[formulaText];
            }
            else
            {
                Operation operation = BuildAbstractSyntaxTree(formulaText);
                return BuildFormula(formulaText, operation);
            }
        }

        /// <summary>
        /// Add a function to the calculation engine.
        /// </summary>
        /// <param name="functionName">The name of the function. This name can be used in mathematical formulas.</param>
        /// <param name="function">The implemenation of the function.</param>
        public void AddFunction(string functionName, Func<Complex> function)
        {
            FunctionRegistry.RegisterFunction(functionName, function);
        }

        /// <summary>
        /// Add a function to the calculation engine.
        /// </summary>
        /// <param name="functionName">The name of the function. This name can be used in mathematical formulas.</param>
        /// <param name="function">The implemenation of the function.</param>
        public void AddFunction(string functionName, Func<Complex, Complex> function)
        {
            FunctionRegistry.RegisterFunction(functionName, function);
        }

        /// <summary>
        /// Add a function to the calculation engine.
        /// </summary>
        /// <param name="functionName">The name of the function. This name can be used in mathematical formulas.</param>
        /// <param name="function">The implemenation of the function.</param>
        public void AddFunction(string functionName, Func<Complex, Complex, Complex> function)
        {
            FunctionRegistry.RegisterFunction(functionName, function);
        }

        /// <summary>
        /// Add a function to the calculation engine.
        /// </summary>
        /// <param name="functionName">The name of the function. This name can be used in mathematical formulas.</param>
        /// <param name="function">The implemenation of the function.</param>
        public void AddFunction(string functionName, Func<Complex, Complex, Complex, Complex> function)
        {
            FunctionRegistry.RegisterFunction(functionName, function);
        }

        /// <summary>
        /// Add a function to the calculation engine.
        /// </summary>
        /// <param name="functionName">The name of the function. This name can be used in mathematical formulas.</param>
        /// <param name="function">The implemenation of the function.</param>
        public void AddFunction(string functionName, Func<Complex, Complex, Complex, Complex, Complex> function)
        {
            FunctionRegistry.RegisterFunction(functionName, function);
        }

#if !WINDOWS_PHONE_7
        /// <summary>
        /// Add a function to the calculation engine.
        /// </summary>
        /// <param name="functionName">The name of the function. This name can be used in mathematical formulas.</param>
        /// <param name="function">The implemenation of the function.</param>
        public void AddFunction(string functionName, Func<Complex, Complex, Complex, Complex, Complex, Complex> function)
        {
            FunctionRegistry.RegisterFunction(functionName, function);
        }

        /// <summary>
        /// Add a function to the calculation engine.
        /// </summary>
        /// <param name="functionName">The name of the function. This name can be used in mathematical formulas.</param>
        /// <param name="function">The implemenation of the function.</param>
        public void AddFunction(string functionName, Func<Complex, Complex, Complex, Complex, Complex, Complex, Complex> function)
        {
            FunctionRegistry.RegisterFunction(functionName, function);
        }
#endif

        /// <summary>
        /// Add a constant to the calculation engine.
        /// </summary>
        /// <param name="constantName">The name of the constant. This name can be used in mathematical formulas.</param>
        /// <param name="value">The value of the constant.</param>
        public void AddConstant(string constantName, double value)
        {
            ConstantRegistry.RegisterConstant(constantName, value);
        }

        private void RegisterDefaultFunctions()
        {
            FunctionRegistry.RegisterFunction("sin", (Func<Complex, Complex>)((a) => Complex.Sin(a)), false);
            FunctionRegistry.RegisterFunction("cos", (Func<Complex, Complex>)((a) => Complex.Cos(a)), false);
            FunctionRegistry.RegisterFunction("asin", (Func<Complex, Complex>)((a) => Complex.Asin(a)), false);
            FunctionRegistry.RegisterFunction("acos", (Func<Complex, Complex>)((a) => Complex.Acos(a)), false);
            FunctionRegistry.RegisterFunction("tan", (Func<Complex, Complex>)((a) => Complex.Tan(a)), false);
            FunctionRegistry.RegisterFunction("atan", (Func<Complex, Complex>)((a) => Complex.Atan(a)), false);
            FunctionRegistry.RegisterFunction("ln", (Func<Complex, Complex>)((a) => Complex.Log(a)), false);
            FunctionRegistry.RegisterFunction("lg", (Func<Complex, Complex>)((a) => Complex.Log(a, 10.0)), false);
            FunctionRegistry.RegisterFunction("log", (Func<Complex, double, Complex>)((a, b) => Complex.Log(a, b)), false);
            FunctionRegistry.RegisterFunction("sqrt", (Func<Complex, Complex>)((a) => Complex.Sqrt(a)), false);
            FunctionRegistry.RegisterFunction("abs", (Func<Complex, Complex>)((a) => Complex.Abs(a)), false);
        }

        private void RegisterDefaultConstants()
        {
            ConstantRegistry.RegisterConstant("i", Complex.ImaginaryOne, false);
            ConstantRegistry.RegisterConstant("e", Math.E, false);
            ConstantRegistry.RegisterConstant("pi", Math.PI, false);
        }

        /// <summary>
        /// Build the abstract syntax tree for a given formula. The formula string will
        /// be first tokenized.
        /// </summary>
        /// <param name="formulaText">A string containing the mathematical formula that must be converted 
        /// into an abstract syntax tree.</param>
        /// <returns>The abstract syntax tree of the formula.</returns>
        private Operation BuildAbstractSyntaxTree(string formulaText)
        {
            TokenReader tokenReader = new TokenReader(cultureInfo);
            List<Token> tokens = tokenReader.Read(formulaText);

            AstBuilder astBuilder = new AstBuilder(FunctionRegistry);
            Operation operation = astBuilder.Build(tokens);

            if (optimizerEnabled)
                return optimizer.Optimize(operation, this.FunctionRegistry);
            else
                return operation;
        }

        private Func<IDictionary<string, Complex>, Complex> BuildFormula(string formulaText, Operation operation)
        {
            return executionFormulaCache.GetOrAdd(formulaText, v => executor.BuildFormula(operation, this.FunctionRegistry));
        }

        private bool IsInFormulaCache(string formulaText)
        {
            return cacheEnabled && executionFormulaCache.ContainsKey(formulaText);
        }

        /// <summary>
        /// Verify a collection of variables to ensure that all the variable names are valid.
        /// Users are not allowed to overwrite reserved variables or use function names as variables.
        /// If an invalid variable is detected an exception is thrown.
        /// </summary>
        /// <param name="variables">The colletion of variables that must be verified.</param>
        private void VerifyVariableNames(IDictionary<string, Complex> variables)
        {
            foreach (string variableName in variables.Keys)
            {
                if (ConstantRegistry.IsConstantName(variableName) && !ConstantRegistry.GetConstantInfo(variableName).IsOverWritable)
                    throw new ArgumentException(string.Format("The name \"{0}\" is a reservered variable name that cannot be overwritten.", variableName), "variables");

                if (FunctionRegistry.IsFunctionName(variableName))
                    throw new ArgumentException(string.Format("The name \"{0}\" is a function name. Parameters cannot have this name.", variableName), "variables");
            }
        }
    }
}
