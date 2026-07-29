using Contensive.BaseClasses;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Contensive.Blog.Controllers {
    public sealed class _GenericController {
        private _GenericController() {
        }
        //
        //========================================================================
        /// <summary>
        /// added to cp.utils 260124
        /// Encode a url to be used in an href or src tag.
        /// Preserve the protocol if present, encode between slashes and querystring
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string encodeURLForHrefSrc(string url) {
            if (string.IsNullOrWhiteSpace(url)) { return url; }
            // Check for protocol
            string[] protocols = { "https://", "http://" };
            foreach (var protocol in protocols) {
                if (url.StartsWith(protocol, StringComparison.OrdinalIgnoreCase)) {
                    string withoutProtocol = url.Substring(protocol.Length);
                    return protocol + Uri.EscapeUriString(withoutProtocol);
                }
            }
            // No protocol - relative URL
            return Uri.EscapeUriString(url);
        }
        // 
        // ====================================================================================================
        /// <summary>
        /// if date is invalid, set to minValue
        /// </summary>
        /// <param name="srcDate"></param>
        /// <returns></returns>
        public static DateTime encodeMinDate(DateTime srcDate) {
            var returnDate = srcDate;
            if (srcDate < new DateTime(1900, 1, 1)) {
                returnDate = DateTime.MinValue;
            }
            return returnDate;
        }
        // 
        // ====================================================================================================
        /// <summary>
        /// if valid date, return the short date, else return blank string 
        /// </summary>
        /// <param name="srcDate"></param>
        /// <returns></returns>
        public static string getShortDateString(DateTime srcDate) {
            string returnString = "";
            var workingDate = encodeMinDate(srcDate);
            if (!isDateEmpty(srcDate)) {
                returnString = workingDate.ToShortDateString();
            }
            return returnString;
        }
        // 
        // ====================================================================================================
        // 
        public static bool isDateEmpty(DateTime srcDate) {
            return srcDate < new DateTime(1900, 1, 1);
        }
        // 
        // ====================================================================================================
        // 
        public static string getSortOrderFromInteger(int id) {
            return id.ToString().PadLeft(7, '0');
        }
        // 
        // ====================================================================================================
        // 
        public static string getDateForHtmlInput(DateTime source) {
            if (isDateEmpty(source)) {
                return "";
            } else {
                return source.Year.ToString() + "-" + source.Month.ToString().PadLeft(2, '0') + "-" + source.Day.ToString().PadLeft(2, '0');
            }
        }
        // 
        // ====================================================================================================
        // 
        public static string convertToDosPath(string sourcePath) {
            return sourcePath.Replace("/", @"\");
        }
        // 
        // ====================================================================================================
        // 
        public static string convertToUnixPath(string sourcePath) {
            return sourcePath.Replace(@"\", "/");
        }
        // 
        // ====================================================================================================
        /// <summary>
        /// is a member of a group in the group model list
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="groupList"></param>
        /// <returns></returns>
        public static bool isGroupListMember(CPBaseClass cp, List<Models.GroupModel> groupList) {
            foreach (var Group in groupList) {
                if (cp.User.IsInGroup(Group.name))
                    return true;
            }
            return false;
        }
        // 
        // ====================================================================================================
        /// <summary>
        /// Return a shortened version of the copy
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="rawCopy"></param>
        /// <param name="MaxLength"></param>
        /// <returns></returns>
        internal static string getBriefCopy(CPBaseClass cp, string rawCopy, int MaxLength) {
            try {
                return truncateHtml(rawCopy, MaxLength);
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return "";
            }
        }
        //
        // ====================================================================================================
        /// <summary>
        /// Truncate HTML to a maximum number of visible text characters without splitting tags.
        /// Counts only visible text toward the limit, preserves complete HTML tags,
        /// and closes any open tags in the correct order.
        /// </summary>
        internal static string truncateHtml(string html, int maxLength) {
            if (string.IsNullOrEmpty(html)) { return html; }
            var result = new System.Text.StringBuilder();
            var openTags = new Stack<string>();
            int textLength = 0;
            int i = 0;
            bool truncated = false;
            while (i < html.Length) {
                if (textLength >= maxLength) {
                    truncated = true;
                    break;
                }
                if (html[i] == '<') {
                    // -- find the end of this tag
                    int tagEnd = html.IndexOf('>', i);
                    if (tagEnd < 0) {
                        // malformed html, stop here
                        break;
                    }
                    string tag = html.Substring(i, tagEnd - i + 1);
                    result.Append(tag);
                    // -- determine if this is an opening, closing, or self-closing tag
                    string tagContent = tag.Substring(1, tag.Length - 2).Trim();
                    bool isSelfClosing = tagContent.EndsWith("/") || tag.StartsWith("<br", StringComparison.OrdinalIgnoreCase) || tag.StartsWith("<img", StringComparison.OrdinalIgnoreCase) || tag.StartsWith("<hr", StringComparison.OrdinalIgnoreCase) || tag.StartsWith("<input", StringComparison.OrdinalIgnoreCase);
                    if (tagContent.StartsWith("/")) {
                        // -- closing tag, pop from stack
                        string closingName = Regex.Match(tagContent, @"/\s*(\w+)").Groups[1].Value.ToLowerInvariant();
                        // -- pop tags until we find the matching one (handles minor nesting issues)
                        var tempStack = new Stack<string>();
                        while (openTags.Count > 0) {
                            string top = openTags.Pop();
                            if (top == closingName) { break; }
                            tempStack.Push(top);
                        }
                        // -- push back any non-matching tags
                        while (tempStack.Count > 0) {
                            openTags.Push(tempStack.Pop());
                        }
                    } else if (!isSelfClosing && !tagContent.StartsWith("!")) {
                        // -- opening tag, push tag name onto stack
                        string tagName = Regex.Match(tagContent, @"^(\w+)").Groups[1].Value.ToLowerInvariant();
                        if (!string.IsNullOrEmpty(tagName)) {
                            openTags.Push(tagName);
                        }
                    }
                    i = tagEnd + 1;
                } else if (html[i] == '&') {
                    // -- HTML entity (e.g. &amp;), counts as one visible character
                    int semiPos = html.IndexOf(';', i);
                    if (semiPos > i && semiPos - i < 10) {
                        result.Append(html, i, semiPos - i + 1);
                        i = semiPos + 1;
                    } else {
                        result.Append(html[i]);
                        i++;
                    }
                    textLength++;
                } else {
                    // -- visible text character
                    result.Append(html[i]);
                    textLength++;
                    i++;
                }
            }
            // -- if we truncated, append ellipsis and close open tags
            if (truncated) {
                result.Append("...");
                while (openTags.Count > 0) {
                    result.Append($"</{openTags.Pop()}>");
                }
            }
            return result.ToString();
        }
        // 
        // ====================================================================================
        // 
        public static string getFormTableRow2(CPBaseClass cp, string Innards) {
            return "<tr>" + "<td colspan=2 width=\"100%\">" + Innards + "</td>" + "</tr>";

        }
        // 
        // ====================================================================================
        // 
        public static string getFormTableRow(CPBaseClass cp, string FieldCaption, string Innards, bool AlignLeft = true) {
            // 
            string Stream = "";
            try {
                string AlignmentString = AlignLeft ? " align=right" : " align=left";
                Stream = Stream + "<tr>";
                Stream = Stream + "<td Class=\"aoBlogTableRowCellLeft\" " + AlignmentString + ">" + FieldCaption + "</td>";
                Stream = Stream + "<td Class=\"aoBlogTableRowCellRight\">" + Innards + "</td>";
                Stream = Stream + "</tr>";
                return Stream;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        // 
        // ====================================================================================
        // 
        public static string getField(CPBaseClass cp, string RequestName, int Height, int Width, int MaxLenghth, string DefaultValue) {
            string result = "";
            try {
                if (Height == 0) {
                    Height = 1;
                }
                if (Width == 0) {
                    Width = 25;
                }
                // 
                result = cp.Html.InputText(RequestName, DefaultValue, 255);
                result = Regex.Replace(result, "<INPUT ", $"<INPUT maxlength=\"{MaxLenghth}\" ", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, "<INPUT ", $"<INPUT style=\"width:{MaxLenghth}ch;max-width: 100%;\" ", RegexOptions.IgnoreCase);
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
        // 
        public static string addEditWrapper(CPBaseClass cp, string innerHtml, int recordId, string recordName, string contentName) {

            return cp.Content.GetEditWrapper(innerHtml, contentName, recordId);
        }

    }
}