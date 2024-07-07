using System;
using Contensive.BaseClasses;

namespace Contensive.Blog {
    public class SubscribeEmailRemote : AddonBaseClass {
        // 
        // =====================================================================================
        // addon api
        // =====================================================================================
        // 
        public override object Execute(CPBaseClass CP) {
            string returnHtml;
            try {
                string email = CP.Doc.GetText("email");
                int blogId = CP.Doc.GetInteger("blogId");
                string blogName = "";
                var cs = CP.CSNew();
                int groupId = 0;
                int userId = 0;
                // 
                if (CP.User.IsRecognized & !CP.User.IsAuthenticated) {
                    CP.User.Logout();
                }
                // 
                if (!string.IsNullOrEmpty(email) & blogId != 0) {
                    if (cs.Open("blogs", "id=" + blogId)) {
                        blogName = cs.GetText("name");
                        groupId = cs.GetInteger("emailSubscribeGroupId");
                    }
                    cs.Close();
                    if (!cs.Open("groups", "id=" + groupId)) {
                        cs.Close();
                        if (cs.Insert("Groups")) {
                            groupId = cs.GetInteger("id");
                            cs.SetField("name", "Email Subscriptions for Blog " + blogName);
                            cs.SetField("caption", "Email Subscriptions for Blog " + blogName);
                            cs.SetField("allowbulkemail", "1");
                            cs.SetField("publicjoin", "1");
                        }
                        cs.Close();
                        if (cs.Open("blogs", "id=" + blogId)) {
                            cs.SetField("emailSubscribeGroupId", groupId.ToString());
                        }
                    }
                    cs.Close();
                    // 
                    if (groupId > 0) {
                        if (cs.Open("people", "email=" + CP.Db.EncodeSQLText(email))) {
                            userId = cs.GetInteger("id");
                        }
                        cs.Close();
                        // 
                        if (userId == 0) {
                            // 
                            // this email address was not found in users, set it to the current user, authenticated or not
                            // (recognized case is not possible b/c check at top of routine)
                            // 
                            userId = CP.User.Id;
                            if (cs.Open("people", "id=" + CP.User.Id)) {
                                cs.SetField("email", email);
                            }
                            cs.Close();
                        }
                        if (!cs.Open("member rules", "memberid=" + CP.User.Id + " and groupid=" + groupId)) {
                            cs.Close();
                            if (cs.Insert("member rules")) {
                                cs.SetField("groupid", groupId);
                                cs.SetField("memberid", CP.User.Id);
                            }
                        }
                        cs.Close();
                        // Call CP.Group.AddUser(groupId.ToString())
                    }
                }
                // 
                // set flag to only allow this once per visit (for anonymous users who enter an email for soeone else)
                // 
                CP.Visit.SetProperty("EmailSubscribed-Blog" + blogId + "-user" + CP.User.Id, "1");
                // 
                // return OK to display thank you message
                // 

                returnHtml = "OK";
            }
            catch (Exception ex) {
                CP.Site.ErrorReport(ex, "execute");
                returnHtml = "";
            }
            return returnHtml;
        }

    }
}