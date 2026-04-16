using System;
using Contensive.BaseClasses;
using Contensive.Models.Db;

namespace Contensive.Blog {
    public class SubscribeEmailRemote : AddonBaseClass {
        //
        // =====================================================================================
        // addon api
        // =====================================================================================
        //
        public override object Execute(CPBaseClass cp) {
            string returnHtml;
            try {
                string email = cp.Doc.GetText("email");
                int blogId = cp.Doc.GetInteger("blogId");
                int groupId = 0;
                int userId = 0;
                //
                if (cp.User.IsRecognized & !cp.User.IsAuthenticated) {
                    cp.User.Logout();
                }
                //
                if (!string.IsNullOrEmpty(email) & blogId != 0) {
                    //
                    // -- get blog and groupId
                    var blog = DbBaseModel.create<Models.BlogModel>(cp, blogId);
                    string blogName = blog?.name ?? "";
                    groupId = blog?.emailSubscribeGroupId ?? 0;
                    //
                    // -- verify group exists, create if needed
                    if (groupId > 0) {
                        var group = DbBaseModel.create<Models.GroupModel>(cp, groupId);
                        if (group is null) { groupId = 0; }
                    }
                    if (groupId == 0) {
                        var newGroup = DbBaseModel.addDefault<Models.GroupModel>(cp);
                        if (newGroup is not null) {
                            groupId = newGroup.id;
                            newGroup.name = $"Email Subscriptions for Blog {blogName}";
                            newGroup.caption = $"Email Subscriptions for Blog {blogName}";
                            newGroup.allowBulkEmail = true;
                            newGroup.publicJoin = true;
                            newGroup.save(cp);
                        }
                        if (blog is not null) {
                            blog.emailSubscribeGroupId = groupId;
                            blog.save(cp);
                        }
                    }
                    //
                    if (groupId > 0) {
                        //
                        // -- find user by email
                        var personList = DbBaseModel.createList<Models.PersonModel>(cp, $"email={cp.Db.EncodeSQLText(email)}", "id", 1, 1);
                        if (personList.Count > 0) {
                            userId = personList[0].id;
                        }
                        //
                        if (userId == 0) {
                            //
                            // -- email not found, set to current user
                            userId = cp.User.Id;
                            var person = DbBaseModel.create<Models.PersonModel>(cp, cp.User.Id);
                            if (person is not null) {
                                person.email = email;
                                person.save(cp);
                            }
                        }
                        //
                        // -- add member rule if not exists
                        var existingRules = DbBaseModel.createList<MemberRuleModel>(cp, $"memberid={cp.User.Id} and groupid={groupId}", "id", 1, 1);
                        if (existingRules.Count == 0) {
                            var memberRule = DbBaseModel.addDefault<MemberRuleModel>(cp);
                            if (memberRule is not null) {
                                memberRule.groupId = groupId;
                                memberRule.memberId = cp.User.Id;
                                memberRule.save(cp);
                            }
                        }
                    }
                }
                //
                // -- set flag to only allow this once per visit
                cp.Visit.SetProperty($"EmailSubscribed-Blog{blogId}-user{cp.User.Id}", "1");
                //
                returnHtml = "OK";
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex, "execute");
                returnHtml = "";
            }
            return returnHtml;
        }
    }
}
