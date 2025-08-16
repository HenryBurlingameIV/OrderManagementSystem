using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderNotificationService.Domain.Entities
{
    public class NotificationTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string TemplateText {  get; set; }
    }
}
