using AutoFixture.Xunit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Tests.OrderFixture
{
    public class InlineAutoOrderDataAttribute : CompositeDataAttribute
    {
        public InlineAutoOrderDataAttribute(params object[] values)
            : base(new InlineDataAttribute(values), new AutoOrderDataAttribute())
        { }
    }
}
