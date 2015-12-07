using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Jace.Operations;
using Jace.Execution;
using System.Numerics;

#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#elif __ANDROID__
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace Jace.Tests
{
    [TestClass]
    public class CalculationEngineTests
    {
        [TestMethod]
        public void TestCalculationFormula1FloatingPointCompiled()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture, ExecutionMode.Compiled);
            Complex result = engine.Calculate("2.0+3.0");

            Assert.AreEqual(5.0, result);
        }

        [TestMethod]
        public void TestCalculationFormula1IntegersCompiled()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture, ExecutionMode.Compiled);
            Complex result = engine.Calculate("2+3");

            Assert.AreEqual(5.0, result);
        }

        [TestMethod]
        public void TestCalculateFormula1()
        {
            CalculationEngine engine = new CalculationEngine();
            Complex result = engine.Calculate("2+3");

            Assert.AreEqual(5.0, result);
        }

        [TestMethod]
        public void TestCalculatePowCompiled()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture, ExecutionMode.Compiled);
            Complex result = engine.Calculate("2^3.0");

            Assert.AreEqual(8.0, result);
        }

        [TestMethod]
        public void TestCalculatePowInterpreted()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture, ExecutionMode.Interpreted);
            Complex result = engine.Calculate("2^3.0");

            Assert.AreEqual(8.0, result);
        }

        [TestMethod]
        public void TestCalculateFormulaWithVariables()
        {
            Dictionary<string, Complex> variables = new Dictionary<string, Complex>();
            variables.Add("var1", 2.5);
            variables.Add("var2", 3.4);

            CalculationEngine engine = new CalculationEngine();
            Complex result = engine.Calculate("var1*var2", variables);

            Assert.AreEqual(8.5, result);
        }

        [TestMethod]
        public void TestCalculateFormulaVariableNotDefinedInterpreted()
        {
            Dictionary<string, Complex> variables = new Dictionary<string, Complex>();
            variables.Add("var1", 2.5);

            AssertExtensions.ThrowsException<VariableNotDefinedException>(() =>
                {
                    CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture, ExecutionMode.Interpreted);
                    Complex result = engine.Calculate("var1*var2", variables);
                });
        }

        [TestMethod]
        public void TestCalculateFormulaVariableNotDefinedCompiled()
        {
            Dictionary<string, Complex> variables = new Dictionary<string, Complex>();
            variables.Add("var1", 2.5);

            AssertExtensions.ThrowsException<VariableNotDefinedException>(() =>
                {
                    CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture, ExecutionMode.Compiled);
                    Complex result = engine.Calculate("var1*var2", variables);
                });
        }

        [TestMethod]
        public void TestCalculateSineFunctionInterpreted()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture, ExecutionMode.Interpreted);
            Complex result = engine.Calculate("sin(14)");

            Assert.AreEqual(Math.Sin(14.0), result);
        }

        [TestMethod]
        public void TestCalculateSineFunctionCompiled()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture, ExecutionMode.Compiled, true, false);
            Complex result = engine.Calculate("sin(14)");

            Assert.AreEqual(Math.Sin(14.0), result);
        }

        [TestMethod]
        public void TestCalculateCosineFunctionInterpreted()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture, ExecutionMode.Interpreted);
            Complex result = engine.Calculate("cos(41)");

            Assert.AreEqual(Math.Cos(41.0), result);
        }

        [TestMethod]
        public void TestCalculateCosineFunctionCompiled()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture, ExecutionMode.Compiled, true, false);
            Complex result = engine.Calculate("cos(41)");

            Assert.AreEqual(Math.Cos(41.0), result);
        }

        [TestMethod]
        public void TestCalculateLognFunctionInterpreted()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture, ExecutionMode.Interpreted, true, false);
            Complex result = engine.Calculate("log(14, 3)");

            Assert.AreEqual(Math.Log(14.0, 3.0), result);
        }

        [TestMethod]
        public void TestCalculateLognFunctionCompiled()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture, ExecutionMode.Compiled, true, false);
            Complex result = engine.Calculate("log(14, 3)");

            Assert.AreEqual(Math.Log(14.0, 3.0), result);
        }

        [TestMethod]
        public void TestNegativeConstant()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture, ExecutionMode.Compiled, true, false);
            Complex result = engine.Calculate("-100");

            Assert.AreEqual(-100.0, result);
        }

        [TestMethod]
        public void TestMultiplicationWithNegativeConstant()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture, ExecutionMode.Compiled, true, false);
            Complex result = engine.Calculate("5*-100");

            Assert.AreEqual(-500.0, result);
        }

        [TestMethod]
        public void TestUnaryMinus1Compiled()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture, ExecutionMode.Compiled, true, false);
            Complex result = engine.Calculate("-(1+2+(3+4))");

            Assert.AreEqual(-10.0, result);
        }

        [TestMethod]
        public void TestUnaryMinus1Interpreted()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture, ExecutionMode.Interpreted, true, false);
            Complex result = engine.Calculate("-(1+2+(3+4))");

            Assert.AreEqual(-10.0, result);
        }

        [TestMethod]
        public void TestUnaryMinus2Compiled()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture, ExecutionMode.Compiled, true, false);
            Complex result = engine.Calculate("5+(-(1*2))");

            Assert.AreEqual(3.0, result);
        }

        [TestMethod]
        public void TestUnaryMinus2Interpreted()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture, ExecutionMode.Interpreted, true, false);
            Complex result = engine.Calculate("5+(-(1*2))");

            Assert.AreEqual(3.0, result);
        }

        [TestMethod]
        public void TestUnaryMinus3Compiled()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture, ExecutionMode.Compiled, true, false);
            Complex result = engine.Calculate("5*(-(1*2)*3)");

            Assert.AreEqual(-30.0, result);
        }

        [TestMethod]
        public void TestUnaryMinus3Interpreted()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture, ExecutionMode.Interpreted, true, false);
            Complex result = engine.Calculate("5*(-(1*2)*3)");

            Assert.AreEqual(-30.0, result);
        }

        [TestMethod]
        public void TestUnaryMinus4Compiled()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture, ExecutionMode.Compiled, true, false);
            Complex result = engine.Calculate("5* -(1*2)");

            Assert.AreEqual(-10.0, result);
        }

        [TestMethod]
        public void TestUnaryMinus4Interpreted()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture, ExecutionMode.Interpreted, true, false);
            Complex result = engine.Calculate("5* -(1*2)");

            Assert.AreEqual(-10.0, result);
        }

        [TestMethod]
        public void TestUnaryMinus5Compiled()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture, ExecutionMode.Compiled, true, false);
            Complex result = engine.Calculate("-(1*2)^3");

            Assert.AreEqual(-8.0, result);
        }

        [TestMethod]
        public void TestUnaryMinus5Interpreted()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture, ExecutionMode.Interpreted, true, false);
            Complex result = engine.Calculate("-(1*2)^3");

            Assert.AreEqual(-8.0, result);
        }

        [TestMethod]
        public void TestBuild()
        { 
            CalculationEngine engine = new CalculationEngine();
            Func<Dictionary<string, Complex>, Complex> function = engine.Build("var1+2*(3*age)");

            Dictionary<string, Complex> variables = new Dictionary<string, Complex>();
            variables.Add("var1", 2);
            variables.Add("age", 4);

            Complex result = function(variables);
            Assert.AreEqual(26.0, result);
        }

        [TestMethod]
        public void TestFormulaBuilder()
        {
            CalculationEngine engine = new CalculationEngine();
            Func<int, Complex, Complex> function = (Func<int, Complex, Complex>)engine.Formula("var1+2*(3*age)")
                .Parameter("var1", DataType.Integer)
                .Parameter("age", DataType.FloatingPoint)
                .Result(DataType.FloatingPoint)
                .Build();

            Complex result = function(2, 4);
            Assert.AreEqual(26.0, result);
        }

        [TestMethod]
        public void TestFormulaBuilderCompiled()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture, ExecutionMode.Compiled);
            Func<int, Complex, Complex> function = (Func<int, Complex, Complex>)engine.Formula("var1+2*(3*age)")
                .Parameter("var1", DataType.Integer)
                .Parameter("age", DataType.FloatingPoint)
                .Result(DataType.FloatingPoint)
                .Build();

            Complex result = function(2, 4);
            Assert.AreEqual(26.0, result);
        }

        [TestMethod]
        public void TestFormulaBuilderInvalidParameterName()
        {
            AssertExtensions.ThrowsException<ArgumentException>(() =>
                {
                    CalculationEngine engine = new CalculationEngine();
                    Func<int, Complex, Complex> function = (Func<int, Complex, Complex>)engine.Formula("sin+2")
                        .Parameter("sin", DataType.Integer)
                        .Build();
                });
        }

        [TestMethod]
        public void TestFormulaBuilderDuplicateParameterName()
        {
            AssertExtensions.ThrowsException<ArgumentException>(() =>
                {
                    CalculationEngine engine = new CalculationEngine();
                    Func<int, Complex, Complex> function = (Func<int, Complex, Complex>)engine.Formula("var1+2")
                        .Parameter("var1", DataType.Integer)
                        .Parameter("var1", DataType.FloatingPoint)
                        .Build();
                });
        }

        [TestMethod]
        public void TestPiMultiplication()
        {
            CalculationEngine engine = new CalculationEngine();
            Complex result = engine.Calculate("2 * pI");

            Assert.AreEqual(2 * Math.PI, result);
        }

        [TestMethod]        
        public void TestReservedVariableName()
        {
            AssertExtensions.ThrowsException<ArgumentException>(() =>
            {
                Dictionary<string, Complex> variables = new Dictionary<string, Complex>();
                variables.Add("pi", 2.0);

                CalculationEngine engine = new CalculationEngine();
                Complex result = engine.Calculate("2 * pI", variables);
            });
        }

        [TestMethod]
        public void TestVariableNameCaseSensitivity()
        {
            Dictionary<string, Complex> variables = new Dictionary<string, Complex>();
            variables.Add("blabla", 42.5);

            CalculationEngine engine = new CalculationEngine();
            Complex result = engine.Calculate("2 * BlAbLa", variables);

            Assert.AreEqual(85.0, result);
        }

        [TestMethod]
        public void TestCustomFunctionInterpreted()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture,
                ExecutionMode.Interpreted, false, false);
            engine.AddFunction("test", (a, b) => a + b);

            Complex result = engine.Calculate("test(2,3)");
            Assert.AreEqual(5.0, result);
        }

        [TestMethod]
        public void TestCustomFunctionCompiled()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture,
                ExecutionMode.Compiled, false, false);
            engine.AddFunction("test", (a, b) => a + b);

            Complex result = engine.Calculate("test(2,3)");
            Assert.AreEqual(5.0, result);
        }

        /*
        [TestMethod]
        public void TestComplicatedPrecedence1()
        {
            CalculationEngine engine = new CalculationEngine();

            Complex result = engine.Calculate("1+2-3*4/5+6-7*8/9+0");
            Assert.AreEqual(0.378, Math.Round(result, 3));
        }

        [TestMethod]
        public void TestComplicatedPrecedence2()
        {
            CalculationEngine engine = new CalculationEngine();

            Complex result = engine.Calculate("1+2-3*4/sqrt(25)+6-7*8/9+0");
            Assert.AreEqual(0.378, Math.Round(result, 3));
        }

        [TestMethod]
        public void TestNestedFunctions()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture);

            // TODO max is removed
            Complex result = engine.Calculate("max(sin(67), cos(67))");
            Assert.AreEqual(-0.517769799789505, Math.Round(result, 15));
        }
        */

        [TestMethod]
        public void TestVariableCaseFuncInterpreted()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture, ExecutionMode.Interpreted);
            Func<Dictionary<string, Complex>, Complex> formula = engine.Build("var1+2/(3*otherVariablE)");

            Dictionary<string, Complex> variables = new Dictionary<string, Complex>();
            variables.Add("var1", 2);
            variables.Add("otherVariable", 4.2);

            Complex result = formula(variables);
        }

        [TestMethod]
        public void TestVariableCaseFuncCompiled()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture, ExecutionMode.Compiled);
            Func<Dictionary<string, Complex>, Complex> formula = engine.Build("var1+2/(3*otherVariablE)");

            Dictionary<string, Complex> variables = new Dictionary<string, Complex>();
            variables.Add("var1", 2);
            variables.Add("otherVariable", 4.2);

            Complex result = formula(variables);
        }

        [TestMethod]
        public void TestVariableCaseNonFunc()
        {
            CalculationEngine engine = new CalculationEngine();

            Dictionary<string, Complex> variables = new Dictionary<string, Complex>();
            variables.Add("var1", 2);
            variables.Add("otherVariable", 4.2);

            Complex result = engine.Calculate("var1+2/(3*otherVariablE)", variables);
        }
    }
}
