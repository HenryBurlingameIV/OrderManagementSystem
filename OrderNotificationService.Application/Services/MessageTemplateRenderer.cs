using OrderNotificationService.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderNotificationService.Application.Services
{
    public class MessageTemplateRenderer : IMessageTemplateRenderer
    {
        public string Render(string template, object values)
        {
            foreach (var property in values.GetType().GetProperties())
            {
                template = template.Replace($@"{{{property.Name}}}", property?.GetValue(values)?.ToString() ?? "");
            }
            return template;
        }
    }
}
