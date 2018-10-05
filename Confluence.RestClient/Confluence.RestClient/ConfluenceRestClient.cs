using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluence.RestClient.Model;
using Refit;

namespace Confluence.RestClient
{
    public class ConfluenceRestClient
   {
      private IConfluenceRestApi _endpoint;

      public ConfluenceRestClient(string baseUri, string username, string key)
      {
         _endpoint = RestService.For<IConfluenceRestApi>(
            new HttpClient(new AuthenticatedHttpClientHandler(username, key))
            {
               BaseAddress = new Uri(baseUri)
            });
      }

      public async Task<ContentData> GetContentAsync(string spaceKey, string pageId)
      {
         Content content = await _endpoint.GetContentAsync(pageId, "body.storage,version");

         return new ContentData
         {
            Version = content.Version.Number,
            Content = content.Body.Storage.Value
         };
      }

      public async Task<IReadOnlyCollection<ContentDescription>> GetAllContentAsync(string spaceKey)
      {
         var all = new List<Content>();

         for (int i = 0; ; i += 25)
         {
            ContentList page = await _endpoint.ListContentAsync(spaceKey, i, 25);

            if (page.Size == 0)
               break;

            all.AddRange(page.Results);
         }

         return all.Select(c => new ContentDescription { Id = c.Id, Title = c.Title }).ToList();
      }

      public async Task<string> FindPageIdByContentPropertyValueAsync(string spaceKey, string propertyName, string propertyValue)
      {
         var pageIds = new List<string>();

         for (int i = 0; ; i += 25)
         {
            ContentList page = await _endpoint.ListContentAsync(spaceKey, i, 25);

            if (page.Size == 0)
               break;

            pageIds.AddRange(page.Results.Select(p => p.Id));
         }

         List<string> pValues = (await Task.WhenAll(pageIds.Select(pid => GetPagePropertiesIgnoreErrorsAsync(pid))))
            .Where(pp => pp != null)
            .Select(cps => cps.Results.FirstOrDefault(cp => cp.Key == propertyName))
            .Select(cp => cp == null ? null : cp.Value.ToString())
            .ToList();

         int index = pValues.FindIndex(v => v == propertyValue);

         return index == -1 ? null : pageIds[index];
      }

      private async Task<ContentProperties> GetPagePropertiesIgnoreErrorsAsync(string pageId)
      {
         try
         {
            return await _endpoint.GetPagePropertiesAsync(pageId);
         }
         catch(Exception ex)
         {
            return null;
         }
      }

      public async Task<string> GetPageIdByTitle(string spaceKey, string title)
      {
         ContentArray ca = await _endpoint.SearchContentByTitleAsync(spaceKey, title);

         if(ca == null || ca.Contents == null || ca.Contents.Length == 0)
         {
            return null;
         }

         return ca.Contents.Select(c => c.Id).FirstOrDefault();
      }

      public async Task<string> CreateContentAsync(string spaceKey, string title, string htmlBody,
         string[] labels = null)
      {
         var content = new Content
         {
            Title = title,
            Type = "page",
            Status = "current",
            Space = new Space
            {
               Key = spaceKey
            },
            Body = new Body
            {
               Storage = new Storage
               {
                  Value = htmlBody,
                  Representation = "storage"
               }
            }
         };

         content.Metadata.Labels = Label.CreateArray(labels);

         Content response = await _endpoint.CreateContentAsync(content);

         return response.Id;
      }

      public async Task CreatePagePropertyAsync(string pageId, string propertyName, string propertyValue)
      {
         var prop = new ContentProperty
         {
            Key = propertyName,
            Value = propertyValue
         };

         await _endpoint.CreatePagePropertyAsync(pageId, prop);
      }

      public async Task<Dictionary<string, string>> GetPagePropertiesAsync(string pageId)
      {
         ContentProperties cps = await _endpoint.GetPagePropertiesAsync(pageId);

         return cps.Results.ToDictionary(cp => cp.Key, cp => cp.Value.ToString());

      }

      public async Task UpdateContentAsync(string spaceKey, string pageId, string title, string htmlBody, int nextVersionNumber, string[] labels)
      {
         var content = new Content
         {
            Id = pageId,
            Title = title,
            Type = "page",
            Status = "current",
            Space = new Space
            {
               Key = spaceKey
            },
            Body = new Body
            {
               Storage = new Storage
               {
                  Value = htmlBody,
                  Representation = "storage"
               }
            },
            Version = new Version
            {
               Number = nextVersionNumber
            }
         };

         content.Metadata.Labels = Label.CreateArray(labels);

         await _endpoint.UpdateContentAsync(pageId, content);
      }

      public async Task DeleteContentAsync(string pageId)
      {
         await _endpoint.DeleteContentAsync(pageId);
      }

      public async Task AttachFileAsync(string pageId, string fileName, string fileContent)
      {
         var fileContentPart = new StreamPart(
            new MemoryStream(Encoding.UTF8.GetBytes(fileContent)),
            fileName);

         var minorEditPart = new StreamPart(
            new MemoryStream(Encoding.UTF8.GetBytes("true")), string.Empty);

         var commentPart = new StreamPart(
            new MemoryStream(Encoding.UTF8.GetBytes("")), string.Empty);


         await _endpoint.AttachOrRewriteFileAsync(pageId, fileContentPart, minorEditPart, commentPart);
      }


      class AuthenticatedHttpClientHandler : HttpClientHandler
      {
         private readonly string _username;
         private readonly string _key;

         public AuthenticatedHttpClientHandler(string username, string key)
         {
            _username = username;
            _key = key;
         }

         protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
         {
            var authHeader = new AuthenticationHeaderValue(
               "Basic",
               Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", _username, _key))));

            request.Headers.Authorization = authHeader;

            return base.SendAsync(request, cancellationToken);
         }
      }
   }
}
