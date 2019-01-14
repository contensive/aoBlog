
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions
Imports Contensive.Addons.Blog.Controllers
Imports Contensive.Addons.Blog.Models
Imports Contensive.BaseClasses

Namespace Views
    '
    Public Class BlogBodySearchClass
        '
        '====================================================================================
        '
        Public Shared Function GetFormBlogSearch(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, request As View.RequestModel, user As PersonModel, blogListLink As String, blogListQs As String) As String
            Dim result As New StringBuilder()
            Try
                Call cp.Doc.AddRefreshQueryString(RequestNameBlogEntryID, "")
                '
                result.Append("<h2>" & blog.name & " Search</h2>")
                '
                ' Search results
                '
                Dim QueryTag As String = cp.Doc.GetText(RequestNameQueryTag)
                Dim Button As String = cp.Doc.GetText("button")
                If (Button = FormButtonSearch) Or (QueryTag <> "") Then
                    Dim Subcaption As String = ""
                    Dim sqlCriteria As New StringBuilder("(blogid=" & blog.id & ")")
                    '
                    ' -- Date
                    Dim DateSearch As Date = Date.MinValue
                    If ((Not String.IsNullOrWhiteSpace(request.DateSearchText)) AndAlso (IsDate(request.DateSearchText))) Then
                        If CDate(request.DateSearchText) > CDate("1/1/2000") Then DateSearch = CDate(request.DateSearchText)
                    End If
                    '
                    ' -- Keyword list
                    Dim subCriteria As New StringBuilder()
                    If (request.KeywordList <> "") Then
                        Dim KeyWordsArray() As String = Split("," & request.KeywordList & ",", ",", , vbTextCompare)
                        For Each keyword In KeyWordsArray
                            If (Not String.IsNullOrWhiteSpace(keyword)) Then
                                Subcaption &= " or '<i>" & cp.Db.EncodeSQLText(keyword) & "</i>'"
                                subCriteria.Append("or(Copy like " & cp.Db.EncodeSQLText("%" & keyword & "%") & ") or (name like " & cp.Db.EncodeSQLText("%" & keyword & "%") & ")")
                            End If
                        Next
                        If Subcaption <> "" Then Subcaption = " containing " & Subcaption.Substring(4)
                        If (subCriteria.Length > 0) Then sqlCriteria.Append("and(" & subCriteria.ToString().Substring(2) & ")")
                    End If
                    If (DateSearch <> Date.MinValue) Then
                        Dim SearchMonth As Integer = Month(DateSearch)
                        Dim SearchYear As Integer = Year(DateSearch)
                        Subcaption &= " in " & SearchMonth & "/" & SearchYear
                        sqlCriteria.Append("and(DateAdded>=" & cp.Db.EncodeSQLDate(DateSearch) & ")and(DateAdded<" & cp.Db.EncodeSQLDate(DateSearch.AddMonths(1)) & ")")
                    End If
                    If (QueryTag <> "") Then
                        Subcaption &= " tagged with '<i>" & cp.Utils.EncodeHTML(QueryTag) & "</i>'"
                        QueryTag = cp.Db.EncodeSQLText(QueryTag)
                        QueryTag = "'%" & Mid(QueryTag, 2, Len(QueryTag) - 2) & "%'"
                        sqlCriteria.Append("and(taglist like " & QueryTag & ")")
                    End If
                    If Subcaption <> "" Then
                        Subcaption = "Search for posts " & Subcaption
                    End If
                    '
                    ' Display the results
                    If Not cp.UserError.OK Then
                        Subcaption &= cp.Html.ul(cp.UserError.GetList)
                    End If
                    result.Append("<h4 class=""aoBlogEntryCopy"">" & Subcaption & "</h4>")
                    '
                    Dim BlogEntryList = DbModel.createList(Of BlogEntryModel)(cp, sqlCriteria.ToString())
                    If (BlogEntryList.Count = 0) Then
                        result.Append("</br>" & "<div class=""aoBlogProblem"">There were no matches to your search</div>")
                    Else
                        result.Append("</br>" & "<div class=""aoBlogEntryDivider"">&nbsp;</div>")
                        For Each blogSearchEntry In BlogEntryList
                            Dim AuthorMemberID As Integer = blogSearchEntry.AuthorMemberID
                            If AuthorMemberID = 0 Then AuthorMemberID = blogSearchEntry.CreatedBy
                            Dim Return_CommentCnt As Integer
                            result.Append(genericController.GetBlogEntryCell(cp, blog, rssFeed, blogSearchEntry, user, False, True, Return_CommentCnt, "", blogListQs))
                            result.Append(cr & "<div class=""aoBlogEntryDivider"">&nbsp;</div>")
                        Next
                    End If
                    result = New StringBuilder(cp.Html.div(result.ToString(), "", "aoBlogSearchResultsCon"))
                End If
                '
                result.Append("" _
                    & "<div  class=""aoBlogSearchFormCon"">" _
                    & "<table width=100% border=0 cellspacing=0 cellpadding=5 class=""aoBlogSearchTable"">" _
                    & genericController.GetFormTableRow(cp, "Date:", genericController.GetField(cp, RequestNameDateSearch, 1, 10, 10, cp.Doc.GetText(RequestNameDateSearch).ToString) & " " & "&nbsp;(mm/yyyy)") _
                    & genericController.GetFormTableRow(cp, "Keyword(s):", genericController.GetField(cp, RequestNameKeywordList, 1, 10, 30, cp.Doc.GetText(RequestNameKeywordList))) _
                    & genericController.GetFormTableRow(cp, "", cp.Html.Button(rnButton, FormButtonSearch)) _
                    & genericController.GetFormTableRow2(cp, "<div class=""aoBlogFooterLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>") _
                    & "</table>" _
                    & "</div>")
                result.Append(cr & "<input type=""hidden"" name=""" & RequestNameSourceFormID & """ value=""" & FormBlogSearch.ToString & """>")
                result.Append(cr & "<input type=""hidden"" name=""" & RequestNameFormID & """ value=""" & FormBlogSearch.ToString & """>")
                Return cp.Html.Form(result.ToString())
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Return String.Empty
            End Try
        End Function
    End Class
End Namespace
