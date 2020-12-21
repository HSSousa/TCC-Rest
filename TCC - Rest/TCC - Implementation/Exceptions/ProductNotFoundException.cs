using System;
using System.Collections.Generic;
using System.Text;

namespace TCC___Implementation.Exceptions
{
    public class ProductNotFoundException : Exception
    {
        public ProductNotFoundException() : base()
        {
        }
        public ProductNotFoundException(string msg) : base(msg) { }
    }
}
