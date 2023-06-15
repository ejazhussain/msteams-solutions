using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Table;
using PerkPilot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PerkPilot.Helpers
{
    public static class ChatHelper
    {
        
        public static async Task<MessageEntity> GetPreviousMessages(string partitionKey, string rowKey, CloudTable table)
        {
            var cleanPrompt = AzureTableHelper.ToAzureKeyString(rowKey);
            MessageEntity message = await AzureTableHelper.RetrieveEntityUsingPointQueryAsync(table, partitionKey, rowKey);
            
            if (message == null)
            {
                return new MessageEntity();
            }

            return message;
        }
    }
}
