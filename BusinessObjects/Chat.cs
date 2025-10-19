using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class Chat
    {
        public string ChatId { get; set; }

        public string Message { get; set; } = string.Empty;

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{HH:mm dd/MM/yyyy}")]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string? SenderId { get; set; }

        [ValidateNever]
        public virtual Users Sender { get; set; }

        public string? ReceiverId { get; set; }
        [ValidateNever]
        public virtual Users Receiver { get; set; }
    }
}
