using System;
using Contensive.Addons.Blog.Models;
using Contensive.BaseClasses;
using Microsoft.VisualBasic;

namespace Contensive.Addons.Blog.Controllers {
    public sealed class LinkAliasController {
        // 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="defaultLink"></param>
        /// <returns></returns>
        /// 

        public static string getLinkAlias(CPBaseClass cp, string defaultLink, int pageId) {
            string getLinkAliasRet = default;
            // 
            string result = "";
            try {
                result = defaultLink;
                if (cp.Site.GetBoolean("allowLinkAlias", true)) {
                    string Link = defaultLink;
                    // 
                    string[] pageQs = Strings.Split(Strings.LCase(Link), "?");
                    if (Information.UBound(pageQs) > 0) {
                        string[] nameValues = Strings.Split(pageQs[1], "&");
                        int cnt = Information.UBound(nameValues) + 1;
                        if (Information.UBound(nameValues) < 0) {
                        }
                        else {
                            string qs = "";
                            int Ptr;
                            //var pageId = default(int);
                            var loopTo = cnt - 1;
                            for (Ptr = 0; Ptr <= loopTo; Ptr++) {
                                string NameValue = nameValues[Ptr];
                                if (pageId == 0) {
                                    if (Strings.Mid(NameValue, 1, 4) == "bid=") {
                                        pageId = cp.Utils.EncodeInteger(Strings.Mid(NameValue, 5));
                                        NameValue = "";
                                    }
                                }
                                if (!string.IsNullOrEmpty(NameValue)) {
                                    qs = qs + "&" + NameValue;
                                }
                            }
                            if (pageId != 0) {
                                if (Strings.Len(qs) > 1) {
                                    qs = Strings.Mid(qs, 2);
                                }
                                result = cp.Content.GetLinkAliasByPageID(pageId, qs, defaultLink);
                            }
                        }
                    }
                }
                getLinkAliasRet = result;
            }
            // 
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }

        public static string getLinkAlias(CPBaseClass cp, string defaultLink) {
            string getLinkAliasRet = default;
            // 
            string result = "";
            try {
                result = defaultLink;
                if (cp.Site.GetBoolean("allowLinkAlias", true)) {
                    string Link = defaultLink;
                    // 
                    string[] pageQs = Strings.Split(Strings.LCase(Link), "?");
                    if (Information.UBound(pageQs) > 0) {
                        string[] nameValues = Strings.Split(pageQs[1], "&");
                        int cnt = Information.UBound(nameValues) + 1;
                        if (Information.UBound(nameValues) < 0) {
                        }
                        else {
                            string qs = "";
                            int Ptr;
                            var pageId = default(int);
                            var loopTo = cnt - 1;
                            for (Ptr = 0; Ptr <= loopTo; Ptr++) {
                                string NameValue = nameValues[Ptr];
                                if (pageId == 0) {
                                    if (Strings.Mid(NameValue, 1, 4) == "bid=") {
                                        pageId = cp.Utils.EncodeInteger(Strings.Mid(NameValue, 5));
                                        NameValue = "";
                                    }
                                }
                                if (!string.IsNullOrEmpty(NameValue)) {
                                    qs = qs + "&" + NameValue;
                                }
                            }
                            if (pageId != 0) {
                                if (Strings.Len(qs) > 1) {
                                    qs = Strings.Mid(qs, 2);
                                }
                                result = cp.Content.GetLinkAliasByPageID(pageId, qs, defaultLink);
                            }
                        }
                    }
                }
                getLinkAliasRet = result;
            }
            // 
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
        // 
        /// <summary>
        /// Add a blog entry to link alias
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="blogPostname"></param>
        /// <param name="pageId"></param>
        /// <param name="blogEntryId"></param>
        public static void addLinkAlias(CPBaseClass cp, string blogPostname, int pageId, int blogEntryId) {
            string qs = getLinkAliasQueryString(cp, pageId, blogEntryId);
            cp.Site.AddLinkAlias(blogPostname, cp.Doc.PageId, qs);
        }
        // 
        /// <summary>
        /// get the linkalias querystring for this blogid
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="pageId"></param>
        /// <param name="blogEntryId"></param>
        /// <returns></returns>
        public static string getLinkAliasQueryString(CPBaseClass cp, int pageId, int blogEntryId) {
            string qs = cp.Utils.ModifyQueryString("", constants.RequestNameBlogEntryID, blogEntryId.ToString());
            return cp.Utils.ModifyQueryString(qs, constants.rnFormID, constants.FormBlogPostDetails.ToString());
        }
        // 
        /// <summary>
        /// delete linkalias for this blog entry
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="pageId"></param>
        /// <param name="blogEntryId"></param>
        public static void deleteLinkAlias(CPBaseClass cp, int pageId, int blogEntryId) {
            string linkAliasQS = getLinkAliasQueryString(cp, pageId, blogEntryId);
            foreach (var linkAlias in DbModel.createList<LinkAliasesModel>(cp, "(QueryStringSuffix=" + cp.Db.EncodeSQLText(linkAliasQS) + ")"))
                DbModel.delete<LinkAliasesModel>(cp, linkAlias.id);
        }

    }
}