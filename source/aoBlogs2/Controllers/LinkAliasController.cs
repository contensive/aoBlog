
using Contensive.Models.Db;
using System;
using Contensive.Blog.Models;
using Contensive.BaseClasses;
using Microsoft.VisualBasic;

namespace Contensive.Blog.Controllers {
    public sealed class LinkAliasController {
        // 
        /// <summary>
        /// Add a blog entry to link alias
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="blogPostname"></param>
        /// <param name="blogEntryId"></param>
        public static void addLinkAlias(CPBaseClass cp, string blogPostname, int blogEntryId, string src) {
            string qs = getLinkAliasQueryString(cp, blogEntryId);
            cp.Site.AddLinkAlias(blogPostname, cp.Doc.PageId, qs);
        }
        // 
        /// <summary>
        /// get the linkalias querystring for this blogid
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="blogEntryId"></param>
        /// <returns></returns>
        public static string getLinkAliasQueryString(CPBaseClass cp, int blogEntryId) {
            string qs = cp.Utils.ModifyQueryString("", constants.RequestNameBlogEntryID, blogEntryId.ToString());
            return cp.Utils.ModifyQueryString(qs, constants.rnFormID, constants.FormBlogPostDetails.ToString());
        }
        // 
        /// <summary>
        /// delete linkalias for this blog entry
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="blogEntryId"></param>
        public static void deleteLinkAlias(CPBaseClass cp, int blogEntryId) {
            string linkAliasQS = getLinkAliasQueryString(cp, blogEntryId);
            foreach (var linkAlias in DbBaseModel.createList<LinkAliasModel>(cp, "(QueryStringSuffix=" + cp.Db.EncodeSQLText(linkAliasQS) + ")"))
                DbBaseModel.delete<LinkAliasModel>(cp, linkAlias.id);
        }

    }
}