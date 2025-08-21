using OrderNotificationService.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OrderNotificationService.Application.Services
{
    public class MessageTemplateRenderer : IMessageTemplateRenderer
    {
        public string Render(string template, Dictionary<string, string> values)
        {
            foreach(var value in values)
            {
                template = template.Replace($"{{{value.Key}}}", value.Value);
            }
            return template;
        }
    }
}
