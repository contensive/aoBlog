using Contensive.Blog.Models;
using Contensive.Blog.Views;
using Contensive.BaseClasses;
using System;

namespace Contensive.Blog {
    public class OnInstall : AddonBaseClass {
        // 
        // =====================================================================================
        /// <summary>
        /// Blog Addon
        /// </summary>
        /// <param name="CP"></param>
        /// <returns></returns>
        public override object Execute(CPBaseClass CP) {
            try {
                //
                CP.Log.Info("Blog OnInstall");
                //
                CP.Db.ExecuteNonQuery("delete from ccaggregatefunctions where ccguid='{656E95EA-2799-45CD-9712-D4CEDF0E2D02}'");
                CP.Db.ExecuteNonQuery("delete from ccaggregatefunctions where ccguid='{534B4D50-B894-42BC-AB42-7AF6EC6265DE}'");
                CP.Db.ExecuteNonQuery($"update ccfields set MemberSelectGroupID={CP.Content.GetRecordID("groups", "Site Managers")} where name='authormemberid'");
                CP.Db.ExecuteNonQuery("delete from ccfields where name='boaddedupuserid'");
                //
                // -- reset layouts
                CP.Db.ExecuteNonQuery($"delete from cclayouts where ccguid={CP.Db.EncodeSQLText(constants.layoutGuidLastestPosts)}");
                return "";
            }
            // 
            catch (Exception ex) {
                CP.Site.ErrorReport(ex);
                throw;
            }
        }

    }
}