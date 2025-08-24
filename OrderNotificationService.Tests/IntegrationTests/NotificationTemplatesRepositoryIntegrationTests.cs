using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderNotificationService.Tests.IntegrationTests
{
    public class NotificationTemplatesRepositoryIntegrationTests : IClassFixture<NotificationTemplatesRepositoryFixture>
    {
        private readonly NotificationTemplatesRepositoryFixture _fixture;

        public NotificationTemplatesRepositoryIntegrationTests(NotificationTemplatesRepositoryFixture fixture)
        {
            _fixture = fixture;
        }


    }
}
