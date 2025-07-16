using AutoFixture;
using AutoFixture.Xunit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Tests.ProductFixture
{
    public class AutoProductDataAttribute : AutoDataAttribute
    {
        public AutoProductDataAttribute() : base(() => new Fixture().Customize(new ProductFixtureCustomization())) { }

    }
}
