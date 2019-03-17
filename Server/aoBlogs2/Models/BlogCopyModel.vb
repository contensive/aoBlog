

Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Models
    ''' <summary>
    ''' Blog copy is the root content for the ccBlogCopy table. 
    ''' Blog Entries (posts) and Blog Comments (comments to posts) are both stored in Blog Copy
    ''' </summary>
    Public Class BlogCopyModel
        Inherits DbModel
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "Blog Copy"      '<------ set content name
        Public Const contentTableName As String = "ccBlogCopy"   '<------ set to tablename for the primary content (used for cache names)
        Private Shadows Const contentDataSource As String = "default"             '<------ set to datasource if not default
        '
        '====================================================================================================
        ' -- instance properties
        '
        Public Property allowComments As Boolean
        Public Property anonymous As Boolean
        Public Property approved As Boolean
        Public Property articlePrimaryImagePositionId As Integer
        Public Property authorMemberID As Integer
        Public Property blogCategoryID As Integer
        Public Property blogID As Integer
        Public Property entryID As Integer
        Public Property imageDisplayTypeId As Integer
        Public Property primaryImagePositionId As Integer
        Public Property viewings As Integer
        Public Property podcastMediaLink As String
        Public Property podcastSize As Integer
        Public Property tagList As String
        Public Property copy As String
        '
        ''' <summary>
        ''' Return a list of Blog Copy
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="blogId">The id of the Blog Copy</param>
        ''' <returns></returns>
        Public Shared Function createListFromBlogCopy(cp As CPBaseClass, blogId As Integer) As List(Of BlogCopyModel)
            Dim result As New List(Of BlogCopyModel)
            Try
                Dim Sql As String = "select p.ID" _
                        & " from (ccBlogCopy p" _
                        & " left join BlogCategories c on c.id=p.blogCategoryID)" _
                        & " where (p.blogid=" & blogId & ")" _
                        & " and((c.id is null)or(c.UserBlocking=0)or(c.UserBlocking is null))"
                result = createList(Of BlogCopyModel)(cp, "(id in (" & Sql & "))")
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        ''' <summary>
        ''' Return a list of Archive Blog Copy
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="blogId">The id of the Blog Copy</param>
        ''' <returns></returns>
        Public Shared Function createArchiveListFromBlogCopy(cp As CPBaseClass, blogId As Integer) As List(Of ArchiveDateModel)
            Dim result As New List(Of ArchiveDateModel)
            Try
                'result = createList(cp, "(BlogID=" & blogId & ")", "year(dateadded) desc, Month(DateAdded) desc")
                Dim SQL = "SELECT DISTINCT month(dateadded) as archiveMonth, year(dateadded) as archiveYear" _
                            & " From ccBlogCopy" _
                            & " Where (ContentControlID = " & cp.Content.GetID(cnBlogEntries) & ") And (Active <> 0)" _
                            & " AND (BlogID=" & blogId & ")" _
                            & " ORDER BY year(dateadded) desc, month(dateadded) desc "
                Dim cs As CPCSBaseClass = cp.CSNew()
                cp.Utils.AppendLog("archiveQuery=" & SQL)
                If (cs.OpenSQL(SQL)) Then
                    Do
                        Dim archiveDate As New ArchiveDateModel()
                        archiveDate.Month = cs.GetInteger("archiveMonth")
                        archiveDate.Year = cs.GetInteger("archiveYear")
                        result.Add(archiveDate)
                        cp.Utils.AppendLog("archiveDate=" & archiveDate.ToString)
                        cs.GoNext()
                    Loop While cs.OK()
                End If

                cs.Close()
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function

        Public Class ArchiveDateModel
            Public Year As Integer
            Public Month As Integer
        End Class

    End Class
End Namespace
