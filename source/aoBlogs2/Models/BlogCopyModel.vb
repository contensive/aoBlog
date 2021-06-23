
Imports Contensive.BaseClasses
Imports Contensive.Models.Db

Namespace Models
    ''' <summary>
    ''' Blog copy is the root content for the ccBlogCopy table. 
    ''' Blog Entries (posts) and Blog Comments (comments to posts) are both stored in Blog Copy
    ''' </summary>
    Public Class BlogCopyModel
        Inherits DbBaseModel
        '
        '====================================================================================================
        ''' <summary>
        '''table definition
        '''</summary>
        Public Shared ReadOnly Property tableMetadata As DbBaseTableMetadataModel = New DbBaseTableMetadataModel("Blog Copy", "ccBlogCopy", "default", False)
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
            Try
                Dim Sql As String = "select p.ID" _
                    & " from (ccBlogCopy p" _
                    & " left join BlogCategories c on c.id=p.blogCategoryID)" _
                    & " where (p.blogid=" & blogId & ")" _
                    & " and((c.id is null)or(c.UserBlocking=0)or(c.UserBlocking is null))"
                Return createList(Of BlogCopyModel)(cp, "(id in (" & Sql & "))")
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try

        End Function
        '
        ''' <summary>
        ''' Return a list of Archive Blog Copy
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="blogId">The id of the Blog Copy</param>
        ''' <returns></returns>
        Public Shared Function createArchiveListFromBlogCopy(cp As CPBaseClass, blogId As Integer) As List(Of ArchiveDateModel)
            Try
                Dim result As New List(Of ArchiveDateModel)
                Dim SQL = "SELECT DISTINCT month(dateadded) as archiveMonth, year(dateadded) as archiveYear" _
                            & " From ccBlogCopy" _
                            & " Where (ContentControlID = " & cp.Content.GetID(cnBlogEntries) & ") And (Active <> 0)" _
                            & " AND (BlogID=" & blogId & ")" _
                            & " ORDER BY year(dateadded) desc, month(dateadded) desc "
                Dim cs As CPCSBaseClass = cp.CSNew()
                If (cs.OpenSQL(SQL)) Then
                    Do
                        result.Add(New ArchiveDateModel With {
                            .Month = cs.GetInteger("archiveMonth"),
                            .Year = cs.GetInteger("archiveYear")
                        })
                        cs.GoNext()
                    Loop While cs.OK()
                End If

                cs.Close()
                Return result
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function

        Public Class ArchiveDateModel
            Public Year As Integer
            Public Month As Integer
        End Class

    End Class
End Namespace
