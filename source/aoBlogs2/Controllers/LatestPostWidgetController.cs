
using Contensive.BaseClasses;
using System;

namespace Contensive.Addons.Blog {
    public class LatestPostWidgetController {
        public static string limitString(CPBaseClass cp, string content, int maxLength) {
            try {
                string newResult = "";
                if (content.Length > maxLength) {
                    if (content[maxLength] == ' ') {
                        newResult = content.Substring(0, maxLength) + "...";
                    }
                    else {
                        int j = maxLength;
                        while (j < content.Length && content[j] != ' ') {
                            j++;
                        }
                        newResult = content.Substring(0, j) + "...";
                    }
                }
                else {
                    newResult = content;
                }
                return newResult;
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return "";
            }
        }
    }
}