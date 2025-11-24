using AutoFixture;
using AutoFixture.Xunit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Tests.ProcessingOrderFixture
{
    public class AutoProcessingOrderData : AutoDataAttribute
    {
        public AutoProcessingOrderData()
            : base(() => new Fixture().Customize(new ProcessingOrderFixtureCustomization())) 
        { }
    }
}
