using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Refit;

namespace Confluence.RestClient
{
   interface IConfluenceRestApi
   {
      [Post("/content")]
      Task<Content> CreateContentAsync([Body] Content content);

      [Get("/content/{pageId}?expand={expand}")]
      Task<Content> GetContentAsync(string pageId, string expand);

      [Get("/content?spaceKey={spaceKey}&type=page&start={start}&limit={limit}")]
      Task<ContentList> ListContentAsync(string spaceKey, int start, int limit);

      [Put("/content/{pageId}")]
      Task UpdateContentAsync(string pageId, [Body] Content content);

      [Delete("/content/{pageId}")]
      Task DeleteContentAsync(string pageId);

      [Get("/content/?spaceKey={spaceKey}&title={title}")]
      Task<ContentArray> SearchContentByTitleAsync(string spaceKey, string title);

      [Post("/content/{pageId}/property")]
      Task CreatePagePropertyAsync(string pageId, [Body] ContentProperty contentProperty);

      [Get("/content/{pageId}/property")]
      Task<ContentProperties> GetPagePropertiesAsync(string pageId);

      [Multipart]
      [Headers("X-Atlassian-Token: nocheck")]
      [Put("/content/{pageId}/child/attachment")]
      Task AttachOrRewriteFileAsync(string pageId,
         [AliasAs("file")] StreamPart fileContent,
         [AliasAs("minorEdit")] StreamPart miniorEdit,
         [AliasAs("comment")] StreamPart comment);
   }

   #region [ Wrappers ]

   class ContentArray
   {
      [JsonProperty("results")]
      public Content[] Contents { get; set; }
   }

   class Content
   {
      [JsonProperty("id")]
      public string Id { get; set; }

      [JsonProperty("title")]
      public string Title { get; set; }

      [JsonProperty("type")]
      public string Type { get; set; }

      [JsonProperty("status")]
      public string Status { get; set; }

      [JsonProperty("version")]
      public Version Version { get; set; }

      [JsonProperty("space")]
      public Space Space { get; set; }

      [JsonProperty("body")]
      public Body Body { get; set; }

      [JsonProperty("metadata")]
      public Metadata Metadata { get; set; } = new Metadata();
   }

   class ContentList
   {
      [JsonProperty("start")]
      public int Start { get; set; }

      [JsonProperty("limit")]
      public int Limit { get; set; }

      [JsonProperty("size")]
      public int Size { get; set; }

      [JsonProperty("results")]
      public Content[] Results { get; set; }
   }

   class Version
   {
      [JsonProperty("number")]
      public int Number { get; set; }
   }

   class Space
   {
      [JsonProperty("key")]
      public string Key { get; set; }
   }

   class Body
   {
      [JsonProperty("storage")]
      public Storage Storage { get; set; }
   }

   class Storage
   {
      [JsonProperty("value")]
      public string Value { get; set; }

      [JsonProperty("representation")]
      public string Representation { get; set; }
   }

   class Metadata
   {
      [JsonProperty("labels")]
      public Label[] Labels { get; set; }
   }

   class Label
   {
      [JsonProperty("name")]
      public string Name { get; set; }

      public static Label[] CreateArray(string[] names)
      {
         if (names == null || names.Length == 0)
            return null;

         //labels error: v1.0 contains invalid characters( , !, #, &, (, ), *, , , ., :, ;, <, >, ?, @, [, ], ^).

         return names.Select(name => new Label { Name = Sanitise(name) }).ToArray();
      }

      private static string Sanitise(string name)
      {
         const string r = "-";
         return name
            .Replace(",", r)
            .Replace("!", r)
            .Replace("#", r)
            .Replace("&", r)
            .Replace("(", r)
            .Replace(")", r)
            .Replace("*", r)
            .Replace(",", r)
            .Replace(".", r)
            .Replace(":", r)
            .Replace(";", r)
            .Replace("<", r)
            .Replace(">", r)
            .Replace("?", r)
            .Replace("@", r)
            .Replace("[", r)
            .Replace("]", r)
            .Replace("^", r)
            .Replace(" ", r)
            .ToLower();
      }
   }

   class ContentProperty
   {
      [JsonProperty("key")]
      public string Key { get; set; }

      [JsonProperty("value")]
      public object Value { get; set; }
   }

   class ContentProperties
   {
      [JsonProperty("results")]
      public ContentProperty[] Results { get; set; }
   }

   #endregion
}
