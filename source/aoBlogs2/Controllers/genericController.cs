using System;
using System.Collections.Generic;
using Contensive.Addons.Blog.Models;
using Contensive.BaseClasses;
using Contensive.Models.Db;
using Microsoft.VisualBasic;

namespace Contensive.Addons.Blog.Controllers {
    public sealed class genericController {
        private genericController() {
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
                string Copy = cp.Utils.ConvertHTML2Text(rawCopy);
                if (Strings.Len(Copy) > MaxLength) {
                    Copy = Strings.Left(Copy, MaxLength);
                    Copy = Copy + "...";
                }
                return Copy;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return "";
            }
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
                result = Strings.Replace(result, "<INPUT ", "<INPUT maxlength=\"" + MaxLenghth + "\" ", 1, 99, CompareMethod.Text);
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