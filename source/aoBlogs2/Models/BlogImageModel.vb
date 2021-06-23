
Imports Contensive.BaseClasses
Imports Contensive.Models.Db

Namespace Models
    Public Class BlogImageModel
        Inherits DbBaseModel
        '
        '====================================================================================================
        ''' <summary>
        '''table definition
        '''</summary>
        Public Shared ReadOnly Property tableMetadata As DbBaseTableMetadataModel = New DbBaseTableMetadataModel("Blog Images", "BlogImages", "default", False)
        '
        '====================================================================================================
        ' -- instance properties
        '
        Public Property AltSizeList As String
        Public Property description As String
        Public Property Filename As DbBaseModel.FieldTypeFile
        Public Property height As Integer
        Public Property width As Integer
        '
        ''' <summary>
        ''' Return a list of blog entry images for the blog entry
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="entryId">The id of the blog entry</param>
        ''' <returns></returns>
        Public Shared Function createListFromBlogEntry(cp As CPBaseClass, entryId As Integer) As List(Of BlogImageModel)
            Try
                Dim result As New List(Of BlogImageModel)
                For Each Rule In DbBaseModel.createList(Of BlogImageRuleModel)(cp, "(BlogEntryID=" & entryId & ")")
                    Dim blogimage As BlogImageModel = DbBaseModel.create(Of BlogImageModel)(cp, Rule.BlogImageID)
                    If (blogimage IsNot Nothing) Then result.Add(blogimage)
                Next
                Return result
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
    End Class
End Namespace
