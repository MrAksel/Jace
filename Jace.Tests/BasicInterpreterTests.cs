﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jace.Operations;
using Jace.Execution;
using Jace.Tests.Mocks;
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
    public class BasicInterpreterTests
    {
        [TestMethod]
        public void TestBasicInterpreterSubstraction()
        {
            IFunctionRegistry functionRegistry = new MockFunctionRegistry();

            IExecutor executor = new Interpreter();
            Complex result = executor.Execute(new Subtraction(
                DataType.Integer,
                new IntegerConstant(6),
                new IntegerConstant(9)), functionRegistry);

            Assert.AreEqual(-3.0, result);
        }

        [TestMethod]
        public void TestBasicInterpreter1()
        {
            IFunctionRegistry functionRegistry = new MockFunctionRegistry();

            IExecutor executor = new Interpreter();
            // 6 + (2 * 4)
            Complex result = executor.Execute(
                new Addition(
                    DataType.Integer,
                    new IntegerConstant(6),
                    new Multiplication(
                        DataType.Integer, 
                        new IntegerConstant(2), 
                        new IntegerConstant(4))), functionRegistry);

            Assert.AreEqual(14.0, result);
        }

        [TestMethod]
        public void TestBasicInterpreterWithVariables()
        {
            IFunctionRegistry functionRegistry = new MockFunctionRegistry();

            Dictionary<string, Complex> variables = new Dictionary<string, Complex>();
            variables.Add("var1", 2);
            variables.Add("age", 4);

            IExecutor interpreter = new Interpreter();
            // var1 + 2 * (3 * age)
            Complex result = interpreter.Execute(
                new Addition(DataType.FloatingPoint,
                    new Variable("var1"),
                    new Multiplication(
                        DataType.FloatingPoint,
                        new IntegerConstant(2),
                        new Multiplication(
                            DataType.FloatingPoint, 
                            new IntegerConstant(3),
                            new Variable("age")))), functionRegistry, variables);

            Assert.AreEqual(26.0, result);
        }
    }
}
