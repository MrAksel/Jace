using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Jace.Execution
{
    public interface IConstantRegistry : IEnumerable<ConstantInfo>
    {
        ConstantInfo GetConstantInfo(string constantName);
        bool IsConstantName(string constantName);
        void RegisterConstant(string constantName, Complex value);
        void RegisterConstant(string constantName, Complex value, bool isOverWritable);
    }
}
