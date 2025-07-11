using AutoFixture;
using AutoFixture.Xunit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Tests
{
    public class AutoOrderDataAttribute : AutoDataAttribute
    {
        public AutoOrderDataAttribute() : base(() => new Fixture().Customize(new OrderFixtureCustomization()))
        { }
    }
}
