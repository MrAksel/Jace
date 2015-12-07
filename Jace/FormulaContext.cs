using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jace.Execution;
using System.Numerics;

namespace Jace
{
    public class FormulaContext
    {
        public FormulaContext(IDictionary<string, Complex> variables,
            IFunctionRegistry functionRegistry)
        {
            this.Variables = variables;
            this.FunctionRegistry = functionRegistry;
        }

        public IDictionary<string, Complex> Variables { get; private set; }

        public IFunctionRegistry FunctionRegistry { get; private set; }
    }
}
