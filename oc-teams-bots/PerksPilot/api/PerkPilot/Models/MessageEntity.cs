using Microsoft.WindowsAzure.Storage.Table;
using PerkPilot.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerkPilot.Models
{
    public class MessageEntity: TableEntity
    {
        public MessageEntity()
        {
            
        }

        public MessageEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = AzureTableHelper.ToAzureKeyString(rowKey);
        }
        /// <summary>
        /// Gets or sets the email address for the customer.
        /// A property for use in Table storage must be a public property of a 
        /// supported type that exposes both a getter and a setter.        
        /// </summary>
        /// <value>
        /// The email address.
        /// </value>
        public string ChatResponse { get; set; }
        public string ChatQuestion { get; set; }

       
    }
}
