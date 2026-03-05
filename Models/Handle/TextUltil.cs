using HtmlAgilityPack;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text;
using System.Xml;
using System.Globalization; 
using Ganss.Xss;
using System.Web;
namespace API.Models.Handle
{
    public static class TextUltil
    {
        public const string Domain = "http://localhost:44365/";
        public static string RemoveHtmlTags(this string text)
        {
            if (text == null)
            {
                return string.Empty;
            }

            if (text.Length > 1000)
            {
                text = text.Substring(0, 1000);
            }

            string text2 = HttpUtility.HtmlDecode(text.ToLower());
            int num = text2.IndexOf("alert");
            if (num > 0)
            {
                text = text.Substring(0, num - 1).Replace("write(", "").Replace("console", "");
            }

            return Regex.Replace(text, "<[^>]*>", string.Empty).Trim().Replace("\"", "")
                .Replace("'", "");
        }
        public static string RemoveXSS(this string? html)
        {
            if (string.IsNullOrEmpty(html)) return html;
            HtmlSanitizer htmlSanitizer = new HtmlSanitizer();
            htmlSanitizer.AllowedAttributes.Add("style");
            htmlSanitizer.AllowedAttributes.Add("class");
            htmlSanitizer.AllowedAttributes.Add("loading");
            htmlSanitizer.AllowedAttributes.Add("controls");
            htmlSanitizer.AllowedAttributes.Add("type");
            htmlSanitizer.AllowedAttributes.Add("valign");
            htmlSanitizer.AllowedAttributes.Add("resizable");
            htmlSanitizer.AllowedAttributes.Add("autoplay");
            htmlSanitizer.AllowedAttributes.Add("controls");
            htmlSanitizer.AllowedAttributes.Add("muted");
            htmlSanitizer.AllowedAttributes.Add("src");
            htmlSanitizer.AllowedAttributes.Add("id");
            htmlSanitizer.AllowedAttributes.Add("href");
            htmlSanitizer.AllowedAttributes.Add("preload");
            htmlSanitizer.AllowedAttributes.Add("poster");
            htmlSanitizer.AllowedAttributes.Add("controlbox");
            htmlSanitizer.AllowedAttributes.Add("admin");
            htmlSanitizer.AllowedAttributes.Add("notnull");
            htmlSanitizer.AllowedTags.Add("iframe");
            htmlSanitizer.AllowedTags.Add("audio");
            htmlSanitizer.AllowedTags.Add("title");
            htmlSanitizer.AllowedTags.Add("description");
            htmlSanitizer.AllowedTags.Add("enclosure");
            htmlSanitizer.AllowedTags.Add("channel");
            htmlSanitizer.AllowedTags.Add("item");
            htmlSanitizer.AllowedTags.Add("marquee");
            htmlSanitizer.AllowedTags.Add("link");
            htmlSanitizer.AllowedTags.Add("pubdate");
            htmlSanitizer.AllowedTags.Add("source");
            htmlSanitizer.AllowedTags.Add("img");
            htmlSanitizer.AllowedTags.Add("video");
            htmlSanitizer.AllowedTags.Add("input");
            htmlSanitizer.AllowedTags.Add("textarea");
            htmlSanitizer.AllowedTags.Add("figure");
            htmlSanitizer.AllowedTags.Add("oembed");
            htmlSanitizer.AllowedCssProperties.Add("background-size");
            htmlSanitizer.AllowedCssProperties.Add("box-sizing");
            htmlSanitizer.AllowedCssProperties.Add("position");
            htmlSanitizer.AllowedCssProperties.Add("gap");
            htmlSanitizer.AllowedCssProperties.Add("flex");
            htmlSanitizer.AllowedCssProperties.Add("src");
            htmlSanitizer.AllowedCssProperties.Add("font-family");
            htmlSanitizer.AllowedCssProperties.Add("sans-serif");
            return htmlSanitizer.Sanitize(html);
        }
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
        public static string? AddFirstHostUrl(this string? Url, string? Firsthost)
        {
            if (string.IsNullOrEmpty(Url) || string.IsNullOrEmpty(Firsthost)) return Url;
            Uri uriResult;
            bool result = Uri.TryCreate(Url, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (result) return Url;
            return Firsthost.Trim().TrimEnd('/').Trim() + '/' + Url.Trim('/');
        }
        public static bool IsValidHttpUrl(this string uriName)
        {
            if (uriName == null) return false;
            Uri uriResult;
            return Uri.TryCreate(uriName, UriKind.Absolute, out uriResult)
                 && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
        public static XmlDocument ToXmlDocument(this string XmlText)
        {
            if (XmlText.Trim() == "")
            {
                return null;
            }

            XmlDocument xmlDocument = new XmlDocument();
            try
            {
                xmlDocument.LoadXml(XmlText);
                return xmlDocument;
            }
            catch
            {
                return null;
            }
        }
        public static string GenerateSlug(this string phrase)
        {
            string str = phrase.ToLower();
            // Xóa dấu tiếng việt
            string[] signs = new string[] {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ",
                "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ",
                "ÍÌỊỈĨ",
                "đ",
                "Đ",
                "ýỳỵỷỹ",
                "ÝỲỴỶỸ"
            };
            for (int i = 1; i < signs.Length; i++)
            {
                for (int j = 0; j < signs[i].Length; j++)
                {
                    str = str.Replace(signs[i][j], signs[0][i - 1]);
                }
            }
            // Thay khoảng trắng thành dấu gạch ngang
            str = System.Text.RegularExpressions.Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", " ").Trim();
            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s", "-");
            return str;
        }
        public static string VnTextToRequestText(this string? s)
        {
            if (s == null) return string.Empty;
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string input = s.Normalize(NormalizationForm.FormD);
            input = regex.Replace(input, string.Empty).Replace("đ", "d").Replace("Đ", "D");
            input = Regex.Replace(input, "[^a-zA-Z0-9_]+", "-").TrimEnd('-').ToLower();
            if (input.Length > 96)
            {
                return input.Substring(0, 96);
            }

            if (string.IsNullOrEmpty(input) || input.StartsWith("-"))
            {
                MD5 mD = MD5.Create();
                StringBuilder stringBuilder = new StringBuilder();
                byte[] array = mD.ComputeHash(Encoding.UTF8.GetBytes(s));
                foreach (byte b in array)
                {
                    stringBuilder.Append(b.ToString("x2").ToLower());
                }

                return stringBuilder.ToString();
            }

            return input;
        }
        public static bool IsInteger(this string? TextNummber)
        {
            int result;
            return int.TryParse(TextNummber, out result);
        }


        public static IEnumerable<TreeItem<T>> GenerateTree<T, K>(
        this IEnumerable<T> collection,
        Func<T, K> id_selector,
        Func<T, K> parent_id_selector,
        K root_id = default(K))
        {
            foreach (var c in collection.Where(c => EqualityComparer<K>.Default.Equals(parent_id_selector(c), root_id)))
            {
                yield return new TreeItem<T>
                {
                    Item = c,
                    Children = collection.GenerateTree(id_selector, parent_id_selector, id_selector(c))
                };
            }
        }
        public static string RemoveDiacritics(this string input)
        {
            string normalizedString = input.Normalize(NormalizationForm.FormD);
            StringBuilder result = new StringBuilder();

            foreach (char c in normalizedString)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    result.Append(c);
            }

            var rs = result.ToString().Normalize(NormalizationForm.FormC).ToLower().Replace("đ", "d");

            char[] buffer = new char[rs.Length];
            int idx = 0;

            foreach (char c in rs)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z')
                    || (c >= 'a' && c <= 'z') || (c == '.') || (c == '_') || (c == ' '))
                {
                    buffer[idx] = c;
                    idx++;
                }
            }
            rs = new string(buffer, 0, idx);
            while (rs.Contains("  ")) rs = rs.Replace("  ", " ");
            return rs.Trim().Replace(" ", "-");
        }

        public static string IncludeDomainToDetail(this string html, string? domain, Dictionary<string, string> tags, bool ConvertTable)
        {
            if (html == null || string.IsNullOrEmpty(domain) || domain.Trim() == string.Empty || tags == null || tags.Count == 0) return html;
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            if (ConvertTable)
            {
                IEnumerable<HtmlNode> tableNodes = htmlDocument.DocumentNode.SelectNodes("//table");
                if (tableNodes != null)
                {
                    foreach (HtmlNode tableNode in tableNodes)
                    {
                        HtmlNode divNode = htmlDocument.CreateElement("div");

                        IEnumerable<HtmlNode> rowNodes = tableNode.Descendants("tr");

                        foreach (HtmlNode rowNode in rowNodes)
                        {
                            HtmlNode divRow = htmlDocument.CreateElement("div");

                            IEnumerable<HtmlNode> cellNodes = rowNode.Descendants("td");

                            foreach (HtmlNode cellNode in cellNodes)
                            {
                                HtmlNode divCell = htmlDocument.CreateElement("div");
                                divCell.InnerHtml = cellNode.InnerHtml;
                                divRow.AppendChild(divCell);
                            }

                            divNode.AppendChild(divRow);
                        }
                        if (tableNode.ParentNode != null)
                            tableNode.ParentNode.ReplaceChild(divNode, tableNode);
                        else htmlDocument.DocumentNode.ReplaceChild(divNode, tableNode);
                    }
                }
            }
            string pattern = @"width(.*?)(;)";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            foreach (KeyValuePair<string, string> tag in tags)
            {
                var nodes = htmlDocument.DocumentNode.Descendants(tag.Key);
                foreach (var node in nodes)
                {
                    string attr = node.GetAttributeValue(tag.Value, null);
                    if (!attr.IsValidHttpUrl())
                    {
                        attr = attr.AddFirstHostUrl(domain);
                        node.SetAttributeValue(tag.Value, attr);
                    }
                    if (tag.Key == "img")
                    {
                        var attrWidth = node.GetAttributeValue("width", string.Empty);
                        if (!string.IsNullOrEmpty(attrWidth)) node.Attributes["width"].Remove();
                        var styleAttribute = node.Attributes["style"];
                        if (styleAttribute != null)
                        {
                            // Remove the 'width' property from the style attribute 
                            styleAttribute.Value = regex.Replace(styleAttribute.Value, String.Empty);
                        }
                    }
                }
            }
            var videoSourcePairs = htmlDocument.DocumentNode.SelectNodes("//video/source");
            if (videoSourcePairs != null)
            {
                foreach (var pair in videoSourcePairs)
                {
                    // Get the video tag
                    var videoTag = pair.ParentNode;

                    // Get the src attribute value from the source tag
                    string srcAttributeValue = pair.GetAttributeValue("src", "");

                    // Add a new attribute to the video tag with the src attribute value
                    videoTag.SetAttributeValue("src", srcAttributeValue);
                }
            }
            return htmlDocument.DocumentNode.OuterHtml;
        }
    }
    public class TreeItem<T>
    {
        public T Item { get; set; }
        public IEnumerable<TreeItem<T>>? Children { get; set; }
    }
}
