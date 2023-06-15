using Azure.Search.Documents;
using Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerkPilot.Models;
using System.Runtime.CompilerServices;
using Azure.Search.Documents.Models;
using Microsoft.AspNetCore.WebUtilities;
using PerkPilot.Common;
using System.IO;

namespace PerkPilot.Services
{
    public class AzureSearchService
    {
        
        public async Task<List<SearchResult>> QueryDocumentsAsync(SearchClient searchClient, string query)
        {            
            var documentContent = string.Empty;
            
           
            SearchOptions options = new SearchOptions() {
                IncludeTotalCount = true,
                SearchMode = SearchMode.All,
                Size = 1,
                HighlightPreTag = "<b>",
                HighlightPostTag = "</b>",
                HighlightFields = { "content"}               
            };
            options.Select.Add("content");
            options.Select.Add("metadata_storage_name");
            options.Select.Add("metadata_storage_path");
            options.Select.Add("metadata_storage_file_extension");
            options.Select.Add("metadata_content_type");
            //SearchResults<SearchResult> results = searchClient.Search<SearchResult>(query, options);
            var searchResultResponse = await searchClient.SearchAsync<SearchDocument>(query, options);
            if (searchResultResponse.Value is null)
            {
                throw new InvalidOperationException("fail to get search result");
            }
             SearchResults<SearchDocument> searchResult = searchResultResponse.Value;

            var sb = new StringBuilder();
            List<SearchResult> results = new List<SearchResult>();
            foreach (var doc in searchResult.GetResults())
            {
                string docHighLightedText = string.Empty;
                
                var docName = doc.Document["metadata_storage_name"].ToString();
                var docPath = doc.Document["metadata_storage_path"].ToString();
                docPath = docPath.Substring(0, docPath.Length - 1);
                var docContent = doc.Document["content"].ToString();
                var docExtension = doc.Document["metadata_storage_file_extension"].ToString();
                var docContentType = doc.Document["metadata_content_type"].ToString();
                
                if (doc.Highlights.Count > 0)
                {
                    foreach (var highlight in doc.Highlights["content"].ToList())
                    {
                        docHighLightedText += highlight;
                    }
                }

                results.Add(new SearchResult
                {

                    Name = docName,
                    Path = Util.UrlDecode(docPath),
                    Content = docContent,
                    FileExtension = docExtension,
                    ContentType = docContentType,
                    HighlightedText = docHighLightedText
                });                
               
            }            

            return results;            
        }
        private static SearchClient CreateSearchClientForQueries()
        {
            string SearchServiceEndPoint = Environment.GetEnvironmentVariable("SearchServiceEndPoint");
            string SearchIndexName = Environment.GetEnvironmentVariable("SearchIndexName");
            string SearchServiceQueryApiKey = Environment.GetEnvironmentVariable("SearchServiceQueryApiKey");

            SearchClient searchClient = new SearchClient(new Uri(SearchServiceEndPoint), SearchIndexName, new AzureKeyCredential(SearchServiceQueryApiKey));
            
            return searchClient;
        }


    }
}
