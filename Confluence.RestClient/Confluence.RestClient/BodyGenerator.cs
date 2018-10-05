/*using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace Confluence.RestClient
{
   public class BodyGenerator
   {
      private readonly HtmlDocument _html;

      public BodyGenerator(string htmlContent = null)
      {
         _html = new HtmlDocument();

         if(htmlContent != null)
         {
            _html.LoadHtml(htmlContent);
         }

         //otherwise <br/> is written as <br> and Confluence complains
         _html.OptionWriteEmptyNodes = true;
      }

      public void Inject(Schema schema, string anchorName)
      {
         HtmlNode container = GetInsertContainerForAnchor(anchorName);
         if (container == null)
            return;

         //delete all container's content
         container.ChildNodes.Clear();

         //declare table
         HtmlNode table = _html.CreateElement("table");
         container.AppendChild(table);
         HtmlNode tbody = _html.CreateElement("tbody");
         table.AppendChild(tbody);

         //create header row
         CreateTableRow(tbody, "th",
            "Position", "Field Name", "Data Type", "Format", "Required", "Description");

         int position = 0;
         foreach (Column column in schema.Columns)
         {
            CreateTableRow(tbody, "td",
               (++position).ToString(),
               column.Name,
               column.DataType,
               column.Format,
               column.Required != null ? (column.Required.Value ? "Yes" : "Optional") : string.Empty,
               column.Description);
         }
      }

      public void Inject(Example example, Schema schema, string anchorName)
      {
         HtmlNode container = GetInsertContainerForAnchor(anchorName);
         if (container == null)
            return;

         //delete all container's content
         container.ChildNodes.Clear();

         //declare table
         HtmlNode table = _html.CreateElement("table");
         container.AppendChild(table);
         HtmlNode tbody = _html.CreateElement("tbody");
         table.AppendChild(tbody);

         //create header row
         CreateTableRow(tbody, "th",
            schema.Columns.Select(c => c.Name).ToArray());

         foreach (List<object> row in example.Data)
         {
            CreateTableRow(tbody, "td",
               row.Select(c => c == null ? string.Empty : c.ToString()).ToArray()
               );
         }
      }


      public void AddAttachmentLink(string fileName)
      {
         //<ac:link><ri:attachment ri:filename=\"postman_collection.json\" ri:version-at-save=\"1\" /></ac:link>

         HtmlNode linkNode = _html.CreateElement("ac:link");
         _html.DocumentNode.AppendChild(linkNode);

         HtmlNode attachmentNode = _html.CreateElement("ri:attachment");
         attachmentNode.SetAttributeValue("ri:filename", fileName);
         linkNode.AppendChild(attachmentNode);
      }

      public void InjectHiddenProperty(string propertyName, string value)
      {
         HtmlNode div =_html.CreateElement("div");
         _html.DocumentNode.AppendChild(div);

         div.SetAttributeValue("id", propertyName);
         div.SetAttributeValue("class", value);
      }

      private void CreateTableRow(HtmlNode parent, string cellTagName, params string[] elements)
      {
         HtmlNode containerNode = _html.CreateElement("tr");
         parent.AppendChild(containerNode);

         foreach(string element in elements)
         {
            HtmlNode cellNode = _html.CreateElement(cellTagName);

            cellNode.InnerHtml = HtmlDocument.HtmlEncode(element);

            containerNode.AppendChild(cellNode);
         }
      }

      private HtmlNode GetInsertContainerForAnchor(string anchorName)
      {
         HtmlNode anchor = FindAtlassianAnchor(anchorName);
         if (anchor == null)
            return null;

         return anchor.ParentNode.NextSibling;
      }

      private HtmlNode FindAtlassianAnchor(string anchorName)
      {
         return Find(hn =>
         {
            if (hn.Name == "ac:structured-macro")
            {
               if (hn.GetAttributeValue("ac:name", null) == "anchor")
               {
                  foreach (HtmlNode cn in hn.ChildNodes)
                  {
                     if (cn.Name == "ac:parameter" && cn.InnerText == anchorName)
                     {
                        return true;
                     }
                  }
               }
            }

            return false;
         });
      }

      private HtmlNode Find(Func<HtmlNode, bool> matcher)
      {
         return Find(_html.DocumentNode.ChildNodes, matcher);
      }

      private HtmlNode Find(HtmlNodeCollection nodes, Func<HtmlNode, bool> matcher)
      {
         foreach (HtmlNode hn in nodes)
         {
            if (matcher(hn))
               return hn;

            HtmlNode cr = Find(hn.ChildNodes, matcher);
            if (cr != null)
               return cr;
         }

         return null;
      }

      public override string ToString()
      {
         return _html.DocumentNode.WriteContentTo();
      }
   }
}*/