using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Jace.Execution
{
    public class ConstantInfo
    {
        public ConstantInfo(string constantName, Complex value, bool isOverWritable)
        {
            this.ConstantName = constantName;
            this.Value = value;
            this.IsOverWritable = isOverWritable;
        }

        public string ConstantName { get; private set; }

        public Complex Value { get; private set; }

        public bool IsOverWritable { get; set; }
    }
}
