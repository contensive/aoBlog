
using System.Collections.Generic;
using Contensive.BaseClasses;

namespace Contensive.Blog.Models {
    public class GroupModel : Contensive.Models.Db.GroupModel {
        //        
        public static List<GroupModel> GetBlockingGroups(CPBaseClass cp, int BlogCategoryID) {
            // 
            var result = new List<GroupModel>();
            // 
            foreach (var BlogCategoryGroupRule in createList<BlogCategoryGroupRulesModel>(cp, "BlogCategoryID=" + BlogCategoryID))
                result.Add(create<GroupModel>(cp, BlogCategoryGroupRule.GroupID));
            return result;
        }

    }
}