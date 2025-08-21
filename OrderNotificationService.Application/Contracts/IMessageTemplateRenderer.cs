using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderNotificationService.Application.Contracts
{
    public interface IMessageTemplateRenderer
    {
        string Render(string template, Dictionary<string, string> values);
    }
}
