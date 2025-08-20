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
        public string Render(string template, params string[] values)
        {
            var result = new StringBuilder();
            var valIndex = 0;
            var position = 0;
            while(valIndex < values.Length && position < template.Length)
            {
                var placeholderStart = template.IndexOf('{', position);
                if (placeholderStart == -1) break;

                var placeholderEnd = template.IndexOf('}', placeholderStart);
                if (placeholderEnd == -1) break;

                result.Append(template.Substring(position, placeholderStart - position));
                result.Append(values[valIndex++] ?? "");
                position = placeholderEnd + 1;
            }

            result.Append(template.Substring(position));
            return result.ToString();  
        }
    }
}
