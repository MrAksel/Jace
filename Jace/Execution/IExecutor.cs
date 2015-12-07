using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jace.Operations;
using System.Numerics;

namespace Jace.Execution
{
    public interface IExecutor
    {
        Complex Execute(Operation operation, IFunctionRegistry functionRegistry);
        Complex Execute(Operation operation, IFunctionRegistry functionRegistry, IDictionary<string, Complex> variables);

        Func<IDictionary<string, Complex>, Complex> BuildFormula(Operation operation, IFunctionRegistry functionRegistry);
    }
}
